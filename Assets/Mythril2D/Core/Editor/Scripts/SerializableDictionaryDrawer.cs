using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Gyvr.Mythril2D;



public class SerializableDictionaryDrawer : PropertyDrawer
{
    // ����������������ͨ���߼�
    public abstract class SerializableDictionaryDrawerBase : PropertyDrawer
    {
        private const float ButtonWidth = 20f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // ��ȡ m_keys �� m_values
            var keysProperty = property.FindPropertyRelative("m_keys");
            var valuesProperty = property.FindPropertyRelative("m_values");

            if (keysProperty == null || valuesProperty == null)
            {
                EditorGUI.LabelField(position, label.text, "Invalid SerializableDictionary");
                return;
            }

            // �����ֵ����
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            EditorGUI.indentLevel++;
            position.y += EditorGUIUtility.singleLineHeight;

            // �������Ƽ�ֵ��
            for (int i = 0; i < keysProperty.arraySize; i++)
            {
                var keyProperty = keysProperty.GetArrayElementAtIndex(i);
                var valueProperty = valuesProperty.GetArrayElementAtIndex(i);

                float lineHeight = EditorGUIUtility.singleLineHeight;
                Rect keyRect = new Rect(position.x, position.y, position.width * 0.4f, lineHeight);
                Rect valueRect = new Rect(position.x + position.width * 0.45f, position.y + position.width * 0.45f, position.width * 0.4f, lineHeight);
                Rect removeButtonRect = new Rect(position.x + position.width * 0.9f, position.y, ButtonWidth, lineHeight);

                // ���� Key �� Value
                EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none);
                EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);

                // ����ɾ����ť
                if (GUI.Button(removeButtonRect, "X"))
                {
                    keysProperty.DeleteArrayElementAtIndex(i);
                    valuesProperty.DeleteArrayElementAtIndex(i);
                }

                position.y += lineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            // �������Ŀ�İ�ť
            Rect addButtonRect = new Rect(position.x, position.y, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
            Rect clearButtonRect = new Rect(position.x + position.width * 0.55f, position.y + position.width * 0.55f, position.width * 0.4f, EditorGUIUtility.singleLineHeight);

            if (GUI.Button(addButtonRect, "Add Entry"))
            {
                keysProperty.arraySize++;
                valuesProperty.arraySize++;
            }

            if (GUI.Button(clearButtonRect, "Clear All"))
            {
                keysProperty.ClearArray();
                valuesProperty.ClearArray();
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var keysProperty = property.FindPropertyRelative("m_keys");

            if (keysProperty == null)
                return EditorGUIUtility.singleLineHeight;

            return (keysProperty.arraySize + 2) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
        }
    }

    // Ϊ�������͵��ֵ䴴�����Ի�����
    [CustomPropertyDrawer(typeof(SerializableDictionary<string, int>))]
    public class StringIntDictionaryDrawer : SerializableDictionaryDrawerBase
    {
    }

    [CustomPropertyDrawer(typeof(SerializableDictionary<Object, string>))]
    public class ObjectStringDictionaryDrawer : SerializableDictionaryDrawerBase
    {
    }

    private const float ButtonWidth = 20f; // ɾ����ť���

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // ��ȡ�ֵ�� keys �� values �б�
        var keysProperty = property.FindPropertyRelative("m_keys");
        var valuesProperty = property.FindPropertyRelative("m_values");

        if (keysProperty == null || valuesProperty == null)
        {
            EditorGUI.LabelField(position, label.text, "Dictionary data is invalid");
            return;
        }

        // �����ֵ����
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // ���ӿհ���������ʾ��ֵ���б�
        EditorGUI.indentLevel++;
        position.y += EditorGUIUtility.singleLineHeight;

        // ����ÿ����ֵ��
        for (int i = 0; i < keysProperty.arraySize; i++)
        {
            var keyProperty = keysProperty.GetArrayElementAtIndex(i);
            var valueProperty = valuesProperty.GetArrayElementAtIndex(i);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            Rect keyRect = new Rect(position.x, position.y, position.width * 0.4f, lineHeight);
            Rect valueRect = new Rect(position.x + position.width * 0.45f, position.y, position.width * 0.4f, lineHeight);
            Rect removeButtonRect = new Rect(position.x + position.width * 0.9f, position.y, ButtonWidth, lineHeight);

            // ���� Key �� Value
            EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none);
            EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);

            // ����ɾ����ť
            if (GUI.Button(removeButtonRect, "X"))
            {
                keysProperty.DeleteArrayElementAtIndex(i);
                valuesProperty.DeleteArrayElementAtIndex(i);
            }

            position.y += lineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        // �����������Ŀ�İ�ť
        Rect addButtonRect = new Rect(position.x, position.y, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
        Rect clearButtonRect = new Rect(position.x + position.width * 0.55f, position.y, position.width * 0.4f, EditorGUIUtility.singleLineHeight);

        if (GUI.Button(addButtonRect, "Add New Entry"))
        {
            keysProperty.arraySize++;
            valuesProperty.arraySize++;
        }

        if (GUI.Button(clearButtonRect, "Clear All"))
        {
            keysProperty.ClearArray();
            valuesProperty.ClearArray();
        }

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var keysProperty = property.FindPropertyRelative("m_keys");

        if (keysProperty == null)
            return EditorGUIUtility.singleLineHeight;

        // Ϊÿ����Ŀ����һ�и߶� + ��ť�߶�
        return (keysProperty.arraySize + 2) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
    }
}
