using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using TMPro;

namespace Gyvr.Mythril2D
{
    public class LocalizeTextEditor : EditorWindow
    {
        private string outputFilePath = "LocalizedText.txt"; // 指定的txt文件路径
        private List<string> localizedTextEntries = new List<string>();

        [MenuItem("Custom/Localize Text")]
        private static void ShowWindow()
        {
            GetWindow<LocalizeTextEditor>("Localize Text");
        }

        private void OnGUI()
        {
            GUILayout.Label("Localize Text Editor", EditorStyles.boldLabel);

            if (GUILayout.Button("Localize Selected GameObjects"))
            {
                LocalizeSelectedGameObjects();
            }

            if (GUILayout.Button("Save Localized Text to File"))
            {
                SaveLocalizedTextToFile();
            }

            if (GUILayout.Button("Print Script Localize Selected GameObjects"))
            {
                PrintScriptLocalize();
            }
        }

        private void LocalizeSelectedGameObjects()
        {
            localizedTextEntries.Clear();

            GameObject[] selectedObjects = Selection.gameObjects;
            foreach (GameObject selectedObject in selectedObjects)
            {
                TextMeshProUGUI[] textComponents = selectedObject.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI textComponent in textComponents)
                {
                    if (!textComponent.name.StartsWith("$"))
                    {
                        // 需要本地化的Text
                        if (textComponent.gameObject.GetComponent<LocalizeStringEvent>() == null)
                        {
                            LocalizeStringEvent localizeEvent = textComponent.gameObject.AddComponent<LocalizeStringEvent>();
                            // 标记对象为“已修改”
                            EditorUtility.SetDirty(selectedObject);
                        }
                        // 添加到列表
                        string entry = $"{selectedObject.name}\t{textComponent.name}\t{textComponent.text}";
                        localizedTextEntries.Add(entry);
                    }
                }
            }
        }

        private void SaveLocalizedTextToFile()
        {
            if (!File.Exists(outputFilePath))
            {
                File.Create(outputFilePath).Dispose();
            }
            using (StreamWriter writer = new StreamWriter(outputFilePath, true))
            {
                foreach (string entry in localizedTextEntries)
                {
                    writer.WriteLine(entry);
                }
            }

            Debug.Log($"Localized text entries saved to {outputFilePath}");
        }

        private void PrintScriptLocalize()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            foreach (GameObject selectedObject in selectedObjects)
            {
                Text[] textComponents = selectedObject.GetComponentsInChildren<Text>(true);
                foreach (Text textComponent in textComponents)
                {
                    if (textComponent.name.StartsWith("$"))
                    {
                        Debug.Log($"{selectedObject.name}\t{textComponent.name}\t{textComponent.text}");
                    }
                }
            }
        }
    }
}
    