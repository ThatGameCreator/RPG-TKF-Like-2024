using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Gyvr.Mythril2D;

namespace Gyvr.Mythril2D
{

    public class ItemGenerationSystemCreator : MonoBehaviour
    {
        [MenuItem("Tools/Create Item Generation System Prefab")]
        public static void CreateItemGenerationSystem()
        {
            // Ԥ�����ȡ·������������ SurfaceItem Ԥ����洢�ڴ�·����
            string surfaceItemFolderPath = "Assets/Mythril2D/Demo/Prefabs/Entities/Generate"; // �޸�Ϊʵ��·��
            string[] surfaceItemGuids = AssetDatabase.FindAssets("t:Prefab", new[] { surfaceItemFolderPath });

            // ���� ItemGenerationSystem ��Ϸ����
            GameObject itemGenSystemObject = new GameObject("ItemGenerationSystem");
            ItemGenerationSystem itemGenSystem = itemGenSystemObject.AddComponent<ItemGenerationSystem>();

            // ��ʼ���ֵ�
            itemGenSystem.InstanceObjects = new SerializableDictionary<Item, SurfaceItem>();

            foreach (string guid in surfaceItemGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab == null) continue;

                // ��ȡ SurfaceItem ���
                SurfaceItem surfaceItem = prefab.GetComponent<SurfaceItem>();
                if (surfaceItem == null)
                {
                    Debug.LogWarning($"Prefab at {prefabPath} does not contain a SurfaceItem component.");
                    continue;
                }

                // ��ȡ Loot ����
                Loot loot = surfaceItem.Loot;
                if (loot.entries == null || loot.entries.Length == 0)
                {
                    Debug.LogWarning($"Prefab {prefab.name} has no Loot entries.");
                    continue;
                }

                // ȡ��һ�� LootEntry �е� Item
                Item item = loot.entries[0].item;
                if (item == null)
                {
                    Debug.LogWarning($"Prefab {prefab.name} has a null Item in its Loot.");
                    continue;
                }

                // �� Item �� SurfaceItem �洢���ֵ���
                itemGenSystem.InstanceObjects[item] = surfaceItem;
            }

            // ���� ItemGenerationSystem ΪԤ����
            string systemPrefabPath = "Assets/Mythril2D/Demo/Prefabs/Systems/ItemGenerationSystem.prefab"; // �޸�Ϊʵ�ʱ���·��
            string directoryPath = System.IO.Path.GetDirectoryName(systemPrefabPath);
            if (!AssetDatabase.IsValidFolder(directoryPath))
            {
                System.IO.Directory.CreateDirectory(directoryPath);
            }

            PrefabUtility.SaveAsPrefabAsset(itemGenSystemObject, systemPrefabPath);

            // ������ʱ����
            DestroyImmediate(itemGenSystemObject);

            Debug.Log($"ItemGenerationSystem prefab created at {systemPrefabPath}.");
        }
    }
}
