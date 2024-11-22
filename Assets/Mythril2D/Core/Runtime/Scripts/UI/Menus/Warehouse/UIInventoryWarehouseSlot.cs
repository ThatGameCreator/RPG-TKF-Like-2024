using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIInventoryWarehouseSlot : MonoBehaviour, IItemSlotHandler, IPointerEnterHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Image m_image = null;
        [SerializeField] private TextMeshProUGUI m_quantity = null;
        [SerializeField] private Button m_button = null;

        public Button button => m_button;

        private Item m_item = null;               // 基础物品信息
        private ItemInstance m_itemInstance = null; // 具体物品实例信息
        private bool m_selected = false;

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

                // 如果 m_itemInstance 为空，则新建一个实例
                if (m_itemInstance == null)
                {
                    m_itemInstance = new ItemInstance(item, quantity);
                }
                else
                {
                    // 给 m_itemInstance 赋值
                    m_itemInstance.itemReference = GameManager.Database.CreateReference(item);
                    m_itemInstance.quantity = quantity;
                }

                // 如果是堆叠物品，显示数量；否则只显示物品图标
                m_quantity.text = item.isStackable ? quantity.ToString() : string.Empty;

                m_image.enabled = true;
                m_image.sprite = item.icon;
            }
            else
            {
                // 如果没有物品，清空槽位
                m_image.enabled = false;
                m_quantity.text = string.Empty;
                m_item = null;
            }

            // 如果槽位被选中，显示物品详情
            if (m_selected)
            {
                GameManager.NotificationSystem.itemDetailsOpened.Invoke(m_item);
            }

        }

        private void Awake()
        {
            m_button.onClick.AddListener(OnSlotClicked);
        }

        private void OnSlotClicked()
        {
            if (m_item != null)
            {
                SendMessageUpwards("OnWarehouseItemClicked", m_item, SendMessageOptions.RequireReceiver);
            }
        }
    }
}
