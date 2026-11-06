using NeonBlocks.Config;
using NeonBlocks.Gameplay;
using NeonBlocks.Scene;
using UnityEngine;
using UnityEngine.UI;

namespace NeonBlocks.Views
{
    /// <summary>
    /// Script gắn trên Canvas của Gameplay. Đây là nơi duy nhất giữ tham chiếu tới nút Back (serialized field trên cùng Canvas)
    /// và là nơi duy nhất thêm/gỡ listener của nút. Không giữ tham chiếu tới Signal: resolve khi đăng ký, khi gỡ và khi phát.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameplayCanvas : MonoBehaviour
    {
        /// <summary>Nút Back nằm dưới Canvas này, gán trong Inspector của chính Canvas.</summary>
        [SerializeField] private Button backButton;
        /// <summary>Nút Hold thuộc Canvas này; chỉ Canvas quản lý listener.</summary>
        [SerializeField] private Button holdButton;
        /// <summary>Nút restart ván sau GameOver thuộc Canvas này.</summary>
        [SerializeField] private Button restartButton;
        private bool isStopping;

        /// <summary>
        /// Đăng ký listener của nút Back và sự kiện dừng scene. Chỉ kiểm tra nút, vì signal không bao giờ null.
        /// </summary>
        private void OnEnable()
        {
            isStopping = false;
            if (backButton == null)
            {
                Debug.LogError("GameplayCanvas chưa gán Back Button trong Inspector của Canvas.");
                return;
            }

            ServiceLocator.Instance.Resolve<SceneStopSignal>().AddListener(OnSceneStopping);
            backButton.onClick.AddListener(OnBackClicked);
            if (holdButton != null) { holdButton.onClick.AddListener(OnHoldClicked); holdButton.interactable = false; }
            if (restartButton != null) { restartButton.onClick.AddListener(OnRestartClicked); restartButton.interactable = false; }
            ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().AddListener(OnSnapshot);
            ServiceLocator.Instance.Resolve<GameplaySnapshotRequestSignal>().Dispatch();
        }

        /// <summary>
        /// Gỡ listener đã đăng ký trong OnEnable. Signal được resolve, không giữ tham chiếu.
        /// </summary>
        private void OnDisable()
        {
            ServiceLocator.Instance.Resolve<SceneStopSignal>().RemoveListener(OnSceneStopping);
            ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().RemoveListener(OnSnapshot);
            if (holdButton != null) { holdButton.onClick.RemoveListener(OnHoldClicked); }
            if (restartButton != null) { restartButton.onClick.RemoveListener(OnRestartClicked); }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBackClicked);
            }
        }

        /// <summary>
        /// Khi scene sắp rời, khóa nút Back để không gửi thêm yêu cầu trong lúc chuyển scene.
        /// </summary>
        /// <param name="stoppingSceneKey">Address key của scene đang rời.</param>
        private void OnSceneStopping(string stoppingSceneKey)
        {
            if (gameObject.scene.name != stoppingSceneKey)
            {
                return;
            }

            isStopping = true;
            backButton.interactable = false;
            if (holdButton != null) { holdButton.interactable = false; }
            if (restartButton != null) { restartButton.interactable = false; }
        }

        /// <summary>Yêu cầu Hold qua Signal typed, không gọi trực tiếp presenter.</summary>
        private void OnHoldClicked()
        {
            ServiceLocator.Instance.Resolve<GameplayCommandSignal>().Dispatch(GameplayCommand.Hold);
        }

        /// <summary>Yêu cầu ván mới; presenter chỉ chấp nhận khi GameOver.</summary>
        private void OnRestartClicked()
        {
            ServiceLocator.Instance.Resolve<GameplayCommandSignal>().Dispatch(GameplayCommand.Restart);
        }

        /// <summary>Đồng bộ quyền tương tác của nút theo model.</summary>
        private void OnSnapshot(GameplaySnapshot snapshot)
        {
            if (isStopping)
            {
                // Snapshot scene mới không được bật lại nút của Canvas đang unload.
                return;
            }
            if (holdButton != null) { holdButton.interactable = snapshot.CanHold; }
            if (restartButton != null) { restartButton.interactable = snapshot.State == GameSessionState.GameOver; }
        }

        /// <summary>
        /// Gửi yêu cầu quay về MainMenu; tên scene đọc từ cấu hình đã tải.
        /// </summary>
        private void OnBackClicked()
        {
            string mainMenuScene = ServiceLocator.Instance.Resolve<ConfigManager>().Config.Scenes.MainMenu;
            ServiceLocator.Instance.Resolve<SceneRequestSignal>().Dispatch(mainMenuScene);
        }
    }
}
