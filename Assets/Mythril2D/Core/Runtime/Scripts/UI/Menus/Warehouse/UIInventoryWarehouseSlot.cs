using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIInventoryWarehouseSlot : MonoBehaviour, IItemSlotHandler, IPointerEnterHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Image m_image = null;
        [SerializeField] private TextMeshProUGUI m_quantity = null;
        [SerializeField] private Button m_button = null;

        public Button button => m_button;

        private Item m_item = null;               // ������Ʒ��Ϣ
        private ItemInstance m_itemInstance = null; // ������Ʒʵ����Ϣ
        private bool m_selected = false;

        private void Start()
        {
            GameManager.InputSystem.ui.drop.performed += OnDropItem;
        }

        private void OnDestroy()
        {
            GameManager.InputSystem.ui.drop.performed -= OnDropItem;
        }

        private void Awake()
        {
            m_button.onClick.AddListener(OnSlotClicked);
        }

        public void setSlectedFalse()
        {
            m_selected = false;
        }

        private void OnDropItem(InputAction.CallbackContext context)
        {
            if (m_item != null && m_selected)
            {
                // ���ö�����Ʒ���߼�
                GameManager.NotificationSystem.OnWarehouseItemDiscarded?.Invoke(m_itemInstance, EItemLocation.Warehouse);
                //Clear(); // �����Ʒ��
            }
        }

        private void OnSlotClicked()
        {
            if (m_item != null)
            {
                SendMessageUpwards("OnWarehouseItemClicked", m_item, SendMessageOptions.RequireReceiver);
            }
        }

        public void Clear() => SetItem(null, 0);

        public Item GetItem()
        {
            return m_item;
        }

        public ItemInstance GetItemInstance()
        {
            return m_itemInstance;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_button.Select();
        }

        public void OnSelect(BaseEventData eventData)
        {
            m_selected = true;
            GameManager.NotificationSystem.itemDetailsOpened.Invoke(m_item);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            m_selected = false;
            GameManager.NotificationSystem.itemDetailsClosed.Invoke();
        }

        public void SetItem(ItemInstance itemInstance)
        {
            if (itemInstance != null)
            {
                SetItem(itemInstance.GetItem(), itemInstance.quantity);
                m_itemInstance = itemInstance;
            }
            else
            {
                Clear();
            }
        }

        public void SetItem(Item item, int quantity)
        {
            if (item != null)
            {
                m_item = item;

                // ��� m_itemInstance Ϊ�գ����½�һ��ʵ��
                if (m_itemInstance == null)
                {
                    m_itemInstance = new ItemInstance(item, quantity);
                }
                else
                {
                    // �� m_itemInstance ��ֵ
                    m_itemInstance.itemReference = GameManager.Database.CreateReference(item);
                    m_itemInstance.quantity = quantity;
                }

                // ����Ƕѵ���Ʒ����ʾ����������ֻ��ʾ��Ʒͼ��
                m_quantity.text = item.IsStackable ? quantity.ToString() : string.Empty;

                m_image.enabled = true;
                m_image.sprite = item.Icon;
            }
            else
            {
                // ���û����Ʒ����ղ�λ
                m_image.enabled = false;
                m_quantity.text = string.Empty;
                m_item = null;
            }

            // �����λ��ѡ�У���ʾ��Ʒ����
            if (m_selected)
            {
                GameManager.NotificationSystem.itemDetailsOpened.Invoke(m_item);
            }
        }
    }
}
