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

                    // ����Ƿ��ж���� Sprite
                    var spriteRects = dataProvider.GetSpriteRects();
                    if (spriteRects == null || spriteRects.Length == 0)
                    {
                        Debug.LogWarning($"No Sprites found in: {assetPath}");
                        continue;
                    }

                    // ���� Sprite �� Pivot
                    SetPivot(dataProvider, new Vector2(0.5f, 0.25f)); // �������ĵ�
                    dataProvider.Apply();

                    // ���沢���µ���
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
                rect.alignment = SpriteAlignment.Custom; // ����Ϊ�Զ������
                rect.pivot = pivot; // ���� Pivot
            }

            // �����޸ĺ�� Sprite ����
            dataProvider.SetSpriteRects(spriteRects);
        }
    }
}
