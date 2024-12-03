using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

        private Equipment m_equipment = null;
        private bool m_selected = false;

        private void Awake()
        {
            m_button.onClick.AddListener(OnSlotClicked);
        }
        private void Start()
        {
            GameManager.InputSystem.ui.drop.performed += OnDropEquipment;
        }

        private void OnDestroy()
        {
            GameManager.InputSystem.ui.drop.performed -= OnDropEquipment;
        }

        public void setSlectedFalse()
        {
            m_selected = false;
        }

        private void OnDropEquipment(InputAction.CallbackContext context)
        {
            //Debug.Log("OnDropEquipment");

            // 不晓得为什么GameManager.InventorySystem.HasItemInBag(m_item)放前面会空指针
            if (m_equipment  && m_selected)
            {
                GameManager.Player.equipments.TryGetValue(m_equipment.type, out Equipment toUnequip);

                // 调用丢弃物品的逻辑
                if (toUnequip != null)
                {
                    Debug.Log("toUnequip != null");

                    GameManager.NotificationSystem.OnEquipmentDiscarded?.Invoke(toUnequip, EItemLocation.Equipment);
                }
                //Clear(); // 清空物品槽
            }
        }

        private void OnSlotClicked()
        {
            if (m_equipment != null)
            {
                SendMessageUpwards("OnEquipmentItemClicked", m_equipment, SendMessageOptions.RequireReceiver);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_button.Select();
        }

        public void OnSelect(BaseEventData eventData)
        {
            m_selected = true;
            GameManager.NotificationSystem.itemDetailsOpened.Invoke(m_equipment);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            m_selected = false;
            GameManager.NotificationSystem.itemDetailsClosed.Invoke();
        }

        public void SetEquipment(Equipment equipment)
        {
            m_equipment = equipment;

            if (equipment)
            {
                Debug.Assert(equipment.type == m_equipmentType, "Equipment type mismatch");

                m_placeholder.enabled = false;
                m_content.enabled = true;
                m_content.sprite = equipment.Icon;
            }
            else
            {
                m_placeholder.enabled = true;
                m_content.enabled = false;
                m_content.sprite = null;
            }

            if (m_selected)
            {
                GameManager.NotificationSystem.itemDetailsOpened.Invoke(m_equipment);
            }
        }

    }
}
