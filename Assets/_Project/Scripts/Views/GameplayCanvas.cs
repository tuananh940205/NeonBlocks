using NeonBlocks.Config;
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

        /// <summary>
        /// Đăng ký listener của nút Back và sự kiện dừng scene. Chỉ kiểm tra nút, vì signal không bao giờ null.
        /// </summary>
        private void OnEnable()
        {
            if (backButton == null)
            {
                Debug.LogError("GameplayCanvas chưa gán Back Button trong Inspector của Canvas.");
                return;
            }

            ServiceLocator.Instance.Resolve<SceneStopSignal>().AddListener(OnSceneStopping);
            backButton.onClick.AddListener(OnBackClicked);
        }

        /// <summary>
        /// Gỡ listener đã đăng ký trong OnEnable. Signal được resolve, không giữ tham chiếu.
        /// </summary>
        private void OnDisable()
        {
            ServiceLocator.Instance.Resolve<SceneStopSignal>().RemoveListener(OnSceneStopping);

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

            backButton.interactable = false;
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
