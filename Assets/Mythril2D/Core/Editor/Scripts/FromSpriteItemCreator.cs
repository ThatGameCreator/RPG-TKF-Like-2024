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
        string path = EditorUtility.OpenFilePanel("选择Spritesheet", "Assets", "png");
        if (!string.IsNullOrEmpty(path))
        {
            string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
            FromSpriteItemCreator.CreateItemsFromSpritesheet(relativePath);
        }
    }

    public static void CreateItemsFromSpritesheet(string spritesheetPath)
    {
        // 加载spritesheet
        var spritesheet = AssetDatabase.LoadAssetAtPath<Texture2D>(spritesheetPath);
        if (spritesheet == null)
        {
            Debug.LogError("无法加载Spritesheet: " + spritesheetPath);
            return;
        }

        // 获取所有的子精灵
        var sprites = AssetDatabase.LoadAllAssetsAtPath(spritesheetPath).OfType<Sprite>().ToArray();
        if (sprites.Length == 0)
        {
            Debug.LogError("未找到任何子精灵: " + spritesheetPath);
            return;
        }

        // 创建保存路径
        if (!Directory.Exists(ItemSavePath))
        {
            Directory.CreateDirectory(ItemSavePath);
        }

        foreach (var sprite in sprites)
        {
            // 创建新的Item实例
            Item newItem = ScriptableObject.CreateInstance<Item>();

            // 设置基本信息
            newItem.Icon = sprite;
            newItem.DisplayName = sprite.name; // 使用精灵名字作为物品名字
            newItem.Category = EItemCategory.Gear; // 示例，实际根据需要修改

            // 保存到指定路径
            string assetPath = $"{ItemSavePath}{sprite.name}.asset";
            AssetDatabase.CreateAsset(newItem, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Item创建完成！");
    }
}
