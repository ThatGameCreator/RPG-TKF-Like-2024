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
            // ָ�� ItemGenerationSystem Ԥ�����·��
            string systemPrefabPath = "Assets/Mythril2D/Demo/Prefabs/Entities/Systems/Item Generation System.prefab";

            // ����Դ�������м���Ԥ����
            GameObject systemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(systemPrefabPath);
            if (systemPrefab == null)
            {
                Debug.LogError($"ItemGenerationSystem prefab not found at path: {systemPrefabPath}");
                return;
            }

            // ��ȡ ItemGenerationSystem ���
            ItemGenerationSystem itemGenSystem = systemPrefab.GetComponent<ItemGenerationSystem>();
            if (itemGenSystem == null)
            {
                Debug.LogError("ItemGenerationSystem component not found on the prefab.");
                return;
            }

            // ��ʼ���ֵ�����������
            if (itemGenSystem.InstanceObjects == null)
            {
                itemGenSystem.InstanceObjects = new SerializableDictionary<Item, SurfaceItem>();
            }

            // �����ȡ���� SurfaceItem Ԥ���岢�����ֵ�
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

            // Ӧ���޸ĵ�Ԥ����
            PrefabUtility.SavePrefabAsset(systemPrefab);

            Debug.Log("ItemGenerationSystem prefab has been successfully updated.");
        }
    }
}
