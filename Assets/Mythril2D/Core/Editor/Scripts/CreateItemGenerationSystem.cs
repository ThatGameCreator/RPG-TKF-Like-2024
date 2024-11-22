using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Gyvr.Mythril2D;

public class ItemGenerationSystemCreator : MonoBehaviour
{
    [MenuItem("Tools/Create Item Generation System Prefab")]
    public static void CreateItemGenerationSystem()
    {
        // 预制体读取路径（假设所有 SurfaceItem 预制体存储在此路径）
        string surfaceItemFolderPath = "Assets/Mythril2D/Demo/Prefabs/Entities/Generate"; // 修改为实际路径
        string[] surfaceItemGuids = AssetDatabase.FindAssets("t:Prefab", new[] { surfaceItemFolderPath });

        // 创建 ItemGenerationSystem 游戏对象
        GameObject itemGenSystemObject = new GameObject("ItemGenerationSystem");
        ItemGenerationSystem itemGenSystem = itemGenSystemObject.AddComponent<ItemGenerationSystem>();

        // 初始化字典
        itemGenSystem.InstanceObjects = new SerializableDictionary<Item, SurfaceItem>();

        foreach (string guid in surfaceItemGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null) continue;

            // 获取 SurfaceItem 组件
            SurfaceItem surfaceItem = prefab.GetComponent<SurfaceItem>();
            if (surfaceItem == null)
            {
                Debug.LogWarning($"Prefab at {prefabPath} does not contain a SurfaceItem component.");
                continue;
            }

            // 获取 Loot 数据
            Loot loot = surfaceItem.Loot;
            if (loot.entries == null || loot.entries.Length == 0)
            {
                Debug.LogWarning($"Prefab {prefab.name} has no Loot entries.");
                continue;
            }

            // 取第一个 LootEntry 中的 Item
            Item item = loot.entries[0].item;
            if (item == null)
            {
                Debug.LogWarning($"Prefab {prefab.name} has a null Item in its Loot.");
                continue;
            }

            // 将 Item 和 SurfaceItem 存储到字典中
            itemGenSystem.InstanceObjects[item] = surfaceItem;
        }

        // 保存 ItemGenerationSystem 为预制体
        string systemPrefabPath = "Assets/Mythril2D/Demo/Prefabs/Systems/ItemGenerationSystem.prefab"; // 修改为实际保存路径
        string directoryPath = System.IO.Path.GetDirectoryName(systemPrefabPath);
        if (!AssetDatabase.IsValidFolder(directoryPath))
        {
            System.IO.Directory.CreateDirectory(directoryPath);
        }

        PrefabUtility.SaveAsPrefabAsset(itemGenSystemObject, systemPrefabPath);

        // 清理临时对象
        DestroyImmediate(itemGenSystemObject);

        Debug.Log($"ItemGenerationSystem prefab created at {systemPrefabPath}.");
    }
}
