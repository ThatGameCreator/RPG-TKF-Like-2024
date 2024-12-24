using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.CSV;
using UnityEditor.Localization.Plugins.CSV.Columns;

public static class LocalizationCsvUtility
{
    [MenuItem("Localization/CSV/Export All CSV Files")]
    public static void ExportAllCsv()
    {
        // 选择导出路径
        string exportPath = EditorUtility.SaveFolderPanel("Export String Table Collections - CSV", "", "");
        if (string.IsNullOrEmpty(exportPath))
        {
            Debug.LogWarning("Export path not selected. Operation cancelled.");
            return;
        }

        // 获取所有的 String Table Collections
        var stringTableCollections = LocalizationEditorSettings.GetStringTableCollections();

        foreach (var collection in stringTableCollections)
        {
            string fileName = Path.Combine(exportPath, collection.TableCollectionName + ".csv");

            // 写入 CSV 文件
            using (var stream = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                Csv.Export(stream, collection);
            }

            Debug.Log($"Exported CSV: {fileName}");
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("Localization/CSV/Import All CSV Files")]
    public static void ImportAllCsv()
    {
        // 选择 CSV 文件所在的文件夹
        string importPath = EditorUtility.OpenFolderPanel("Import CSV Files to String Table Collections", "", "");
        if (string.IsNullOrEmpty(importPath))
        {
            Debug.LogWarning("Import path not selected. Operation cancelled.");
            return;
        }

        // 获取所有的 String Table Collections
        var stringTableCollections = LocalizationEditorSettings.GetStringTableCollections();

        foreach (var collection in stringTableCollections)
        {
            string fileName = Path.Combine(importPath, collection.TableCollectionName + ".csv");
            if (File.Exists(fileName))
            {
                // 读取 CSV 文件并导入
                using (var stream = new StreamReader(fileName, Encoding.UTF8))
                {
                    Csv.ImportInto(stream, collection);
                }

                Debug.Log($"Imported CSV: {fileName}");
            }
            else
            {
                Debug.LogWarning($"CSV file not found for collection: {collection.TableCollectionName}");
            }
        }

        AssetDatabase.Refresh();
    }
}
