using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIItemDetails : MonoBehaviour
    {
        // Inspector Settings
        [Header("References")]
        [SerializeField] private GameObject m_itemDetailsBox = null;
        [SerializeField] private Image m_itemIcon = null;
        [SerializeField] private TextMeshProUGUI m_itemName = null;
        [SerializeField] private TextMeshProUGUI m_itemDescription = null;

        private void Start()
        {
            m_itemDetailsBox.SetActive(false);

            GameManager.NotificationSystem.itemDetailsOpened.AddListener(OnDetailsOpened);
            GameManager.NotificationSystem.itemDetailsClosed.AddListener(OnDetailsClosed);
        }

        private void OnDetailsOpened(Item item)
        {
            //Debug.Log("OnDetailsOpened");

            if (item)
            {
                m_itemDetailsBox.SetActive(true);
                m_itemIcon.sprite = item.Icon;
                m_itemName.text = GameManager.LocalizationSystem.GetItemNameLocalizedString(item.LocalizationKey, item.Category);

                // 比较两个字符串，如果两个字符串不相等返回-1，两个相等则返回0。
                if (string.Compare(item.DescriptionKey, string.Empty) == -1)
                {
                    m_itemDescription.text = GameManager.LocalizationSystem.GetItemDescriptionLocalizedString(item.DescriptionKey);
                }
                else
                {
                    m_itemDescription.text = string.Empty;
                }

                if (item is Equipment)
                {
                    Equipment equipment = (Equipment)item;

                    for (int i = 0; i < Stats.StatCount; i++)
                    {
                        // 精力值和容量还没做进来
                        EStat stat = (EStat)i;
                        int value = equipment.bonusStats[stat];

                        if (value != 0)
                        {
                            m_itemDescription.text += 
                            $" <u>{(value > 0 ? '+' : '-')}{value}\u00A0" +
                            $"{GameManager.LocalizationSystem.GetStatsTermDefinitionLocalizedString(GameManager.Config.GetTermDefinition(stat).fullName)}</u>";
                        }
                    }
                }
            }
            else
            {
                OnDetailsClosed();
            }
        }

        private void OnDetailsClosed()
        {
            m_itemDetailsBox.SetActive(false);
        }
    }
}
