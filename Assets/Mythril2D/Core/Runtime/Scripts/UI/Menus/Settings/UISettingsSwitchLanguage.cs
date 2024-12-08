using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

namespace Gyvr.Mythril2D
{
    public class UISettingsSwitchLanguage : MonoBehaviour
    {
        [SerializeField] private Button m_button = null;

        private Coroutine imageAlphaCoroutine;
        private Coroutine colorCoroutine;

        public Button button => m_button;
        private int currentLocaleIndex = 0; // 当前语言索引

        private void Awake()
        {
            if (m_button != null)
            {
                m_button.onClick.AddListener(OnClick);
            }
        }
        private void Start()
        {
            // 初始化当前语言索引
            if (LocalizationSettings.SelectedLocale != null)
            {
                var locales = LocalizationSettings.AvailableLocales.Locales;
                currentLocaleIndex = locales.IndexOf(LocalizationSettings.SelectedLocale);
            }
        }

        public void OnClick()
        {
            SwitchLanguage();
        }

        public void SwitchLanguage()
        {
            var locales = LocalizationSettings.AvailableLocales.Locales; // 获取所有可用语言列表

            if (locales.Count == 0)
            {
                Debug.LogWarning("No available locales found!");
                return;
            }

            // 切换到下一个语言
            currentLocaleIndex = (currentLocaleIndex + 1) % locales.Count; // 循环更新语言索引
            LocalizationSettings.SelectedLocale = locales[currentLocaleIndex]; // 更新语言
        }
    }
}
