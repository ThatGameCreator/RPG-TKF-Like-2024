using TMPro;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
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
                m_itemName.text = LocalizationSystem.Instance.GetItemNameLocalizedString(item.LocalizationKey, item.Category);

                // 比较两个字符串，如果两个字符串不相等返回-1，两个相等则返回0。
                //if (string.Compare(item.DescriptionKey, string.Empty) == 0)
                if (item.DescriptionKey.Length > 1)
                {
                    m_itemDescription.text = LocalizationSystem.Instance.GetItemDescriptionLocalizedString(item.DescriptionKey);
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
                        EStat stat = (EStat)i;
                        int value = equipment.bonusStats[stat];

                        if (value != 0)
                        {
                            m_itemDescription.text += 
                            $"\n<u>{(value > 0 ? '+' : '-')}{value}\u00A0" +
                            $"{LocalizationSystem.Instance.GetStatsTermDefinitionLocalizedString(GameManager.Config.GetTermDefinition(stat).fullName)}</u>";
                        }
                    }

                    if(equipment.stamina != 0)
                    {
                        m_itemDescription.text += $"\n<u>{(equipment.stamina > 0 ? '+' : '-')}{equipment.stamina}\u00A0{LocalizationSystem.Instance.GetStatsTermDefinitionLocalizedString("id_term_definition_stamina")}</u>"; ;
                    }

                    if(equipment.capacity != 0)
                    {
                        m_itemDescription.text += $"\n<u>{(equipment.capacity > 0 ? '+' : '-')}{equipment.capacity}\u00A0{LocalizationSystem.Instance.GetStatsTermDefinitionLocalizedString("id_term_definition_capacity")}</u>"; ;
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
