using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public struct InventoryDataBlock
    {
        public int money;
        public SerializableDictionary<DatabaseEntryReference<Item>, int> items;
    }

    public class InventorySystem : AGameSystem, IDataBlockHandler<InventoryDataBlock>
    {
        public int backpackMoney => m_backpackMoney;
        public Dictionary<Item, int> backpackItems => m_backpackItems;

        private int m_backpackMoney = 0;
        private Dictionary<Item, int> m_backpackItems = new Dictionary<Item, int>();

        public int GetItemCount(Item item)
        {
            if (backpackItems.TryGetValue(item, out int count))
            {
                return count;
            }

            return 0;
        }

        public void AddMoney(int value)
        {
            if (value > 0)
            {
                m_backpackMoney += value;
                GameManager.NotificationSystem.moneyAdded.Invoke(value);
            }
        }

        public void RemoveMoney(int value)
        {
            if (value > 0)
            {
                m_backpackMoney = math.max(backpackMoney - value, 0);
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
            return value <= backpackMoney;
        }

        public bool HasItemInBag(Item item, int quantity = 1)
        {
            return backpackItems.ContainsKey(item) && backpackItems[item] >= quantity;
        }

        public Equipment GetEquipment(EEquipmentType type)
        {
            if (GameManager.Player.equipments.ContainsKey(type))
            {
                return GameManager.Player.equipments[type];
            }

            return null;
        }

        public void Equip(Equipment equipment)
        {
            Debug.Assert(equipment, "Cannot equip a null equipment");

            Equipment previousEquipment = GameManager.Player.Equip(equipment);

            RemoveFromBag(equipment, 1, true);

            if (previousEquipment)
            {
                AddToBag(previousEquipment, 1, true);
            }
        }

        public void UnEquip(EEquipmentType type)
        {
            Equipment previousEquipment = GameManager.Player.Unequip(type);

            if (previousEquipment)
            {
                AddToBag(previousEquipment, 1, true);
            }
        }

        public void UnEquipAll()
        {
            foreach (EEquipmentType type in Enum.GetValues(typeof(EEquipmentType)))
            {
                GameManager.Player.Unequip(type);
            }
        }


        public void AddToBag(Item item, int quantity = 1, bool forceNoEvent = false)
        {
            if (!backpackItems.ContainsKey(item))
            {
                backpackItems.Add(item, quantity);
            }
            else
            {
                backpackItems[item] += quantity;
            }

            if (!forceNoEvent)
            {
                GameManager.NotificationSystem.itemAdded.Invoke(item, quantity);
            }
        }

        public bool RemoveFromBag(Item item, int quantity = 1, bool forceNoEvent = false)
        {
            bool success = false;

            if (backpackItems.ContainsKey(item))
            {
                if (quantity >= backpackItems[item])
                {
                    backpackItems.Remove(item);
                }
                else
                {
                    backpackItems[item] -= quantity;
                }

                success = true;
            }

            if (!forceNoEvent)
            {
                GameManager.NotificationSystem.itemRemoved.Invoke(item, quantity);
            }

            return success;
        }

        public void EmptyBag()
        {
            m_backpackMoney = 0;
            m_backpackItems = new Dictionary<Item, int>();
        }

        public void LoadDataBlock(InventoryDataBlock block)
        {
            m_backpackMoney = block.money;
            m_backpackItems = block.items.ToDictionary(kvp => GameManager.Database.LoadFromReference(kvp.Key), kvp => kvp.Value);
        }

        public InventoryDataBlock CreateDataBlock()
        {
            return new InventoryDataBlock
            {
                money = m_backpackMoney,
                items = new SerializableDictionary<DatabaseEntryReference<Item>, int>(m_backpackItems.ToDictionary(kvp => GameManager.Database.CreateReference(kvp.Key), kvp => kvp.Value))
            };
        }
    }
}
