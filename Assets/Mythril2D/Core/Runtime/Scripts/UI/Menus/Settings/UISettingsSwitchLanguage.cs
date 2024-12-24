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
            // �л�����һ������
            LocalizationSystem.Instance.currentLocaleIndex = (LocalizationSystem.Instance.currentLocaleIndex + 1) % LocalizationSystem.Instance.locales.Count; // ѭ��������������
            LocalizationSettings.SelectedLocale = LocalizationSystem.Instance.locales[LocalizationSystem.Instance.currentLocaleIndex]; // ��������
        }
    }
}
