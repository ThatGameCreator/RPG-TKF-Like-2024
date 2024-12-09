using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace Gyvr.Mythril2D
{
    [Serializable]
    public struct LocalizationDataBlock
    {
        public int LocaleIndex;
    }

    public enum EMenuStringTableType
    {
        MainMenu,
        TimeReminder,
    }

    public class LocalizationSystem : Singleton<LocalizationSystem>
    {
        public List<Locale> locales => m_locales;
        // 当前语言索引
        private int m_currentLocaleIndex = 0;
        private List<Locale> m_locales = null;

        public int currentLocaleIndex
        {
            get => m_currentLocaleIndex;
            set => m_currentLocaleIndex = value;
        }

        public string saveFileName => m_saveFileName;
        private string m_saveFileName = "Localization_Save";

        //代码本地化表
        private StringTable m_EquipmentsStringTable;          
        private StringTable m_WeaponsStringTable;          
        private StringTable m_MaterialsStringTable;          
        private StringTable m_MonsterDropsStringTable;          
        private StringTable m_ConsumersStringTable;     

        private StringTable m_ItemDescriptionStringTable;     
        
        private StringTable m_MainMenuStringTable;  
        private StringTable m_EvacuationPositionStringTable;  

        private StringTable m_NPCNameStringTable;  
        private StringTable m_NPCDialogueStringTable;  
        private StringTable m_NPCDialogueOptionStringTable;  

        private StringTable m_StatsTermDefinitionStringTable;  

        public override void Awake()
        {
            // 用变量判断不得 不知道为什么
            // 难道用实例的话检测的是父类的属性？
            // 但变量也是放在父类里面的，有什么区别？
            if (m_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            base.Awake();

            // 我服了，我以为他destory之后就没了，结果后面的函数居然还执行了一遍
            //Debug.Log("if");

            CheckAndHandleSaveFile();

            if (LocalizationSettings.AvailableLocales.Locales.Count > 0)
            {
                GetLocalizationTable();
            }
            else
            {
                LocalizationSettings.InitializationOperation.Completed += OnLocalizationInitialized;
            }
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

            InitialLanguage();
        }

        public void InitialLanguage()
        {
            m_locales = LocalizationSettings.AvailableLocales.Locales; // 获取所有可用语言列表

            if (m_locales.Count == 0)
            {
                Debug.LogWarning("No available locales found!");
                return;
            }

            LocalizationSettings.SelectedLocale = m_locales[currentLocaleIndex]; // 更新语言
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
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
            m_MaterialsStringTable    = LocalizationSettings.StringDatabase.GetTable("MaterialsList");
            m_MonsterDropsStringTable = LocalizationSettings.StringDatabase.GetTable("Monster_DropsList");
            m_ConsumersStringTable    = LocalizationSettings.StringDatabase.GetTable("ConsumersList");

            m_ItemDescriptionStringTable = LocalizationSettings.StringDatabase.GetTable("ItemDescriptionTable");

            m_MainMenuStringTable = LocalizationSettings.StringDatabase.GetTable("MainMenuTable");
            m_EvacuationPositionStringTable = LocalizationSettings.StringDatabase.GetTable("EvacuationPositionTable");

            m_NPCNameStringTable = LocalizationSettings.StringDatabase.GetTable("NPCNameTable");
            m_NPCDialogueStringTable = LocalizationSettings.StringDatabase.GetTable("NPCDialogueTable");
            m_NPCDialogueOptionStringTable = LocalizationSettings.StringDatabase.GetTable("NPCDialogueOptionTable");


            m_StatsTermDefinitionStringTable = LocalizationSettings.StringDatabase.GetTable("TermDefinitionTable");

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

        public string GetNPCNameLocalizedString(string key)
        {
            return m_NPCNameStringTable.GetEntry(key).GetLocalizedString();
        }

        public string GetMenuLocalizedString(string key, EMenuStringTableType eMenuStringTableType)
        {
            if (eMenuStringTableType == EMenuStringTableType.MainMenu)
            {
                return m_MainMenuStringTable.GetEntry(key).GetLocalizedString();
            }
            else if (eMenuStringTableType == EMenuStringTableType.TimeReminder)
            {
                return m_EvacuationPositionStringTable.GetEntry(key).GetLocalizedString();
            }

            return null;
        }

        public string GetItemDescriptionLocalizedString(string key)
        {
            return m_ItemDescriptionStringTable.GetEntry(key).GetLocalizedString();
        }

        public string GetNPCDialogueLocalizedString(string key)
        {
            return m_NPCDialogueStringTable.GetEntry(key).GetLocalizedString();
        }

        public string GetNPCDialogueOptionLocalizedString(string key)
        {
            return m_NPCDialogueOptionStringTable.GetEntry(key).GetLocalizedString();
        }

        public string GetStatsTermDefinitionLocalizedString(string key)
        {
            return m_StatsTermDefinitionStringTable.GetEntry(key).GetLocalizedString();
        }

        public void CheckAndHandleSaveFile()
        {
            // 生成存档文件路径
            string filePath = Path.Combine(Application.persistentDataPath, m_saveFileName);

            if (File.Exists(filePath))
            {
                // 文件存在，加载存档
                Debug.Log("Save file found, loading...");
                LoadFromFile(m_saveFileName);
            }
            else
            {
                // 文件不存在，创建新存档
                Debug.Log("Save file not found, creating a new one...");
                SaveToFile(m_saveFileName);
            }
        }

        public void LoadFromFile(string saveFileName)
        {
            // 保存当前存档文件名，便于后续操作（例如支持多存档机制）
            m_saveFileName = saveFileName;

            Debug.Log($"Loading from {saveFileName}...");

            LocalizationDataBlock saveLocalizationFile;

            // 尝试从指定存档文件中提取存档数据
            if (TryExtractingSaveData(saveFileName, out saveLocalizationFile))
            {
                // 调用自定义方法加载存档数据并应用到游戏状态
                LoadSaveFile(saveLocalizationFile, saveFileName);
                Debug.Log($"Loading succeeded!");
            }
            else
            {
                // 若文件不存在或数据提取失败，记录错误信息
                Debug.LogError($"Loading failed!");
            }
        }

        public void LoadSaveFile(LocalizationDataBlock localizationSaveFile, string saveFileName)
        {
            m_saveFileName = saveFileName;

            LoadDataBlock(localizationSaveFile);
        }

        public static bool TryExtractingSaveData(string saveFileName, out LocalizationDataBlock output)
        {
            string filepath = Path.GetFullPath(Path.Combine(Application.persistentDataPath, saveFileName));

            if (!File.Exists(filepath))
            {
                output = new LocalizationDataBlock { };
                return false;
            }

            try
            {
                using (FileStream stream = new FileStream(filepath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string dataToLoad = reader.ReadToEnd();
                        output = JsonUtility.FromJson<LocalizationDataBlock>(dataToLoad);
                        return true;
                    }
                }
            }
            catch
            {
                output = new LocalizationDataBlock { };
                return false;
            }
        }

        public void SaveToFile(string saveFileName)
        {
            m_saveFileName = saveFileName;

            // 测试的时候没有绑定存档所以为空
            //Debug.Log("saveFileName = " + saveFileName);

            // 生成文件的完整路径，保存在 Unity 的持久化数据路径中
            string filepath = Path.GetFullPath(Path.Combine(Application.persistentDataPath, saveFileName));

            Debug.Log($"Saving to {filepath}...");

            try
            {
                // 调用自定义方法生成存档数据对象
                LocalizationDataBlock saveFile = CreateSaveFile();

                // 将存档数据序列化为 JSON 字符串，第二个参数 true 表示格式化输出（便于调试和阅读）
                string dataToStore = JsonUtility.ToJson(saveFile, true);

                // 创建文件流以写入数据到目标文件路径
                using (FileStream stream = new FileStream(filepath, FileMode.Create))
                {
                    // 使用 StreamWriter 包装文件流，写入 JSON 数据
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(dataToStore);

                        Debug.Log($"Saving succeeded!");
                    }
                }
            }
            catch (System.Exception e)
            {
                // 捕获任何文件操作相关的异常并记录错误信息
                Debug.LogError($"Saving failed: {e.Message}");
            }

        }
        private LocalizationDataBlock CreateSaveFile()
        {
            return new LocalizationDataBlock
            {
                LocaleIndex = GetCurrentLocalIndex(),
            };
        }

        public int GetCurrentLocalIndex()
        {
            return m_currentLocaleIndex;
        }

        public void LoadDataBlock(LocalizationDataBlock block)
        {
            m_currentLocaleIndex = block.LocaleIndex;
        }
    }
}
