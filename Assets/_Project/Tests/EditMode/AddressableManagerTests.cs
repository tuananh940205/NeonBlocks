using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeonBlocks.Assets;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlocks.Tests
{
    /// <summary>
    /// Kiểm thử AddressableManager với backend giả: giải phóng đúng một lần theo chủ sở hữu, lỗi tải được bọc đúng loại,
    /// hủy khi đang tải và kết quả đến muộn được giải phóng.
    /// </summary>
    public class AddressableManagerTests
    {
        private FakeAddressableBackend backend;
        private AddressableManager manager;
        private AssetOwnerToken owner;

        /// <summary>
        /// Tạo backend giả, manager và chủ sở hữu mới cho mỗi test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            backend = new FakeAddressableBackend();
            backend.Assets["prefab"] = new GameObject("PrefabTemplate");
            manager = new AddressableManager(backend);
            owner = new AssetOwnerToken("Test:Owner");
        }

        /// <summary>
        /// Dispose manager và hủy GameObject giả để không ảnh hưởng test khác.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            manager.Dispose();
            backend.DestroyCreatedObjects();
        }

        /// <summary>
        /// Kiểm tra tải asset trả đúng giá trị và ghi nhận đúng một handle cho chủ sở hữu.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task LoadAsset_ReturnsAssetAndTracksOneHandle()
        {
            backend.Assets["cfg"] = new TextAsset("{}");

            TextAsset asset = await manager.LoadAssetAsync<TextAsset>("cfg", owner, CancellationToken.None);

            Assert.AreEqual("{}", asset.text);
            Assert.AreEqual(1, manager.TrackedCount);
        }

        /// <summary>
        /// Kiểm tra ReleaseOwnerAsync giải phóng handle đúng một lần, kể cả khi gọi lần thứ hai.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task ReleaseOwner_CalledTwice_ReleasesHandleOnce()
        {
            backend.Assets["cfg"] = new TextAsset("{}");
            await manager.LoadAssetAsync<TextAsset>("cfg", owner, CancellationToken.None);

            await manager.ReleaseOwnerAsync(owner);
            await manager.ReleaseOwnerAsync(owner);

            Assert.AreEqual(1, backend.ReleasedAssetHandles.Count);
            Assert.AreEqual(0, manager.TrackedCount);
            Assert.IsTrue(owner.IsReleased);
        }

        /// <summary>
        /// Kiểm tra key tải lỗi ném AddressableLoadException và không ghi nhận handle nào.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task LoadAsset_FailingKey_ThrowsAddressableLoadException_AndTracksNothing()
        {
            backend.FailingKeys.Add("missing");
            bool thrown = false;

            try
            {
                await manager.LoadAssetAsync<TextAsset>("missing", owner, CancellationToken.None);
            }
            catch (AddressableLoadException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
            Assert.AreEqual(0, manager.TrackedCount);
        }

        /// <summary>
        /// Kiểm tra key rỗng bị từ chối bằng AddressableLoadException trước khi gọi backend.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task LoadAsset_EmptyKey_ThrowsAddressableLoadException()
        {
            bool thrown = false;

            try
            {
                await manager.LoadAssetAsync<TextAsset>("  ", owner, CancellationToken.None);
            }
            catch (AddressableLoadException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
            Assert.AreEqual(0, backend.HeldCount);
        }

        /// <summary>
        /// Kiểm tra asset sai kiểu bị giải phóng ngay và ném AddressableLoadException.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task LoadAsset_WrongType_ReleasesHandleAndThrows()
        {
            backend.Assets["cfg"] = new TextAsset("{}");
            bool thrown = false;

            try
            {
                await manager.LoadAssetAsync<GameObject>("cfg", owner, CancellationToken.None);
            }
            catch (AddressableLoadException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
            Assert.AreEqual(1, backend.ReleasedAssetHandles.Count);
            Assert.AreEqual(0, manager.TrackedCount);
        }

        /// <summary>
        /// Kiểm tra hủy khi đang tải: yêu cầu ném OperationCanceledException, và kết quả đến sau đó bị giải phóng đúng một lần.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task Cancel_WhileLoading_ThrowsOperationCanceled_AndReleasesLateResult()
        {
            backend.HoldLoads = true;
            backend.Assets["cfg"] = new TextAsset("{}");
            var cts = new CancellationTokenSource();
            Task<TextAsset> load = manager.LoadAssetAsync<TextAsset>("cfg", owner, cts.Token);
            bool cancelled = false;

            cts.Cancel();
            try
            {
                await load;
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
            }

            backend.CompleteHeld(0, true);

            Assert.IsTrue(cancelled);
            Assert.AreEqual(1, backend.ReleasedAssetHandles.Count);
            Assert.AreEqual(0, manager.TrackedCount);
        }

        /// <summary>
        /// Kiểm tra chủ sở hữu kết thúc khi đang tải: kết quả đến sau bị giải phóng và yêu cầu ném OperationCanceledException.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task OwnerReleasedWhileLoading_LateResultIsReleasedAndCancelled()
        {
            backend.HoldLoads = true;
            backend.Assets["cfg"] = new TextAsset("{}");
            Task<TextAsset> load = manager.LoadAssetAsync<TextAsset>("cfg", owner, CancellationToken.None);
            await manager.ReleaseOwnerAsync(owner);
            bool cancelled = false;

            backend.CompleteHeld(0, true);
            try
            {
                await load;
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
            }

            Assert.IsTrue(cancelled);
            Assert.AreEqual(1, backend.ReleasedAssetHandles.Count);
            Assert.AreEqual(0, manager.TrackedCount);
        }

        /// <summary>
        /// Kiểm tra instance được giải phóng bằng ReleaseInstance đúng một lần; lần gọi sau trả false.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task Instantiate_ReleaseInstance_ReleasesOnce()
        {
            GameObject go = await manager.InstantiateAsync("prefab", null, owner, CancellationToken.None);

            Assert.IsTrue(manager.ReleaseInstance(go));
            Assert.IsFalse(manager.ReleaseInstance(go));
            Assert.IsTrue(go == null);
        }

        /// <summary>
        /// Kiểm tra instance đã giải phóng thủ công không bị giải phóng lần nữa khi chủ sở hữu kết thúc.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task ReleaseOwner_AfterInstanceAlreadyReleased_DoesNotReleaseInstanceAgain()
        {
            GameObject go = await manager.InstantiateAsync("prefab", null, owner, CancellationToken.None);
            manager.ReleaseInstance(go);

            await manager.ReleaseOwnerAsync(owner);

            Assert.IsTrue(go == null);
        }

        /// <summary>
        /// Kiểm tra scene tải rồi unload bằng ReleaseSceneAsync đúng một lần; lần gọi sau trả false.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task LoadScene_ReleaseSceneAsync_UnloadsOnce()
        {
            LoadedScene scene = await manager.LoadSceneAsync("MainMenu", owner, CancellationToken.None);

            Assert.IsTrue(await manager.ReleaseSceneAsync(scene));
            Assert.IsFalse(await manager.ReleaseSceneAsync(scene));
            CollectionAssert.AreEqual(new[] { "load:MainMenu", "unload:MainMenu" }, backend.SceneLog);
        }

        /// <summary>
        /// Kiểm tra ReleaseOwnerAsync chỉ giải phóng tài nguyên của chủ sở hữu đó, không đụng chủ sở hữu khác.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task ReleaseOwner_ReleasesOnlyThatOwner()
        {
            var other = new AssetOwnerToken("Test:Other");
            backend.Assets["a"] = new TextAsset("a");
            backend.Assets["b"] = new TextAsset("b");
            await manager.LoadAssetAsync<TextAsset>("a", owner, CancellationToken.None);
            await manager.LoadAssetAsync<TextAsset>("b", other, CancellationToken.None);

            await manager.ReleaseOwnerAsync(owner);

            Assert.AreEqual(1, manager.TrackedCount);
            Assert.AreEqual(1, backend.ReleasedAssetHandles.Count);
            Assert.IsFalse(other.IsReleased);
        }

        /// <summary>
        /// Kiểm tra Dispose giải phóng toàn bộ tài nguyên còn lại và các yêu cầu mới sau đó bị từ chối bằng InvalidOperationException.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task Dispose_ReleasesEverything_AndRejectsNewRequests()
        {
            backend.Assets["cfg"] = new TextAsset("{}");
            await manager.LoadAssetAsync<TextAsset>("cfg", owner, CancellationToken.None);
            GameObject go = await manager.InstantiateAsync("prefab", null, owner, CancellationToken.None);
            bool rejected = false;

            manager.Dispose();
            manager.Dispose();

            try
            {
                await manager.LoadAssetAsync<TextAsset>("cfg", owner, CancellationToken.None);
            }
            catch (InvalidOperationException)
            {
                rejected = true;
            }

            Assert.IsTrue(rejected);
            Assert.AreEqual(2, backend.ReleasedAssetHandles.Count);
            Assert.IsTrue(go == null);
        }

        /// <summary>
        /// Kiểm tra tải cùng key và kiểu trong owner trả cùng asset từ một handle cached duy nhất.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task LoadAsset_SameKeyAndType_ReusesOneSuccessHandle()
        {
            backend.Assets["cfg"] = new TextAsset("{}");

            TextAsset first = await manager.LoadAssetAsync<TextAsset>("cfg", owner, CancellationToken.None);
            TextAsset second = await manager.LoadAssetAsync<TextAsset>("cfg", owner, CancellationToken.None);

            Assert.AreSame(first, second);
            Assert.AreEqual(1, backend.LoadAssetRequestCount);
            await manager.ReleaseOwnerAsync(owner);
            Assert.AreEqual(1, backend.ReleasedAssetHandles.Count);
        }

        /// <summary>
        /// Kiểm tra các request đồng thời cùng key và kiểu cùng chờ một request backend duy nhất.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task LoadAsset_ConcurrentSameKeyAndType_SharesOneRequest()
        {
            backend.HoldLoads = true;
            backend.Assets["cfg"] = new TextAsset("{}");

            Task<TextAsset> first = manager.LoadAssetAsync<TextAsset>("cfg", owner, CancellationToken.None);
            Task<TextAsset> second = manager.LoadAssetAsync<TextAsset>("cfg", owner, CancellationToken.None);
            backend.CompleteHeld(0, true);

            await Task.WhenAll(first, second);
            Assert.AreEqual(1, backend.LoadAssetRequestCount);
        }

        /// <summary>
        /// Kiểm tra prefab cached chỉ tải một handle nhưng mỗi lần tạo view là một instance thuần túy riêng biệt.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task Instantiate_SamePrefab_ReusesCachedHandleAndCreatesDistinctInstances()
        {
            GameObject first = await manager.InstantiateAsync("prefab", null, owner, CancellationToken.None);
            GameObject second = await manager.InstantiateAsync("prefab", null, owner, CancellationToken.None);

            Assert.AreNotSame(first, second);
            Assert.AreEqual(1, backend.LoadAssetRequestCount);
            await manager.ReleaseOwnerAsync(owner);
            Assert.AreEqual(1, backend.ReleasedAssetHandles.Count);
            Assert.IsTrue(first == null);
            Assert.IsTrue(second == null);
        }
    }
}
