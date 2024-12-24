using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIWarehouseBag : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform m_slotParent = null; // �������ӵĸ�����
        [SerializeField] private UIInventoryWarehouseSlot m_slotPrefab = null; // ���ӵ�Ԥ����
        private List<UIInventoryWarehouseSlot> m_slots = new List<UIInventoryWarehouseSlot>();

        //private UIInventoryWarehouseSlot[] m_slots = null;
        private EItemCategory m_category = 0;

        private void Start()
        {
            Init();

            //GameManager.NotificationSystem.UICategorySelected.AddListener(OnBagCategorySelected);
        }

        public void Init()
        {
            GenerateSlots(GameManager.WarehouseSystem.warehouseCapacity);
            FillSlots();
        }
        public void UpdateSlots()
        {
            FillSlots(); // ֻ������Ʒ��ʾ�������´�������
        }

        private void ClearSlots()
        {
            foreach (UIInventoryWarehouseSlot slot in m_slots)
            {
                Destroy(slot.gameObject); // ɾ�����ӵ� GameObject
            }

            m_slots.Clear();
        }

        private void GenerateSlots(int capacity)
        {
            for (int i = 0; i < capacity; i++)
            {
                UIInventoryWarehouseSlot slotObject = Instantiate(m_slotPrefab, m_slotParent);
                slotObject.SetItem(null, 0);
                m_slots.Add(slotObject);
            }
        }
        private void FillSlots()
        {
            // ������и���
            foreach (var slot in m_slots)
            {
                slot.Clear();

                slot.setSlectedFalse();
            }

            int usedSlots = 0;
            List<ItemInstance> items = GameManager.WarehouseSystem.warehouseItems;

            // ȷ�������е���Ʒ����������λ����
            int maxSlots = Mathf.Min(m_slots.Count, items.Count);

            // ����λ
            for (int i = 0; i < maxSlots; i++)
            {
                ItemInstance instance = items[i];
                UIInventoryWarehouseSlot slot = m_slots[usedSlots++];
                slot.SetItem(instance.GetItem(), instance.quantity);
            }
        }

        public UIInventoryWarehouseSlot GetFirstSlot()
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
    }
}
