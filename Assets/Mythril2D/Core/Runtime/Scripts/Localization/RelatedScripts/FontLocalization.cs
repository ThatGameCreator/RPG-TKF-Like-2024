using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Localization.Tables;

public class FontLocalization : MonoBehaviour
{
    // 私有字段
    [SerializeField] private TMP_Text m_textMeshPro;
    [SerializeField] private string m_tableName = "FontAssets"; // Asset Table 的名称
    [SerializeField] private string m_fontKey = "mainFont";     // 字体键值
    [SerializeField] private float m_defaultFontSize = 28f;     // 默认字号
    [SerializeField] private float m_englishFontSize = 28f;     // 英文字号
    [SerializeField] private float m_chineseFontSize = 19f;     // 中文字号


    // 公有属性 (Getter and Setter)
    public TMP_Text TextMeshPro
    {
        get
        {
            return m_textMeshPro;
        }
        set
        {
            m_textMeshPro = value;
        }
    }

    public string TableName
    {
        get
        {
            return m_tableName;
        }
        set
        {
            m_tableName = value;
        }
    }

    public string FontKey
    {
        get
        {
            return m_fontKey;
        }
        set
        {
            m_fontKey = value;
        }
    }

    public float DefaultFontSize
    {
        get
        {
            return m_defaultFontSize;
        }
        set
        {
            m_defaultFontSize = value;
        }
    }

    public float EnglishFontSize
    {
        get
        {
            return m_englishFontSize;
        }
        set
        {
            m_englishFontSize = value;
        }
    }

    public float ChineseFontSize
    {
        get
        {
            return m_chineseFontSize;
        }
        set
        {
            m_chineseFontSize = value;
        }
    }

    // 可选的辅助方法，例如更改字体
    public void ChangeFontSize(float newSize)
    {
        if (m_textMeshPro != null)
        {
            m_textMeshPro.fontSize = newSize;
        }
    }

    private void Start()
    {
        // 初始化：根据当前语言设置字体
        UpdateFontAndSizeForCurrentLocale();

        // 监听语言切换事件
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDestroy()
    {
        // 移除监听器
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale newLocale)
    {
        // 更新字体
        UpdateFontAndSizeForCurrentLocale();
    }

    private void UpdateFontAndSizeForCurrentLocale()
    {
        // 获取当前语言对应的字体
        LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<TMP_FontAsset>(m_tableName, m_fontKey).Completed += handle =>
        {
            if (handle.Result != null)
            {
                m_textMeshPro.font = handle.Result; // 应用字体
            }
            else
            {
                Debug.LogError($"Font not found in Asset Table '{m_tableName}' with key '{m_fontKey}'!");
            }
        };
        // 根据当前语言设置字号
        SetFontSizeBasedOnLocale(LocalizationSettings.SelectedLocale.Identifier.Code);
    }

    private void SetFontSizeBasedOnLocale(string localeCode)
    {
        // 设置不同语言的字号
        if (localeCode == "en")
        {
            m_textMeshPro.fontSize = m_englishFontSize;
        }
        else if (localeCode == "zh-Hans")
        {
            m_textMeshPro.fontSize = m_chineseFontSize;
        }
        else if (localeCode == "ja")
        {
            m_textMeshPro.fontSize = m_chineseFontSize;
        }
        else
        {
            // 默认字号
            m_textMeshPro.fontSize = m_defaultFontSize;
        }
    }
}
