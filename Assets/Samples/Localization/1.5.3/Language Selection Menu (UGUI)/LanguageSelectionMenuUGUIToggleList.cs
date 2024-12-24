#if PACKAGE_UGUI

using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace UnityEngine.Localization.Samples
{
    /// <summary>
    /// �˽ű�չʾ�����ʹ��UGUI��Toggle�ؼ�ʵ������ѡ��˵���
    /// </summary>
    public class LanguageSelectionMenuUGUIToggleList : MonoBehaviour
    {
        // Toggle�ؼ������������ڴ�������л���ť
        public Transform container;

        // ���ڴ��������л���ť��Ԥ����
        public GameObject languageTogglePrefab;

        // �첽������������ڸ������Գ�ʼ��������״̬
        AsyncOperationHandle m_InitializeOperation;

        // �洢ÿ�����Զ�Ӧ��Toggle�ؼ�
        Dictionary<Locale, Toggle> m_Toggles = new Dictionary<Locale, Toggle>();

        // Toggle�飬ȷ��ÿ��ֻ��ѡ��һ������
        ToggleGroup m_ToggleGroup;

        void Start()
        {
            // ��ʼ���������ã�SelectedLocaleAsyncȷ���������Լ�����ɲ�ѡ����Ĭ������
            m_InitializeOperation = LocalizationSettings.SelectedLocaleAsync;

            if (m_InitializeOperation.IsDone)
            {
                // �����ʼ���Ѿ���ɣ�ֱ��ִ�г�ʼ������߼�
                InitializeCompleted(m_InitializeOperation);
            }
            else
            {
                // ���δ��ɣ���ע��ص�������ʼ�����ʱ����
                m_InitializeOperation.Completed += InitializeCompleted;
            }
        }

        void InitializeCompleted(AsyncOperationHandle obj)
        {
            // �������Ը����¼���ȷ��UI�������л�����ͬ��
            LocalizationSettings.SelectedLocaleChanged += LocalizationSettings_SelectedLocaleChanged;

            // Ϊ�������ToggleGroup�����ȷ��ÿ��ֻ��ѡ��һ������
            m_ToggleGroup = container.gameObject.AddComponent<ToggleGroup>();

            // ��ȡ���п�������
            var locales = LocalizationSettings.AvailableLocales.Locales;

            // Ϊÿ�����Դ���һ��Toggle
            for (int i = 0; i < locales.Count; ++i)
            {
                var locale = locales[i];

                // ʵ���������л���ť
                var languageToggle = Instantiate(languageTogglePrefab, container);

                // ���ð�ť����Ϊ���Եı������ƣ������������ʹ��Ĭ�ϱ�ʶ
                languageToggle.name = locale.Identifier.CultureInfo != null ? locale.Identifier.CultureInfo.NativeName : locale.ToString();

                // ���°�ť�ϵ��ı���ǩ
                var label = languageToggle.GetComponentInChildren<Text>();
                label.text = languageToggle.name;

                // ��ȡ��ť�ϵ�Toggle���
                var toggle = languageToggle.GetComponent<Toggle>();

                // ��ʼ��Toggle״̬������ǰ����Ϊѡ�����ԣ���ѡ
                toggle.SetIsOnWithoutNotify(LocalizationSettings.SelectedLocale == locale);

                // ��Toggle�����ֵ䣬�����������
                m_Toggles[locale] = toggle;

                // ��Ӽ���������Toggle��ѡ��ʱ�л�����
                toggle.onValueChanged.AddListener(val =>
                {
                    if (val)
                    {
                        // ��ʱȡ�����ģ���ֹ��������ʱ��������Ļص�
                        LocalizationSettings.SelectedLocaleChanged -= LocalizationSettings_SelectedLocaleChanged;

                        // ����Ϊ��ǰѡ�е�����
                        LocalizationSettings.SelectedLocale = locale;

                        // ���¶������Ը����¼�
                        LocalizationSettings.SelectedLocaleChanged += LocalizationSettings_SelectedLocaleChanged;
                    }
                });

                // ��Toggle��ӵ�Toggle����
                toggle.group = m_ToggleGroup;
            }
        }

        void LocalizationSettings_SelectedLocaleChanged(Locale locale)
        {
            // �������л�ʱ�����¶�ӦToggle��ѡ��״̬
            if (m_Toggles.TryGetValue(locale, out var toggle))
            {
                toggle.SetIsOnWithoutNotify(true);
            }
        }
    }
}

#endif
