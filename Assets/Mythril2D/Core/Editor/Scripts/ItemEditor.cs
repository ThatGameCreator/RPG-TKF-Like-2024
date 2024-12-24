using Gyvr.Mythril2D;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
    private bool batchEditMode = false; // �����༭ģʽ����
    private string newDisplayName = "";
    private string newDescription = "";
    private int newBuyPrice = 0;
    private int newSellPrice = 0;
    private bool newIsStackable = false;
    private EItemCategory newCategory = EItemCategory.Consumable;

    public override void OnInspectorGUI()
    {
        // ��ȡĿ�����
        Item item = (Item)target;

        EditorGUILayout.LabelField("Item Details", EditorStyles.boldLabel);

        item.Icon = (Sprite)EditorGUILayout.ObjectField("Icon", item.Icon, typeof(Sprite), false);
        item.DisplayName = EditorGUILayout.TextField("Display Name", item.DisplayName);
        item.Description = EditorGUILayout.TextField("Description", item.Description);
        item.Category = (EItemCategory)EditorGUILayout.EnumPopup("Category", item.Category);
        item.buyPrice = EditorGUILayout.IntField("Price", item.buyPrice);
        item.sellPrice = EditorGUILayout.IntField("Price", item.sellPrice);
        item.IsStackable = EditorGUILayout.Toggle("Is Stackable", item.IsStackable);

        // ���浥����Ŀ�޸�
        if (GUI.changed)
        {
            EditorUtility.SetDirty(item);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Batch Edit Mode", EditorStyles.boldLabel);

        // �����༭ģʽ����
        batchEditMode = EditorGUILayout.Toggle("Enable Batch Edit", batchEditMode);

        if (batchEditMode)
        {
            EditorGUILayout.LabelField("Batch Edit Settings", EditorStyles.boldLabel);

            // �����༭�ֶ�
            newDisplayName = EditorGUILayout.TextField("New Display Name", newDisplayName);
            newDescription = EditorGUILayout.TextField("New Description", newDescription);
            newBuyPrice = EditorGUILayout.IntField("New Buy Price", newBuyPrice);
            newSellPrice = EditorGUILayout.IntField("New Sell Price", newSellPrice);
            newIsStackable = EditorGUILayout.Toggle("New Stackable Value", newIsStackable);
            newCategory = (EItemCategory)EditorGUILayout.EnumPopup("New Category", newCategory);

            if (GUILayout.Button("Apply to All Selected Items"))
            {
                ApplyBatchEdits();
            }
        }
    }

    private void ApplyBatchEdits()
    {
        // ��ȡ����ѡ�еĶ���
        Object[] selectedObjects = Selection.objects;
        foreach (Object obj in selectedObjects)
        {
            if (obj is Item selectedItem)
            {
                if (!string.IsNullOrEmpty(newDisplayName))
                {
                    selectedItem.DisplayName = newDisplayName;
                }

                if (!string.IsNullOrEmpty(newDescription))
                {
                    selectedItem.Description = newDescription;
                }

                selectedItem.buyPrice = newBuyPrice;
                selectedItem.sellPrice = newSellPrice;
                selectedItem.IsStackable = newIsStackable;
                selectedItem.Category = newCategory;

                EditorUtility.SetDirty(selectedItem); // ���Ϊ���޸�
            }
        }

        Debug.Log("Batch edits applied to selected items.");
    }
}
