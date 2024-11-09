using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIInventoryBagCategory : MonoBehaviour, ISelectHandler
    {
        [Header("Settings")]
        [SerializeField] private Sprite m_defaultSprite = null;
        [SerializeField] private Sprite m_highlightSprite = null;

        [Header("References")]
        [SerializeField] private Image m_targetGraphic = null;
        [SerializeField] private Image m_icon = null;
        [SerializeField] private TextMeshProUGUI m_text = null;

        private EItemCategory m_category;

        public void SetCategory(EItemCategory category)
        {
            m_category = category;
            m_icon.sprite = GameManager.Config.GetTermDefinition(m_category).icon;
            m_text.text = GameManager.Config.GetTermDefinition(m_category).shortName;
        }

        public void SetHighlight(bool value)
        {
            m_targetGraphic.sprite = value ? m_highlightSprite : m_defaultSprite;
        }

        public void OnSelect(BaseEventData eventData)
        {
            GameManager.NotificationSystem.UICategorySelected.Invoke(m_category);
            //SendMessageUpwards("OnBagCategorySelected", m_category, SendMessageOptions.RequireReceiver);
        }
    }
}
