using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIGameMenuEntry : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        public enum EGameMenuAction
        {
            None,
            OpenInventory,
            OpenJournal,
            OpenSaveMenu,
            OpenAbilities,
            OpenCharacter,
            OpenCraft,
            OpenSettings,
            GoToMainMenu
        }

        [Header("Settings")]
        [SerializeField] private EGameMenuAction m_action = EGameMenuAction.None;

        [Header("References")]
        [SerializeField] private Button m_button = null;
        [SerializeField] private TextMeshProUGUI m_text = null;

        private void Awake()
        {
            m_button.onClick.AddListener(OnButtonClicked);
            m_text.enabled = false;

            // Disable this menu entry if no "On The Go" CraftingStation has been provided
            if (m_action == EGameMenuAction.OpenCraft && GameManager.Config.onTheGoCraftingStation == null)
            {
                gameObject.SetActive(false);
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            m_text.enabled = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            m_text.enabled = true;
            SendMessageUpwards("OnGameMenuEntrySelected", m_button);
        }

        private void OnButtonClicked()
        {
            switch (m_action)
            {
                case EGameMenuAction.OpenJournal:
                    GameManager.NotificationSystem.journalRequested.Invoke();
                    break;

                case EGameMenuAction.OpenCharacter:
                    GameManager.NotificationSystem.statsRequested.Invoke();
                    break;

                case EGameMenuAction.OpenCraft:
                    Debug.Assert(GameManager.Config.onTheGoCraftingStation != null, "Cannot open the craft menu with a default recipe book defined in the game config!");
                    GameManager.NotificationSystem.craftRequested.Invoke(GameManager.Config.onTheGoCraftingStation);
                    break;

                case EGameMenuAction.OpenSaveMenu:
                    GameManager.NotificationSystem.saveMenuRequested.Invoke();
                    break;

                case EGameMenuAction.OpenInventory:
                    GameManager.NotificationSystem.inventoryRequested.Invoke();
                    break;

                case EGameMenuAction.OpenAbilities:
                    GameManager.NotificationSystem.spellBookRequested.Invoke();
                    break;

                case EGameMenuAction.OpenSettings:
                    GameManager.NotificationSystem.settingsRequested.Invoke();
                    break;

                case EGameMenuAction.GoToMainMenu:
                    // 不是战斗场景则保存数据
                    if(GameManager.TeleportLoadingSystem.currentMap == "Pilgrimage_Place")
                    {
                        GameManager.SaveSystem.SaveToFile(GameManager.SaveSystem.saveFileName);
                    }

                    SceneManager.LoadScene(GameManager.Config.mainMenuSceneName);
                    break;
            }
        }
    }
}
