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

        private void Awake()
        {
            if (m_button != null)
            {
                m_button.onClick.AddListener(OnClick);
            }
        }
        public void OnClick()
        {
            SwitchLanguage();
        }

        public void SwitchLanguage()
        {
            // 切换到下一个语言
            LocalizationSystem.Instance.currentLocaleIndex = (LocalizationSystem.Instance.currentLocaleIndex + 1) % LocalizationSystem.Instance.locales.Count; // 循环更新语言索引
            LocalizationSettings.SelectedLocale = LocalizationSystem.Instance.locales[LocalizationSystem.Instance.currentLocaleIndex]; // 更新语言
        }
    }
}
