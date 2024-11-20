using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D + nameof(DatabaseRegistry))]
    public class DatabaseRegistry : ScriptableObject
    {
        public bool autoAddNewDatabaseEntries => m_autoAddNewDatabaseEntries;
        public bool autoRemoveDatabaseEntries => m_autoRemoveDatabaseEntries;
        //public SerializableDictionary<string, DatabaseEntry> entries => m_entries;
        public SerializableDictionary<string, string> entries => m_entries;  // GUID 和类型映射
        public SerializableDictionary<string, string> GUIDConversionMap => m_GUIDConversionMap;  // GUID 转换映射

        [Header("Automation Settings")]
        [SerializeField] private bool m_autoAddNewDatabaseEntries = true;
        [SerializeField] private bool m_autoRemoveDatabaseEntries = true;

        [Header("Database Content")]
        [SerializeField] private SerializableDictionary<string, string> m_entries = null; // 存储 GUID 和物品类型的映射
        [SerializeField] private SerializableDictionary<string, string> m_GUIDConversionMap = null; // 存储 GUID 转换映射

        // 使用 GUID 加载物品实例
        public Item LoadItemByGUID(string guid)
        {
            if (m_entries.ContainsKey(guid))
            {
                // 获取对应 GUID 的物品类型
                string itemType = m_entries[guid];

                // 根据 itemType 来加载物品的 ScriptableObject 类型实例
                return LoadItemOfType(itemType);
            }

            return null;
        }

        // 加载物品实例的辅助方法
        private Item LoadItemOfType(string itemType)
        {
            // 根据 itemType 创建并返回对应类型的物品实例
            // 这里可以通过反射或者其他方式加载对应类型的 Item 对象
            // 假设这里返回的是通过资源加载的 Item 实例
            return Resources.Load<Item>(itemType);
        }

        private T LoadItemOfType<T>(string itemType) where T : DatabaseEntry
        {
            // 根据类型字符串加载物品实例
            return Resources.Load<T>(itemType);  // 这里可以根据实际项目需要替换加载方法
        }


        // 添加新物品到数据库
        public void AddItem(string guid, string itemType)
        {
            if (!m_entries.ContainsKey(guid))
            {
                m_entries.Add(guid, itemType);
            }
        }

        // 移除数据库中的物品
        public void RemoveItem(string guid)
        {
            if (m_entries.ContainsKey(guid))
            {
                m_entries.Remove(guid);
            }
        }
        public DatabaseEntryReference<T> CreateReference<T>(T entry) where T : DatabaseEntry
        {
            // 创建 GUID 引用时，我们根据条目获取 GUID，并返回 DatabaseEntryReference
            string guid = DatabaseEntryToGUID(entry);
            return new DatabaseEntryReference<T>(guid);
        }

        public T LoadFromReference<T>(DatabaseEntryReference<T> reference) where T : DatabaseEntry
        {
            // 根据引用的 GUID 加载相应的物品（或数据库条目）
            return GUIDToDatabaseEntry<T>(reference.guid);
        }

        public T GUIDToDatabaseEntry<T>(string guid) where T : DatabaseEntry
        {
            HashSet<string> visited = new HashSet<string>();

            // 避免环形引用，转换 GUID 如果存在
            while (m_GUIDConversionMap.ContainsKey(guid))
            {
                guid = m_GUIDConversionMap[guid];
                if (visited.Contains(guid))
                {
                    Debug.LogError($"Circular reference detected in DatabaseRegistry: {guid}");
                    return null;
                }
                visited.Add(guid);
            }

            // 通过 GUID 加载物品或数据库条目
            if (m_entries.ContainsKey(guid))
            {
                // 根据 GUID 获取条目类型并加载实例
                string itemType = m_entries[guid];
                return LoadItemOfType<T>(itemType);  // 加载对应类型的条目
            }

            return null;
        }

        public string DatabaseEntryToGUID<T>(T instance) where T : DatabaseEntry
        {
            // 获取实例的类型名称作为字符串
            string instanceTypeName = instance.GetType().FullName;

            // 根据实例类型名称查找对应的 GUID
            string guid = m_entries.FirstOrDefault(entry => entry.Value == instanceTypeName).Key;

            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError($"Database entry {instance} does not exist in the registry.");
            }

            return guid;
        }


        public void Initialize(Dictionary<string, DatabaseEntry> data)
        {
            // 通过 GUID 映射初始化数据库条目
            m_entries = new SerializableDictionary<string, string>();

            foreach (var entry in data)
            {
                string guid = entry.Key;  // GUID 为字典的键
                string itemType = entry.Value.GetType().FullName;  // 获取条目的类型字符串
                m_entries[guid] = itemType;  // 保存 GUID -> 类型 映射
            }
        }
    }
}
