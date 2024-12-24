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
        [SerializeField] private Transform m_slotParent = null; // �������ӵĸ�����
        [SerializeField] private UIInventoryBagSlot m_slotPrefab = null; // ���ӵ�Ԥ����
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
            FillSlots(); // ֻ������Ʒ��ʾ�������´�������
        }

        // Always reset to the first category when shown
        //private void OnEnable() => SetCategory(0);

        private void ClearSlots()
        {
            foreach (UIInventoryBagSlot slot in m_slots)
            {
                Destroy(slot.gameObject); // ɾ�����ӵ� GameObject
            }

            m_slots.Clear();
        }

        public void GenerateSlots(int capacity)
        {
            // ������и���
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
            // ������и���
            foreach (var slot in m_slots)
            {
                //Debug.Log(slot);
                slot.Clear();
                // ���ñ�ѡ�б�ǩ ����ֱ���˳��˵�����һ�������Ա����Ϊѡ��
                slot.setSlectedFalse();
            }

            int usedSlots = 0;
            List<ItemInstance> items = GameManager.InventorySystem.backpackItems;

            // ȷ�������е���Ʒ����������λ����
            int maxSlots = Mathf.Min(m_slots.Count, items.Count);

            // ����λ
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

    }
}
