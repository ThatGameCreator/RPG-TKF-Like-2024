using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIInventoryEquipmentSlot : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private EEquipmentType m_equipmentType = EEquipmentType.Head;
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

            // ������ΪʲôGameManager.InventorySystem.HasItemInBag(m_item)��ǰ����ָ��
            if (m_equipment  && m_selected)
            {
                GameManager.Player.equipments.TryGetValue(m_equipment.type, out Equipment toUnequip);

                // ���ö�����Ʒ���߼�
                if (toUnequip != null)
                {
                    Debug.Log("toUnequip != null");

                    GameManager.NotificationSystem.OnEquipmentDiscarded?.Invoke(toUnequip, EItemLocation.Equipment);
                }
                //Clear(); // �����Ʒ��
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

                m_content.enabled = true;
                m_content.sprite = equipment.Icon;
            }
            else
            {
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
