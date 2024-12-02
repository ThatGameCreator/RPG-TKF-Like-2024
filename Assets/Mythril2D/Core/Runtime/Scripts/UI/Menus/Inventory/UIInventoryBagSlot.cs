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

        private Item m_item = null;               // 基础物品信息
        private ItemInstance m_itemInstance = null; // 具体物品实例信息
        private bool m_selected = false;
        private bool isPointerDown = false;
        private float pointerDownTimer = 0f;
        private bool longPressTriggered = false;
        private const float longPressThreshold = 0.2f; // 0.2s 判断短按和长按的分界点

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
            // 不晓得为什么GameManager.InventorySystem.HasItemInBag(m_item)放前面会空指针
            if (m_item != null && GameManager.InventorySystem.HasItemInBag(m_item) && m_selected)
            {
                // 调用丢弃物品的逻辑
                if (GameManager.UIManagerSystem.UIMenu.inventory.isActiveAndEnabled)
                {
                    GameManager.NotificationSystem.OnBagItemDiscarded?.Invoke(m_itemInstance, EItemLocation.Bag);

                }
                else if (GameManager.UIManagerSystem.UIMenu.shop.isActiveAndEnabled)
                {
                    GameManager.NotificationSystem.OnShopItemDiscarded?.Invoke(m_itemInstance, EItemLocation.Bag);

                }
                //Clear(); // 清空物品槽
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
            // 只有指向按钮的时候执行选中和不选中
            // 如果直接关闭菜单，不会对之前的被选中按钮进行bool变量的消除
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
                m_quantity.text = item.IsStackable ? quantity.ToString() : string.Empty;

                m_image.enabled = true;
                m_image.sprite = item.Icon;
            }
            else
            {
                // 如果没有物品，清空槽位
                m_image.enabled = false;
                m_quantity.text = string.Empty;
                m_item = null;
                m_itemInstance = null; // 同时清除实例数据
            }

            // 如果槽位被选中，显示物品详情
            if (m_selected)
            {
                GameManager.NotificationSystem.itemDetailsOpened.Invoke(m_item);
            }
        }
    }
}
