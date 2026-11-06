using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NeonBlocks.Save.Dto;
namespace NeonBlocks.Save
{
    /// <summary>Manager save session: tuần tự hóa load/update/write, lỗi chuyển thành kết quả rõ ràng.</summary>
    public sealed class LocalSaveManager
    {
        private readonly ISaveStorage storage;
        private readonly SemaphoreSlim gate = new SemaphoreSlim(1, 1);
        private SaveData data = new SaveData();
        private SaveResult lastResult = new SaveResult(SaveStatus.Missing);
        private bool writeBlocked;
        public SaveData Data => data;
        public SaveResult LastResult => lastResult;
        /// <summary>Inject storage dùng thật hoặc giả.</summary>
        public LocalSaveManager(ISaveStorage storage) { this.storage = storage ?? throw new ArgumentNullException(nameof(storage)); }
        /// <summary>Load có version; dữ liệu hỏng hoặc version lạ fallback mặc định và khóa ghi để bảo toàn file cũ.</summary>
        public async Task<SaveResult> LoadAsync()
        {
            await gate.WaitAsync();
            try
            {
                data = new SaveData(); writeBlocked = false;
                string json = await storage.ReadAsync();
                if (json == null) { return lastResult = new SaveResult(SaveStatus.Missing); }
                JObject document = JObject.Parse(json);
                JToken version = document["version"];
                if (version == null || version.Type != JTokenType.Integer) { throw new JsonSerializationException("Thiếu version hợp lệ."); }
                if (version.Value<long>() != SaveData.CurrentVersion)
                { writeBlocked = true; return lastResult = new SaveResult(SaveStatus.UnsupportedVersion, "Save khác phiên bản; dùng mặc định và giữ nguyên file cũ."); }
                SaveDataDto dto = document.ToObject<SaveDataDto>();
                data = new SaveData(dto.bestScore, dto.musicVolume, dto.sfxVolume, dto.muted);
                return lastResult = new SaveResult(SaveStatus.Loaded);
            }
            catch (JsonException) { writeBlocked = true; return lastResult = new SaveResult(SaveStatus.Corrupt, "Save hỏng; dùng mặc định, file cũ được giữ nguyên."); }
            catch (ArgumentOutOfRangeException) { writeBlocked = true; return lastResult = new SaveResult(SaveStatus.Corrupt, "Giá trị save không hợp lệ; dùng mặc định, file cũ được giữ nguyên."); }
            catch (IOException ex) { writeBlocked = true; return lastResult = new SaveResult(SaveStatus.IoFailure, "Không đọc được save: " + ex.Message); }
            catch (UnauthorizedAccessException ex) { writeBlocked = true; return lastResult = new SaveResult(SaveStatus.IoFailure, "Không có quyền đọc save: " + ex.Message); }
            finally { gate.Release(); }
        }
        /// <summary>Cập nhật settings và ghi; giữ dữ liệu mới trong session để người dùng có thể thử ghi lại.</summary>
        public Task<SaveResult> SetSettingsAsync(float music, float sfx, bool muted) => UpdateAsync(null, music, sfx, muted);
        /// <summary>Best score chỉ tăng; không ghi lặp khi không có thay đổi.</summary>
        public Task<SaveResult> RecordScoreAsync(int score) => UpdateAsync(score, null, null, null);
        /// <summary>Gom update và write trong cùng gate để không mất best score khi settings ghi đồng thời.</summary>
        private async Task<SaveResult> UpdateAsync(int? score, float? music, float? sfx, bool? muted)
        {
            await gate.WaitAsync();
            try
            {
                if (score < 0) { throw new ArgumentOutOfRangeException(nameof(score)); }
                SaveData next = new SaveData(Math.Max(data.BestScore, score ?? 0), music ?? data.MusicVolume, sfx ?? data.SfxVolume, muted ?? data.Muted);
                bool unchanged = next.BestScore == data.BestScore && next.MusicVolume == data.MusicVolume && next.SfxVolume == data.SfxVolume && next.Muted == data.Muted;
                data = next;
                if (writeBlocked) { return lastResult = new SaveResult(SaveStatus.WriteBlocked, "Chưa lưu: hãy sao lưu/đổi tên file lỗi hoặc khác version rồi khởi động lại."); }
                if (unchanged && lastResult.Status == SaveStatus.Saved) { return lastResult; }
                await storage.WriteAtomicAsync(JsonConvert.SerializeObject(new SaveDataDto(SaveData.CurrentVersion, data.BestScore, data.MusicVolume, data.SfxVolume, data.Muted), Formatting.Indented));
                return lastResult = new SaveResult(SaveStatus.Saved);
            }
            catch (IOException ex) { return lastResult = new SaveResult(SaveStatus.IoFailure, "Ghi save thất bại, dữ liệu session vẫn còn: " + ex.Message); }
            catch (UnauthorizedAccessException ex) { return lastResult = new SaveResult(SaveStatus.IoFailure, "Không có quyền ghi save: " + ex.Message); }
            finally { gate.Release(); }
        }
    }
}