using UnityEditor;
using UnityEngine;
namespace NeonBlocks.Editor
{
    /// <summary>Import riêng ba PNG bài 12 đã duyệt, không tác động ảnh khác.</summary>
    public sealed class Lesson12AssetImporter : AssetPostprocessor
    {
        /// <summary>Áp Sprite settings ở lần import đầu hoặc reimport.</summary>
        private void OnPreprocessTexture()
        {
            if (!assetPath.EndsWith("NB-UI-009-PopupPanel.png") && !assetPath.EndsWith("NB-UI-010-Settings.png") && !assetPath.EndsWith("NB-UI-011-HighScore.png")) { return; }
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
            importer.filterMode = FilterMode.Bilinear; importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true; importer.sRGBTexture = true; importer.mipmapEnabled = false; importer.maxTextureSize = 2048;
        }
    }
}