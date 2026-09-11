using UnityEditor;
using UnityEngine;

namespace Game.Editor.Tools
{
    /// <summary>Aria 选材一次性导入设置（UI 纹理：关 mipmap、Clamp、Bilinear、alpha 透明）。</summary>
    public static class AriaImport
    {
        public const string Root = "Assets/Resources/UI/AriaGUI";

        [MenuItem("Tools/S5U/Aria Apply Import Settings")]
        public static void ApplyImportSettings()
        {
            int count = 0;
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { Root });
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null) continue;
                imp.textureType = TextureImporterType.Default;
                imp.mipmapEnabled = false;
                imp.wrapMode = TextureWrapMode.Clamp;
                imp.filterMode = FilterMode.Bilinear;
                imp.sRGBTexture = true;
                imp.alphaIsTransparency = true;
                imp.npotScale = TextureImporterNPOTScale.None;
                imp.maxTextureSize = 1024;
                imp.SaveAndReimport();
                count++;
            }
            Debug.Log("[AriaImport] textures configured: " + count);
        }
    }
}
