using System;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIDeath : MonoBehaviour, IUIMenu
    {
        [Header("References")]
        [SerializeField] private Button m_revivalButton = null;
        [SerializeField] private Button m_payToRevivalButton = null;
        [SerializeField] private Button m_quitButton = null;

        public void Init()
        {
        }

        public void Show(params object[] args)
        {
            gameObject.SetActive(true);
        }

        //public bool CanPop() => false;
        // 死亡界面不应该能够回退 所以这里设置为 false 无法pop
        public bool CanPop(){
            return false;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void EnableInteractions(bool enable)
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup)
            {
                canvasGroup.interactable = enable;
            }
        }

        public GameObject FindSomethingToSelect()
        {
            return m_revivalButton.gameObject;
            //return m_quitButton.gameObject;
        }

        private void ReviveFunction(bool isFullTORecover = false)
        {
            GameManager.UIManagerSystem.UIMenu.ClearMenuStackOnDeath();

            GameManager.DayNightSystem.OnDisableDayNightSystem();

            // 暂时不懂这个 GameStateSystem 是来干嘛的，但是如果不添加这个状态可能会一直卡在 UI 层
            // 而导致没办法控制人物各种行为？
            GameManager.GameStateSystem.AddLayer(EGameState.Gameplay);

            GameManager.TeleportLoadingSystem.RequestTransition(null, null, () => {
                // 这里恢复不了血量
                // 不是位置的问题 是tm currentStats 是只读的 虽然能够 = 但没修改任何角色身上的数据
                GameManager.Player.RecoverPlayerStats(isFullTORecover);
            },
            () => {
                // 这也恢复不了
                GameManager.Player.TryPlayRevivalAnimation();
            }, ETeleportType.Revival);
        }

        public void RevivePlayer()
        {
            ReviveFunction();
        }

        public void PayToRevivePlayer()
        {
            if (GameManager.WarehouseSystem.HasSufficientFunds(25))
            {
                GameManager.WarehouseSystem.RemoveMoney(25);

                ReviveFunction(true);
            }
            else
            {
                GameManager.DialogueSystem.Main.PlayNow
                (LocalizationSettings.StringDatabase.GetLocalizedString("NPCDialogueTable", "id_dialogue_Death_can_not_pay"));
            }
        }

        public void GoToMainMenu()
        {
            // 不是战斗场景则保存数据
            if (GameManager.TeleportLoadingSystem.currentMap == "Pilgrimage_Place")
            {
                GameManager.SaveSystem.SaveToFile(GameManager.SaveSystem.saveFileName);
            }

            SceneManager.LoadScene(GameManager.Config.mainMenuSceneName);
        }
    }
}
