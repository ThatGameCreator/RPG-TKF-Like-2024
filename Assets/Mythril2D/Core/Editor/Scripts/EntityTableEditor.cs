using Gyvr.Mythril2D;
using UnityEditor;
using UnityEngine;

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
            if (GUILayout.Button("Add New Entity"))
            {
                if (entityTable.entries == null)
                {
                    entityTable.entries = new EntityTable.EntityData[0];
                }
                ArrayUtility.Add(ref entityTable.entries, new EntityTable.EntityData());
            }

            // 保存修改
            if (GUI.changed)
            {
                EditorUtility.SetDirty(entityTable);
            }
        }
    }
}