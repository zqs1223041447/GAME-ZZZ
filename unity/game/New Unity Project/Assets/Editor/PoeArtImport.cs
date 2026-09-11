using UnityEditor;
using UnityEngine;

namespace Game.Editor.Tools
{
    /// <summary>
    /// PoE 天赋树美术的导入设置（UI 纹理：关 mipmap、Clamp、Bilinear、alpha 透明）。
    /// 自动生效，避免 750 张节点图标按默认设置被压缩/模糊。
    /// </summary>
    public sealed class PoeArtImport : AssetPostprocessor
    {
        public const string Root = "Assets/Resources/UI/PoE/";

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Root))
                return;
            var imp = (TextureImporter)assetImporter;
            imp.textureType = TextureImporterType.Default;
            imp.mipmapEnabled = false;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.filterMode = FilterMode.Bilinear;
            imp.sRGBTexture = true;
            imp.alphaIsTransparency = true;
            imp.npotScale = TextureImporterNPOTScale.None;
            // 图集（Chrome）比单张图标大得多，按目录给不同上限
            imp.maxTextureSize = assetPath.Contains("/Chrome/") ? 4096 : 256;
        }

        [MenuItem("Tools/S5U/PoE Apply Import Settings")]
        public static void Apply()
        {
            int count = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Root.TrimEnd('/') }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null)
                    continue;
                imp.textureType = TextureImporterType.Default;
                imp.mipmapEnabled = false;
                imp.wrapMode = TextureWrapMode.Clamp;
                imp.filterMode = FilterMode.Bilinear;
                imp.sRGBTexture = true;
                imp.alphaIsTransparency = true;
                imp.npotScale = TextureImporterNPOTScale.None;
                imp.maxTextureSize = path.Contains("/Chrome/") ? 4096 : 256;
                imp.SaveAndReimport();
                count++;
            }
            Debug.Log("[PoeArtImport] textures configured: " + count);
        }
    }
}
