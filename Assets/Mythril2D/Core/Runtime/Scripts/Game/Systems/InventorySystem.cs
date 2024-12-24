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
       public int backpackCapacity;
        //public SerializableDictionary<DatabaseEntryReference<Item>, int> items;
       public List<ItemInstance> items; // ��Ϊ List<ItemInstance>
    }

    [Serializable]
    public class ItemInstance
    {
        public DatabaseEntryReference<Item> itemReference; // ʹ�����ݿ����ñ������л�
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
        private int m_backpackCapacity = 20; // Ĭ������

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

        // ���ӱ�������
        public void IncreaseBackpackCapacity(int additionalCapacity)
        {
            m_backpackCapacity += additionalCapacity;
            int newCapacity = backpackCapacity;

            // �����µĸ���
            GameManager.UIManagerSystem.UIMenu.inventory.bag.GenerateSlots(newCapacity);
            GameManager.UIManagerSystem.UIMenu.warehouse.bag.GenerateSlots(newCapacity);
            GameManager.UIManagerSystem.UIMenu.shop.bag.GenerateSlots(newCapacity);
            GameManager.UIManagerSystem.UIMenu.craft.bag.GenerateSlots(newCapacity);

            GameManager.UIManagerSystem.UIMenu.inventory.FindSomethingToSelect();
        }

        // ���ٱ�������
        public void DecreaseBackpackCapacity(int reducedCapacity)
        {
            m_backpackCapacity -= reducedCapacity;
            int newCapacity = backpackCapacity;

            if (newCapacity < 0)
            {
                Debug.LogWarning("Reduced capacity exceeds current slots. Ignoring operation.");
                return;
            }

            // ���������С�����и�������
            if (newCapacity < backpackCapacity + reducedCapacity)
            {
                // ��������ĸ���
                for (int i = backpackCapacity + reducedCapacity - 1; i >= newCapacity; i--)
                {
                    if (i >= GameManager.UIManagerSystem.UIMenu.inventory.bag.slots.Count || i < 0)
                    {
                        Debug.LogWarning($"Index {i} is out of range for slots. Skipping.");
                        continue;
                    }

                    UIInventoryBagSlot slot = GameManager.UIManagerSystem.UIMenu.inventory.bag.slots[i];
                    Item item = slot.GetItem();
                    int quantity = slot.GetItemNumber();

                    // �������������Ʒ�����ɶ�Ӧ����Ʒ����
                    if (item != null && quantity > 0)
                    {
                        GameManager.ItemGenerationSystem.DropItemToPlayer(item, quantity);

                        // һ�������list��item ������ɾ��UI����
                        m_backpackItems.RemoveAt(i);
                    }

                    // ɾ�����ӵ� GameObject
                    // ��� inventory �Ƿ��ʼ���Լ��Ƿ��������
                    if (i < GameManager.UIManagerSystem.UIMenu.inventory.bag.slots.Count && i >= 0)
                    {
                        Destroy(GameManager.UIManagerSystem.UIMenu.inventory.bag.slots[i].gameObject);
                        GameManager.UIManagerSystem.UIMenu.inventory.bag.slots.RemoveAt(i);
                    }

                    // ��� warehouse �Ƿ��ʼ���Լ��Ƿ��������
                    if (GameManager.UIManagerSystem.UIMenu.warehouse?.bag?.slots != null &&
                        i < GameManager.UIManagerSystem.UIMenu.warehouse.bag.slots.Count && i >= 0)
                    {
                        Destroy(GameManager.UIManagerSystem.UIMenu.warehouse.bag.slots[i].gameObject);
                        GameManager.UIManagerSystem.UIMenu.warehouse.bag.slots.RemoveAt(i);
                    }

                    // ��� shop �Ƿ��ʼ���Լ��Ƿ��������
                    if (GameManager.UIManagerSystem.UIMenu.shop?.bag?.slots != null &&
                        i < GameManager.UIManagerSystem.UIMenu.shop.bag.slots.Count && i >= 0)
                    {
                        Destroy(GameManager.UIManagerSystem.UIMenu.shop.bag.slots[i].gameObject);
                        GameManager.UIManagerSystem.UIMenu.shop.bag.slots.RemoveAt(i);
                    }

                    // ��� craft �Ƿ��ʼ���Լ��Ƿ��������
                    if (GameManager.UIManagerSystem.UIMenu.craft?.bag?.slots != null &&
                        i < GameManager.UIManagerSystem.UIMenu.craft.bag.slots.Count && i >= 0)
                    {
                        Destroy(GameManager.UIManagerSystem.UIMenu.craft.bag.slots[i].gameObject);
                        GameManager.UIManagerSystem.UIMenu.craft.bag.slots.RemoveAt(i);
                    }
                }
            }

            GameManager.UIManagerSystem.UIMenu.inventory.FindSomethingToSelect();
        }

        public void Equip(Equipment equipment)
        {
            Debug.Assert(equipment, "Cannot equip a null equipment");

            // ��ȡ֮ǰ��װ��
            Equipment previousEquipment = GameManager.Player.Equip(equipment);

            int capacityDifference = 0;

            // �����װ��������Ӱ��
            if (equipment.capacity != 0)
            {
                capacityDifference += equipment.capacity;
            }

            // ����װ��������Ӱ��
            if (previousEquipment != null && previousEquipment.capacity != 0)
            {
                capacityDifference -= previousEquipment.capacity;
            }

            // ��������
            if (capacityDifference > 0)
            {
                GameManager.InventorySystem.IncreaseBackpackCapacity(capacityDifference);
            }
            else if (capacityDifference < 0)
            {
                GameManager.InventorySystem.DecreaseBackpackCapacity(-capacityDifference);
            }

            // ���±��� UI
            GameManager.UIManagerSystem.UIMenu.inventory.Init();

            // �Ƴ���װ��������֮ǰ��װ��
            RemoveFromBag(equipment, 1, true);

            if (previousEquipment != null)
            {
                if (IsBackpackFull(previousEquipment))
                {
                    // ���������������װ��������
                    GameManager.ItemGenerationSystem.DropItemToPlayer(previousEquipment, 1);
                }
                else
                {
                    // ����δ��������װ����ӻر���
                    AddToBag(previousEquipment, 1, true);
                }
            }
        }

        public void UnEquip(EEquipmentType type)
        {
            Equipment previousEquipment = GameManager.Player.Unequip(type);

            if (previousEquipment)
            {
                // ���ж�µ�װ������Ӱ��
                if (previousEquipment.capacity != 0)
                {
                    GameManager.InventorySystem.DecreaseBackpackCapacity(previousEquipment.capacity);

                    // ���±��� UI
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
                    // ж�±���ʱ�����ٱ�������
                    int reducedCapacity = equipment.capacity; // ���� equipment �� capacity ����
                    GameManager.InventorySystem.DecreaseBackpackCapacity(reducedCapacity);

                    // ���±��� UI
                    GameManager.UIManagerSystem.UIMenu.inventory.Init(); // �����µ��������� UI
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
                // �Ƕѵ���Ʒ��������Ҫ��ʵ��������
                return backpackItems.Count(i => i.GetItem() == item) >= quantity;
            }
        }

        public bool IsBackpackFull(Item item)
        {
            if(GetCurrentItemCount() >= m_backpackCapacity)
            {
                if (item.IsStackable)
                {
                    // �����Ƿ񱳰��пɶѵ���Ʒ ������
                    // ����жѵ�Ӧ���ǲ��� ��ȡ����
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
            // �����ĵ�ǰ��Ʒ�������Ƕѵ���Ʒ��ʵ�������㣬�ѵ���Ʒ���ܶѵ�������
            return backpackItems.Sum(instance => instance.GetItem().IsStackable ? 1 : instance.quantity);
        }

        public void AddToBag(Item item, int quantity = 1, bool forceNoEvent = false)
        {
            // ���������������Ʒ������ӣ�ֱ�ӷ���
            if (IsBackpackFull(item))
            {
                Debug.LogWarning("�����������޷������Ʒ��");
                return;
            }

            if (item.IsStackable)
            {
                // ����ɶѵ�������Ƿ�������ͬ��Ʒ
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
                // ������ɶѵ���ÿ�����һ����ʵ��
                for (int i = 0; i < quantity; i++)
                {
                    if (IsBackpackFull(item))
                    {
                        Debug.LogWarning("�����������޷���Ӹ��಻�ɶѵ���Ʒ��");
                        break;
                    }

                    backpackItems.Add(new ItemInstance(item, 1)); // ��ʽ���� quantity ����Ϊ 1
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
                // ����ɶѵ��������������Ƴ�
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
                // ������ɶѵ�����ʵ������Ƴ�
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
            backpackItems.Clear(); // ����б�
        }

        public void LoadDataBlock(InventoryDataBlock block)
        {
            m_backpackMoney = block.money;
            m_backpackCapacity = block.backpackCapacity;
            m_backpackItems = block.items
                .Select(instanceData => new ItemInstance(GameManager.Database.LoadFromReference(instanceData.itemReference), instanceData.quantity))
                .ToList();
        }

        public InventoryDataBlock CreateDataBlock()
        {
            return new InventoryDataBlock
            {
                money = backpackMoney,
                backpackCapacity = backpackCapacity,
                items = backpackItems.Select(instance => new ItemInstance(instance.GetItem(), instance.quantity)).ToList()
            };
        }
    }
}
