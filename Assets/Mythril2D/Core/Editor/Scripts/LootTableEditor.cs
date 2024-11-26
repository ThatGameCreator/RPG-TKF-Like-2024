using Gyvr.Mythril2D;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LootTable))]
public class LootTableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LootTable lootTable = (LootTable)target;

        EditorGUILayout.LabelField("Loot Table Configuration", EditorStyles.boldLabel);

        // 绘制权重分布条形图
        if (lootTable.entries != null && lootTable.entries.Length > 0)
        {
            float totalWeight = 0f;
            foreach (var entry in lootTable.entries)
            {
                totalWeight += entry.weight;
            }

            foreach (var entry in lootTable.entries)
            {
                float percentage = (totalWeight > 0) ? entry.weight / totalWeight : 0;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(entry.item ? entry.item.name : "Unnamed Item");
                Rect rect = EditorGUILayout.GetControlRect(false, 20);
                EditorGUI.ProgressBar(rect, percentage, $"{percentage * 100:F1}%");
                EditorGUILayout.EndHorizontal();
            }
        }

        // 显示和编辑条目数据
        if (lootTable.entries != null)
        {
            foreach (var entry in lootTable.entries)
            {
                EditorGUILayout.BeginVertical("box");
                entry.item = (Item)EditorGUILayout.ObjectField("Item", entry.item, typeof(Item), false);
                entry.maxQuantity = EditorGUILayout.IntField("Max Quantity", entry.maxQuantity);
                entry.weight = EditorGUILayout.FloatField("Weight", entry.weight);
                EditorGUILayout.EndVertical();
            }
        }

        // 添加新的条目按钮
        if (GUILayout.Button("Add New Loot Entry"))
        {
            if (lootTable.entries == null)
            {
                lootTable.entries = new LootTable.LootEntryData[0];
            }
            ArrayUtility.Add(ref lootTable.entries, new LootTable.LootEntryData());
        }

        // 金钱奖励
        lootTable.money = EditorGUILayout.IntField("Max Money", lootTable.money);

        // 保存修改
        if (GUI.changed)
        {
            EditorUtility.SetDirty(lootTable);
        }
    }
}
