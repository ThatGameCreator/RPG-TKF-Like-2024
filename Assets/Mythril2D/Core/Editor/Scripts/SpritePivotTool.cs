using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using System.Collections.Generic;

namespace Gyvr.Mythril2D
{
    public static class SpritePivotTool
    {
        [MenuItem("Tools/Sprites/Set Custom Pivot")]
        public static void SetCustomPivot()
        {
            foreach (var obj in Selection.objects)
            {
                if (obj is Texture2D texture)
                {
                    string assetPath = AssetDatabase.GetAssetPath(texture);
                    var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                    if (importer == null || importer.textureType != TextureImporterType.Sprite)
                    {
                        Debug.LogError($"Texture is not set as Sprite or importer is null: {assetPath}");
                        continue;
                    }

                    var factory = new SpriteDataProviderFactories();
                    factory.Init();

                    var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
                    if (dataProvider == null)
                    {
                        Debug.LogError($"Failed to get ISpriteEditorDataProvider for: {assetPath}");
                        continue;
                    }

                    dataProvider.InitSpriteEditorDataProvider();

                    // 检查是否有多个子 Sprite
                    var spriteRects = dataProvider.GetSpriteRects();
                    if (spriteRects == null || spriteRects.Length == 0)
                    {
                        Debug.LogWarning($"No Sprites found in: {assetPath}");
                        continue;
                    }

                    // 设置 Sprite 的 Pivot
                    SetPivot(dataProvider, new Vector2(0.5f, 0.3f)); // 设置中心点
                    dataProvider.Apply();

                    // 保存并重新导入
                    importer.SaveAndReimport();
                    Debug.Log($"Updated Sprite Pivot for: {assetPath}");
                }
            }
        }

        private static void SetPivot(ISpriteEditorDataProvider dataProvider, Vector2 pivot)
        {
            var spriteRects = dataProvider.GetSpriteRects();

            foreach (var rect in spriteRects)
            {
                rect.alignment = SpriteAlignment.Custom; // 设置为自定义对齐
                rect.pivot = pivot; // 更新 Pivot
            }

            // 更新修改后的 Sprite 数据
            dataProvider.SetSpriteRects(spriteRects);
        }
    }
}
