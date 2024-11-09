using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIDeath : MonoBehaviour, IUIMenu
    {
        [Header("References")]
        [SerializeField] private Button m_revivalButton = null;
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

        public void RevivePlayer()
        {
            GameManager.UIManagerSystem.UIMenu.PopDeathMenu();

            GameManager.DayNightSystem.OnDisableDayNightSystem();

            // 暂时不懂这个 GameStateSystem 是来干嘛的，但是如果不添加这个状态可能会一直卡在 UI 层
            // 而导致没办法控制人物各种行为？
            GameManager.GameStateSystem.AddLayer(EGameState.Gameplay);

            GameManager.Player.TryPlayRevivalAnimation();
        }

        public void GoToMainMenu()
        {
            SceneManager.LoadScene(GameManager.Config.mainMenuSceneName);
        }
    }
}
