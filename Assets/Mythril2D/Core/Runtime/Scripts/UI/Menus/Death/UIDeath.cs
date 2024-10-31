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

        public bool CanPop() => false;

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
            Hide();

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
