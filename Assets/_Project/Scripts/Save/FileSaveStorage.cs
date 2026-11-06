using System.IO;
using System.Threading.Tasks;
namespace NeonBlocks.Save
{
    /// <summary>Backend file; ghi temp cùng thư mục rồi replace, giữ bản trước ở .bak.</summary>
    public sealed class FileSaveStorage : ISaveStorage
    {
        private readonly string path;
        /// <summary>Nhận đường dẫn persistentDataPath từ bootstrap.</summary>
        public FileSaveStorage(string path) { this.path = path; }
        /// <summary>Đọc file async; file thiếu không phải lỗi.</summary>
        public Task<string> ReadAsync() => Task.Run(() => File.Exists(path) ? File.ReadAllText(path) : null);
        /// <summary>Không xóa file đích trước khi bản mới được ghi hoàn tất.</summary>
        public Task WriteAtomicAsync(string content) => Task.Run(() =>
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, content);
            if (File.Exists(path)) { File.Replace(temporary, path, path + ".bak"); }
            else { File.Move(temporary, path); }
        });
    }
}