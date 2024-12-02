using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIInventoryBag : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SerializableDictionary<EItemCategory, UIInventoryBagCategory> m_categories = null;
        [SerializeField] private Transform m_slotParent = null; // 背包格子的父对象
        [SerializeField] private UIInventoryBagSlot m_slotPrefab = null; // 格子的预制体
        private List<UIInventoryBagSlot> m_slots = new List<UIInventoryBagSlot>();

        public List<UIInventoryBagSlot> slots => m_slots;

        //private UIInventoryBagSlot[] m_slots = null;
        private EItemCategory m_category = 0;

        private void Start()
        {
            Init();
            //GameManager.NotificationSystem.UICategorySelected.AddListener(OnBagCategorySelected);
        }

        public void Init()
        {
            GenerateSlots(GameManager.InventorySystem.backpackCapacity);
            UpdateSlots();
        }

        public void UpdateSlots()
        {
            FillSlots(); // 只更新物品显示，不重新创建格子
        }

        // Always reset to the first category when shown
        private void OnEnable() => SetCategory(0);

        private void ClearSlots()
        {
            foreach (UIInventoryBagSlot slot in m_slots)
            {
                Destroy(slot.gameObject); // 删除格子的 GameObject
            }

            m_slots.Clear();
        }

        public void GenerateSlots(int capacity)
        {
            // 清空现有格子
            ClearSlots();

            for (int i = 0; i < capacity; i++)
            {
                UIInventoryBagSlot slotObject = Instantiate(m_slotPrefab, m_slotParent);
                slotObject.SetItem(null, 0);
                m_slots.Add(slotObject);
            }
        }

        private void FillSlots()
        {
            // 清空所有格子
            foreach (var slot in m_slots)
            {
                //Debug.Log(slot);
                slot.Clear();
                // 重置被选中标签 否则直接退出菜单会有一个格子仍被标记为选中
                slot.setSlectedFalse();
            }

            int usedSlots = 0;
            List<ItemInstance> items = GameManager.InventorySystem.backpackItems;

            // 确保背包中的物品数不超过槽位数量
            int maxSlots = Mathf.Min(m_slots.Count, items.Count);

            // 填充槽位
            for (int i = 0; i < maxSlots; i++)
            {
                ItemInstance instance = items[i];
                UIInventoryBagSlot slot = m_slots[usedSlots++];
                slot.SetItem(instance.GetItem(), instance.quantity);
            }
        }

        public UIInventoryBagSlot GetFirstSlot()
        {
            return m_slots.Count > 0 ? m_slots[0] : null;
        }

        public UINavigationCursorTarget FindNavigationTarget()
        {
            if (m_slots.Count > 0)
            {
                return m_slots[0].gameObject.GetComponentInChildren<UINavigationCursorTarget>();
            }

            return null;
        }

        public void SetCategory(EItemCategory category)
        {
            // Make sure this category is available in the bag
            if (!m_categories.ContainsKey(category))
            {
                Debug.LogWarning($"Category {category} not found in the bag");
                return;
            }
            
            foreach (var entry in m_categories)
            {
                entry.Value.SetHighlight(false);
            }

            m_category = category;
            m_categories[m_category].SetHighlight(true);

            UpdateSlots();
        }

        private void OnBagCategorySelected(EItemCategory category) => SetCategory(category);
    }
}
