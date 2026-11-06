using System;
using System.IO;
using System.Linq;
using NeonBlocks.Config;
using NeonBlocks.Gameplay;
using NeonBlocks.Scene;
using NeonBlocks.Views;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NeonBlocks.Editor
{
    /// <summary>Playtest qua Entry, scene flow và Signal thật; lưu bằng chứng Hold/HUD/restart.</summary>
    [InitializeOnLoad]
    public static class Lesson11Playtest
    {
        private static GameplaySnapshot latest;
        private static int phase;
        private static int drops;
        private static string initialPiece;
        private static string[] initialNext;
        private static double deadline;
        private static bool listening;
        private static readonly System.Text.StringBuilder evidence = new System.Text.StringBuilder();

        /// <summary>Tiếp tục quan sát sau domain reload khi menu playtest đã được gọi.</summary>
        static Lesson11Playtest()
        {
            EditorApplication.update += Update;
        }

        /// <summary>Khởi động Entry chuẩn; không chạy Gameplay trực tiếp khi thiếu session.</summary>
        [MenuItem("NeonBlocks/Lesson 11/Run PlayMode smoke")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // Không chiếm ván đang được người dùng chơi.
                throw new InvalidOperationException("Thoát Play Mode trước khi chạy smoke.");
            }
            if (Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCount).Any(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty))
            {
                // Không ghi đè scene chưa lưu.
                throw new InvalidOperationException("Lưu scene trước khi chạy smoke.");
            }
            SessionState.SetBool("NeonBlocks.Lesson11Smoke", true);
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Entry.unity");
            EditorApplication.isPlaying = true;
        }

        /// <summary>Giữ snapshot cuối để phát lệnh ở update riêng, tránh dispatch đệ quy trong callback.</summary>
        private static void OnSnapshot(GameplaySnapshot snapshot) { latest = snapshot; }

        /// <summary>Chạy các bước qua public Signal và kiểm tra HUD render thật trong scene.</summary>
        private static void Update()
        {
            if (!SessionState.GetBool("NeonBlocks.Lesson11Smoke", false) || !EditorApplication.isPlaying) { return; }
            if (!listening)
            {
                // Chỉ đăng ký sau khi editor thật sự vào Play.
                ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().AddListener(OnSnapshot);
                listening = true; phase = 0; deadline = EditorApplication.timeSinceStartup + 90; evidence.Clear();
            }
            if (EditorApplication.timeSinceStartup > deadline) { Finish(false, "Timeout phase " + phase); return; }
            if (phase == 0)
            {
                if (!ServiceLocator.Instance.IsRegistered(typeof(ConfigManager))) { return; }
                GameConfig config = ServiceLocator.Instance.Resolve<ConfigManager>().Config;
                if (config == null || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != config.Scenes.MainMenu) { return; }
                phase = 1; ServiceLocator.Instance.Resolve<SceneRequestSignal>().Dispatch(config.Scenes.Gameplay); return;
            }
            if (latest == null) { return; }
            if (phase == 1)
            {
                if (!Check(latest.HeldPieceId == null && latest.CanHold && latest.Score == 0, "Initial state")) { return; }
                if (!Check(UnityEngine.Object.FindObjectsByType<GameplayHudView>(FindObjectsSortMode.None).Length == 1, "One HUD view")) { return; }
                initialPiece = latest.ActivePieceId; initialNext = latest.NextPieceIds.ToArray();
                phase = 2; Command(GameplayCommand.Hold); return;
            }
            if (phase == 2)
            {
                if (!Check(latest.HeldPieceId == initialPiece && !latest.CanHold && latest.ActivePieceId == initialNext[0], "Hold stores active, consumes next, disables Hold")) { return; }
                if (!Check(HoldCellCount() == 4, "Hold preview renders four cells")) { return; }
                phase = 3; Command(GameplayCommand.Hold); return;
            }
            if (phase == 3)
            {
                if (!Check(latest.HeldPieceId == initialPiece && latest.ActivePieceId == initialNext[0], "Second Hold rejected")) { return; }
                phase = 4; Command(GameplayCommand.HardDrop); return;
            }
            if (phase == 4)
            {
                if (!Check(latest.CanHold, "Lock/spawn reopens Hold")) { return; }
                phase = 5; Command(GameplayCommand.Hold); return;
            }
            if (phase == 5)
            {
                if (!Check(latest.ActivePieceId == initialPiece && !latest.CanHold, "Swap returns stored piece")) { return; }
                phase = 6; Command(GameplayCommand.TogglePause); return;
            }
            if (phase == 6)
            {
                if (!Check(latest.State == GameSessionState.Paused && !latest.CanHold, "Pause blocks Hold")) { return; }
                phase = 7; Command(GameplayCommand.TogglePause); return;
            }
            if (phase == 7)
            {
                if (latest.State == GameSessionState.Playing && drops++ < 100) { Command(GameplayCommand.HardDrop); return; }
                if (!Check(latest.State == GameSessionState.GameOver, "Stack reaches GameOver")) { return; }
                phase = 8; Command(GameplayCommand.Restart); return;
            }
            if (phase == 8)
            {
                GameConfig config = ServiceLocator.Instance.Resolve<ConfigManager>().Config;
                if (!Check(latest.State == GameSessionState.Playing && latest.HeldPieceId == null && latest.CanHold
                    && latest.Score == 0 && latest.Lines == 0 && latest.Level == config.StartLevel
                    && latest.ActivePieceId == initialPiece && latest.NextPieceIds.SequenceEqual(initialNext), "Restart resets model and queue")) { return; }
                if (!Check(HoldCellCount() == 0, "Restart clears Hold preview")) { return; }
                TMP_Text score = UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).First(t => t.name == "Score");
                if (!Check(score.text == "SCORE\n0", "HUD score resets")) { return; }
                int systems = UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length;
                int listeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
                if (!Check(systems == 1 && listeners == 1, "One EventSystem and AudioListener after transition")) { return; }
                Finish(true, "PlayMode smoke passed.");
            }
        }

        /// <summary>Đếm cell đang bật dưới slot Hold thật của HUD.</summary>
        private static int HoldCellCount()
        {
            GameplayHudView hud = UnityEngine.Object.FindObjectsByType<GameplayHudView>(FindObjectsSortMode.None).Single();
            return hud.transform.Find("HoldPreview").GetComponentsInChildren<UnityEngine.UI.Image>().Count(i => i.name == "PreviewCell");
        }

        /// <summary>Gửi command qua cùng Signal với keyboard/touch.</summary>
        private static void Command(GameplayCommand command) { ServiceLocator.Instance.Resolve<GameplayCommandSignal>().Dispatch(command); }

        /// <summary>Ghi từng kết quả; dừng ngay nếu khác kỳ vọng.</summary>
        private static bool Check(bool valid, string label)
        {
            evidence.AppendLine((valid ? "PASS " : "FAIL ") + label);
            if (!valid) { Finish(false, label); }
            return valid;
        }

        /// <summary>Lưu bằng chứng và thoát Play, không thay đổi state scene đã lưu.</summary>
        private static void Finish(bool passed, string reason)
        {
            SessionState.SetBool("NeonBlocks.Lesson11Smoke", false);
            ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().RemoveListener(OnSnapshot);
            listening = false; latest = null; drops = 0;
            Directory.CreateDirectory("docs/evidence");
            File.WriteAllText("docs/evidence/lesson11-playmode.txt", DateTime.UtcNow.ToString("O") + "\n" + evidence + reason);
            Debug.Log("Bài 11 smoke: " + reason);
            EditorApplication.isPlaying = false;
        }
    }
}
