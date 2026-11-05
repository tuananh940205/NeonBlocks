using System;
using System.Threading;
using System.Threading.Tasks;
using NeonBlocks.Assets;
using UnityEngine;

namespace NeonBlocks.Scene
{
    /// <summary>
    /// Manager điều phối chuyển scene xuyên suốt session: rời scene cũ theo đúng thứ tự (stop → dispose → release scope)
    /// rồi tải scene mới bằng Addressables. Không kế thừa MonoBehaviour. Không giữ tham chiếu tới Signal:
    /// mỗi lần cần phát hoặc gỡ listener thì resolve từ ServiceLocator.
    /// Yêu cầu đổi scene từ view đến qua SceneRequestSignal; cùng lúc chỉ có một chuyển scene đang chạy, yêu cầu mới hủy yêu cầu cũ.
    /// </summary>
    public sealed class SceneFlowManager : IDisposable
    {
        private readonly AddressableManager assets;
        private SceneAssetScope currentScope;
        private CancellationTokenSource transitionSource;
        private bool isDisposed;

        /// <summary>Address key của scene đang hoạt động, null khi chưa có scene nào.</summary>
        public string CurrentSceneKey => currentScope?.SceneKey;

        /// <summary>Phạm vi tài nguyên của scene đang hoạt động, null khi chưa có scene nào.</summary>
        public SceneAssetScope CurrentScope => currentScope;

        /// <summary>
        /// Tạo manager và đăng ký lắng nghe SceneRequestSignal (resolve lazy từ ServiceLocator).
        /// </summary>
        /// <param name="assets">AddressableManager dùng để tải scene.</param>
        public SceneFlowManager(AddressableManager assets)
        {
            this.assets = assets ?? throw new ArgumentNullException(nameof(assets));
            ServiceLocator.Instance.Resolve<SceneRequestSignal>().AddListener(OnSceneRequested);
        }

        /// <summary>
        /// Chuyển sang scene mới: rời scene hiện tại rồi tải scene đích bằng Addressables (Additive).
        /// Nếu tải lỗi, ném AddressableLoadException và không có scene nào hoạt động cho đến lần chuyển thành công tiếp theo.
        /// Nếu yêu cầu bị hủy bởi yêu cầu mới, ném OperationCanceledException.
        /// </summary>
        /// <param name="sceneKey">Address key của scene đích, trùng tên asset scene.</param>
        /// <param name="externalCt">Token hủy từ bên gọi, ví dụ khi session kết thúc.</param>
        /// <returns>Task hoàn tất khi scene đích đã tải.</returns>
        public async Task GoToAsync(string sceneKey, CancellationToken externalCt)
        {
            if (isDisposed)
            {
                throw new InvalidOperationException("SceneFlowManager đã dispose.");
            }

            if (string.IsNullOrWhiteSpace(sceneKey))
            {
                throw new AddressableLoadException("Scene key không được rỗng.");
            }

            transitionSource?.Cancel();
            transitionSource?.Dispose();
            transitionSource = new CancellationTokenSource();

            using (CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(transitionSource.Token, externalCt))
            {
                CancellationToken ct = linked.Token;

                await ExitCurrentSceneAsync();

                var scope = new SceneAssetScope(sceneKey, assets);

                try
                {
                    await scope.LoadSceneAsync(ct);
                }
                catch
                {
                    await scope.ReleaseAsync();
                    throw;
                }

                currentScope = scope;
            }
        }

        /// <summary>
        /// Rời scene hiện tại theo đúng thứ tự: phát stop, phát dispose, rồi giải phóng phạm vi scene.
        /// Signal được resolve tại thời điểm phát. Không có scene hiện tại thì không làm gì.
        /// </summary>
        /// <returns>Task hoàn tất khi scene cũ đã unload.</returns>
        private async Task ExitCurrentSceneAsync()
        {
            if (currentScope == null)
            {
                return;
            }

            SceneAssetScope previous = currentScope;
            currentScope = null;

            ServiceLocator.Instance.Resolve<SceneStopSignal>().Dispatch();
            ServiceLocator.Instance.Resolve<SceneDisposeSignal>().Dispatch();
            await previous.ReleaseAsync();
        }

        /// <summary>
        /// Xử lý SceneRequestSignal: khởi động chuyển scene và ghi log nếu thất bại, vì signal không trả Task.
        /// </summary>
        /// <param name="sceneKey">Address key của scene đích.</param>
        private void OnSceneRequested(string sceneKey)
        {
            GoToAsync(sceneKey, CancellationToken.None).ContinueWith(t =>
            {
                Debug.LogError($"Chuyển sang scene '{sceneKey}' thất bại: {t.Exception?.GetBaseException().Message}");
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Hủy chuyển scene đang chạy, gỡ listener khỏi SceneRequestSignal và rời scene hiện tại. Việc unload được khởi động
        /// nhưng không chờ. Signal được resolve lúc dọn dẹp, không giữ tham chiếu. Gọi nhiều lần an toàn.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            ServiceLocator.Instance.Resolve<SceneRequestSignal>().RemoveListener(OnSceneRequested);
            transitionSource?.Cancel();
            transitionSource?.Dispose();
            transitionSource = null;

            if (currentScope != null)
            {
                SceneAssetScope previous = currentScope;
                currentScope = null;
                ServiceLocator.Instance.Resolve<SceneStopSignal>().Dispatch();
                ServiceLocator.Instance.Resolve<SceneDisposeSignal>().Dispatch();
                _ = previous.ReleaseAsync();
            }
        }
    }
}
