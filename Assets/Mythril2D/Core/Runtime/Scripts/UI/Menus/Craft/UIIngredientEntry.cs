using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIIngredientEntry : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image m_image = null;
        [SerializeField] private TextMeshProUGUI m_name = null;
        [SerializeField] private TextMeshProUGUI m_quantity = null;

        [Header("Settings")]
        [SerializeField] private Color m_requirementMetColor;
        [SerializeField] private Color m_requirementNotMetColor;

        public void Initialize(Item item, int quantity)
        {
            int itemCountInBag = GameManager.InventorySystem.GetItemCount(item);

            Color textColor = itemCountInBag >= quantity ? m_requirementMetColor : m_requirementNotMetColor;

            m_image.sprite = item.Icon;

            m_name.text = item.DisplayName;
            m_quantity.text = $"{itemCountInBag}/{quantity}";

            m_name.color = textColor;
            m_quantity.color = textColor;
        }
    }
}
