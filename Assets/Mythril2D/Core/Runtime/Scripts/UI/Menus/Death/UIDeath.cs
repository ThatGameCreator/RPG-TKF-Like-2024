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
        // �������治Ӧ���ܹ����� ������������Ϊ false �޷�pop
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

            // ��ʱ������� GameStateSystem ��������ģ����������������״̬���ܻ�һֱ���� UI ��
            // ������û�취�������������Ϊ��
            GameManager.GameStateSystem.AddLayer(EGameState.Gameplay);

            GameManager.TeleportLoadingSystem.RequestTransition(null, null, () => {
                // ����ָ�����Ѫ��
                // ����λ�õ����� ��tm currentStats ��ֻ���� ��Ȼ�ܹ� = ��û�޸��κν�ɫ���ϵ�����
                GameManager.Player.RecoverPlayerStats(isFullTORecover);
            },
            () => {
                // ��Ҳ�ָ�����
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
            // ����ս�������򱣴�����
            if (GameManager.TeleportLoadingSystem.currentMap == "Pilgrimage_Place")
            {
                GameManager.SaveSystem.SaveToFile(GameManager.SaveSystem.saveFileName);
            }

            SceneManager.LoadScene(GameManager.Config.mainMenuSceneName);
        }
    }
}
