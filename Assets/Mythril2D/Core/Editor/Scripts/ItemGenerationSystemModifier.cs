using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Gyvr.Mythril2D
{
    public class ItemGenerationSystemModifier : MonoBehaviour
    {
        [MenuItem("Tools/Modify Item Generation System in Assets")]
        public static void ModifyItemGenerationSystemInAssets()
        {
            // 指定 ItemGenerationSystem 预制体的路径
            string systemPrefabPath = "Assets/Mythril2D/Demo/Prefabs/Entities/Systems/Item Generation System.prefab";

            // 从资源管理器中加载预制体
            GameObject systemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(systemPrefabPath);
            if (systemPrefab == null)
            {
                Debug.LogError($"ItemGenerationSystem prefab not found at path: {systemPrefabPath}");
                return;
            }

            // 获取 ItemGenerationSystem 组件
            ItemGenerationSystem itemGenSystem = systemPrefab.GetComponent<ItemGenerationSystem>();
            if (itemGenSystem == null)
            {
                Debug.LogError("ItemGenerationSystem component not found on the prefab.");
                return;
            }

            // 初始化字典或更新其内容
            if (itemGenSystem.InstanceObjects == null)
            {
                itemGenSystem.InstanceObjects = new SerializableDictionary<Item, SurfaceItem>();
            }

            // 假设读取所有 SurfaceItem 预制体并更新字典
            string surfaceItemFolderPath = "Assets/Mythril2D/Demo/Prefabs/Entities/Generate";
            string[] surfaceItemGuids = AssetDatabase.FindAssets("t:Prefab", new[] { surfaceItemFolderPath });

            foreach (string guid in surfaceItemGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab == null) continue;

                SurfaceItem surfaceItem = prefab.GetComponent<SurfaceItem>();
                if (surfaceItem == null) continue;

                Loot loot = surfaceItem.Loot;
                if (loot.entries == null || loot.entries.Length == 0) continue;

                Item item = loot.entries[0].item;
                if (item == null) continue;

                itemGenSystem.InstanceObjects[item] = surfaceItem;
            }

            // 应用修改到预制体
            PrefabUtility.SavePrefabAsset(systemPrefab);

            Debug.Log("ItemGenerationSystem prefab has been successfully updated.");
        }
    }
}
