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
        //public SerializableDictionary<DatabaseEntryReference<Item>, int> items;
        public List<ItemInstance> items; // 改为 List<ItemInstance>
    }

    [Serializable]
    public class ItemInstance
    {
        public DatabaseEntryReference<Item> itemReference; // 使用数据库引用便于序列化
        public int quantity;

        public ItemInstance(Item item, int quantity = 1)
        {
            this.itemReference = GameManager.Database.CreateReference(item);
            this.quantity = quantity;
        }

        public Item GetItem()
        {
            return GameManager.Database.LoadFromReference(itemReference);
        }
    }

    public class InventorySystem : AGameSystem, IDataBlockHandler<InventoryDataBlock>
    {
        public int backpackMoney => m_backpackMoney;
        //public Dictionary<Item, int> backpackItems => m_backpackItems;

        private int m_backpackMoney = 0;
        //private Dictionary<Item, int> m_backpackItems = new Dictionary<Item, int>();

        public List<ItemInstance> backpackItems => m_backpackItems;
        private List<ItemInstance> m_backpackItems = new List<ItemInstance>();
        public int backpackCapacity => m_backpackCapacity;
        private int m_backpackCapacity = 20; // 默认容量为 20

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

        public Equipment GetEquipment(EEquipmentType type)
        {
            if (GameManager.Player.equipments.ContainsKey(type))
            {
                return GameManager.Player.equipments[type];
            }

            return null;
        }

        // 增加背包容量
        public void IncreaseBackpackCapacity(int additionalCapacity)
        {
            m_backpackCapacity = GameManager.UIManagerSystem.UIMenu.inventory.bag.slots.Count;
            int newCapacity = backpackCapacity + additionalCapacity;

            // 生成新的格子
            GameManager.UIManagerSystem.UIMenu.inventory.bag.GenerateSlots(newCapacity);

            GameManager.UIManagerSystem.UIMenu.inventory.FindSomethingToSelect();

        }

        // 减少背包容量
        public void DecreaseBackpackCapacity(int reducedCapacity)
        {
            int m_backpackCapacity = GameManager.UIManagerSystem.UIMenu.inventory.bag.slots.Count;
            int newCapacity = backpackCapacity - reducedCapacity;

            // 如果新容量小于现有格子数量，则移除多余的格子
            if (newCapacity < backpackCapacity)
            {
                for (int i = backpackCapacity - 1; i >= newCapacity; i--)
                {
                    Destroy(GameManager.UIManagerSystem.UIMenu.inventory.bag.slots[i].gameObject);
                    GameManager.UIManagerSystem.UIMenu.inventory.bag.slots.RemoveAt(i);
                }
            }

            GameManager.UIManagerSystem.UIMenu.inventory.FindSomethingToSelect();
        }

        public void Equip(Equipment equipment)
        {
            Debug.Assert(equipment, "Cannot equip a null equipment");

            // 检查装备是否是背包
            if (equipment.type == EEquipmentType.Backpack)
            {
                // 增加背包容量
                int additionalCapacity = equipment.capacity; // 假设 equipment 有 capacity 属性
                GameManager.InventorySystem.IncreaseBackpackCapacity(additionalCapacity);

                // 更新背包 UI
                GameManager.UIManagerSystem.UIMenu.inventory.Init(); // 假设 UpdateSlots 方法已经根据容量自动更新格子
            }

            Equipment previousEquipment = GameManager.Player.Equip(equipment);

            RemoveFromBag(equipment, 1, true);

            if (previousEquipment)
            {
                // 如果是卸下的背包，则减少背包容量
                if (previousEquipment.type == EEquipmentType.Backpack)
                {
                    int reducedCapacity = previousEquipment.capacity; // 假设 previousEquipment 也有 capacity 属性
                    GameManager.InventorySystem.DecreaseBackpackCapacity(reducedCapacity);

                    // 更新背包 UI
                    GameManager.UIManagerSystem.UIMenu.inventory.Init(); // 根据新的容量更新 UI
                }

                AddToBag(previousEquipment, 1, true);
            }

        }

        public void UnEquip(EEquipmentType type)
        {
            Equipment previousEquipment = GameManager.Player.Unequip(type);

            if (previousEquipment)
            {
                // 如果卸下的是背包装备，减少背包容量
                if (previousEquipment.type == EEquipmentType.Backpack)
                {
                    int reducedCapacity = previousEquipment.capacity; // 假设 previousEquipment 有 capacity 属性
                    GameManager.InventorySystem.DecreaseBackpackCapacity(reducedCapacity);

                    // 更新背包 UI
                    GameManager.UIManagerSystem.UIMenu.inventory.Init(); // 根据新的容量更新 UI
                }

                AddToBag(previousEquipment, 1, true);
            }
        }

        public void UnEquipAll()
        {
            foreach (EEquipmentType type in Enum.GetValues(typeof(EEquipmentType)))
            {
                Equipment equipment = GameManager.Player.Unequip(type);

                if (equipment != null && equipment.type == EEquipmentType.Backpack)
                {
                    // 卸下背包时，减少背包容量
                    int reducedCapacity = equipment.capacity; // 假设 equipment 有 capacity 属性
                    GameManager.InventorySystem.DecreaseBackpackCapacity(reducedCapacity);

                    // 更新背包 UI
                    GameManager.UIManagerSystem.UIMenu.inventory.Init(); // 根据新的容量更新 UI
                }

                if (equipment != null)
                {
                    AddToBag(equipment, 1, true);
                }
            }
        }

        public int GetItemCount(Item item)
        {
            if (item.isStackable)
            {
                var instance = backpackItems.FirstOrDefault(i => i.GetItem() == item);
                return instance?.quantity ?? 0;
            }
            else
            {
                return backpackItems.Count(i => i.GetItem() == item);
            }
        }

        public bool HasItemInBag(Item item, int quantity = 1)
        {
            if (item.isStackable)
            {
                var instance = backpackItems.FirstOrDefault(i => i.GetItem() == item);
                return instance != null && instance.quantity >= quantity;
            }
            else
            {
                // 非堆叠物品的数量需要按实例数计算
                return backpackItems.Count(i => i.GetItem() == item) >= quantity;
            }
        }

        public bool IsBackpackFull()
        {
            return GetCurrentItemCount() >= m_backpackCapacity;
        }

        public int GetCurrentItemCount()
        {
            // 背包的当前物品数量：非堆叠物品按实例数计算，堆叠物品按总堆叠数计算
            return backpackItems.Sum(instance => instance.GetItem().isStackable ? 1 : instance.quantity);
        }

        public void TryAddItemToBag(Item item, int quantity = 1)
        {
            if (GameManager.InventorySystem.IsBackpackFull())
            {
                //GameManager.NotificationSystem.ShowMessage("背包已满，无法添加物品！");
                return;
            }

            GameManager.InventorySystem.AddToBag(item, quantity);
        }


        public void AddToBag(Item item, int quantity = 1, bool forceNoEvent = false)
        {
            // 如果背包已满且物品不可添加，直接返回
            if (IsBackpackFull())
            {
                Debug.LogWarning("背包已满，无法添加物品！");
                return;
            }

            if (item.isStackable)
            {
                // 如果可堆叠，检查是否已有相同物品
                var instance = backpackItems.FirstOrDefault(i => i.GetItem() == item);
                if (instance != null)
                {
                    instance.quantity += quantity;
                }
                else
                {
                    backpackItems.Add(new ItemInstance(item, quantity));
                }
            }
            else
            {
                // 如果不可堆叠，每次添加一个新实例
                for (int i = 0; i < quantity; i++)
                {
                    if (IsBackpackFull())
                    {
                        Debug.LogWarning("背包已满，无法添加更多不可堆叠物品！");
                        break;
                    }

                    backpackItems.Add(new ItemInstance(item, 1)); // 显式传递 quantity 参数为 1
                }
            }

            if (!forceNoEvent)
            {
                if (!GameManager.WarehouseSystem.isOpenning)
                {
                    GameManager.NotificationSystem.itemAdded.Invoke(item, quantity);
                }
            }
        }

        public void SetBackpackCapacity(int newCapacity)
        {
            m_backpackCapacity = newCapacity;

            // 检查是否超出新容量，并移除多余物品（可选逻辑）
            while (GetCurrentItemCount() > m_backpackCapacity)
            {
                RemoveFromBag(backpackItems.Last().GetItem());
            }
        }


        public bool RemoveFromBag(Item item, int quantity = 1, bool forceNoEvent = false)
        {
            bool success = false;

            if (item.isStackable)
            {
                // 如果可堆叠，减少数量或移除
                var instance = backpackItems.FirstOrDefault(i => i.GetItem() == item);
                if (instance != null)
                {
                    if (instance.quantity <= quantity)
                    {
                        backpackItems.Remove(instance);
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
                    var instance = backpackItems.FirstOrDefault(i => i.GetItem() == item);
                    if (instance != null)
                    {
                        backpackItems.Remove(instance);
                        success = true;
                    }
                }
            }

            if (!forceNoEvent)
            {
                if (!GameManager.WarehouseSystem.isOpenning)
                {
                    GameManager.NotificationSystem.itemRemoved.Invoke(item, quantity);
                }
            }

            return success;
        }

        public void EmptyBag()
        {
            m_backpackMoney = 0;
            backpackItems.Clear(); // 清空列表
        }

        public void LoadDataBlock(InventoryDataBlock block)
        {
            m_backpackMoney = block.money;
            m_backpackItems = block.items
                .Select(instanceData => new ItemInstance(GameManager.Database.LoadFromReference(instanceData.itemReference), instanceData.quantity))
                .ToList();
        }

        public InventoryDataBlock CreateDataBlock()
        {
            return new InventoryDataBlock
            {
                money = m_backpackMoney,
                items = backpackItems.Select(instance => new ItemInstance(instance.GetItem(), instance.quantity)).ToList()
            };
        }
    }
}
