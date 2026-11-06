using System;
using System.IO;
using System.Linq;
using NeonBlocks.Audio;
using NeonBlocks.Config;
using NeonBlocks.Views;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.SceneManagement;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
namespace NeonBlocks.Editor
{
    /// <summary>Setup bài 13 qua Unity API; chạy lại không tạo bản sao asset hoặc host.</summary>
    public static class Lesson13Setup
    {
        private const string Cells = "Assets/_Project/Art/Gameplay/Cells/";
        private const string ThemePath = "Assets/_Project/Art/Gameplay/CellTheme.asset";
        /// <summary>Import approved asset, tạo atlas/bank, gán theme và background vào scene.</summary>
        [MenuItem("NeonBlocks/Lesson 13/Import and configure")]
        public static void Configure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCount).Any(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty))
            { throw new InvalidOperationException("Thoát Play và lưu các scene trước khi setup bài 13."); }
            var previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                AssetDatabase.Refresh();
                foreach (string file in Directory.GetFiles(Cells, "*.png")) { ImportSprite(file.Replace('\\', '/'), 256); }
                ImportSprite("Assets/_Project/Art/UI/Backgrounds/NB-UI-012-QuietBackground.png", 2048);
                foreach (string path in Directory.GetFiles("Assets/_Project/Audio", "*.wav", SearchOption.AllDirectories))
                {
                    var importer = (AudioImporter)AssetImporter.GetAtPath(path);
                    bool music = path.Contains("Music");
                    importer.forceToMono = !music;
                    AudioImporterSampleSettings sample = importer.defaultSampleSettings;
                    sample.preloadAudioData = !music;
                    sample.loadType = music ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
                    sample.compressionFormat = music ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.PCM;
                    sample.quality = .8f; importer.defaultSampleSettings = sample; importer.SaveAndReimport();
                }
                CellTheme theme = AssetDatabase.LoadAssetAtPath<CellTheme>(ThemePath);
                if (theme == null) { theme = ScriptableObject.CreateInstance<CellTheme>(); AssetDatabase.CreateAsset(theme, ThemePath); }
                var fields = new SerializedObject(theme); var sprites = fields.FindProperty("cells"); sprites.arraySize = 7;
                string[] ids = { "I", "O", "T", "J", "L", "S", "Z" };
                for (int i = 0; i < ids.Length; i++) { sprites.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(Cells + "NB-GAME-002-Cell" + ids[i] + ".png"); }
                fields.FindProperty("ghost").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(Cells + "NB-GAME-003-GhostCell.png");
                fields.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(theme);
                BuildAtlas(); BuildBank();
                ConfigureScene("MainMenu", theme); ConfigureScene("Gameplay", theme);
                AssetDatabase.SaveAssets();
                Directory.CreateDirectory("docs/evidence");
                File.WriteAllText("docs/evidence/lesson13-setup.txt", "Configured approved sprites, atlas, audio bank, Addressables and scene theme via Unity API.\n" + DateTime.UtcNow.ToString("O"));
                Debug.Log("Bài 13 setup hoàn tất. Play từ Entry. Mixer là tùy chọn; source volume hoạt động ngay.");
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(previous); }
        }
        /// <summary>Sprite single FullRect; mipmap off, bilinear và clamp.</summary>
        private static void ImportSprite(string path, int maxSize)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
            importer.maxTextureSize = maxSize; importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear; importer.wrapMode = TextureWrapMode.Clamp; importer.sRGBTexture = true;
            importer.textureCompression = TextureImporterCompression.Compressed; importer.SaveAndReimport();
        }
        /// <summary>Atlas chỉ chứa tám cell sprite, không xoay/tight packing.</summary>
        private static void BuildAtlas()
        {
            string path = "Assets/_Project/Art/Gameplay/GameplayCells.spriteatlas";
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
            if (atlas == null) { atlas = new SpriteAtlas(); AssetDatabase.CreateAsset(atlas, path); }
            var packing = atlas.GetPackingSettings(); packing.enableRotation = false; packing.enableTightPacking = false; packing.padding = 4; atlas.SetPackingSettings(packing);
            var texture = atlas.GetTextureSettings(); texture.generateMipMaps = false; texture.filterMode = FilterMode.Bilinear; atlas.SetTextureSettings(texture);
            atlas.Remove(atlas.GetPackables());
            atlas.Add(Directory.GetFiles(Cells, "*.png").Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p.Replace('\\', '/'))).Cast<UnityEngine.Object>().ToArray());
            atlas.SetIncludeInBuild(true); EditorUtility.SetDirty(atlas);
        }
        /// <summary>Tạo bank một lần; chạy lại cập nhật clip nhưng giữ tùy chỉnh mixer.</summary>
        private static void BuildBank()
        {
            string key = GameConfigParser.Parse(File.ReadAllText("Assets/_Project/Configs/GameConfig.json")).AudioBankKey;
            string folder = "Assets/_Project/Prefabs/Audio"; Directory.CreateDirectory(folder);
            string path = folder + "/" + key + ".prefab"; AssetDatabase.Refresh();
            bool exists = File.Exists(path);
            GameObject root = exists ? PrefabUtility.LoadPrefabContents(path) : new GameObject(key);
            try
            {
                AudioHost host = root.GetComponent<AudioHost>();
                if (host == null) { host = root.AddComponent<AudioHost>(); }
                var fields = new SerializedObject(host);
                AudioSource menu = Source(root.transform, "MenuMusic", true), game = Source(root.transform, "GameplayMusic", true);
                menu.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Music/NB-MUS-001-MenuLoop.wav");
                game.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Music/NB-MUS-002-GameplayLoop.wav");
                fields.FindProperty("menuSource").objectReferenceValue = menu; fields.FindProperty("gameplaySource").objectReferenceValue = game;
                var voices = fields.FindProperty("voices"); voices.arraySize = 8;
                for (int i = 0; i < 8; i++) { voices.GetArrayElementAtIndex(i).objectReferenceValue = Source(root.transform, "SfxVoice" + i, false); }
                string[] clips = { "002-Rotate", "001-Lock", "003-Drop", "004-Clear", "005-GameOver" };
                var effects = fields.FindProperty("effects"); effects.arraySize = clips.Length;
                for (int i = 0; i < clips.Length; i++) { effects.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/NB-SFX-" + clips[i] + ".wav"); }
                fields.ApplyModifiedPropertiesWithoutUndo(); PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { if (exists) { PrefabUtility.UnloadPrefabContents(root); } else { UnityEngine.Object.DestroyImmediate(root); } }
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) { throw new InvalidOperationException("Chưa có Addressables Settings."); }
            settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(path), settings.DefaultGroup).address = key;
            EditorUtility.SetDirty(settings);
        }
        /// <summary>AudioSource 2D thuộc bank, không tự Play và không thêm AudioListener.</summary>
        private static AudioSource Source(Transform parent, string name, bool loop)
        {
            Transform child = parent.Find(name);
            if (child == null) { var go = new GameObject(name); go.transform.SetParent(parent, false); child = go.transform; }
            AudioSource source = child.GetComponent<AudioSource>();
            if (source == null) { source = child.gameObject.AddComponent<AudioSource>(); }
            source.playOnAwake = false; source.loop = loop; source.spatialBlend = 0; source.volume = 0; return source;
        }
        /// <summary>Giữ layout/nút hiện có, thay theme và bỏ grid raster để dùng grid logic.</summary>
        private static void ConfigureScene(string name, CellTheme theme)
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/" + name + ".unity", OpenSceneMode.Single);
            // OpenScene có thể unload asset không được scene cũ giữ; lấy lại theme sau thao tác đó.
            theme = AssetDatabase.LoadAssetAtPath<CellTheme>(ThemePath);
            if (theme == null) { throw new InvalidOperationException("Không tải được CellTheme sau khi mở scene."); }
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (BoardView view in root.GetComponentsInChildren<BoardView>(true))
                {
                    var fields = new SerializedObject(view); fields.FindProperty("theme").objectReferenceValue = theme; fields.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(view);
                    if (view is BoardView && view.GetComponent<SafeGameplayLayout>() == null) { view.gameObject.AddComponent<SafeGameplayLayout>(); }
                }
                foreach (GameplayHudView view in root.GetComponentsInChildren<GameplayHudView>(true))
                {
                    var fields = new SerializedObject(view); fields.FindProperty("theme").objectReferenceValue = theme; fields.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(view);
                }
                foreach (Image image in root.GetComponentsInChildren<Image>(true))
                {
                    if (image.name == "BoardRoot") { image.sprite = null; image.color = new Color(.03f, .05f, .11f, .96f); image.raycastTarget = false; }
                    if (image.name == "Background")
                    {
                        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/Backgrounds/NB-UI-012-QuietBackground.png");
                        image.color = Color.white; image.raycastTarget = false; image.preserveAspect = false;
                        AspectRatioFitter fitter = image.GetComponent<AspectRatioFitter>();
                        if (fitter == null) { fitter = image.gameObject.AddComponent<AspectRatioFitter>(); }
                        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent; fitter.aspectRatio = 941f / 1672;
                    }
                }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}