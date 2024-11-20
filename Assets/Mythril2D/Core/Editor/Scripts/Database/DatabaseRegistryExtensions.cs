using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Gyvr.Mythril2D
{
    public static class DatabaseRegistryExtensions
    {
        public static bool HasGUID(this DatabaseRegistry registry, string guid)
        {
            // 检查 GUID 是否存在
            return registry.entries.ContainsKey(guid);
        }

        public static bool HasGUIDConversion(this DatabaseRegistry registry, string guid)
        {
            // 检查 GUID 转换映射是否存在
            return registry.GUIDConversionMap.ContainsKey(guid);
        }

        public static void Set(this DatabaseRegistry registry, DatabaseEntry[] entries)
        {
            // 初始化时，使用 GUID 作为键，条目作为值
            registry.Initialize(entries.ToDictionary(entry => entry.GetAssetGUID(), entry => entry));
            registry.ForceSave();
        }

        public static void Register(this DatabaseRegistry registry, DatabaseEntry entry)
        {
            // 使用条目的 GUID 注册
            string guid = entry.GetAssetGUID();

            // 获取条目的类型名称
            string typeName = entry.GetType().FullName;

            // 检查是否已存在该 GUID
            if (!registry.entries.ContainsKey(guid))
            {
                // 将 GUID 映射到类型名称
                registry.entries[guid] = typeName;
                registry.ForceSave();
            }
        }


        public static void Unregister(this DatabaseRegistry registry, DatabaseEntry entry)
        {
            // 使用 GUID 删除条目
            string guid = entry.GetAssetGUID();
            registry.RemoveAt(guid);
        }

        public static void RemoveAt(this DatabaseRegistry registry, string guid)
        {
            // 删除指定 GUID 的条目
            if (registry.HasGUID(guid))
            {
                registry.entries.Remove(guid);
                registry.ForceSave();
            }
        }

        public static void RemoveMissingReferences(this DatabaseRegistry registry)
        {
            List<string> keysToRemove = new List<string>();

            foreach (var entry in registry.entries)
            {
                if (entry.Value == null)
                {
                    keysToRemove.Add(entry.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                registry.entries.Remove(key);
            }

            registry.ForceSave();
        }

        public static void Clear(this DatabaseRegistry registry)
        {
            // 清空所有条目
            registry.entries.Clear();
            registry.ForceSave();
        }

        public static void RemoveConversion(this DatabaseRegistry registry, string from)
        {
            // 移除 GUID 转换映射
            registry.GUIDConversionMap.Remove(from);
            registry.ForceSave();
        }

        public static void SetConversion(this DatabaseRegistry registry, string from, string to)
        {
            // 设置 GUID 转换映射
            registry.GUIDConversionMap[from] = to;
            registry.ForceSave();
        }

        private static void ForceSave(this DatabaseRegistry registry)
        {
            // 强制保存更改
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssetIfDirty(registry);
        }
    }
}
