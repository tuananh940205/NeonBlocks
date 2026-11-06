using System;
using System.IO;
using System.Linq;
using NeonBlocks.Config;
using NeonBlocks.Popups;
using NeonBlocks.Views;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace NeonBlocks.Editor
{
    /// <summary>Dựng prefab và scene host bằng Unity API; chạy theo yêu cầu, không tự sửa scene.</summary>
    public static class Lesson12Setup
    {
        private const string Folder = "Assets/_Project/Prefabs/UI";
        /// <summary>Tạo bốn prefab Addressable rồi nối host trong MainMenu/Gameplay; giữ scene chưa lưu.</summary>
        [MenuItem("NeonBlocks/Lesson 12/Create prefabs and configure scenes")]
        public static void Configure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCount).Any(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty))
            { throw new InvalidOperationException("Thoát Play và lưu scene trước khi setup."); }
            Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            foreach (string path in new[] { "Assets/_Project/Art/UI/Popups/NB-UI-009-PopupPanel.png", "Assets/_Project/Art/UI/Icons/NB-UI-010-Settings.png", "Assets/_Project/Art/UI/Icons/NB-UI-011-HighScore.png" })
            { AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate); }
            var config = GameConfigParser.Parse(File.ReadAllText("Assets/_Project/Configs/GameConfig.json"));
            Build(config.Ui.MainMenu, PopupKind.MainMenu);
            Build(config.Ui.Pause, PopupKind.Pause);
            Build(config.Ui.Settings, PopupKind.Settings);
            Build(config.Ui.GameOver, PopupKind.GameOver);
            ConfigureScene("Assets/_Project/Scenes/MainMenu.unity", true);
            ConfigureScene("Assets/_Project/Scenes/Gameplay.unity", false);
            AssetDatabase.SaveAssets();
            Debug.Log("Bài 12: đã dựng prefab, Addressables và scene host. Chạy Play từ Entry để kiểm tra.");
        }
        /// <summary>Tạo prefab nếu chưa có để không ghi đè prefab người dùng đã chỉnh.</summary>
        private static void Build(string key, PopupKind kind)
        {
            string path = Folder + "/" + key + ".prefab";
            if (!File.Exists(path))
            {
                var root = Rect(null, key, Vector2.zero, Vector2.zero);
                Stretch(root);
                Canvas canvas = root.gameObject.AddComponent<Canvas>(); canvas.overrideSorting = true; canvas.sortingOrder = 200;
                root.gameObject.AddComponent<GraphicRaycaster>();
                Image dim = root.gameObject.AddComponent<Image>(); dim.color = new Color(0, 0, 0.03f, kind == PopupKind.MainMenu ? 0.15f : 0.85f);
                var view = root.gameObject.AddComponent<PopupCanvas>();
                RectTransform panel = Rect(root, "Panel", Vector2.zero, new Vector2(800, 1000));
                Image plate = panel.gameObject.AddComponent<Image>();
                plate.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/Popups/NB-UI-009-PopupPanel.png"); plate.preserveAspect = true; plate.raycastTarget = false;
                Label(panel, "Title", kind == PopupKind.MainMenu ? "NEONBLOCKS" : kind == PopupKind.GameOver ? "GAME OVER" : kind.ToString().ToUpperInvariant(), new Vector2(0, 380), new Vector2(700, 90), 52);
                TMP_Text details = Label(panel, "Details", "BEST 0", new Vector2(0, 280), new Vector2(650, 100), 36);
                TMP_Text status = Label(panel, "SaveStatus", "", new Vector2(0, -395), new Vector2(680, 120), 24);
                var fields = new SerializedObject(view);
                fields.FindProperty("kind").enumValueIndex = (int)kind;
                fields.FindProperty("detailsText").objectReferenceValue = details; fields.FindProperty("statusText").objectReferenceValue = status;
                if (kind == PopupKind.MainMenu)
                {
                    AssignButton(fields, panel, "startButton", "START", 100);
                    AssignButton(fields, panel, "settingsButton", "SETTINGS", -50);
                    Icon(panel, "SettingsIcon", "Assets/_Project/Art/UI/Icons/NB-UI-010-Settings.png", new Vector2(-280, -50), 60);
                }
                if (kind == PopupKind.Pause)
                {
                    AssignButton(fields, panel, "resumeButton", "RESUME", 120);
                    AssignButton(fields, panel, "restartButton", "RESTART", -10);
                    AssignButton(fields, panel, "settingsButton", "SETTINGS", -140);
                    AssignButton(fields, panel, "homeButton", "HOME", -270);
                }
                if (kind == PopupKind.GameOver)
                {
                    AssignButton(fields, panel, "restartButton", "RESTART", 50);
                    AssignButton(fields, panel, "homeButton", "HOME", -100);
                }
                if (kind == PopupKind.MainMenu || kind == PopupKind.GameOver)
                { Icon(panel, "HighScoreIcon", "Assets/_Project/Art/UI/Icons/NB-UI-011-HighScore.png", new Vector2(-280, 280), 90); }
                if (kind == PopupKind.Settings)
                {
                    fields.FindProperty("musicSlider").objectReferenceValue = Slider(panel, "Music", 100);
                    fields.FindProperty("sfxSlider").objectReferenceValue = Slider(panel, "Sfx", -30);
                    RectTransform toggleRect = Rect(panel, "MuteToggle", new Vector2(-200, -140), new Vector2(64, 64));
                    Image background = toggleRect.gameObject.AddComponent<Image>(); background.color = new Color(0.08f, 0.2f, 0.3f);
                    RectTransform checkRect = Rect(toggleRect, "Checkmark", Vector2.zero, Vector2.one * 42);
                    Image check = checkRect.gameObject.AddComponent<Image>(); check.color = Color.cyan;
                    Toggle toggle = toggleRect.gameObject.AddComponent<Toggle>(); toggle.targetGraphic = background; toggle.graphic = check;
                    Label(panel, "MuteLabel", "MUTE", new Vector2(20, -140), new Vector2(300, 60), 30);
                    fields.FindProperty("muteToggle").objectReferenceValue = toggle;
                    AssignButton(fields, panel, "applyButton", "APPLY", -250, -175, 300);
                    AssignButton(fields, panel, "backButton", "BACK", -250, 175, 300);
                }
                fields.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root.gameObject, path);
                UnityEngine.Object.DestroyImmediate(root.gameObject);
            }
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) { throw new InvalidOperationException("Tạo Addressables Settings trước."); }
            var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(path), settings.DefaultGroup);
            entry.address = key;
        }
        /// <summary>Gán nút con vào field controller; prefab không chứa persistent onClick.</summary>
        private static void AssignButton(SerializedObject fields, Transform panel, string field, string title, float y, float x = 0, float width = 580)
        {
            RectTransform rect = Rect(panel, title, new Vector2(x, y), new Vector2(width, 100));
            Image image = rect.gameObject.AddComponent<Image>(); image.color = new Color(0.04f, 0.2f, 0.32f);
            Button button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.navigation = new Navigation { mode = Navigation.Mode.None };
            Label(rect, "Label", title, Vector2.zero, new Vector2(width - 12, 85), 34);
            fields.FindProperty(field).objectReferenceValue = button;
        }
        /// <summary>Tạo slider có handle thực để thao tác pointer, range 0..1.</summary>
        private static Slider Slider(Transform parent, string name, float y)
        {
            Label(parent, name + "Label", name.ToUpperInvariant(), new Vector2(0, y + 48), new Vector2(580, 45), 28);
            RectTransform rect = Rect(parent, name + "Slider", new Vector2(0, y), new Vector2(580, 45));
            Image track = rect.gameObject.AddComponent<Image>(); track.color = new Color(0.05f, 0.25f, 0.35f);
            RectTransform handle = Rect(rect, "Handle", Vector2.zero, new Vector2(35, 60));
            Image image = handle.gameObject.AddComponent<Image>(); image.color = Color.cyan;
            Slider slider = rect.gameObject.AddComponent<Slider>(); slider.handleRect = handle; slider.targetGraphic = image; slider.minValue = 0; slider.maxValue = 1; slider.value = 1;
            return slider;
        }
        /// <summary>Đặt icon trang trí, không chặn raycast.</summary>
        private static void Icon(Transform parent, string name, string path, Vector2 position, float size)
        {
            RectTransform rect = Rect(parent, name, position, Vector2.one * size);
            Image image = rect.gameObject.AddComponent<Image>(); image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path); image.preserveAspect = true; image.raycastTarget = false;
        }
        /// <summary>Gắn host vào Canvas scene và tắt UI legacy đã được popup thay thế.</summary>
        private static void ConfigureScene(string path, bool menu)
        {
            var scene = EditorSceneManager.OpenScene(path);
            Canvas canvas = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Canvas>(true)).First();
            Transform existing = canvas.transform.Find("PopupHost");
            if (existing == null)
            {
                RectTransform rect = Rect(canvas.transform, "PopupHost", Vector2.zero, Vector2.zero); Stretch(rect);
                rect.gameObject.AddComponent<Canvas>(); rect.gameObject.AddComponent<GraphicRaycaster>();
                var host = rect.gameObject.AddComponent<PopupHost>();
                TMP_Text error = Label(rect, "LoadError", "", new Vector2(0, 600), new Vector2(900, 150), 30);
                RectTransform retryRect = Rect(rect, "Retry", new Vector2(0, 450), new Vector2(400, 100));
                Image image = retryRect.gameObject.AddComponent<Image>(); image.color = new Color(0.1f, 0.25f, 0.4f);
                Button retry = retryRect.gameObject.AddComponent<Button>(); retry.targetGraphic = image;
                Label(retryRect, "Label", "RETRY", Vector2.zero, new Vector2(380, 80), 34);
                var fields = new SerializedObject(host); fields.FindProperty("errorText").objectReferenceValue = error; fields.FindProperty("retryButton").objectReferenceValue = retry; fields.ApplyModifiedPropertiesWithoutUndo();
                retry.gameObject.SetActive(false);
            }
            if (menu)
            {
                foreach (MainMenuCanvas legacy in canvas.GetComponentsInChildren<MainMenuCanvas>(true))
                {
                    var fields = new SerializedObject(legacy);
                    if (fields.FindProperty("startButton").objectReferenceValue is Button button) { button.gameObject.SetActive(false); }
                    legacy.enabled = false;
                }
            }
            else
            {
                foreach (GameplayCanvas legacy in canvas.GetComponentsInChildren<GameplayCanvas>(true))
                {
                    var fields = new SerializedObject(legacy);
                    foreach (string name in new[] { "backButton", "restartButton" })
                    { if (fields.FindProperty(name).objectReferenceValue is Button button) { button.gameObject.SetActive(false); } }
                }
            }
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }
        /// <summary>Tạo RectTransform nằm giữa parent theo layout portrait.</summary>
        private static RectTransform Rect(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.layer = 5; go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * 0.5f; rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        /// <summary>Cho rect phủ toàn parent.</summary>
        private static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        /// <summary>Tạo TMP label thuộc panel.</summary>
        private static TMP_Text Label(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize)
        {
            RectTransform rect = Rect(parent, name, position, size); var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = value; text.fontSize = fontSize; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false; text.color = Color.white; return text;
        }
    }
}