using System;
using System.Threading.Tasks;
using NeonBlocks.Save;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
namespace NeonBlocks.Tests
{
    /// <summary>Test file thiếu/hỏng/version/I/O và race score/settings, không chạm save thật.</summary>
    public sealed class LocalSaveManagerTests
    {
        /// <summary>File thiếu dùng default.</summary>
        [Test] public async Task Missing_Defaults()
        {
            var manager = new LocalSaveManager(new FakeSaveStorage());
            Assert.AreEqual(SaveStatus.Missing, (await manager.LoadAsync()).Status);
            Assert.AreEqual(0, manager.Data.BestScore); Assert.AreEqual(1f, manager.Data.MusicVolume);
        }
        /// <summary>Round-trip score và settings, điểm không giảm.</summary>
        [Test] public async Task RoundTrip_AndScoreNeverDecreases()
        {
            var storage = new FakeSaveStorage(); var manager = new LocalSaveManager(storage);
            await manager.LoadAsync(); await manager.RecordScoreAsync(700); await manager.RecordScoreAsync(20);
            await manager.SetSettingsAsync(0.3f, 0.7f, true);
            var loaded = new LocalSaveManager(storage);
            Assert.AreEqual(SaveStatus.Loaded, (await loaded.LoadAsync()).Status);
            Assert.AreEqual(700, loaded.Data.BestScore); Assert.AreEqual(0.3f, loaded.Data.MusicVolume);
            Assert.IsTrue(loaded.Data.Muted);
        }
        /// <summary>JSON hỏng không bị ghi đè âm thầm.</summary>
        [Test] public async Task Corrupt_IsProtected()
        {
            var storage = new FakeSaveStorage { Content = "{broken" };
            var manager = new LocalSaveManager(storage);
            Assert.AreEqual(SaveStatus.Corrupt, (await manager.LoadAsync()).Status);
            Assert.AreEqual(SaveStatus.WriteBlocked, (await manager.RecordScoreAsync(300)).Status);
            Assert.AreEqual("{broken", storage.Content); Assert.AreEqual(300, manager.Data.BestScore);
        }
        /// <summary>Version lạ không bị downgrade.</summary>
        [Test] public async Task FutureVersion_IsProtected()
        {
            var storage = new FakeSaveStorage(); var first = new LocalSaveManager(storage);
            await first.RecordScoreAsync(1); JObject json = JObject.Parse(storage.Content); json["version"] = 99; storage.Content = json.ToString();
            string before = storage.Content; var manager = new LocalSaveManager(storage);
            Assert.AreEqual(SaveStatus.UnsupportedVersion, (await manager.LoadAsync()).Status);
            Assert.AreEqual(SaveStatus.WriteBlocked, (await manager.SetSettingsAsync(0.5f, 0.5f, false)).Status);
            Assert.AreEqual(before, storage.Content);
            storage.Content = "{\"version\":99,\"futureField\":true}";
            Assert.AreEqual(SaveStatus.UnsupportedVersion, (await manager.LoadAsync()).Status);
        }
        /// <summary>Field thiếu và volume ngoài range đều corrupt.</summary>
        [Test] public async Task InvalidPayload_IsCorrupt()
        {
            var storage = new FakeSaveStorage { Content = "{}" }; var manager = new LocalSaveManager(storage);
            Assert.AreEqual(SaveStatus.Corrupt, (await manager.LoadAsync()).Status);
            storage.Content = "{\"version\":1,\"bestScore\":0,\"musicVolume\":2,\"sfxVolume\":1,\"muted\":false}";
            Assert.AreEqual(SaveStatus.Corrupt, (await manager.LoadAsync()).Status);
        }
        /// <summary>Write lỗi giữ bản tốt, retry vẫn ghi dữ liệu session.</summary>
        [Test] public async Task FailedWrite_PreservesOldAndCanRetry()
        {
            var storage = new FakeSaveStorage(); var manager = new LocalSaveManager(storage);
            await manager.RecordScoreAsync(10); string old = storage.Content; storage.FailWrite = true;
            Assert.AreEqual(SaveStatus.IoFailure, (await manager.RecordScoreAsync(100)).Status);
            Assert.AreEqual(old, storage.Content); storage.FailWrite = false;
            Assert.AreEqual(SaveStatus.Saved, (await manager.RecordScoreAsync(100)).Status);
        }
        /// <summary>Read lỗi có status và chặn ghi để bảo toàn file chưa đọc được.</summary>
        [Test] public async Task ReadFailure_IsExplicit()
        {
            var manager = new LocalSaveManager(new FakeSaveStorage { FailRead = true });
            Assert.AreEqual(SaveStatus.IoFailure, (await manager.LoadAsync()).Status);
            Assert.AreEqual(SaveStatus.WriteBlocked, (await manager.RecordScoreAsync(10)).Status);
        }
        /// <summary>Các update đồng thời không làm mất best score.</summary>
        [Test] public async Task ConcurrentUpdates_DoNotLoseScore()
        {
            var manager = new LocalSaveManager(new FakeSaveStorage());
            await Task.WhenAll(manager.RecordScoreAsync(200), manager.SetSettingsAsync(0.2f, 0.4f, true), manager.RecordScoreAsync(50));
            Assert.AreEqual(200, manager.Data.BestScore); Assert.AreEqual(0.2f, manager.Data.MusicVolume);
        }
        /// <summary>API validate không cho NaN.</summary>
        [Test] public void NonFiniteVolume_IsRejected() => Assert.Throws<ArgumentOutOfRangeException>(() => new SaveData(0, float.NaN));
    }
}