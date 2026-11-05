using System;
using System.Threading;
using NeonBlocks.Assets;
using NeonBlocks.App;
using NeonBlocks.Config;
using NeonBlocks.Scene;
using UnityEngine;

namespace NeonBlocks
{
    /// <summary>
    /// Điểm khởi động của Entry scene: tạo các Manager theo thứ tự phụ thuộc, đăng ký vào SessionManager (lấy qua ServiceLocator),
    /// tải cấu hình, rồi chuyển sang MainMenu và phát BootVisualsHideSignal để ẩn BootVisuals.
    /// Chỉ chạy trong scene Entry (scene đầu tiên trong Build Settings). SessionManager sống trên GameObject DontDestroyOnLoad riêng.
    /// DefaultExecutionOrder thấp để Start chạy trước BootVisualsView.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    public class EntryLauncher : MonoBehaviour
    {
        private CancellationTokenSource lifetime;

        /// <summary>
        /// Khởi động session: kiểm tra đang ở scene Entry và session đang sở hữu hợp lệ, đăng ký Manager trước mọi await,
        /// tải cấu hình, chuyển scene đầu, rồi phát signal ẩn BootVisuals.
        /// Lỗi được ghi log; hủy do thoát app hoặc Restart Game thì không ghi lỗi.
        /// </summary>
        private async void Start()
        {
            // Entry là scene đầu tiên trong Build Settings; EntryLauncher không được đặt ở scene khác.
            if (gameObject.scene.buildIndex != 0)
            {
                Debug.LogError("EntryLauncher chỉ được đặt trong scene Entry.");
                return;
            }

            SessionManager session = ServiceLocator.Instance.Resolve<SessionManager>();

            // Session là singleton đã được Awake đăng ký; chỉ owner được phép khởi động các manager dùng chung.
            if (!session.IsOwner)
            {
                return;
            }

            lifetime = new CancellationTokenSource();

            try
            {
                var assets = new AddressableManager(new UnityAddressableBackend());
                var config = new ConfigManager(assets);
                var sceneFlow = new SceneFlowManager(assets);

                session.RegisterSingleton(assets);
                session.RegisterSingleton(config);
                session.RegisterSingleton(sceneFlow);

                await config.LoadAsync(lifetime.Token);
                await sceneFlow.GoToAsync(config.Config.Scenes.MainMenu, lifetime.Token);

                ServiceLocator.Instance.Resolve<BootVisualsHideSignal>().Dispatch();
            }
            catch (OperationCanceledException)
            {
                // Session kết thúc trong lúc khởi động (thoát Play Mode hoặc Restart Game): không phải lỗi.
            }
            catch (Exception ex)
            {
                Debug.LogError($"Khởi động Entry thất bại: {ex.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Hủy mọi tác vụ khởi động đang chạy khi Entry bị hủy; việc giải phóng Manager do SessionManager thực hiện.
        /// </summary>
        private void OnDestroy()
        {
            if (lifetime != null)
            {
                lifetime.Cancel();
                lifetime.Dispose();
                lifetime = null;
            }
        }
    }
}
