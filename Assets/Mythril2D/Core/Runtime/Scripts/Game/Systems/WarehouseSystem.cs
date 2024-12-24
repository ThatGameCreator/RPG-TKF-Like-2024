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
        public int warehouseCapacity;
        //public SerializableDictionary<DatabaseEntryReference<Item>, int> items;
        public List<ItemInstance> items; // 改为 List<ItemInstance>
    }

    public class WarehouseSystem : AGameSystem, IDataBlockHandler<WarehouseDataBlock>
    {
        public int warehouseMoney => m_warehouseMoney;
        //public Dictionary<Item, int> warehouseItems => m_warehouseItems;


        private int m_warehouseMoney = 0;
        //private Dictionary<Item, int> m_warehouseItems = new Dictionary<Item, int>();

        public List<ItemInstance> warehouseItems => m_warehouseItems;
        private List<ItemInstance> m_warehouseItems = new List<ItemInstance>();
        public int warehouseCapacity => m_warehouseCapacity;
        private int m_warehouseCapacity = 90; // 默认容量

        public bool isOpenning = false;

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

        public int GetItemCount(Item item)
        {
            if (item.IsStackable)
            {
                var instance = warehouseItems.FirstOrDefault(i => i.GetItem() == item);
                return instance?.quantity ?? 0;
            }
            else
            {
                return warehouseItems.Count(i => i.GetItem() == item);
            }
        }

        public bool IsWarehouseFull(Item item)
        {
            if (GetCurrentItemCount() >= m_warehouseCapacity)
            {
                if (item.IsStackable)
                {
                    // 返回是否背包有可堆叠物品 有则不满
                    // 如果有堆叠应该是不满 得取反！
                    return !HasItemInWarehouse(item);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public int GetCurrentItemCount()
        {
            // 背包的当前物品数量：非堆叠物品按实例数计算，堆叠物品按总堆叠数计算
            return warehouseItems.Sum(instance => instance.GetItem().IsStackable ? 1 : instance.quantity);
        }

        public bool HasItemInWarehouse(Item item, int quantity = 1)
        {
            if (item.IsStackable)
            {
                var instance = warehouseItems.FirstOrDefault(i => i.GetItem() == item);
                return instance != null && instance.quantity >= quantity;
            }
            else
            {
                // 非堆叠物品的数量需要按实例数计算
                return warehouseItems.Count(i => i.GetItem() == item) >= quantity;
            }
        }

        public void AddToWarehouse(Item item, int quantity = 1, bool forceNoEvent = false)
        {
            // 如果背包已满且物品不可添加，直接返回
            if (IsWarehouseFull(item))
            {
                Debug.LogWarning("背包已满，无法添加物品！");
                return;
            }

            if (item.IsStackable)
            {
                // 如果可堆叠，检查是否已有相同物品
                var instance = warehouseItems.FirstOrDefault(i => i.GetItem() == item);
                if (instance != null)
                {
                    instance.quantity += quantity;
                }
                else
                {
                    warehouseItems.Add(new ItemInstance(item, quantity));
                }
            }
            else
            {
                // 如果不可堆叠，每次添加一个新实例
                for (int i = 0; i < quantity; i++)
                {
                    if (IsWarehouseFull(item))
                    {
                        Debug.LogWarning("背包已满，无法添加更多不可堆叠物品！");
                        break;
                    }

                    warehouseItems.Add(new ItemInstance(item, 1)); // 显式传递 quantity 参数为 1
                }
            }

            if (!forceNoEvent)
            {
                if (!GameManager.WarehouseSystem.isOpenning)
                {
                    //GameManager.NotificationSystem.itemAdded.Invoke(item, quantity);
                }
            }
        }

        public bool RemoveFromWarehouse(Item item, int quantity = 1, bool forceNoEvent = false)
        {
            bool success = false;

            if (item.IsStackable)
            {
                // 如果可堆叠，减少数量或移除
                var instance = warehouseItems.FirstOrDefault(i => i.GetItem() == item);
                if (instance != null)
                {
                    if (instance.quantity <= quantity)
                    {
                        warehouseItems.Remove(instance);
                    }
                    else
                    {
                        instance.quantity -= quantity;
                    }
                    success = true;
                }
            }
            else
            {
                // 如果不可堆叠，按实例逐个移除
                for (int i = 0; i < quantity; i++)
                {
                    var instance = warehouseItems.FirstOrDefault(i => i.GetItem() == item);
                    if (instance != null)
                    {
                        warehouseItems.Remove(instance);
                        success = true;
                    }
                }
            }

            if (!forceNoEvent)
            {
                if (!GameManager.WarehouseSystem.isOpenning)
                {
                    //GameManager.NotificationSystem.itemRemoved.Invoke(item, quantity);
                }
            }

            return success;
        }

        public void LoadDataBlock(WarehouseDataBlock block)
        {
            m_warehouseMoney = block.money;
            m_warehouseCapacity = block.warehouseCapacity;
            m_warehouseItems = block.items
                .Select(instanceData => new ItemInstance(GameManager.Database.LoadFromReference(instanceData.itemReference), instanceData.quantity))
                .ToList();
        }

        public WarehouseDataBlock CreateDataBlock()
        {
            return new WarehouseDataBlock
            {
                money = m_warehouseMoney,
                warehouseCapacity = warehouseCapacity,
                items = warehouseItems.Select(instance => new ItemInstance(instance.GetItem(), instance.quantity)).ToList()
            };
        }
    }
}
