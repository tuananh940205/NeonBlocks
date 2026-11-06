using System;
using System.Threading;
using System.Threading.Tasks;
using NeonBlocks.Assets;
using NeonBlocks.Config;
using NeonBlocks.Gameplay;
using NeonBlocks.Popups;
using NeonBlocks.Save;
using NeonBlocks.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace NeonBlocks.Views
{
    /// <summary>Host scene điều phối modal qua Signal; manager sở hữu instance và handle.</summary>
    [DisallowMultipleComponent]
    public sealed class PopupHost : MonoBehaviour
    {
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private Button retryButton;
        private CancellationTokenSource lifetime;
        private PopupKind desired, shown, returnKind;
        private GameSessionState previousState = GameSessionState.Ready;
        private bool syncing, stopping, saving, transition;
        private int revision;
        /// <summary>Đăng ký trước khi GameplayPresenter phát snapshot đầu.</summary>
        private void OnEnable()
        {
            if (!Application.isPlaying) { return; }
            lifetime = new CancellationTokenSource(); stopping = false;
            ServiceLocator.Instance.Resolve<UiRequestSignal>().AddListener(OnRequest);
            ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().AddListener(OnSnapshot);
            ServiceLocator.Instance.Resolve<SceneStopSignal>().AddListener(OnStop);
            if (retryButton != null) { retryButton.onClick.AddListener(Retry); }
        }
        /// <summary>Gỡ listener, hủy task và đóng instance trước scope release.</summary>
        private void OnDisable()
        {
            if (!Application.isPlaying) { return; }
            stopping = true; lifetime?.Cancel();
            ServiceLocator.Instance.Resolve<UiRequestSignal>().RemoveListener(OnRequest);
            ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().RemoveListener(OnSnapshot);
            ServiceLocator.Instance.Resolve<SceneStopSignal>().RemoveListener(OnStop);
            if (retryButton != null) { retryButton.onClick.RemoveListener(Retry); }
            if (ServiceLocator.Instance.IsRegistered(typeof(PopupManager))) { ServiceLocator.Instance.Resolve<PopupManager>().Close(gameObject.scene.name); }
            lifetime?.Dispose(); lifetime = null;
        }
        /// <summary>Chờ đúng CurrentScope sau khi scene được active và scope trước đã dọn.</summary>
        private void Update()
        {
            if (stopping || syncing || transition || !ServiceLocator.Instance.IsRegistered(typeof(SceneFlowManager))) { return; }
            SceneAssetScope scope = ServiceLocator.Instance.Resolve<SceneFlowManager>().CurrentScope;
            if (scope == null || scope.SceneKey != gameObject.scene.name) { return; }
            if (desired == PopupKind.None && shown == PopupKind.None && gameObject.scene.name == ServiceLocator.Instance.Resolve<ConfigManager>().Config.Scenes.MainMenu)
            { SetDesired(PopupKind.MainMenu); }
            if (desired != shown) { _ = SyncAsync(scope); }
        }
        /// <summary>Khóa input ngay khi muốn modal, kể cả trong thời gian load prefab.</summary>
        private void SetDesired(PopupKind kind)
        {
            desired = kind; revision++;
            if (gameObject.scene.name == ServiceLocator.Instance.Resolve<ConfigManager>().Config.Scenes.Gameplay)
            { ServiceLocator.Instance.Resolve<GameplayModalSignal>().Dispatch(kind != PopupKind.None); }
        }
        /// <summary>Task chuẩn hóa mọi lỗi popup để view có thông báo và nút Retry.</summary>
        private async Task SyncAsync(SceneAssetScope scope)
        {
            syncing = true;
            int expected = revision; PopupKind target = desired;
            try
            {
                ServiceLocator.Instance.Resolve<PopupManager>().Close(scope.SceneKey);
                shown = PopupKind.None;
                if (target != PopupKind.None)
                {
                    PopupKeys keys = ServiceLocator.Instance.Resolve<ConfigManager>().Config.Ui;
                    string key = target switch { PopupKind.MainMenu => keys.MainMenu, PopupKind.Pause => keys.Pause, PopupKind.Settings => keys.Settings, _ => keys.GameOver };
                    await ServiceLocator.Instance.Resolve<PopupManager>().OpenAsync(scope, key, transform, lifetime.Token);
                }
                if (stopping || expected != revision)
                { ServiceLocator.Instance.Resolve<PopupManager>().Close(scope.SceneKey); return; }
                shown = target; ShowError("");
            }
            catch (OperationCanceledException) { /* Scene dừng hoặc modal đã được thay thế, không phải lỗi người chơi. */ }
            catch (Exception ex)
            {
                // Task boundary: báo lỗi tường minh, không tự lặp tải mỗi frame.
                shown = desired; ShowError("Không mở được panel: " + ex.Message);
            }
            finally { syncing = false; }
        }
        /// <summary>Hiển thị lỗi và quyền retry trên UI thuộc host.</summary>
        private void ShowError(string message)
        {
            if (errorText != null) { errorText.text = message; }
            if (retryButton != null) { retryButton.gameObject.SetActive(!string.IsNullOrEmpty(message)); }
        }
        /// <summary>Cho phép thử tải lại sau lỗi; không tự nhân request.</summary>
        private void Retry() { shown = PopupKind.None; }
        /// <summary>Chỉ phản ứng với snapshot từ gameplay scene này; xử lý GameOver đúng một lần.</summary>
        private void OnSnapshot(GameplaySnapshot snapshot)
        {
            if (stopping || gameObject.scene.name != ServiceLocator.Instance.Resolve<ConfigManager>().Config.Scenes.Gameplay) { return; }
            GameSessionState state = snapshot.State;
            if (state == previousState) { return; }
            previousState = state;
            if (state == GameSessionState.GameOver)
            { SetDesired(PopupKind.GameOver); _ = SaveScoreAsync(snapshot.Score); }
            else if (state == GameSessionState.Paused) { SetDesired(PopupKind.Pause); }
            else if (state == GameSessionState.Playing) { SetDesired(PopupKind.None); }
        }
        /// <summary>Ghi điểm trước/sau popup đều thông báo lại view qua Signal.</summary>
        private async Task SaveScoreAsync(int score)
        {
            await ServiceLocator.Instance.Resolve<LocalSaveManager>().RecordScoreAsync(score);
            if (!stopping) { ServiceLocator.Instance.Resolve<SaveChangedSignal>().Dispatch(); }
        }
        /// <summary>Chỉ chấp nhận hành động hợp lệ của panel hiện hành, chặn double click trong lúc đổi scene/lưu.</summary>
        private void OnRequest(UiRequest request)
        {
            if (stopping || saving || transition || request.SceneKey != gameObject.scene.name) { return; }
            switch (request.Action)
            {
                case UiAction.Start:
                    if (desired == PopupKind.MainMenu) { _ = GoHomeOrGameAsync(true); }
                    break;
                case UiAction.Settings:
                    if (desired == PopupKind.MainMenu || desired == PopupKind.Pause) { returnKind = desired; SetDesired(PopupKind.Settings); }
                    break;
                case UiAction.Back:
                    if (desired == PopupKind.Settings) { SetDesired(returnKind); }
                    break;
                case UiAction.ApplySettings:
                    if (desired == PopupKind.Settings) { _ = ApplyAsync(request); }
                    break;
                case UiAction.Resume:
                    if (desired == PopupKind.Pause) { SetDesired(PopupKind.None); Command(GameplayCommand.ModalResume); }
                    break;
                case UiAction.Restart:
                    if (desired == PopupKind.Pause || desired == PopupKind.GameOver) { SetDesired(PopupKind.None); Command(GameplayCommand.ModalRestart); }
                    break;
                case UiAction.Home:
                    if (desired == PopupKind.Pause || desired == PopupKind.GameOver) { _ = GoHomeOrGameAsync(false); }
                    break;
            }
        }
        /// <summary>Lưu settings; lỗi giữ modal để hiển thị thông báo, Back vẫn cho thoát.</summary>
        private async Task ApplyAsync(UiRequest request)
        {
            saving = true;
            try
            {
                SaveResult result = await ServiceLocator.Instance.Resolve<LocalSaveManager>().SetSettingsAsync(request.Music, request.Sfx, request.Muted);
                if (!stopping)
                {
                    ServiceLocator.Instance.Resolve<SaveChangedSignal>().Dispatch();
                    if (result.Success) { SetDesired(returnKind); }
                }
            }
            finally { saving = false; }
        }
        /// <summary>Chuyển scene trực tiếp qua task manager để lỗi được hiển thị thay vì bị bỏ qua.</summary>
        private async Task GoHomeOrGameAsync(bool game)
        {
            transition = true;
            try
            {
                SceneKeys scenes = ServiceLocator.Instance.Resolve<ConfigManager>().Config.Scenes;
                await ServiceLocator.Instance.Resolve<SceneFlowManager>().GoToAsync(game ? scenes.Gameplay : scenes.MainMenu, lifetime.Token);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { if (!stopping) { ShowError("Chuyển scene thất bại: " + ex.Message); } }
            finally { transition = false; }
        }
        /// <summary>Gửi command cho presenter, không serialized cross-view.</summary>
        private static void Command(GameplayCommand command) => ServiceLocator.Instance.Resolve<GameplayCommandSignal>().Dispatch(command);
        /// <summary>Dọn popup ngay khi scene đúng bắt đầu dừng, trước dispose và scope release.</summary>
        private void OnStop(string sceneKey)
        {
            if (sceneKey != gameObject.scene.name) { return; }
            stopping = true; lifetime.Cancel();
            ServiceLocator.Instance.Resolve<PopupManager>().Close(sceneKey);
        }
    }
}