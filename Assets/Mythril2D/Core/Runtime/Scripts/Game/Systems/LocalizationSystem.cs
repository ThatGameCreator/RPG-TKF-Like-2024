using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace Gyvr.Mythril2D
{
    public class LocalizationSystem : AGameSystem
    {
        private StringTable ScriptStringTable;          //代码本地化表

        void Start()
        {
            if (LocalizationSettings.AvailableLocales.Locales.Count > 0)
            {
                GetLocalizationTable();
            }
            else
            {
                LocalizationSettings.InitializationOperation.Completed += OnLocalizationInitialized;
            }
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        protected void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }

        /// <summary>
        /// 本地化初始化完成
        /// </summary>
        private void OnLocalizationInitialized(AsyncOperationHandle<LocalizationSettings> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("Localization initialized successfully!");
                GetLocalizationTable();
            }
            else
            {
                Debug.LogError("Localization initialization failed.");
            }
        }

        /// <summary>
        /// 切换语言
        /// </summary>
        private void OnLocaleChanged(Locale newLocale)
        {
            GetLocalizationTable();
        }

        /// <summary>
        /// 获取本地化表
        /// </summary>
        public void GetLocalizationTable()
        {
            ScriptStringTable = LocalizationSettings.StringDatabase.GetTable("ScriptLocalization");
            //Debug.LogWarning(ScriptStringTable.GetEntry("CommonTip_NoItem").GetLocalizedString());
        }

        /// <summary>
        /// 获取本地化文本
        /// </summary>
        public string GetLocalizedString(string key)
        {
            return ScriptStringTable.GetEntry(key).GetLocalizedString();
        }
    }
}
