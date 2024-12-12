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
        private int m_backpackCapacity = 20; // 默认容量

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
            m_backpackCapacity += additionalCapacity;
            int newCapacity = backpackCapacity;

            // 生成新的格子
            GameManager.UIManagerSystem.UIMenu.inventory.bag.GenerateSlots(newCapacity);
            GameManager.UIManagerSystem.UIMenu.warehouse.bag.GenerateSlots(newCapacity);
            GameManager.UIManagerSystem.UIMenu.shop.bag.GenerateSlots(newCapacity);
            GameManager.UIManagerSystem.UIMenu.craft.bag.GenerateSlots(newCapacity);

            GameManager.UIManagerSystem.UIMenu.inventory.FindSomethingToSelect();
        }

        // 减少背包容量
        public void DecreaseBackpackCapacity(int reducedCapacity)
        {
            m_backpackCapacity -= reducedCapacity;
            int newCapacity = backpackCapacity;

            if (newCapacity < 0)
            {
                Debug.LogWarning("Reduced capacity exceeds current slots. Ignoring operation.");
                return;
            }

            // 如果新容量小于现有格子数量
            if (newCapacity < backpackCapacity + reducedCapacity)
            {
                // 遍历多余的格子
                for (int i = backpackCapacity + reducedCapacity - 1; i >= newCapacity; i--)
                {
                    UIInventoryBagSlot slot = GameManager.UIManagerSystem.UIMenu.inventory.bag.slots[i];
                    Item item = slot.GetItem();
                    int quantity = slot.GetItemNumber();

                    // 如果格子中有物品，生成对应的物品对象
                    if (item != null && quantity > 0)
                    {
                        GameManager.ItemGenerationSystem.DropItemToPlayer(item, quantity);

                        // 一个存的是list的item 下面是删除UI格子
                        m_backpackItems.RemoveAt(i);
                    }

                    // 删除格子的 GameObject
                    Destroy(GameManager.UIManagerSystem.UIMenu.inventory.bag.slots[i].gameObject);
                    Destroy(GameManager.UIManagerSystem.UIMenu.warehouse.bag.slots[i].gameObject);
                    Destroy(GameManager.UIManagerSystem.UIMenu.shop.bag.slots[i].gameObject);
                    Destroy(GameManager.UIManagerSystem.UIMenu.craft.bag.slots[i].gameObject);
                    GameManager.UIManagerSystem.UIMenu.inventory.bag.slots.RemoveAt(i);
                    GameManager.UIManagerSystem.UIMenu.warehouse.bag.slots.RemoveAt(i);
                    GameManager.UIManagerSystem.UIMenu.shop.bag.slots.RemoveAt(i);
                    GameManager.UIManagerSystem.UIMenu.craft.bag.slots.RemoveAt(i);
                }
            }

            GameManager.UIManagerSystem.UIMenu.inventory.FindSomethingToSelect();
        }

        public void Equip(Equipment equipment)
        {
            Debug.Assert(equipment, "Cannot equip a null equipment");

            // 获取之前的装备
            Equipment previousEquipment = GameManager.Player.Equip(equipment);

            int capacityDifference = 0;

            // 检查新装备的容量影响
            if (equipment.capacity != 0)
            {
                capacityDifference += equipment.capacity;
            }

            // 检查旧装备的容量影响
            if (previousEquipment != null && previousEquipment.capacity != 0)
            {
                capacityDifference -= previousEquipment.capacity;
            }

            // 调整容量
            if (capacityDifference > 0)
            {
                GameManager.InventorySystem.IncreaseBackpackCapacity(capacityDifference);
            }
            else if (capacityDifference < 0)
            {
                GameManager.InventorySystem.DecreaseBackpackCapacity(-capacityDifference);
            }

            // 更新背包 UI
            GameManager.UIManagerSystem.UIMenu.inventory.Init();

            // 移除新装备并处理之前的装备
            RemoveFromBag(equipment, 1, true);

            if (previousEquipment != null)
            {
                if (IsBackpackFull(previousEquipment))
                {
                    // 背包已满，掉落旧装备到地上
                    GameManager.ItemGenerationSystem.DropItemToPlayer(previousEquipment, 1);
                }
                else
                {
                    // 背包未满，将旧装备添加回背包
                    AddToBag(previousEquipment, 1, true);
                }
            }
        }

        public void UnEquip(EEquipmentType type)
        {
            Equipment previousEquipment = GameManager.Player.Unequip(type);

            if (previousEquipment)
            {
                // 检查卸下的装备容量影响
                if (previousEquipment.capacity != 0)
                {
                    GameManager.InventorySystem.DecreaseBackpackCapacity(previousEquipment.capacity);

                    // 更新背包 UI
                    GameManager.UIManagerSystem.UIMenu.inventory.Init();
                }

                if (IsBackpackFull(previousEquipment))
                {
                    GameManager.ItemGenerationSystem.DropItemToPlayer(previousEquipment, 1);
                }
                else
                {
                    AddToBag(previousEquipment, 1, true);
                }
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
            if (item.IsStackable)
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
            if (item.IsStackable)
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

        public bool IsBackpackFull(Item item)
        {
            if(GetCurrentItemCount() >= m_backpackCapacity)
            {
                if (item.IsStackable)
                {
                    // 返回是否背包有可堆叠物品 有则不满
                    // 如果有堆叠应该是不满 得取反！
                    return !HasItemInBag(item);
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
            return backpackItems.Sum(instance => instance.GetItem().IsStackable ? 1 : instance.quantity);
        }

        public void TryAddItemToBag(Item item, int quantity = 1)
        {
            if (GameManager.InventorySystem.IsBackpackFull(item))
            {
                //GameManager.NotificationSystem.ShowMessage("背包已满，无法添加物品！");
                return;
            }

            GameManager.InventorySystem.AddToBag(item, quantity);
        }


        public void AddToBag(Item item, int quantity = 1, bool forceNoEvent = false)
        {
            // 如果背包已满且物品不可添加，直接返回
            if (IsBackpackFull(item))
            {
                Debug.LogWarning("背包已满，无法添加物品！");
                return;
            }

            if (item.IsStackable)
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
                    if (IsBackpackFull(item))
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


        public bool RemoveFromBag(Item item, int quantity = 1, bool forceNoEvent = false)
        {
            //Debug.Log("RemoveFromBag");
            bool success = false;

            if (item.IsStackable)
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
                money = backpackMoney,
                items = backpackItems.Select(instance => new ItemInstance(instance.GetItem(), instance.quantity)).ToList()
            };
        }
    }
}
