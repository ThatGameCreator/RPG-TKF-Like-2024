using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using TMPro;
using UnityEditor.PackageManager;
using System.Reflection;

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
            // 清空存储本地化文本条目的列表
            localizedTextEntries.Clear();

            // 获取当前选中的所有游戏对象
            GameObject[] selectedObjects = Selection.gameObjects;

            // 遍历每一个选中的游戏对象
            foreach (GameObject selectedObject in selectedObjects)
            {
                // 获取当前游戏对象及其所有子物体中包含的所有 TextMeshProUGUI 组件（包括禁用的对象）
                TextMeshProUGUI[] textComponents = selectedObject.GetComponentsInChildren<TextMeshProUGUI>(true);

                // 遍历所有 TextMeshProUGUI 组件
                foreach (TextMeshProUGUI textComponent in textComponents)
                {
                    // 忽略以 "$" 开头的 Text 组件（这些组件可能是一些特殊用途的组件）
                    if (!textComponent.name.StartsWith("$"))
                    {
                        LocalizeStringEvent localizeEvent = textComponent.gameObject.GetComponent<LocalizeStringEvent>();

                        // 需要本地化的Text
                        if (localizeEvent == null)
                        {
                            textComponent.gameObject.AddComponent<LocalizeStringEvent>();
                            // 标记对象为“已修改”
                            EditorUtility.SetDirty(selectedObject);
                        }
                        if (localizeEvent.OnUpdateString == null)
                        {
                            SetupForLocalization(textComponent, localizeEvent);

                            // 标记该对象为已修改，以便保存时能够识别更改
                            EditorUtility.SetDirty(selectedObject);
                        }

                        FontLocalization fontLocalization = textComponent.gameObject.GetComponent<FontLocalization>();

                        // 需要本地化的Text
                        if (fontLocalization == null)
                        {
                            fontLocalization = textComponent.gameObject.AddComponent<FontLocalization>();

                            fontLocalization.TextMeshPro = textComponent;

                            // 标记对象为“已修改”
                            EditorUtility.SetDirty(selectedObject);
                        }
                        else if (fontLocalization.TextMeshPro == null || fontLocalization.DefaultFontSize == 0)
                        {
                            Debug.Log("fontLocalization.TextMeshPro == null");

                            fontLocalization.TextMeshPro = textComponent;
                            fontLocalization.TableName = "FontAssets";
                            fontLocalization.FontKey = "mainFont";
                            fontLocalization.DefaultFontSize = 28f;
                            fontLocalization.EnglishFontSize = 28f;
                            fontLocalization.ChineseFontSize = 19f;

                            // 标记对象为“已修改”
                            EditorUtility.SetDirty(selectedObject);
                        }

                        // 将游戏对象和对应的 Text 组件的名称、文本内容保存到列表中，格式为 “游戏对象名称\tText组件名称\tText内容”
                        string entry = $"{selectedObject.name}\t{textComponent.name}\t{textComponent.text}";
                        localizedTextEntries.Add(entry);
                    }
                }
            }
        }

        private void SetupForLocalization(TextMeshProUGUI target, LocalizeStringEvent localizeEvent)
        {
            // 获取 TextMeshProUGUI 组件的 "text" 属性的 set 方法，用于更新文本内容
            var setStringMethod = target.GetType().GetProperty("text").GetSetMethod();

            // 创建一个委托，将 "text" 属性的 set 方法绑定到目标对象
            var methodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<string>), target, setStringMethod) as UnityAction<string>;

            // 将该委托添加为 LocalizeStringEvent 组件的 OnUpdateString 事件的监听器
            UnityEditor.Events.UnityEventTools.AddPersistentListener(localizeEvent.OnUpdateString, methodDelegate);

            // 设置事件监听器的状态，使其在编辑器和运行时都可以触发
            localizeEvent.OnUpdateString.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
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
    