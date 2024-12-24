using Gyvr.Mythril2D;
using UnityEditor;
using UnityEngine;
using System.Collections;

namespace Gyvr.Mythril2D
{
    [CustomEditor(typeof(EntityTable))]
    public class EntityTableEditor : Editor
    {
        private float newEntryWeight = 1f; // 新条目的默认权重
        private bool showWeightDistributionGraph = true;
        private bool showEntityTableEntries = true;

        public override void OnInspectorGUI()
        {
            EntityTable entityTable = (EntityTable)target;

            EditorGUILayout.LabelField("Entity Table Configuration", EditorStyles.boldLabel);

            // 绘制权重分布条形图
            DrawWeightDistributionGraph(entityTable);

            // 显示和编辑条目数据
            DrawEntityTableEntries(entityTable);

            // 添加新的条目按钮
            if (GUILayout.Button("Add New Entry"))
            {
                if (entityTable.entries == null)
                {
                    entityTable.entries = new EntityTable.EntityData[0];
                }
                ArrayUtility.Add(ref entityTable.entries, new EntityTable.EntityData());
            }

            // 生成概率
            entityTable.generateRate = EditorGUILayout.IntField("Generate Rate", entityTable.generateRate);

            // 添加新的条目
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Add New Entity", EditorStyles.boldLabel);
            newEntryWeight = EditorGUILayout.FloatField("Default Weight", newEntryWeight);

            if (GUILayout.Button("Add Selected Entities"))
            {
                AddSelectedEntities(entityTable);
            }

            // 删除全部条目
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Delete All Entities", EditorStyles.boldLabel);
            if (GUILayout.Button("Delete All"))
            {
                DeleteAllEntities(entityTable);
            }

            // 保存修改
            if (GUI.changed)
            {
                EditorUtility.SetDirty(entityTable);
            }
        }


        private void DrawWeightDistributionGraph(EntityTable entityTable)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Weight Distribution Graph", EditorStyles.boldLabel);
            if (GUILayout.Button(showWeightDistributionGraph ? "Hide" : "Show"))
            {
                showWeightDistributionGraph = !showWeightDistributionGraph;
            }
            EditorGUILayout.EndHorizontal();

            if (showWeightDistributionGraph)
            {
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
            }
        }

        private void DrawEntityTableEntries(EntityTable entityTable)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Entity Table Entries", EditorStyles.boldLabel);
            if (GUILayout.Button(showEntityTableEntries ? "Hide" : "Show"))
            {
                showEntityTableEntries = !showEntityTableEntries;
            }
            EditorGUILayout.EndHorizontal();

            if (showEntityTableEntries)
            {
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
            }
        }

        private void AddSelectedEntities(EntityTable entityTable)
        {
            // 从选中的对象中获取 Entity 组件并添加到 EntityTable
            // 添加新的条目按钮
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
                        weight = newEntryWeight
                    };
                    ArrayUtility.Add(ref entityTable.entries, newEntry);
                }
                else
                {
                    Debug.Log($"Could not find Entity component on {selected.name}");
                }
            }
        }
        private void DeleteAllEntities(EntityTable entityTable)
        {
            // 删除 EntityTable 中的所有条目
            entityTable.entries = new EntityTable.EntityData[0];
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