#if PACKAGE_UGUI

using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace UnityEngine.Localization.Samples
{
    /// <summary>
    /// 此脚本展示了如何使用UGUI的Toggle控件实现语言选择菜单。
    /// </summary>
    public class LanguageSelectionMenuUGUIToggleList : MonoBehaviour
    {
        // Toggle控件的容器，用于存放语言切换按钮
        public Transform container;

        // 用于创建语言切换按钮的预制体
        public GameObject languageTogglePrefab;

        // 异步操作句柄，用于跟踪语言初始化操作的状态
        AsyncOperationHandle m_InitializeOperation;

        // 存储每种语言对应的Toggle控件
        Dictionary<Locale, Toggle> m_Toggles = new Dictionary<Locale, Toggle>();

        // Toggle组，确保每次只能选择一个语言
        ToggleGroup m_ToggleGroup;

        void Start()
        {
            // 初始化语言设置，SelectedLocaleAsync确保可用语言加载完成并选择了默认语言
            m_InitializeOperation = LocalizationSettings.SelectedLocaleAsync;

            if (m_InitializeOperation.IsDone)
            {
                // 如果初始化已经完成，直接执行初始化后的逻辑
                InitializeCompleted(m_InitializeOperation);
            }
            else
            {
                // 如果未完成，则注册回调，当初始化完成时调用
                m_InitializeOperation.Completed += InitializeCompleted;
            }
        }

        void InitializeCompleted(AsyncOperationHandle obj)
        {
            // 订阅语言更改事件，确保UI和语言切换保持同步
            LocalizationSettings.SelectedLocaleChanged += LocalizationSettings_SelectedLocaleChanged;

            // 为容器添加ToggleGroup组件，确保每次只能选择一个语言
            m_ToggleGroup = container.gameObject.AddComponent<ToggleGroup>();

            // 获取所有可用语言
            var locales = LocalizationSettings.AvailableLocales.Locales;

            // 为每种语言创建一个Toggle
            for (int i = 0; i < locales.Count; ++i)
            {
                var locale = locales[i];

                // 实例化语言切换按钮
                var languageToggle = Instantiate(languageTogglePrefab, container);

                // 设置按钮名称为语言的本地名称，如果不可用则使用默认标识
                languageToggle.name = locale.Identifier.CultureInfo != null ? locale.Identifier.CultureInfo.NativeName : locale.ToString();

                // 更新按钮上的文本标签
                var label = languageToggle.GetComponentInChildren<Text>();
                label.text = languageToggle.name;

                // 获取按钮上的Toggle组件
                var toggle = languageToggle.GetComponent<Toggle>();

                // 初始化Toggle状态，若当前语言为选中语言，则勾选
                toggle.SetIsOnWithoutNotify(LocalizationSettings.SelectedLocale == locale);

                // 将Toggle存入字典，方便后续更新
                m_Toggles[locale] = toggle;

                // 添加监听器，当Toggle被选中时切换语言
                toggle.onValueChanged.AddListener(val =>
                {
                    if (val)
                    {
                        // 临时取消订阅，防止设置语言时触发额外的回调
                        LocalizationSettings.SelectedLocaleChanged -= LocalizationSettings_SelectedLocaleChanged;

                        // 设置为当前选中的语言
                        LocalizationSettings.SelectedLocale = locale;

                        // 重新订阅语言更改事件
                        LocalizationSettings.SelectedLocaleChanged += LocalizationSettings_SelectedLocaleChanged;
                    }
                });

                // 将Toggle添加到Toggle组中
                toggle.group = m_ToggleGroup;
            }
        }

        void LocalizationSettings_SelectedLocaleChanged(Locale locale)
        {
            // 当语言切换时，更新对应Toggle的选中状态
            if (m_Toggles.TryGetValue(locale, out var toggle))
            {
                toggle.SetIsOnWithoutNotify(true);
            }
        }
    }
}

#endif
