using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIInventoryBagSlot : MonoBehaviour, IItemSlotHandler, IPointerEnterHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Image m_image = null;
        [SerializeField] private TextMeshProUGUI m_quantity = null;
        [SerializeField] private Button m_button = null;

        public Button button => m_button;

        private Item m_item = null;               // ������Ʒ��Ϣ
        private ItemInstance m_itemInstance = null; // ������Ʒʵ����Ϣ
        private bool m_selected = false;
        private bool isPointerDown = false;
        private float pointerDownTimer = 0f;
        private bool longPressTriggered = false;
        private const float longPressThreshold = 0.2f; // 0.2s �ж϶̰��ͳ����ķֽ��

        private void Awake()
        {
            m_button.onClick.AddListener(OnSlotClicked);
        }

        private void Start()
        {
            GameManager.InputSystem.ui.drop.performed += OnDropItem;
        }

        private void OnDestroy()
        {
            GameManager.InputSystem.ui.drop.performed -= OnDropItem;
        }

        public void setSlectedFalse()
        {
            m_selected = false;
        }

        private void OnDropItem(InputAction.CallbackContext context)
        {
            // ������ΪʲôGameManager.InventorySystem.HasItemInBag(m_item)��ǰ����ָ��
            if (m_item != null && GameManager.InventorySystem.HasItemInBag(m_item) && m_selected)
            {
                // ���ö�����Ʒ���߼�
                if (GameManager.UIManagerSystem.UIMenu.inventory.isActiveAndEnabled)
                {
                    GameManager.NotificationSystem.OnBagItemDiscarded?.Invoke(m_itemInstance, EItemLocation.Bag);

                }
                else if (GameManager.UIManagerSystem.UIMenu.shop.isActiveAndEnabled)
                {
                    GameManager.NotificationSystem.OnShopItemDiscarded?.Invoke(m_itemInstance, EItemLocation.Bag);

                }
                //Clear(); // �����Ʒ��
            }
        }

        private void OnSlotClicked()
        {
            if (m_item != null) {
                SendMessageUpwards("OnBagItemClicked", m_item, SendMessageOptions.RequireReceiver);
            }
        }

        public void Clear() => SetItem(null, 0);

        public Item GetItem()
        {
            return m_item;
        }

        public int GetItemNumber()
        {
            return m_itemInstance == null ? 0 : m_itemInstance.quantity;
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
            // ֻ��ָ��ť��ʱ��ִ��ѡ�кͲ�ѡ��
            // ���ֱ�ӹرղ˵��������֮ǰ�ı�ѡ�а�ť����bool����������
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
                m_itemInstance = null; // ͬʱ���ʵ������
            }

            // �����λ��ѡ�У���ʾ��Ʒ����
            if (m_selected)
            {
                GameManager.NotificationSystem.itemDetailsOpened.Invoke(m_item);
            }
        }
    }
}
