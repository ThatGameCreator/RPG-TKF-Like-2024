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
        public SerializableDictionary<DatabaseEntryReference<Item>, int> items;
    }

    public class WarehouseSystem : AGameSystem, IDataBlockHandler<WarehouseDataBlock>
    {
        public int warehouseMoney => m_warehouseMoney;
        public Dictionary<Item, int> warehouseItems => m_warehouseItems;


        private int m_warehouseMoney = 0;
        private Dictionary<Item, int> m_warehouseItems = new Dictionary<Item, int>();

        public bool isOpenning = false;

        public int GetItemCount(Item item)
        {
            if (warehouseItems.TryGetValue(item, out int count))
            {
                return count;
            }

            return 0;
        }

        public void AddMoney(int value)
        {
            if (value > 0)
            {
                m_warehouseMoney += value;
                GameManager.NotificationSystem.moneyAdded.Invoke(value);
            }
        }

        public void RemoveMoney(int value)
        {
            if (value > 0)
            {
                m_warehouseMoney = math.max(warehouseMoney - value, 0);
                GameManager.NotificationSystem.moneyRemoved.Invoke(value);
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
            return warehouseItems.ContainsKey(item) && warehouseItems[item] >= quantity;
        }

        public void AddToWarehouse(Item item, int quantity = 1, bool forceNoEvent = false)
        {
            if (!warehouseItems.ContainsKey(item))
            {
                warehouseItems.Add(item, quantity);
            }
            else
            {
                warehouseItems[item] += quantity;
            }

            if (!forceNoEvent)
            {
                GameManager.NotificationSystem.itemAdded.Invoke(item, quantity);
            }
        }

        public bool RemoveFromWarehouse(Item item, int quantity = 1, bool forceNoEvent = false)
        {
            bool success = false;

            if (warehouseItems.ContainsKey(item))
            {
                if (quantity >= warehouseItems[item])
                {
                    warehouseItems.Remove(item);
                }
                else
                {
                    warehouseItems[item] -= quantity;
                }

                success = true;
            }

            if (!forceNoEvent)
            {
                GameManager.NotificationSystem.itemRemoved.Invoke(item, quantity);
            }

            return success;
        }

        public void LoadDataBlock(WarehouseDataBlock block)
        {
            m_warehouseMoney = block.money;
            m_warehouseItems = block.items.ToDictionary(kvp => GameManager.Database.LoadFromReference(kvp.Key), kvp => kvp.Value);
        }

        public WarehouseDataBlock CreateDataBlock()
        {
            return new WarehouseDataBlock
            {
                money = m_warehouseMoney,
                items = new SerializableDictionary<DatabaseEntryReference<Item>, int>(m_warehouseItems.ToDictionary(kvp => GameManager.Database.CreateReference(kvp.Key), kvp => kvp.Value))
            };
        }
    }
}
