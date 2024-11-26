using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Gyvr.Mythril2D;

namespace Gyvr.Mythril2D
{

    public class CharacterSheetEditor : EditorWindow
    {
        private CharacterSheet targetCharacterSheet;
        private List<AbilitySheet> selectedAbilities = new List<AbilitySheet>();
        private int abilityLevel = 1;

        [MenuItem("Tools/Character Sheet Editor")]
        public static void ShowWindow()
        {
            GetWindow<CharacterSheetEditor>("Character Sheet Editor");
        }

        private void OnGUI()
        {
            GUILayout.Label("Configure CharacterSheet Abilities", EditorStyles.boldLabel);

            // CharacterSheet 数据模板选择
            targetCharacterSheet = (CharacterSheet)EditorGUILayout.ObjectField(
                "Target CharacterSheet",
                targetCharacterSheet,
                typeof(CharacterSheet),
                false
            );

            // 技能选择
            GUILayout.Label("Selected Abilities");
            if (GUILayout.Button("Select Abilities"))
            {
                SelectAbilities();
            }

            // 显示已选技能
            EditorGUILayout.LabelField("Selected Abilities:");
            foreach (var ability in selectedAbilities)
            {
                EditorGUILayout.LabelField(ability.name);
            }

            // 输入技能等级
            abilityLevel = EditorGUILayout.IntField("Ability Level", abilityLevel);

            // 配置按钮
            if (GUILayout.Button("Configure Abilities"))
            {
                ConfigureAbilities();
            }
        }

        private void SelectAbilities()
        {
            // 在资源管理器中选择多个 AbilitySheet
            var selectedObjects = Selection.objects;
            selectedAbilities.Clear();

            foreach (var obj in selectedObjects)
            {
                if (obj is AbilitySheet ability)
                {
                    selectedAbilities.Add(ability);
                }
            }

            if (selectedAbilities.Count == 0)
            {
                Debug.LogWarning("No valid AbilitySheet selected!");
            }
        }

        private void ConfigureAbilities()
        {
            if (targetCharacterSheet == null)
            {
                Debug.LogError("Please select a target CharacterSheet.");
                return;
            }

            if (selectedAbilities.Count == 0)
            {
                Debug.LogError("No abilities selected for configuration.");
                return;
            }

            // 通过 SerializedObject 修改 CharacterSheet 的 m_abilitiesPerLevel
            SerializedObject serializedObject = new SerializedObject(targetCharacterSheet);
            SerializedProperty abilitiesPerLevelProperty = serializedObject.FindProperty("m_abilitiesPerLevel");

            serializedObject.Update();

            foreach (var ability in selectedAbilities)
            {
                // 检查是否已存在技能
                bool exists = false;
                for (int i = 0; i < abilitiesPerLevelProperty.FindPropertyRelative("m_keys").arraySize; i++)
                {
                    var key = abilitiesPerLevelProperty.FindPropertyRelative("m_keys").GetArrayElementAtIndex(i);
                    if (key.objectReferenceValue == ability)
                    {
                        exists = true;
                        break;
                    }
                }

                // 如果不存在则添加
                if (!exists)
                {
                    int newIndex = abilitiesPerLevelProperty.FindPropertyRelative("m_keys").arraySize;
                    abilitiesPerLevelProperty.FindPropertyRelative("m_keys").InsertArrayElementAtIndex(newIndex);
                    abilitiesPerLevelProperty.FindPropertyRelative("m_values").InsertArrayElementAtIndex(newIndex);

                    var newKey = abilitiesPerLevelProperty.FindPropertyRelative("m_keys").GetArrayElementAtIndex(newIndex);
                    var newValue = abilitiesPerLevelProperty.FindPropertyRelative("m_values").GetArrayElementAtIndex(newIndex);

                    newKey.objectReferenceValue = ability;
                    newValue.intValue = abilityLevel;
                }
            }

            serializedObject.ApplyModifiedProperties();
            Debug.Log($"Configured {selectedAbilities.Count} abilities in {targetCharacterSheet.name}.");
        }
    }
}