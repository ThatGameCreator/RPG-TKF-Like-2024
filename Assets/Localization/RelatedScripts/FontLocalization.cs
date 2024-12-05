using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;

public class FontLocalization : MonoBehaviour
{
    public TMP_Text textMeshPro;  // 绑定需要切换字体的 TextMeshPro 组件
    public string tableName = "FontAssets"; // Asset Table 的名称
    public string fontKey = "mainFont";     // 字体键值
    public float defaultFontSize = 28f;     // 默认字号
    public float englishFontSize = 28f;     // 英文字号
    public float chineseFontSize = 19f;     // 中文字号

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
        LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<TMP_FontAsset>(tableName, fontKey).Completed += handle =>
        {
            if (handle.Result != null)
            {
                textMeshPro.font = handle.Result; // 应用字体
            }
            else
            {
                Debug.LogError($"Font not found in Asset Table '{tableName}' with key '{fontKey}'!");
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
            textMeshPro.fontSize = englishFontSize;
        }
        else if (localeCode == "zh-Hans")
        {
            textMeshPro.fontSize = chineseFontSize;
        }
        else
        {
            // 默认字号
            textMeshPro.fontSize = defaultFontSize;
        }
    }
}
