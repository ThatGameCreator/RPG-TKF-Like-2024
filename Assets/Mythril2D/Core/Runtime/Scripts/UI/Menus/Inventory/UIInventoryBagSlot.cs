using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIInventoryBagSlot : MonoBehaviour, IItemSlotHandler, IPointerEnterHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Image m_image = null;
        [SerializeField] private TextMeshProUGUI m_quantity = null;
        [SerializeField] private Button m_button = null;

        public Button button => m_button;

        private string m_itemGUID = null; // 使用 GUID 代替直接的 Item 实例
        private Item m_item = null;
        private bool m_selected = false;

        // 清空槽位
        public void Clear() => SetItem(null, 0);

        // 获取当前物品，如果 GUID 有效
        public Item GetItem()
        {
            return string.IsNullOrEmpty(m_itemGUID) ? null : GameManager.Database.LoadItemByGUID(m_itemGUID); // 通过 GUID 获取 Item
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_button.Select();
        }

        public void OnSelect(BaseEventData eventData)
        {
            m_selected = true;
            // 通过 GUID 获取 Item，并触发详细信息显示
            if (!string.IsNullOrEmpty(m_itemGUID))
            {
                // 通过 GUID 获取 Item，并触发详细信息显示
                if (!string.IsNullOrEmpty(m_itemGUID))
                {
                    GameManager.NotificationSystem.itemDetailsOpened.Invoke(m_itemGUID); // 传递加载到的 Item
                }
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            m_selected = false;
            GameManager.NotificationSystem.itemDetailsClosed.Invoke();
        }

        public void SetItem(Item item, int quantity)
        {
            if (item != null)
            {
                m_itemGUID = GameManager.Database.DatabaseEntryToGUID(item); // 存储 GUID 而非 Item 实例
                m_quantity.text = quantity.ToString();
                m_image.enabled = true;
                m_image.sprite = item.icon;
            }
            else
            {
                m_image.enabled = false;
                m_quantity.text = string.Empty;
                m_itemGUID = null; // 清空 GUID
            }

            if (m_selected && !string.IsNullOrEmpty(m_itemGUID))
            {
                // 传递 GUID 给 itemDetailsOpened 事件
                GameManager.NotificationSystem.itemDetailsOpened.Invoke(m_itemGUID); // 传递 GUID 而不是 Item
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
                SendMessageUpwards("OnBagItemClicked", m_item, SendMessageOptions.RequireReceiver);
            }
        }
    }
}
