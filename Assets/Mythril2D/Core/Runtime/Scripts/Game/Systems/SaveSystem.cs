using System.IO;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class SaveSystem : AGameSystem
    {
        public string saveFileName => m_saveFileName;
        private string m_saveFileName = "SAVEFILE_A";

        public void SetSaveFileName()
        {
        }

        public void LoadDefaultSaveFile(SaveFile saveFile, string saveFileName)
        {
            SaveFileData newSaveFile = DuplicateSaveFile(saveFile.content);

            LoadSaveFile(newSaveFile, saveFileName);
        }

        /**
         * Never use the m_defaultSaveFile as-is, but instead, always duplicate it (deep copy) to prevent changing the initial scriptable object data.
         * This is especially useful in editor. (TODO: make it #if UNITY_EDITOR, otherwise directly use the data without cloning it)
         */
        public SaveFileData DuplicateSaveFile(SaveFileData saveFile)
        {
            string saveData = JsonUtility.ToJson(saveFile, true);
            return JsonUtility.FromJson<SaveFileData>(saveData);
        }

        public static void EraseSaveData(string saveFileName)
        {
            string filepath = Path.GetFullPath(Path.Combine(Application.persistentDataPath, saveFileName));

            File.Delete(filepath);
        }

        public static bool TryExtractingSaveData(string saveFileName, out SaveFileData output)
        {
            string filepath = Path.GetFullPath(Path.Combine(Application.persistentDataPath, saveFileName));

            if (!File.Exists(filepath))
            {
                output = new SaveFileData { };
                return false;
            }

            try
            {
                using (FileStream stream = new FileStream(filepath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string dataToLoad = reader.ReadToEnd();
                        output = JsonUtility.FromJson<SaveFileData>(dataToLoad);
                        return true;
                    }
                }
            }
            catch
            {
                output = new SaveFileData { };
                return false;
            }
        }

        public void LoadFromFile(string saveFileName)
        {
            // 保存当前存档文件名，便于后续操作（例如支持多存档机制）
            m_saveFileName = saveFileName;

            Debug.Log($"Loading from {saveFileName}...");

            SaveFileData saveFile;

            // 尝试从指定存档文件中提取存档数据
            if (TryExtractingSaveData(saveFileName, out saveFile))
            {
                // 调用自定义方法加载存档数据并应用到游戏状态
                LoadSaveFile(saveFile);
                Debug.Log($"Loading succeeded!");
            }
            else
            {
                // 若文件不存在或数据提取失败，记录错误信息
                Debug.LogError($"Loading failed!");
            }
        }

        public void SaveToFile(string saveFileName)
        {
            // 通知系统触发存档开始事件，例如用来更新UI或暂停游戏
            GameManager.NotificationSystem.saveStart.Invoke();

            m_saveFileName = saveFileName;

            // 测试的时候没有绑定存档所以为空
            //Debug.Log("saveFileName = " + saveFileName);

            // 生成文件的完整路径，保存在 Unity 的持久化数据路径中
            string filepath = Path.GetFullPath(Path.Combine(Application.persistentDataPath, saveFileName));

            Debug.Log($"Saving to {filepath}...");

            try
            {
                // 调用自定义方法生成存档数据对象
                SaveFileData saveFile = CreateSaveFile();

                LocalizationSystem.Instance.SaveToFile(LocalizationSystem.Instance.saveFileName);

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

            // 通知系统触发存档结束事件，例如用来恢复UI或解锁交互
            GameManager.NotificationSystem.saveEnd.Invoke();
        }

        public void LoadSaveFile(SaveFileData saveFile)
        {
            GameManager.GameFlagSystem.LoadDataBlock(saveFile.gameFlags);
            GameManager.WarehouseSystem.LoadDataBlock(saveFile.warehouse);
            GameManager.InventorySystem.LoadDataBlock(saveFile.inventory);
            GameManager.JournalSystem.LoadDataBlock(saveFile.journal);
            GameManager.PlayerSystem.LoadDataBlock(saveFile.player);
            GameManager.TeleportLoadingSystem.RequestTransition(saveFile.map, null, null, null, ETeleportType.Revival);
            //GameManager.TeleportLoadingSystem.RequestTransition(saveFile.map);
        }

        public void LoadSaveFile(SaveFileData saveFile, string saveFileName)
        {
            Debug.Log(saveFileName);

            m_saveFileName = saveFileName;

            GameManager.GameFlagSystem.LoadDataBlock(saveFile.gameFlags);
            GameManager.WarehouseSystem.LoadDataBlock(saveFile.warehouse);
            GameManager.InventorySystem.LoadDataBlock(saveFile.inventory);
            GameManager.JournalSystem.LoadDataBlock(saveFile.journal);
            GameManager.PlayerSystem.LoadDataBlock(saveFile.player);
            GameManager.TeleportLoadingSystem.RequestTransition(saveFile.map, null, null, () =>
            {
                GameManager.SaveSystem.SaveToFile(saveFileName);
            }, ETeleportType.Revival);
            //GameManager.TeleportLoadingSystem.RequestTransition(saveFile.map);
        }

        private string GenerateSavefileHeader()
        {
            string header;

            Hero player = GameManager.PlayerSystem.PlayerInstance;

            header = string.Format("{0} {1}{2}",
                player.characterSheet.displayName,
                GameManager.Config.GetTermDefinition("level").shortName,
                player.level
                );

            return header;
        }

        private SaveFileData CreateSaveFile()
        {
            return new SaveFileData
            {
                header = GenerateSavefileHeader(),
                map = GameManager.TeleportLoadingSystem.GetCurrentMapName(),
                gameFlags = GameManager.GameFlagSystem.CreateDataBlock(),
                inventory = GameManager.InventorySystem.CreateDataBlock(),
                warehouse = GameManager.WarehouseSystem.CreateDataBlock(),
                journal = GameManager.JournalSystem.CreateDataBlock(),
                player = GameManager.PlayerSystem.CreateDataBlock()
            };
        }
    }
}
