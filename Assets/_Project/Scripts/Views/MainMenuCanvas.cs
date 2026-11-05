using NeonBlocks.Config;
using NeonBlocks.Scene;
using UnityEngine;
using UnityEngine.UI;

namespace NeonBlocks.Views
{
    /// <summary>
    /// Script gắn trên Canvas của MainMenu. Đây là nơi duy nhất giữ tham chiếu tới nút Start (serialized field trên cùng Canvas)
    /// và là nơi duy nhất thêm/gỡ listener của nút. Không giữ tham chiếu tới Signal: resolve khi đăng ký, khi gỡ và khi phát.
    /// </summary>
    [DisallowMultipleComponent]
    public class MainMenuCanvas : MonoBehaviour
    {
        /// <summary>Nút Start nằm dưới Canvas này, gán trong Inspector của chính Canvas.</summary>
        [SerializeField] private Button startButton;

        /// <summary>
        /// Đăng ký listener của nút Start và sự kiện dừng scene. Chỉ kiểm tra nút, vì signal không bao giờ null.
        /// </summary>
        private void OnEnable()
        {
            if (startButton == null)
            {
                Debug.LogError("MainMenuCanvas chưa gán Start Button trong Inspector của Canvas.");
                return;
            }

            ServiceLocator.Instance.Resolve<SceneStopSignal>().AddListener(OnSceneStopping);
            startButton.onClick.AddListener(OnStartClicked);
        }

        /// <summary>
        /// Gỡ listener đã đăng ký trong OnEnable. Signal được resolve, không giữ tham chiếu.
        /// </summary>
        private void OnDisable()
        {
            ServiceLocator.Instance.Resolve<SceneStopSignal>().RemoveListener(OnSceneStopping);

            if (startButton != null)
            {
                startButton.onClick.RemoveListener(OnStartClicked);
            }
        }

        /// <summary>
        /// Khi scene sắp rời, khóa nút Start để không gửi thêm yêu cầu trong lúc chuyển scene.
        /// </summary>
        private void OnSceneStopping()
        {
            startButton.interactable = false;
        }

        /// <summary>
        /// Gửi yêu cầu chuyển sang Gameplay; tên scene đọc từ cấu hình đã tải.
        /// </summary>
        private void OnStartClicked()
        {
            string gameplayScene = ServiceLocator.Instance.Resolve<ConfigManager>().Config.Scenes.Gameplay;
            ServiceLocator.Instance.Resolve<SceneRequestSignal>().Dispatch(gameplayScene);
        }
    }
}
