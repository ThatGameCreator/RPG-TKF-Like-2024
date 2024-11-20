using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIInventoryEquipmentSlot : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private EEquipmentType m_equipmentType = EEquipmentType.Head;
        [SerializeField] private Image m_placeholder = null;
        [SerializeField] private Image m_content = null;
        [SerializeField] private Button m_button = null;

        public EEquipmentType equipmentType => m_equipmentType;

        private string m_equipmentGUID = null; // 使用 GUID 代替 Equipment 实例
        //private Equipment m_equipment = null;
        private bool m_selected = false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_button.Select();
        }

        public void OnSelect(BaseEventData eventData)
        {
            m_selected = true;
            // 如果 m_equipmentGUID 不为空，通过 GUID 获取 Equipment 实例
            if (!string.IsNullOrEmpty(m_equipmentGUID))
            {
                Equipment equipment = GameManager.Database.LoadItemByGUID(m_equipmentGUID) as Equipment;

                // 确保装备存在且是正确的类型
                if (equipment != null && equipment.type == m_equipmentType)
                {
                    string itemGUID = GameManager.Database.DatabaseEntryToGUID(equipment); // 获取对应的 GUID
                                                                                           // 触发 itemDetailsOpened 事件，传递 GUID
                    GameManager.NotificationSystem.itemDetailsOpened.Invoke(itemGUID);
                }
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            m_selected = false;
            GameManager.NotificationSystem.itemDetailsClosed.Invoke();
        }

        public void SetEquipment(Equipment equipment)
        {
            if (equipment != null)
            {
                m_equipmentGUID = GameManager.Database.DatabaseEntryToGUID(equipment); // 存储物品的 GUID

                Debug.Assert(equipment.type == m_equipmentType, "Equipment type mismatch");

                m_placeholder.enabled = false;
                m_content.enabled = true;
                m_content.sprite = equipment.icon;
            }
            else
            {
                m_equipmentGUID = null; // 清除 GUID

                m_placeholder.enabled = true;
                m_content.enabled = false;
                m_content.sprite = null;
            }

            if (m_selected && !string.IsNullOrEmpty(m_equipmentGUID))
            {
                Equipment newEquipment = GameManager.Database.LoadItemByGUID(m_equipmentGUID) as Equipment;
                if (newEquipment != null)
                {
                    GameManager.NotificationSystem.itemDetailsOpened.Invoke(m_equipmentGUID); // 传递 GUID
                }
            }
        }

        private void Awake()
        {
            m_button.onClick.AddListener(OnSlotClicked);
        }

        private void OnSlotClicked()
        {
            if (!string.IsNullOrEmpty(m_equipmentGUID))
            {
                // 通过 GUID 获取 Equipment 实例
                Equipment equipment = GameManager.Database.LoadItemByGUID(m_equipmentGUID) as Equipment;
                if (equipment != null)
                {
                    SendMessageUpwards("OnEquipmentItemClicked", equipment, SendMessageOptions.RequireReceiver);
                }
            }
        }
    }
}
