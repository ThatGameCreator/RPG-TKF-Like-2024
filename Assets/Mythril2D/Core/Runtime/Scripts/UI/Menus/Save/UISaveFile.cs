using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Localization.Settings;

namespace Gyvr.Mythril2D
{
    enum SaveFileActionType
    {
        Save,
        Load
    }

    struct SaveFileActionDesc
    {
        public SaveFileActionType action;
        public string filename;
    }

    public class UISaveFile : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler
    {
        [Header("Settings")]
        [SerializeField] private SaveFileActionType m_action = SaveFileActionType.Load;
        [SerializeField] private string m_saveFileName = null;

        [Header("References")]
        [SerializeField] private TextMeshProUGUI m_details = null;
        [SerializeField] private Image m_image = null;
        [SerializeField] private Button m_button = null;
        [SerializeField] private Button m_cancelButton = null;
        [SerializeField] private UIMainMenu m_UIMainMenu = null;
        [SerializeField] private SaveFile m_saveFile = null;
        [SerializeField] private Color normalColor = new Color(200, 200, 200);
        [SerializeField] private Color highlightedColor = new Color(231, 200, 105);
        [SerializeField] private Color pressedColor = new Color(165, 143, 75);
        [SerializeField] private float fadeDuration = 0.1f;


        public string saveFileName => m_saveFileName;
        public Button button => m_button;
        public bool isEmpty => m_isEmpty;

        private bool m_isEmpty;

        private Coroutine imageAlphaCoroutine;
        private Coroutine colorCoroutine;

        private void Awake()
        {
            normalColor = new Color32(200, 200, 200, 255);
            highlightedColor = new Color32(231, 200, 105, 255);
            pressedColor = new Color32(199, 105, 75, 255);

            m_button.onClick.AddListener(OnClick);
        }

        private void Start()
        {
            // 注册语言切换事件
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

            // 初始化UI
            UpdateUI();
        }

        private void OnDestroy()
        {
            // 在销毁时解除注册，防止内存泄漏
            LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
        }

        // 语言切换事件的回调函数
        private void OnLanguageChanged(UnityEngine.Localization.Locale locale)
        {
            // 更新UI文本以适配新语言
            UpdateUI();
        }

        public void UpdateUI()
        {
            SaveFileData saveFile;

            // Each button try to check their Save Data
            // The button will show the Character name of Save data
            if (SaveSystem.TryExtractingSaveData(m_saveFileName, out saveFile))
            {
                m_details.text = LocalizationSettings.StringDatabase.GetLocalizedString("MainMenuTable", "id_continue_game_btn");
                m_isEmpty = false;
            }
            else
            {
                m_details.text = LocalizationSettings.StringDatabase.GetLocalizedString("MainMenuTable", "id_start_game_btn");
                m_isEmpty = true;

                if (m_cancelButton != null)
                {
                    m_cancelButton.gameObject.SetActive(false);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("OnPointerEnter");
            m_button.Select();
        }

        public void OnSelect(BaseEventData eventData)
        {
            // Start the fade to highlighted color
            StartColorTransition(highlightedColor);
            StartAlphaTransition(1f); // Image 渐入
        }

        public void OnDeselect(BaseEventData eventData)
        {
            // Start the fade to normal color
            StartColorTransition(normalColor);
            StartAlphaTransition(0f); // Image 渐出
        }

        private void StartColorTransition(Color targetColor)
        {
            if (colorCoroutine != null)
            {
                StopCoroutine(colorCoroutine);
            }

            colorCoroutine = StartCoroutine(FadeTextColor(targetColor));
        }

        private IEnumerator FadeTextColor(Color targetColor)
        {
            Color startColor = m_details.color;
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                m_details.color = Color.Lerp(startColor, targetColor, elapsedTime / fadeDuration);
                yield return null;
            }

            m_details.color = targetColor;
        }

        private void StartAlphaTransition(float targetAlpha)
        {
            if (imageAlphaCoroutine != null)
            {
                StopCoroutine(imageAlphaCoroutine);
            }

            imageAlphaCoroutine = StartCoroutine(FadeImageAlpha(targetAlpha));
        }

        private IEnumerator FadeImageAlpha(float targetAlpha)
        {
            if (m_image == null) yield break;

            Color startColor = m_image.color;
            Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                m_image.color = Color.Lerp(startColor, targetColor, elapsedTime / fadeDuration);
                yield return null;
            }

            m_image.color = targetColor;
        }

        public void OnClick()
        {
            StartColorTransition(pressedColor);

            //Debug.Log(saveFileName);

            // 没有存档尝试新建
            if (m_isEmpty == true)
            {
                m_UIMainMenu.StartNewGameFromDefaultSaveFile(m_saveFile, saveFileName);
            }
            else
            {
                SendMessageUpwards("OnSaveFileClicked", new SaveFileActionDesc
                {
                    action = m_action,
                    filename = m_saveFileName,
                    
                }, SendMessageOptions.RequireReceiver);
            }
        }
    }
}
