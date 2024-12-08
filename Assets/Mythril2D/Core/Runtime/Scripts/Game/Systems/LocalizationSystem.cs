using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace Gyvr.Mythril2D
{
    public enum EMenuStringTableType
    {
        MainMenu,
    }

    public class LocalizationSystem : AGameSystem
    {
        //代码本地化表
        private StringTable m_EquipmentsStringTable;          
        private StringTable m_WeaponsStringTable;          
        private StringTable m_MaterialsStringTable;          
        private StringTable m_MonsterDropsStringTable;          
        private StringTable m_ConsumersStringTable;     
        
        private StringTable m_MainMenuStringTable;  

        private StringTable m_NPCNameStringTable;  
        private StringTable m_NPCDialogueStringTable;  
        private StringTable m_NPCDialogueOptionStringTable;  

        public override void OnSystemStart()
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

        public override void OnSystemStop()
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
            m_EquipmentsStringTable   = LocalizationSettings.StringDatabase.GetTable("EquipmentsList");
            m_WeaponsStringTable = LocalizationSettings.StringDatabase.GetTable("WeaponsList");
            m_MaterialsStringTable    = LocalizationSettings.StringDatabase.GetTable("MaterialList");
            m_MonsterDropsStringTable = LocalizationSettings.StringDatabase.GetTable("Monster_DropsList");
            m_ConsumersStringTable    = LocalizationSettings.StringDatabase.GetTable("ConsumersList");

            m_MainMenuStringTable = LocalizationSettings.StringDatabase.GetTable("MainMenuTable");

            m_NPCNameStringTable = LocalizationSettings.StringDatabase.GetTable("NPCNameTable");
            m_NPCDialogueStringTable = LocalizationSettings.StringDatabase.GetTable("NPCDialogueTable");
            m_NPCDialogueOptionStringTable = LocalizationSettings.StringDatabase.GetTable("NPCDialogueOptionTable");

            //Debug.LogWarning(ScriptStringTable.GetEntry("CommonTip_NoItem").GetLocalizedString());
        }

        /// <summary>
        /// 获取本地化文本
        /// </summary>
        public string GetItemNameLocalizedString(string key, EItemCategory eItemCategory)
        {
            if (eItemCategory == EItemCategory.Gear)
            {
                return m_EquipmentsStringTable.GetEntry(key).GetLocalizedString();
            }
            else if (eItemCategory == EItemCategory.Weapon)
            {
                return m_WeaponsStringTable.GetEntry(key).GetLocalizedString();
            }
            else if (eItemCategory == EItemCategory.Resource || eItemCategory == EItemCategory.Key)
            {
                return m_MaterialsStringTable.GetEntry(key).GetLocalizedString();
            }
            else if (eItemCategory == EItemCategory.MonsterDrop)
            {
                return m_MonsterDropsStringTable.GetEntry(key).GetLocalizedString();
            }
            else if (eItemCategory == EItemCategory.Consumable)
            {
                return m_ConsumersStringTable.GetEntry(key).GetLocalizedString();
            }

            return null;
        }

        public string GetMenuLocalizedString(string key, EMenuStringTableType eMenuStringTableType)
        {
            if (eMenuStringTableType == EMenuStringTableType.MainMenu)
            {
                return m_MainMenuStringTable.GetEntry(key).GetLocalizedString();
            }

            return null;
        }

        public string GetNPCNameLocalizedString(string key)
        {
            return m_NPCNameStringTable.GetEntry(key).GetLocalizedString();
        }

        public string GetNPCDialogueLocalizedString(string key)
        {
            return m_NPCDialogueStringTable.GetEntry(key).GetLocalizedString();
        }

        public string GetNPCDialogueOptionLocalizedString(string key)
        {
            return m_NPCDialogueOptionStringTable.GetEntry(key).GetLocalizedString();
        }
    }
}
