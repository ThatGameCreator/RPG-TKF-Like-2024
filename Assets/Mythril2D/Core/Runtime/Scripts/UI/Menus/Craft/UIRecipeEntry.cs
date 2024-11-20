using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIRecipeEntry : MonoBehaviour, IItemSlotHandler, IPointerEnterHandler, ISelectHandler, IDeselectHandler
    {
        [Header("References")]
        [SerializeField] private Image m_icon = null;
        [SerializeField] private TextMeshProUGUI m_name = null;
        [SerializeField] private Image m_status = null;
        [SerializeField] private Button m_button = null;

        [Header("Settings")]
        [SerializeField] private Sprite m_canCraftStatusIcon = null;
        [SerializeField] private Sprite m_cannotCraftStatusIcon = null;

        public Button button => m_button;

        private Recipe m_recipe = null;
        private CraftingStation m_craftingStation = null;

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
            if (m_recipe != null && m_recipe.item != null)
            {
                // 获取物品的 GUID
                string itemGUID = GameManager.Database.DatabaseEntryToGUID(m_recipe.item);
                // 触发事件，传递 GUID 而不是 Item
                GameManager.NotificationSystem.itemDetailsOpened.Invoke(itemGUID);
            }

            // 继续处理其他逻辑（如果需要）
            SendMessageUpwards("OnRecipeEntrySelected", m_recipe, SendMessageOptions.RequireReceiver);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            GameManager.NotificationSystem.itemDetailsClosed.Invoke();
            SendMessageUpwards("OnRecipeEntryDeselected", m_recipe, SendMessageOptions.RequireReceiver);
        }

        public void OnSlotClicked()
        {
            if (m_recipe != null)
            {
                SendMessageUpwards("OnRecipeEntryClicked", m_recipe, SendMessageOptions.RequireReceiver);
            }
        }

        public void Initialize(Recipe recipe, CraftingStation craftingStation)
        {
            m_recipe = recipe;
            m_craftingStation = craftingStation;
            m_name.text = recipe.displayName;
            m_icon.sprite = recipe.icon;

            UpdateUI();
        }

        public void UpdateUI()
        {
            bool canCraft = m_craftingStation.CanCraft(m_recipe, out bool hasMoney, out bool hasIngredients);
            m_status.sprite = canCraft ? m_canCraftStatusIcon : m_cannotCraftStatusIcon;
        }

        public Item GetItem()
        {
            return m_recipe?.item;
        }
    }
}
