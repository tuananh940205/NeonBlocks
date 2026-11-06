using System;
using System.IO;
using System.Linq;
using NeonBlocks.Audio;
using NeonBlocks.Config;
using NeonBlocks.Gameplay;
using NeonBlocks.Popups;
using NeonBlocks.Save;
using NeonBlocks.Views;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace NeonBlocks.Editor
{
    /// <summary>Smoke thực qua Entry/Signal, kiểm tra bank, sprite, mute và Settings Back mà không sửa save.</summary>
    [InitializeOnLoad]
    public static class Lesson13Playtest
    {
        private static int phase;
        private static double next, deadline;
        private static GameplaySnapshot latest;
        private static bool listening;
        private static readonly System.Text.StringBuilder evidence = new System.Text.StringBuilder();
        /// <summary>Tiếp tục smoke sau domain reload vào Play.</summary>
        static Lesson13Playtest() { EditorApplication.update += Update; }
        /// <summary>Chạy Entry chuẩn khi không có scene chưa lưu.</summary>
        [MenuItem("NeonBlocks/Lesson 13/Run PlayMode smoke")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCount).Any(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty))
            { throw new InvalidOperationException("Thoát Play và lưu scene trước khi smoke."); }
            SessionState.SetBool("NeonBlocks.Lesson13Smoke", true);
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Entry.unity");
            EditorApplication.isPlaying = true;
        }
        /// <summary>Giữ snapshot để kiểm tra ở update ngoài callback.</summary>
        private static void Snapshot(GameplaySnapshot value) { latest = value; }
        /// <summary>Đi qua menu, gameplay, settings và Home; chờ frame cho scene/popup async.</summary>
        private static void Update()
        {
            if (!SessionState.GetBool("NeonBlocks.Lesson13Smoke", false) || !EditorApplication.isPlaying) { return; }
            if (!listening)
            {
                ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().AddListener(Snapshot);
                listening = true; phase = 0; next = 0; deadline = EditorApplication.timeSinceStartup + 90; evidence.Clear();
            }
            if (EditorApplication.timeSinceStartup > deadline) { Finish(false, "Timeout phase " + phase); return; }
            if (EditorApplication.timeSinceStartup < next || !ServiceLocator.Instance.IsRegistered(typeof(NeonBlocks.Audio.AudioManager))) { return; }
            var config = ServiceLocator.Instance.Resolve<ConfigManager>().Config;
            if (config == null) { return; }
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (phase == 0)
            {
                if (scene != config.Scenes.MainMenu || UnityEngine.Object.FindObjectsByType<PopupCanvas>(FindObjectsSortMode.None).Length == 0) { return; }
                if (!Check(Bank() != null && Bank().transform.parent == ServiceLocator.Instance.Resolve<SessionManager>().transform, "One AudioBank parented under SessionManager")) { return; }
                AudioSource[] sources = Bank().GetComponentsInChildren<AudioSource>();
                if (!Check(sources.Length == 10 && sources.All(s => s.clip != null || s.name.StartsWith("SfxVoice")), "Two music tracks and eight bounded SFX voices")) { return; }
                Request(UiAction.Start); Advance(); return;
            }
            if (phase == 1)
            {
                if (scene != config.Scenes.Gameplay || latest == null) { return; }
                if (!Check(UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length == 1 && UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length == 1, "One AudioListener and EventSystem")) { return; }
                Command(GameplayCommand.HardDrop); Command(GameplayCommand.Hold); Advance(); return;
            }
            if (phase == 2)
            {
                if (!Check(latest.SettledTiles.Count == 4 && latest.SettledTiles.All(t => !string.IsNullOrEmpty(t.PieceId)), "Locked cells retain piece IDs")) { return; }
                Image[] cells = UnityEngine.Object.FindObjectsByType<BoardView>(FindObjectsSortMode.None).Single().GetComponentsInChildren<Image>().Where(i => i.name == "NeonCell").ToArray();
                if (!Check(cells.Length >= 4 && cells.All(i => i.sprite != null), "Board cells use approved sprites")) { return; }
                var hud = UnityEngine.Object.FindObjectsByType<GameplayHudView>(FindObjectsSortMode.None).Single();
                if (!Check(hud.GetComponentsInChildren<Image>().Where(i => i.name == "PreviewCell").All(i => i.sprite != null), "Hold and Next use theme sprites")) { return; }
                Command(GameplayCommand.TogglePause); Advance(); return;
            }
            if (phase == 3)
            {
                if (!Check(latest.State == GameSessionState.Paused, "Pause preserves gameplay state")) { return; }
                Request(UiAction.Settings); Advance(); return;
            }
            if (phase == 4)
            {
                PopupCanvas popup = UnityEngine.Object.FindObjectsByType<PopupCanvas>(FindObjectsSortMode.None).FirstOrDefault(p => p.GetComponentsInChildren<Slider>().Length == 2);
                if (popup == null) { return; }
                foreach (Slider slider in popup.GetComponentsInChildren<Slider>()) { slider.value = 0; }
                Advance(); return;
            }
            if (phase == 5)
            {
                if (!Check(Bank().GetComponentsInChildren<AudioSource>().All(s => Mathf.Approximately(s.volume, 0)), "Settings slider preview mutes sources immediately")) { return; }
                Request(UiAction.Back); Advance(); return;
            }
            if (phase == 6)
            {
                SaveData data = ServiceLocator.Instance.Resolve<LocalSaveManager>().Data;
                AudioSource game = Bank().transform.Find("GameplayMusic").GetComponent<AudioSource>();
                float expected = data.Muted ? 0 : data.MusicVolume * .35f;
                if (!Check(Mathf.Abs(game.volume - expected) < .01f && latest.State == GameSessionState.Paused, "Back restores saved gain and remains paused")) { return; }
                Request(UiAction.Home); Advance(); return;
            }
            if (phase == 7 && scene == config.Scenes.MainMenu)
            {
                if (!Check(UnityEngine.Object.FindObjectsByType<AudioHost>(FindObjectsSortMode.None).Length == 1, "Home keeps exactly one session AudioBank")) { return; }
                AudioSource menu = Bank().transform.Find("MenuMusic").GetComponent<AudioSource>();
                if (!Check(menu.isPlaying, "Menu track resumes after crossfade")) { return; }
                Finish(true, "PlayMode smoke passed; perceptual listening and Android remain manual.");
            }
        }
        /// <summary>Lấy adapter thật trong runtime; không giữ singleton field.</summary>
        private static AudioHost Bank() => UnityEngine.Object.FindObjectsByType<AudioHost>(FindObjectsSortMode.None).SingleOrDefault();
        /// <summary>Cho fade/popup hoàn tất trước kiểm tra tiếp.</summary>
        private static void Advance() { phase++; next = EditorApplication.timeSinceStartup + 1; }
        /// <summary>Gửi cùng command signal với input thật.</summary>
        private static void Command(GameplayCommand value) => ServiceLocator.Instance.Resolve<GameplayCommandSignal>().Dispatch(value);
        /// <summary>Gửi yêu cầu theo scene hiện tại.</summary>
        private static void Request(UiAction action) => ServiceLocator.Instance.Resolve<UiRequestSignal>().Dispatch(new UiRequest(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, action));
        /// <summary>Ghi và dừng ở lỗi đầu tiên.</summary>
        private static bool Check(bool valid, string label)
        { evidence.AppendLine((valid ? "PASS " : "FAIL ") + label); if (!valid) { Finish(false, label); } return valid; }
        /// <summary>Lưu kết quả thực, gỡ listener rồi thoát Play.</summary>
        private static void Finish(bool passed, string message)
        {
            SessionState.SetBool("NeonBlocks.Lesson13Smoke", false);
            ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().RemoveListener(Snapshot);
            listening = false; latest = null;
            Directory.CreateDirectory("docs/evidence");
            File.WriteAllText("docs/evidence/lesson13-playmode.txt", DateTime.UtcNow.ToString("O") + "\n" + evidence + (passed ? "PASS " : "FAIL ") + message);
            Debug.Log("Bài 13 smoke: " + message); EditorApplication.isPlaying = false;
        }
    }
}