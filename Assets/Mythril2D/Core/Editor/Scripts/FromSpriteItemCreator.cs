using Gyvr.Mythril2D;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FromSpriteItemCreator : MonoBehaviour
{
    private const string ItemSavePath = "Assets/Mythril2D/Demo/Database/Items/Equipments/";

    [MenuItem("Tools/Create Items from Spritesheet")]
    public static void CreateItemsMenu()
    {
        string path = EditorUtility.OpenFilePanel("ѡ��Spritesheet", "Assets", "png");
        if (!string.IsNullOrEmpty(path))
        {
            string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
            FromSpriteItemCreator.CreateItemsFromSpritesheet(relativePath);
        }
    }

    public static void CreateItemsFromSpritesheet(string spritesheetPath)
    {
        // ����spritesheet
        var spritesheet = AssetDatabase.LoadAssetAtPath<Texture2D>(spritesheetPath);
        if (spritesheet == null)
        {
            Debug.LogError("�޷�����Spritesheet: " + spritesheetPath);
            return;
        }

        // ��ȡ���е��Ӿ���
        var sprites = AssetDatabase.LoadAllAssetsAtPath(spritesheetPath).OfType<Sprite>().ToArray();
        if (sprites.Length == 0)
        {
            Debug.LogError("δ�ҵ��κ��Ӿ���: " + spritesheetPath);
            return;
        }

        // ��������·��
        if (!Directory.Exists(ItemSavePath))
        {
            Directory.CreateDirectory(ItemSavePath);
        }

        foreach (var sprite in sprites)
        {
            // �����µ�Itemʵ��
            Item newItem = ScriptableObject.CreateInstance<Item>();

            // ���û�����Ϣ
            newItem.Icon = sprite;
            newItem.DisplayName = sprite.name; // ʹ�þ���������Ϊ��Ʒ����
            newItem.Category = EItemCategory.Gear; // ʾ����ʵ�ʸ�����Ҫ�޸�

            // ���浽ָ��·��
            string assetPath = $"{ItemSavePath}{sprite.name}.asset";
            AssetDatabase.CreateAsset(newItem, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Item������ɣ�");
    }
}
