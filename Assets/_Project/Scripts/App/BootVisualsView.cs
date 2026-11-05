using NeonBlocks.Scene;
using UnityEngine;

namespace NeonBlocks.App
{
    /// <summary>
    /// Gắn trên gốc BootVisuals của Entry scene. Đăng ký listener BootVisualsHideSignal trong Start và tự tắt khi nhận lệnh.
    /// Không giữ tham chiếu tới signal: resolve mỗi khi cần đăng ký hoặc gỡ listener.
    /// </summary>
    [DisallowMultipleComponent]
    public class BootVisualsView : MonoBehaviour
    {
        /// <summary>
        /// Đăng ký listener vào BootVisualsHideSignal. Start chạy sau Awake của mọi object, và EntryLauncher
        /// có DefaultExecutionOrder thấp hơn nên singleton session đã có sẵn tại đây.
        /// </summary>
        private void Start()
        {
            ServiceLocator.Instance.Resolve<BootVisualsHideSignal>().AddListener(Hide);
        }

        /// <summary>
        /// Gỡ listener khi object bị hủy (thoát Play Mode hoặc Restart Game).
        /// </summary>
        private void OnDestroy()
        {
            ServiceLocator.Instance.Resolve<BootVisualsHideSignal>().RemoveListener(Hide);
        }

        /// <summary>
        /// Gỡ listener rồi tắt GameObject; camera và canvas khởi động không còn được render.
        /// </summary>
        private void Hide()
        {
            ServiceLocator.Instance.Resolve<BootVisualsHideSignal>().RemoveListener(Hide);
            gameObject.SetActive(false);
        }
    }
}
