using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

namespace Gyvr.Mythril2D
{
    public class UIShopEntry : MonoBehaviour, IItemSlotHandler, IPointerEnterHandler, ISelectHandler, IDeselectHandler
    {
        [Header("References")]
        [SerializeField] private Image m_image = null;
        [SerializeField] private TextMeshProUGUI m_name = null;
        [SerializeField] private TextMeshProUGUI m_price = null;
        [SerializeField] private Button m_button = null;

        public Button button => m_button;

        //private Item m_target = null;
        private string m_itemGUID = null; // 存储物品的 GUID


        private void Awake()
        {
            m_button.onClick.AddListener(OnSlotClicked);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_button.Select();
        }

        public void OnSelect(BaseEventData eventData)
        {
            // 通过 GUID 获取 Item，并触发详细信息显示
            if (!string.IsNullOrEmpty(m_itemGUID))
            {
                // 传递 GUID 给 itemDetailsOpened 事件
                GameManager.NotificationSystem.itemDetailsOpened.Invoke(m_itemGUID); // 传递 GUID 而不是 Item
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            GameManager.NotificationSystem.itemDetailsClosed.Invoke();
        }

        public void OnSlotClicked()
        {
            if (!string.IsNullOrEmpty(m_itemGUID))
            {
                // 通过 GUID 获取物品实例
                Item item = GameManager.Database.LoadItemByGUID(m_itemGUID);
                if (item != null)
                {
                    // 发送物品点击事件，传递物品实例
                    SendMessageUpwards("OnWarehouseItemClicked", item, SendMessageOptions.RequireReceiver);
                }
            }
        }

        public void Initialize(string itemGUID)
        {
            m_itemGUID = itemGUID; // 存储 GUID 而不是直接存储 Item 实例

            // 通过 GUID 加载 Item 实例
            Item item = GameManager.Database.LoadItemByGUID(itemGUID);

            if (item != null)
            {
                m_name.text = item.displayName;
                m_price.text = item.price.ToString();
                m_image.sprite = item.icon;
            }
        }

        public Item GetItem()
        {
            return string.IsNullOrEmpty(m_itemGUID) ? null : GameManager.Database.LoadItemByGUID(m_itemGUID); // 通过 GUID 获取 Item
        }
    }
}
