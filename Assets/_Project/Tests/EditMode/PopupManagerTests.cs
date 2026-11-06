using System;
using System.Threading;
using System.Threading.Tasks;
using NeonBlocks.Assets;
using NeonBlocks.Popups;
using NUnit.Framework;
using UnityEngine;
namespace NeonBlocks.Tests
{
    /// <summary>Kiểm tra instance/owner qua backend giả và Unity GameObject thật trong EditMode.</summary>
    public sealed class PopupManagerTests
    {
        private FakeAddressableBackend backend;
        private AddressableManager assets;
        private SceneAssetScope scope;
        private PopupManager manager;
        private GameObject prefab;
        /// <summary>Tạo scope và template giả.</summary>
        [SetUp] public void Setup()
        {
            backend = new FakeAddressableBackend(); assets = new AddressableManager(backend);
            scope = new SceneAssetScope("Gameplay", assets); manager = new PopupManager();
            prefab = new GameObject("PausePopup"); backend.Assets["PausePopup"] = prefab;
        }
        /// <summary>Dọn template và owner đúng thứ tự.</summary>
        [TearDown] public async Task Cleanup()
        {
            manager.Dispose(); await scope.ReleaseAsync(); assets.Dispose();
            UnityEngine.Object.DestroyImmediate(prefab); backend.DestroyCreatedObjects();
        }
        /// <summary>Mở trùng chỉ trả cùng instance, close hai lần không nhân release.</summary>
        [Test] public async Task DuplicateOpen_AndCloseAreIdempotent()
        {
            GameObject first = await manager.OpenAsync(scope, "PausePopup", null, CancellationToken.None);
            GameObject second = await manager.OpenAsync(scope, "PausePopup", null, CancellationToken.None);
            Assert.AreSame(first, second); Assert.AreEqual(1, manager.OpenCount);
            manager.Close("Gameplay"); manager.Close("Gameplay");
            Assert.AreEqual(0, manager.OpenCount); Assert.IsTrue(first == null || !first.activeSelf);
        }
        /// <summary>Đóng trong lúc tải không hiển thị lại instance muộn.</summary>
        [Test] public void CloseWhileLoading_ReleasesLateResult()
        {
            backend.HoldLoads = true;
            Task<GameObject> task = manager.OpenAsync(scope, "PausePopup", null, CancellationToken.None);
            manager.Close("Gameplay"); backend.CompleteHeld(0, true);
            Assert.CatchAsync<OperationCanceledException>(async () => await task);
            Assert.AreEqual(0, manager.OpenCount);
        }
        /// <summary>Load lỗi bỏ slot, cho phép retry.</summary>
        [Test] public async Task FailedLoad_AllowsRetry()
        {
            backend.FailingKeys.Add("PausePopup");
            Assert.ThrowsAsync<AddressableLoadException>(async () => await manager.OpenAsync(scope, "PausePopup", null, CancellationToken.None));
            Assert.AreEqual(0, manager.OpenCount);
            backend.FailingKeys.Clear();
            Assert.IsNotNull(await manager.OpenAsync(scope, "PausePopup", null, CancellationToken.None));
        }
        /// <summary>Scope release dọn instance kể cả trước khi host đóng.</summary>
        [Test] public async Task ScopeRelease_RemovesInstance()
        {
            GameObject instance = await manager.OpenAsync(scope, "PausePopup", null, CancellationToken.None);
            await scope.ReleaseAsync(); manager.Close("Gameplay");
            Assert.IsTrue(instance == null);
            Assert.AreEqual(0, manager.OpenCount);
        }
    }
}