using Gyvr.Mythril2D;
using UnityEditor;
using UnityEngine;
using System.Collections;

namespace Gyvr.Mythril2D
{
    [CustomEditor(typeof(EntityTable))]
    public class EntityTableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EntityTable entityTable = (EntityTable)target;

            EditorGUILayout.LabelField("Entity Table Configuration", EditorStyles.boldLabel);

            // 绘制权重分布条形图
            if (entityTable.entries != null && entityTable.entries.Length > 0)
            {
                float totalWeight = 0f;
                foreach (var entry in entityTable.entries)
                {
                    totalWeight += entry.weight;
                }

                foreach (var entry in entityTable.entries)
                {
                    float percentage = (totalWeight > 0) ? entry.weight / totalWeight : 0;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(entry.entity ? entry.entity.name : "Unnamed Entity");
                    Rect rect = EditorGUILayout.GetControlRect(false, 20);
                    EditorGUI.ProgressBar(rect, percentage, $"{percentage * 100:F1}%");
                    EditorGUILayout.EndHorizontal();
                }
            }

            // 显示和编辑条目数据
            if (entityTable.entries != null)
            {
                for (int i = 0; i < entityTable.entries.Length; i++)
                {
                    var entry = entityTable.entries[i];
                    EditorGUILayout.BeginVertical("box");
                    entry.entity = (Entity)EditorGUILayout.ObjectField("Entity", entry.entity, typeof(Entity), false);
                    entry.weight = EditorGUILayout.FloatField("Weight", entry.weight);

                    // 删除按钮
                    if (GUILayout.Button($"Remove Entity {i + 1}"))
                    {
                        // 从数组中移除
                        ArrayUtility.RemoveAt(ref entityTable.entries, i);
                        break; // 结束当前循环，避免 IndexOutOfRangeException
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            // 添加新的条目按钮
            if (GUILayout.Button("Add Selected Entities"))
            {
                Debug.Log($"Selected Objects Count: {Selection.objects.Length}");

                foreach (Object selected in Selection.objects)
                {
                    Debug.Log($"Selected Object: {selected}, Type: {selected.GetType()}");

                    // 尝试从选中的对象中获取 Entity 组件
                    Entity entityToAdd = GetEntityComponent(selected);

                    if (entityToAdd != null)
                    {
                        Debug.Log($"Adding Entity: {entityToAdd.name}");

                        EntityTable.EntityData newEntry = new EntityTable.EntityData
                        {
                            entity = entityToAdd,
                            weight = 1f
                        };
                        ArrayUtility.Add(ref entityTable.entries, newEntry);
                    }
                    else
                    {
                        Debug.Log($"Could not find Entity component on {selected.name}");
                    }
                }
            }

            // 保存修改
            if (GUI.changed)
            {
                EditorUtility.SetDirty(entityTable);
            }
        }

        private Entity GetEntityComponent(Object obj)
        {
            // 如果是 GameObject，从中获取组件
            if (obj is GameObject gameObject)
            {
                // 尝试获取所有继承自 Entity 的组件
                Component[] components = gameObject.GetComponents(typeof(Entity));
                if (components.Length > 0)
                {
                    return components[0] as Entity;
                }
            }
            // 如果是 Component，直接转换
            else if (obj is Component component)
            {
                return component as Entity;
            }

            return null;
        }

    }
}