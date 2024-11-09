using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIWarehouseBag : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SerializableDictionary<EItemCategory, UIInventoryBagCategory> m_categories = null;

        private UIInventoryWarehouseSlot[] m_slots = null;
        private EItemCategory m_category = 0;

        public void Init()
        {
            m_slots = GetComponentsInChildren<UIInventoryWarehouseSlot>();

            foreach (var category in m_categories)
            {
                category.Value.SetCategory(category.Key);
            }
        }

        // Always reset to the first category when shown
        private void OnEnable() => SetCategory(0);

        public void UpdateSlots()
        {
            ClearSlots();
            FillSlots();
        }

        private void ClearSlots()
        {
            foreach (UIInventoryWarehouseSlot slot in m_slots)
            {
                slot.Clear();
            }
        }

        private void FillSlots()
        {
            int usedSlots = 0;

            Dictionary<Item, int> items = GameManager.WarehouseSystem.warehouseItems;

            foreach (KeyValuePair<Item, int> entry in items)
            {
                if (entry.Key.category == m_category)
                {
                    UIInventoryWarehouseSlot slot = m_slots[usedSlots++];
                    slot.SetItem(entry.Key, entry.Value);
                }
            }
        }

        public UIInventoryWarehouseSlot GetFirstSlot()
        {
            return m_slots.Length > 0 ? m_slots[0] : null;
        }

        public UINavigationCursorTarget FindNavigationTarget()
        {
            if (m_slots.Length > 0)
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
