using System;
using System.Threading;
using System.Threading.Tasks;
using NeonBlocks.Assets;
using NeonBlocks.Config;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlocks.Tests
{
    /// <summary>
    /// Kiểm thử ConfigManager với backend giả: tải GameConfig qua key Addressables, parse đúng, lỗi được báo đúng loại
    /// và giải phóng TextAsset khi dispose.
    /// </summary>
    public class ConfigManagerTests
    {
        private FakeAddressableBackend backend;
        private AddressableManager assets;
        private ConfigManager config;

        /// <summary>
        /// Tạo backend, manager tài nguyên và ConfigManager mới cho mỗi test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            backend = new FakeAddressableBackend();
            assets = new AddressableManager(backend);
            config = new ConfigManager(assets);
        }

        /// <summary>
        /// Dispose các manager và hủy GameObject giả.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            config.Dispose();
            assets.Dispose();
            backend.DestroyCreatedObjects();
        }

        /// <summary>
        /// Kiểm tra JSON hợp lệ được tải qua key GameConfig, parse thành cấu hình và đánh dấu sẵn sàng.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task LoadAsync_ValidJson_ParsesAndIsReady()
        {
            backend.Assets[GameConfigParser.AddressKey] = new TextAsset(ConfigTestFiles.ReadGameConfigJson());

            await config.LoadAsync(CancellationToken.None);

            Assert.IsTrue(config.IsReady);
            Assert.AreEqual(10, config.Config.Board.Width);
            Assert.AreEqual(7, config.Config.Pieces.Count);
        }

        /// <summary>
        /// Kiểm tra truy cập Config trước khi tải ném InvalidOperationException.
        /// </summary>
        [Test]
        public void Config_BeforeLoad_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => { _ = config.Config; });
        }

        /// <summary>
        /// Kiểm tra JSON sai cú pháp làm LoadAsync ném JsonException, cấu hình không sẵn sàng.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task LoadAsync_MalformedJson_ThrowsJsonException()
        {
            backend.Assets[GameConfigParser.AddressKey] = new TextAsset("{ \"board\": ");
            bool thrown = false;

            try
            {
                await config.LoadAsync(CancellationToken.None);
            }
            catch (Newtonsoft.Json.JsonException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
            Assert.IsFalse(config.IsReady);
        }

        /// <summary>
        /// Kiểm tra thiếu asset GameConfig làm LoadAsync ném AddressableLoadException.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task LoadAsync_MissingAsset_ThrowsAddressableLoadException()
        {
            backend.FailingKeys.Add(GameConfigParser.AddressKey);
            bool thrown = false;

            try
            {
                await config.LoadAsync(CancellationToken.None);
            }
            catch (AddressableLoadException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
            Assert.IsFalse(config.IsReady);
        }

        /// <summary>
        /// Kiểm tra Dispose giải phóng TextAsset đúng một lần và xóa cấu hình đã tải.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task Dispose_ReleasesTextAssetOnceAndClearsConfig()
        {
            backend.Assets[GameConfigParser.AddressKey] = new TextAsset(ConfigTestFiles.ReadGameConfigJson());
            await config.LoadAsync(CancellationToken.None);

            config.Dispose();
            config.Dispose();
            await Task.Yield();

            Assert.AreEqual(1, backend.ReleasedAssetHandles.Count);
            Assert.IsFalse(config.IsReady);
        }
    }
}
