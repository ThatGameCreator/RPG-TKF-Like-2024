using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageSwitcher : MonoBehaviour
{
    // 切换语言的方法
    public void SwitchLanguage(int languageIndex)
    {
        // 获取支持的语言列表并切换
        if (languageIndex >= 0 && languageIndex < LocalizationSettings.AvailableLocales.Locales.Count)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[languageIndex];
            Debug.Log($"语言切换至：{LocalizationSettings.SelectedLocale.Identifier}");
        }
        else
        {
            Debug.LogWarning("无效的语言索引！");
        }
    }
}
