using System;
using System.IO;
using System.Linq;
using NeonBlocks.Gameplay;
using NeonBlocks.Views;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace NeonBlocks.Editor
{
    /// <summary>Công cụ Editor bài 11: import asset đã duyệt và cập nhật Gameplay bằng API Unity, không sửa YAML thủ công.</summary>
    public static class Lesson11Setup
    {
        private const string HoldPath = "Assets/_Project/Art/UI/Controls/Touch/NB-UI-007-TouchHold.png";
        private const string PanelPath = "Assets/_Project/Art/UI/Gameplay/NB-UI-008-PreviewPanel.png";
        private const string ScenePath = "Assets/_Project/Scenes/Gameplay.unity";

        /// <summary>Import hai PNG đã được người dùng duyệt và dựng HUD/nút thuộc Canvas hiện có.</summary>
        [MenuItem("NeonBlocks/Lesson 11/Import and configure")]
        public static void Configure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // Không sửa scene khi runtime đang dùng object.
                throw new InvalidOperationException("Thoát Play Mode trước khi setup bài 11.");
            }
            if (Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCount)
                .Any(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty))
            {
                // Giữ thay đổi scene chưa lưu của người dùng.
                throw new InvalidOperationException("Lưu scene đang sửa trước khi setup bài 11.");
            }
            Import("art-source/NeonBlocks/NB-UI-007-TouchHold-v1.png", HoldPath);
            Import("art-source/NeonBlocks/NB-UI-008-PreviewPanel-v1.png", PanelPath);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Canvas>(true)).Single();
            GameplayCanvas controller = canvas.GetComponent<GameplayCanvas>();
            if (controller == null)
            {
                // Scene cũ thiếu controller thì tạo đúng trên Canvas.
                controller = canvas.gameObject.AddComponent<GameplayCanvas>();
            }
            RectTransform host = canvas.GetComponentsInChildren<RectTransform>(true).First(r => r.name == "BoardRoot").parent as RectTransform;
            RectTransform board = canvas.GetComponentsInChildren<RectTransform>(true).First(r => r.name == "BoardRoot");
            Place(board, new Vector2(0, 20), new Vector2(700, 1400));
            Image boardImage = board.GetComponent<Image>();
            if (boardImage != null) { boardImage.raycastTarget = false; }

            RectTransform hud = Child(host, "Lesson11HUD");
            hud.anchorMin = Vector2.zero; hud.anchorMax = Vector2.one; hud.offsetMin = hud.offsetMax = Vector2.zero;
            GameplayHudView view = hud.GetComponent<GameplayHudView>();
            if (view == null) { view = hud.gameObject.AddComponent<GameplayHudView>(); }
            Sprite panel = AssetDatabase.LoadAssetAtPath<Sprite>(PanelPath);
            RectTransform held = Panel(hud, "HoldPreview", new Vector2(-440, 410), panel);
            Label(hud, "HoldLabel", "HOLD", new Vector2(-440, 515), new Vector2(180, 45), 30);
            var next = new RectTransform[5];
            Label(hud, "NextLabel", "NEXT", new Vector2(440, 515), new Vector2(180, 45), 30);
            for (int i = 0; i < next.Length; i++) { next[i] = Panel(hud, "Next" + i, new Vector2(440, 410 - i * 185), panel); }

            TMP_Text score = Label(hud, "Score", "SCORE\n0", new Vector2(-280, 815), new Vector2(300, 110), 34);
            TMP_Text level = Label(hud, "Level", "LEVEL\n1", new Vector2(0, 815), new Vector2(220, 110), 34);
            TMP_Text lines = Label(hud, "Lines", "LINES\n0", new Vector2(280, 815), new Vector2(220, 110), 34);
            TMP_Text state = Label(hud, "State", "", new Vector2(0, 710), new Vector2(500, 65), 34);
            Label(hud, "HoldHint", "C / SHIFT", new Vector2(-440, 85), new Vector2(180, 45), 24);
            RectTransform holdRect = Child(hud, "HoldButton");
            Place(holdRect, new Vector2(-440, 210), Vector2.one * 160);
            Image holdImage = ImageOn(holdRect); holdImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HoldPath);
            holdImage.preserveAspect = true; holdImage.raycastTarget = true;
            Button hold = holdRect.GetComponent<Button>();
            if (hold == null) { hold = holdRect.gameObject.AddComponent<Button>(); }
            hold.targetGraphic = holdImage; hold.navigation = new Navigation { mode = Navigation.Mode.None };

            RectTransform restartRect = Child(hud, "RestartButton");
            Place(restartRect, new Vector2(0, -760), new Vector2(300, 65));
            Image restartImage = ImageOn(restartRect); restartImage.color = new Color(0.05f, 0.12f, 0.25f, 0.95f);
            restartImage.raycastTarget = true;
            Button restart = restartRect.GetComponent<Button>();
            if (restart == null) { restart = restartRect.gameObject.AddComponent<Button>(); }
            restart.targetGraphic = restartImage; restart.navigation = new Navigation { mode = Navigation.Mode.None };
            Label(restartRect, "Label", "RESTART (R)", Vector2.zero, new Vector2(290, 60), 28);
            var serialized = new SerializedObject(view);
            serialized.FindProperty("scoreText").objectReferenceValue = score;
            serialized.FindProperty("levelText").objectReferenceValue = level;
            serialized.FindProperty("linesText").objectReferenceValue = lines;
            serialized.FindProperty("stateText").objectReferenceValue = state;
            serialized.FindProperty("holdRoot").objectReferenceValue = held;
            SerializedProperty array = serialized.FindProperty("nextRoots"); array.arraySize = next.Length;
            for (int i = 0; i < next.Length; i++) { array.GetArrayElementAtIndex(i).objectReferenceValue = next[i]; }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var buttons = new SerializedObject(controller);
            buttons.FindProperty("holdButton").objectReferenceValue = hold;
            buttons.FindProperty("restartButton").objectReferenceValue = restart;
            buttons.ApplyModifiedPropertiesWithoutUndo();
            foreach (BoardView oldBoard in canvas.GetComponentsInChildren<BoardView>(true))
            {
                // HUD bài 11 thay text debug, không để hai view cùng hiển thị số liệu.
                var old = new SerializedObject(oldBoard);
                var text = old.FindProperty("hudText");
                if (text.objectReferenceValue is Text legacy) { legacy.gameObject.SetActive(false); }
                text.objectReferenceValue = null; old.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("docs/evidence");
            File.WriteAllText("docs/evidence/lesson11-setup.txt", "Imported approved Hold and PreviewPanel; configured Gameplay HUD, Hold and Restart via Unity Editor API.\n" + DateTime.UtcNow.ToString("O"));
            Debug.Log("Bài 11: import và setup Gameplay đã lưu.");
        }

        /// <summary>Copy nếu chưa có; cấu hình TextureImporter Sprite/FullRect/Bilinear/Clamp bằng API.</summary>
        private static void Import(string source, string destination)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            if (!File.Exists(destination)) { File.Copy(source, destination); }
            AssetDatabase.ImportAsset(destination, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(destination);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings); settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.filterMode = FilterMode.Bilinear; importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true; importer.sRGBTexture = true;
            importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Compressed;
            importer.SaveAndReimport();
        }

        /// <summary>Tìm hoặc tạo child trực tiếp, giúp chạy setup lần hai không tạo object trùng.</summary>
        private static RectTransform Child(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) { return (RectTransform)existing; }
            var go = new GameObject(name, typeof(RectTransform)); go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false); return (RectTransform)go.transform;
        }

        /// <summary>Đặt rect tại tâm parent theo tọa độ Canvas 1080x1920.</summary>
        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * 0.5f;
            rect.anchoredPosition = position; rect.sizeDelta = size; rect.localScale = Vector3.one;
        }

        /// <summary>Lấy hoặc thêm Image chỉ dùng trên chính object cần dựng.</summary>
        private static Image ImageOn(RectTransform rect)
        {
            Image image = rect.GetComponent<Image>();
            return image != null ? image : rect.gameObject.AddComponent<Image>();
        }

        /// <summary>Tạo khung preview vuông không chặn raycast.</summary>
        private static RectTransform Panel(Transform parent, string name, Vector2 position, Sprite sprite)
        {
            RectTransform rect = Child(parent, name); Place(rect, position, Vector2.one * 160);
            Image image = ImageOn(rect); image.sprite = sprite; image.preserveAspect = true; image.raycastTarget = false;
            return rect;
        }

        /// <summary>Tạo label TMP, dùng font mặc định đã import của dự án.</summary>
        private static TMP_Text Label(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize)
        {
            RectTransform rect = Child(parent, name); Place(rect, position, size);
            TextMeshProUGUI text = rect.GetComponent<TextMeshProUGUI>();
            if (text == null) { text = rect.gameObject.AddComponent<TextMeshProUGUI>(); }
            text.text = value; text.fontSize = fontSize; text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.8f, 0.95f, 1f); text.raycastTarget = false;
            return text;
        }
    }
}
