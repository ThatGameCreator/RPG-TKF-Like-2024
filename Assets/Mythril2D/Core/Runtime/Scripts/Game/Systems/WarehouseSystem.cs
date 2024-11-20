using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public struct WarehouseDataBlock
    {
        public int money;
        public SerializableDictionary<DatabaseEntryReference<StringDatabaseEntryReference>, int> items;
    }

    public class WarehouseSystem : AGameSystem, IDataBlockHandler<WarehouseDataBlock>
    {
        public int warehouseMoney => m_warehouseMoney;
        public Dictionary<string, int> warehouseItems => m_warehouseItems;


        private int m_warehouseMoney = 0;
        private Dictionary<string, int> m_warehouseItems = new Dictionary<string, int>();

        public bool isOpenning = false;

        public int GetItemCount(Item item)
        {
            // 如果物品可堆叠，通过堆叠数返回数量
            if (item.isStackable)
            {
                if (warehouseItems.TryGetValue(item.uniqueID, out int count))
                {
                    return count;
                }
            }
            else
            {
                // 对于不可堆叠物品，返回物品实例的数量
                return warehouseItems
                    .Where(kvp => kvp.Key == item.uniqueID)
                    .Count();
            }

            return 0;
        }

        public void AddMoney(int value)
        {
            if (value > 0)
            {
                m_warehouseMoney += value;
                //GameManager.NotificationSystem.moneyAdded.Invoke(value);
            }
        }

        public void RemoveMoney(int value)
        {
            if (value > 0)
            {
                m_warehouseMoney = math.max(warehouseMoney - value, 0);
                //GameManager.NotificationSystem.moneyRemoved.Invoke(value);
            }
        }

        public bool TryRemoveMoney(int value)
        {
            if (HasSufficientFunds(value))
            {
                RemoveMoney(value);
                return true;
            }

            return false;
        }

        public bool HasSufficientFunds(int value)
        {
            return value <= warehouseMoney;
        }

        public bool HasItemInWarehouse(Item item, int quantity = 1)
        {
            if (item.isStackable)
            {
                return warehouseItems.ContainsKey(item.uniqueID) && warehouseItems[item.uniqueID] >= quantity;
            }
            else
            {
                // 对于不可堆叠物品，检查是否有足够的不同实例
                return warehouseItems
                    .Where(kvp => kvp.Key == item.uniqueID)
                    .Count() >= quantity;
            }
        }

        public void AddToWarehouse(Item item, int quantity = 1, bool forceNoEvent = false)
        {
            if (item.isStackable)
            {
                // 可堆叠物品：增加数量
                if (!warehouseItems.ContainsKey(item.uniqueID))
                {
                    warehouseItems.Add(item.uniqueID, quantity);
                }
                else
                {
                    warehouseItems[item.uniqueID] += quantity;
                }
            }
            else
            {
                // 不可堆叠物品：创建新实例并根据uniqueID添加
                for (int i = 0; i < quantity; i++)
                {
                    // 创建唯一副本并为其分配一个新的 uniqueID
                    Item newItem = CreateUniqueItem(item);
                    warehouseItems.Add(newItem.uniqueID, 1); // 每个副本的数量默认是 1
                }
            }

            // 通知物品添加事件
            if (!forceNoEvent)
            {
                if (!GameManager.WarehouseSystem.isOpenning)
                {
                    GameManager.NotificationSystem.itemAdded.Invoke(item, quantity);
                }
            }
        }

        private Item CreateUniqueItem(Item baseItem)
        {
            // 根据基础物品创建一个独立副本（此处需要实际实现逻辑，例如生成新引用或标记）
            Item newItem = ScriptableObject.Instantiate(baseItem);
            newItem.name = $"{baseItem.name}_{System.Guid.NewGuid()}"; // 使用唯一 GUID 标识
            return newItem;
        }

        public bool RemoveFromWarehouse(Item item, int quantity = 1, bool forceNoEvent = false)
        {
            bool success = false;

            if (item.isStackable)
            {
                // 可堆叠物品：减少数量
                if (warehouseItems.ContainsKey(item.uniqueID))
                {
                    if (quantity >= warehouseItems[item.uniqueID])
                    {
                        warehouseItems.Remove(item.uniqueID);
                    }
                    else
                    {
                        warehouseItems[item.uniqueID] -= quantity;
                    }

                    success = true;
                }
            }
            else
            {
                // 不可堆叠物品：按实例逐个删除
                int removed = 0;
                var itemsToRemove = warehouseItems
                    .Where(kvp => kvp.Key == item.uniqueID)
                    .Take(quantity)
                    .ToList();

                foreach (var kvp in itemsToRemove)
                {
                    warehouseItems.Remove(kvp.Key);
                    removed++;
                }

                success = removed == quantity;
            }

            // 通知物品移除事件
            if (!forceNoEvent && success)
            {
                if (!GameManager.WarehouseSystem.isOpenning)
                {
                    GameManager.NotificationSystem.itemRemoved.Invoke(item, quantity);
                }
            }

            return success;

        }

        public void LoadDataBlock(WarehouseDataBlock block)
        {
            m_warehouseMoney = block.money;
            m_warehouseItems = block.items.ToDictionary(
                kvp => kvp.Key.guid,  // 假设 StringDatabaseEntryReference 有 uniqueID 字段
                kvp => kvp.Value);
        }

        public WarehouseDataBlock CreateDataBlock()
        {
            return new WarehouseDataBlock
            {
                money = m_warehouseMoney,
                items = new SerializableDictionary<DatabaseEntryReference<StringDatabaseEntryReference>, int>(
                    m_warehouseItems.ToDictionary(
                        kvp =>
                        {
                            // 通过 guid 获取 StringDatabaseEntryReference 实例
                            var entry = GameManager.Database.LoadFromReference<StringDatabaseEntryReference>(kvp.Key);
                            return GameManager.Database.CreateReference(entry);  // 使用 CreateReference 来创建 DatabaseEntryReference
                        },
                        kvp => kvp.Value
                    )
                )
            };
        }
    }
}
