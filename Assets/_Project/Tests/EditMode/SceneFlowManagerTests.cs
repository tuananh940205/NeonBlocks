using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NeonBlocks.Assets;
using NeonBlocks.Scene;
using NUnit.Framework;

namespace NeonBlocks.Tests
{
    /// <summary>
    /// Kiểm thử SceneFlowManager với backend giả: thứ tự rời scene (stop → dispose → unload), lỗi tải,
    /// hủy chuyển scene khi có yêu cầu mới, và dọn scene khi dispose.
    /// </summary>
    public class SceneFlowManagerTests
    {
        private FakeAddressableBackend backend;
        private AddressableManager assets;
        private SceneFlowManager flow;
        private List<string> log;

        /// <summary>
        /// Signal được resolve lazy từ ServiceLocator (không đăng ký thủ công),
        /// tạo backend, manager và SceneFlowManager mới; ghi nhận stop/dispose vào nhật ký chung.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Instance.UnregisterAll();
            log = new List<string>();

            ServiceLocator.Instance.Resolve<SceneStopSignal>().AddListener(() => log.Add("stop"));
            ServiceLocator.Instance.Resolve<SceneDisposeSignal>().AddListener(() => log.Add("dispose"));

            backend = new FakeAddressableBackend();
            assets = new AddressableManager(backend);
            flow = new SceneFlowManager(assets);
        }

        /// <summary>
        /// Dispose manager, hủy GameObject giả và xóa đăng ký ServiceLocator.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            flow.Dispose();
            assets.Dispose();
            backend.DestroyCreatedObjects();
            ServiceLocator.Instance.UnregisterAll();
        }

        /// <summary>
        /// Kiểm tra chuyển scene đầu tiên tải scene và đặt làm scene hiện tại; không có stop/dispose vì chưa có scene cũ.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task GoTo_FirstScene_LoadsAndBecomesCurrent()
        {
            await flow.GoToAsync("MainMenu", CancellationToken.None);

            Assert.AreEqual("MainMenu", flow.CurrentSceneKey);
            CollectionAssert.AreEqual(new[] { "load:MainMenu" }, backend.SceneLog);
            CollectionAssert.IsEmpty(log);
        }

        /// <summary>
        /// Kiểm tra chuyển scene thứ hai rời scene cũ đúng thứ tự: stop, dispose, unload scene cũ, rồi mới load scene mới.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task GoTo_NextScene_ExitsPreviousInRequiredOrder()
        {
            await flow.GoToAsync("MainMenu", CancellationToken.None);
            await flow.GoToAsync("Gameplay", CancellationToken.None);

            CollectionAssert.AreEqual(new[] { "load:MainMenu", "unload:MainMenu", "load:Gameplay" }, backend.SceneLog);
            CollectionAssert.AreEqual(new[] { "stop", "dispose" }, log);
            Assert.AreEqual("Gameplay", flow.CurrentSceneKey);
        }

        /// <summary>
        /// Kiểm tra scene đích tải lỗi: ném AddressableLoadException và không còn scene hiện tại.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task GoTo_LoadFails_ThrowsAndLeavesNoCurrentScene()
        {
            await flow.GoToAsync("MainMenu", CancellationToken.None);
            backend.FailingKeys.Add("Gameplay");
            bool thrown = false;

            try
            {
                await flow.GoToAsync("Gameplay", CancellationToken.None);
            }
            catch (AddressableLoadException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
            Assert.IsNull(flow.CurrentSceneKey);
            CollectionAssert.Contains(backend.SceneLog, "unload:MainMenu");
        }

        /// <summary>
        /// Kiểm tra yêu cầu mới trong lúc đang tải hủy yêu cầu cũ; scene cũ được giải phóng và scene mới trở thành hiện tại.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task NewRequest_CancelsInFlightTransition()
        {
            backend.HoldLoads = true;
            Task first = flow.GoToAsync("MainMenu", CancellationToken.None);
            Task second = flow.GoToAsync("Gameplay", CancellationToken.None);
            bool firstCancelled = false;

            try
            {
                await first;
            }
            catch (OperationCanceledException)
            {
                firstCancelled = true;
            }

            backend.CompleteHeld(0, true);
            backend.CompleteHeld(1, true);
            await second;

            Assert.IsTrue(firstCancelled);
            Assert.AreEqual("Gameplay", flow.CurrentSceneKey);
            CollectionAssert.Contains(backend.SceneLog, "unload:MainMenu");
        }

        /// <summary>
        /// Kiểm tra Dispose rời scene hiện tại (stop, dispose, unload) và xóa scene hiện tại.
        /// </summary>
        [Test, Timeout(5000)]
        public async Task Dispose_ExitsCurrentScene()
        {
            await flow.GoToAsync("MainMenu", CancellationToken.None);

            flow.Dispose();

            CollectionAssert.AreEqual(new[] { "stop", "dispose" }, log);
            CollectionAssert.Contains(backend.SceneLog, "unload:MainMenu");
            Assert.IsNull(flow.CurrentSceneKey);
        }
    }
}
