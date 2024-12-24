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
        private string outputFilePath = "LocalizedText.txt"; // ָ����txt�ļ�·��
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
            // ��մ洢���ػ��ı���Ŀ���б�
            localizedTextEntries.Clear();

            // ��ȡ��ǰѡ�е�������Ϸ����
            GameObject[] selectedObjects = Selection.gameObjects;

            // ����ÿһ��ѡ�е���Ϸ����
            foreach (GameObject selectedObject in selectedObjects)
            {
                // ��ȡ��ǰ��Ϸ�����������������а��������� TextMeshProUGUI ������������õĶ���
                TextMeshProUGUI[] textComponents = selectedObject.GetComponentsInChildren<TextMeshProUGUI>(true);

                // �������� TextMeshProUGUI ���
                foreach (TextMeshProUGUI textComponent in textComponents)
                {
                    // ������ "$" ��ͷ�� Text �������Щ���������һЩ������;�������
                    if (!textComponent.name.StartsWith("$"))
                    {
                        LocalizeStringEvent localizeEvent = textComponent.gameObject.GetComponent<LocalizeStringEvent>();

                        // ��Ҫ���ػ���Text
                        if (localizeEvent == null)
                        {
                            localizeEvent = textComponent.gameObject.AddComponent<LocalizeStringEvent>();
                            SetupForLocalization(textComponent, localizeEvent);
                            // ��Ƕ���Ϊ�����޸ġ�
                            EditorUtility.SetDirty(selectedObject);
                        }
                        else if (localizeEvent.OnUpdateString == null)
                        {
                            SetupForLocalization(textComponent, localizeEvent);

                            // ��Ǹö���Ϊ���޸ģ��Ա㱣��ʱ�ܹ�ʶ�����
                            EditorUtility.SetDirty(selectedObject);
                        }

                        FontLocalization fontLocalization = textComponent.gameObject.GetComponent<FontLocalization>();

                        // ��Ҫ���ػ���Text
                        if (fontLocalization == null)
                        {
                            fontLocalization = textComponent.gameObject.AddComponent<FontLocalization>();

                            fontLocalization.TextMeshPro = textComponent;

                            // ��Ƕ���Ϊ�����޸ġ�
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

                            // ��Ƕ���Ϊ�����޸ġ�
                            EditorUtility.SetDirty(selectedObject);
                        }

                        // ����Ϸ����Ͷ�Ӧ�� Text ��������ơ��ı����ݱ��浽�б��У���ʽΪ ����Ϸ��������\tText�������\tText���ݡ�
                        string entry = $"{selectedObject.name}\t{textComponent.name}\t{textComponent.text}";
                        localizedTextEntries.Add(entry);
                    }
                }
            }
        }

        private void SetupForLocalization(TextMeshProUGUI target, LocalizeStringEvent localizeEvent)
        {
            // ��ȡ TextMeshProUGUI ����� "text" ���Ե� set ���������ڸ����ı�����
            var setStringMethod = target.GetType().GetProperty("text").GetSetMethod();

            // ����һ��ί�У��� "text" ���Ե� set �����󶨵�Ŀ�����
            var methodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<string>), target, setStringMethod) as UnityAction<string>;

            // ����ί�����Ϊ LocalizeStringEvent ����� OnUpdateString �¼��ļ�����
            UnityEditor.Events.UnityEventTools.AddPersistentListener(localizeEvent.OnUpdateString, methodDelegate);

            // �����¼���������״̬��ʹ���ڱ༭��������ʱ�����Դ���
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
    