using Gyvr.Mythril2D;
using UnityEditor;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CustomEditor(typeof(LootTable))]
    public class LootTableEditor : Editor
    {

        private float newLootItemWeight = 1f; // ����Ŀ��Ĭ��Ȩ��
        private bool showWeightDistributionGraph = true;
        private bool showItemTableEntries = true;

        public override void OnInspectorGUI()
        {
            LootTable lootTable = (LootTable)target;

            EditorGUILayout.LabelField("Loot Table Configuration", EditorStyles.boldLabel);


            // ����Ȩ�طֲ�����ͼ
            DrawWeightDistributionGraph(lootTable);

            // ��ʾ�ͱ༭��Ŀ����
            DrawEntityTableItems(lootTable);


            // ����µ���Ŀ��ť
            if (GUILayout.Button("Add New Loot Entry"))
            {
                if (lootTable.entries == null)
                {
                    lootTable.entries = new LootTable.LootEntryData[0];
                }
                ArrayUtility.Add(ref lootTable.entries, new LootTable.LootEntryData());
            }

            // ��Ǯ����
            lootTable.money = EditorGUILayout.IntField("Max Money", lootTable.money);
            // �Ӷ����
            lootTable.maxLootedCount = EditorGUILayout.IntField("Max Looted Count", lootTable.maxLootedCount);
            // �Ӷ����
            lootTable.lootRate = EditorGUILayout.IntField("Looted Rate", lootTable.lootRate);

            // ����µ���Ŀ
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Add New Entity", EditorStyles.boldLabel);
            newLootItemWeight = EditorGUILayout.FloatField("Default Weight", newLootItemWeight);
            // ����µ���Ŀ��ť
            if (GUILayout.Button("Add Selected Items"))
            {
                AddSelectedItems(lootTable);
            }

            // ɾ��ȫ����Ŀ
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Delete All Entities", EditorStyles.boldLabel);
            if (GUILayout.Button("Delete All"))
            {
                DeleteAllItems(lootTable);
            }

            // �����޸�
            if (GUI.changed)
            {
                EditorUtility.SetDirty(lootTable);
            }
        }

        private void DrawWeightDistributionGraph(LootTable lootTable)
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
                // ����Ȩ�طֲ�����ͼ
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
            }
        }

        private void DrawEntityTableItems(LootTable lootTable)
        {

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Entity Table Entries", EditorStyles.boldLabel);
            if (GUILayout.Button(showItemTableEntries ? "Hide" : "Show"))
            {
                showItemTableEntries = !showItemTableEntries;
            }
            EditorGUILayout.EndHorizontal();

            if (showItemTableEntries)
            {
                // ԭ�е�Ȩ�طֲ�����ͼ����
                // ��ʾ�ͱ༭��Ŀ����
                if (lootTable.entries != null)
                {
                    for (int i = 0; i < lootTable.entries.Length; i++)
                    {
                        var entry = lootTable.entries[i];
                        EditorGUILayout.BeginVertical("box");
                        entry.item = (Item)EditorGUILayout.ObjectField("Item", entry.item, typeof(Item), false);
                        entry.maxQuantity = EditorGUILayout.IntField("Max Quantity", entry.maxQuantity);
                        entry.weight = EditorGUILayout.FloatField("Weight", entry.weight);

                        // ɾ����ť
                        if (GUILayout.Button($"Remove Entity {i + 1}"))
                        {
                            // ���������Ƴ�
                            ArrayUtility.RemoveAt(ref lootTable.entries, i);
                            break; // ������ǰѭ�������� IndexOutOfRangeException
                        }

                        EditorGUILayout.EndVertical();
                    }
                }
            }
        }

        private void AddSelectedItems(LootTable lootTable)
        {
            Debug.Log($"Selected Objects Count: {Selection.objects.Length}");

            foreach (Object selected in Selection.objects)
            {
                Debug.Log($"Selected Object: {selected}, Type: {selected.GetType()}");

                // ���Դ�ѡ�еĶ����л�ȡ Entity ���
                Item entityToAdd = GetItemComponent(selected);

                if (entityToAdd != null)
                {
                    Debug.Log($"Adding Item: {entityToAdd.name}");

                    LootTable.LootEntryData newEntry = new LootTable.LootEntryData
                    {
                        item = entityToAdd,
                        maxQuantity = 1,
                        weight = newLootItemWeight
                    };
                    ArrayUtility.Add(ref lootTable.entries, newEntry);
                }
                else
                {
                    Debug.Log($"Could not find Entity component on {selected.name}");
                }
            }
        }

        private void DeleteAllItems(LootTable lootTable)
        {
            // ɾ�� EntityTable �е�������Ŀ
            lootTable.entries = new LootTable.LootEntryData[0];
        }

        private Item GetItemComponent(Object obj)
        {
            if (obj is ScriptableObject scriptableObject)
            {
                Item item = scriptableObject as Item;
                if (item != null)
                {
                    return item;
                }
            }

            return null;
        }
    }
}
