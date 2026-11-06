using NeonBlocks.Gameplay;
using NeonBlocks.Save;
using NeonBlocks.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace NeonBlocks.Views
{
    /// <summary>Canvas prefab popup chỉ quản lý UI con và phát Signal, không gọi view khác.</summary>
    [DisallowMultipleComponent]
    public sealed class PopupCanvas : MonoBehaviour
    {
        [SerializeField] private PopupKind kind;
        [SerializeField] private Button startButton, settingsButton, backButton, resumeButton, restartButton, homeButton, applyButton;
        [SerializeField] private Slider musicSlider, sfxSlider;
        [SerializeField] private Toggle muteToggle;
        [SerializeField] private TMP_Text detailsText, statusText;
        private int score;
        /// <summary>Gắn listener và khởi tạo control từ save session.</summary>
        private void OnEnable()
        {
            if (!Application.isPlaying) { return; }
            Bind(true);
            ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().AddListener(OnSnapshot);
            ServiceLocator.Instance.Resolve<SaveChangedSignal>().AddListener(Render);
            SaveData data = ServiceLocator.Instance.Resolve<LocalSaveManager>().Data;
            if (musicSlider != null) { musicSlider.SetValueWithoutNotify(data.MusicVolume); }
            if (sfxSlider != null) { sfxSlider.SetValueWithoutNotify(data.SfxVolume); }
            if (muteToggle != null) { muteToggle.SetIsOnWithoutNotify(data.Muted); }
            ServiceLocator.Instance.Resolve<GameplaySnapshotRequestSignal>().Dispatch();
            Render();
        }
        /// <summary>Gỡ listener đối xứng trước khi instance release.</summary>
        private void OnDisable()
        {
            if (!Application.isPlaying) { return; }
            Bind(false);
            ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().RemoveListener(OnSnapshot);
            ServiceLocator.Instance.Resolve<SaveChangedSignal>().RemoveListener(Render);
        }
        /// <summary>Nối các nút thuộc chính Canvas này bằng AddListener/RemoveListener.</summary>
        private void Bind(bool add)
        {
            Listen(startButton, StartGame, add); Listen(settingsButton, Settings, add);
            Listen(backButton, Back, add); Listen(resumeButton, Resume, add);
            Listen(restartButton, Restart, add); Listen(homeButton, Home, add); Listen(applyButton, Apply, add);
        }
        /// <summary>Đăng ký một callback thuộc Canvas.</summary>
        private static void Listen(Button button, UnityEngine.Events.UnityAction action, bool add)
        {
            if (button == null) { return; }
            if (add) { button.onClick.AddListener(action); } else { button.onClick.RemoveListener(action); }
        }
        /// <summary>Gửi yêu cầu có scene key, host chỉ xử lý đúng scene.</summary>
        private void Send(UiAction action) => ServiceLocator.Instance.Resolve<UiRequestSignal>().Dispatch(new UiRequest(gameObject.scene.name, action));
        /// <summary>Bắt đầu ván từ menu.</summary>
        private void StartGame() => Send(UiAction.Start);
        /// <summary>Mở Settings, giữ ngữ cảnh Menu/Pause.</summary>
        private void Settings() => Send(UiAction.Settings);
        /// <summary>Quay lại không áp dụng giá trị slider chưa lưu.</summary>
        private void Back() => Send(UiAction.Back);
        /// <summary>Tiếp tục ván đã pause.</summary>
        private void Resume() => Send(UiAction.Resume);
        /// <summary>Restart từ modal hợp lệ.</summary>
        private void Restart() => Send(UiAction.Restart);
        /// <summary>Về Menu theo lifecycle scene.</summary>
        private void Home() => Send(UiAction.Home);
        /// <summary>Gửi payload settings hiện tại để manager validate/lưu.</summary>
        private void Apply() => ServiceLocator.Instance.Resolve<UiRequestSignal>().Dispatch(new UiRequest(gameObject.scene.name,
            UiAction.ApplySettings, musicSlider.value, sfxSlider.value, muteToggle.isOn));
        /// <summary>Nhận điểm hiện tại phục vụ GameOver, không sửa model.</summary>
        private void OnSnapshot(GameplaySnapshot snapshot) { score = snapshot.Score; Render(); }
        /// <summary>Đọc best score và thông báo lưu; không ghi đè slider đang được chỉnh.</summary>
        private void Render()
        {
            LocalSaveManager save = ServiceLocator.Instance.Resolve<LocalSaveManager>();
            if (detailsText != null)
            { detailsText.text = kind == PopupKind.GameOver ? $"SCORE  {score}\nBEST  {save.Data.BestScore}" : $"BEST  {save.Data.BestScore}"; }
            if (statusText != null) { statusText.text = save.LastResult.Message; }
        }
    }
}