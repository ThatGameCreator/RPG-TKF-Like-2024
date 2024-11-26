using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Gyvr.Mythril2D;



public class SerializableDictionaryDrawer : PropertyDrawer
{
    // 基础绘制器，处理通用逻辑
    public abstract class SerializableDictionaryDrawerBase : PropertyDrawer
    {
        private const float ButtonWidth = 20f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 获取 m_keys 和 m_values
            var keysProperty = property.FindPropertyRelative("m_keys");
            var valuesProperty = property.FindPropertyRelative("m_values");

            if (keysProperty == null || valuesProperty == null)
            {
                EditorGUI.LabelField(position, label.text, "Invalid SerializableDictionary");
                return;
            }

            // 绘制字典标题
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            EditorGUI.indentLevel++;
            position.y += EditorGUIUtility.singleLineHeight;

            // 遍历绘制键值对
            for (int i = 0; i < keysProperty.arraySize; i++)
            {
                var keyProperty = keysProperty.GetArrayElementAtIndex(i);
                var valueProperty = valuesProperty.GetArrayElementAtIndex(i);

                float lineHeight = EditorGUIUtility.singleLineHeight;
                Rect keyRect = new Rect(position.x, position.y, position.width * 0.4f, lineHeight);
                Rect valueRect = new Rect(position.x + position.width * 0.45f, position.y + position.width * 0.45f, position.width * 0.4f, lineHeight);
                Rect removeButtonRect = new Rect(position.x + position.width * 0.9f, position.y, ButtonWidth, lineHeight);

                // 绘制 Key 和 Value
                EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none);
                EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);

                // 绘制删除按钮
                if (GUI.Button(removeButtonRect, "X"))
                {
                    keysProperty.DeleteArrayElementAtIndex(i);
                    valuesProperty.DeleteArrayElementAtIndex(i);
                }

                position.y += lineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            // 添加新条目的按钮
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

    // 为具体类型的字典创建属性绘制器
    [CustomPropertyDrawer(typeof(SerializableDictionary<string, int>))]
    public class StringIntDictionaryDrawer : SerializableDictionaryDrawerBase
    {
    }

    [CustomPropertyDrawer(typeof(SerializableDictionary<Object, string>))]
    public class ObjectStringDictionaryDrawer : SerializableDictionaryDrawerBase
    {
    }

    private const float ButtonWidth = 20f; // 删除按钮宽度

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // 获取字典的 keys 和 values 列表
        var keysProperty = property.FindPropertyRelative("m_keys");
        var valuesProperty = property.FindPropertyRelative("m_values");

        if (keysProperty == null || valuesProperty == null)
        {
            EditorGUI.LabelField(position, label.text, "Dictionary data is invalid");
            return;
        }

        // 绘制字典标题
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // 增加空白行用于显示键值对列表
        EditorGUI.indentLevel++;
        position.y += EditorGUIUtility.singleLineHeight;

        // 绘制每个键值对
        for (int i = 0; i < keysProperty.arraySize; i++)
        {
            var keyProperty = keysProperty.GetArrayElementAtIndex(i);
            var valueProperty = valuesProperty.GetArrayElementAtIndex(i);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            Rect keyRect = new Rect(position.x, position.y, position.width * 0.4f, lineHeight);
            Rect valueRect = new Rect(position.x + position.width * 0.45f, position.y, position.width * 0.4f, lineHeight);
            Rect removeButtonRect = new Rect(position.x + position.width * 0.9f, position.y, ButtonWidth, lineHeight);

            // 绘制 Key 和 Value
            EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none);
            EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);

            // 绘制删除按钮
            if (GUI.Button(removeButtonRect, "X"))
            {
                keysProperty.DeleteArrayElementAtIndex(i);
                valuesProperty.DeleteArrayElementAtIndex(i);
            }

            position.y += lineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        // 绘制添加新条目的按钮
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

        // 为每个条目增加一行高度 + 按钮高度
        return (keysProperty.arraySize + 2) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
    }
}
