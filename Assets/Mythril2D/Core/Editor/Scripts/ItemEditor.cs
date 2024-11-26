using Gyvr.Mythril2D;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
    private bool batchEditMode = false; // 批量编辑模式开关
    private string newDisplayName = "";
    private string newDescription = "";
    private int newPrice = 0;
    private bool newIsStackable = false;
    private EItemCategory newCategory = EItemCategory.Consumable;

    public override void OnInspectorGUI()
    {
        // 获取目标对象
        Item item = (Item)target;

        EditorGUILayout.LabelField("Item Details", EditorStyles.boldLabel);

        item.Icon = (Sprite)EditorGUILayout.ObjectField("Icon", item.Icon, typeof(Sprite), false);
        item.DisplayName = EditorGUILayout.TextField("Display Name", item.DisplayName);
        item.Description = EditorGUILayout.TextField("Description", item.Description);
        item.Category = (EItemCategory)EditorGUILayout.EnumPopup("Category", item.Category);
        item.Price = EditorGUILayout.IntField("Price", item.Price);
        item.IsStackable = EditorGUILayout.Toggle("Is Stackable", item.IsStackable);

        // 保存单个条目修改
        if (GUI.changed)
        {
            EditorUtility.SetDirty(item);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Batch Edit Mode", EditorStyles.boldLabel);

        // 批量编辑模式开关
        batchEditMode = EditorGUILayout.Toggle("Enable Batch Edit", batchEditMode);

        if (batchEditMode)
        {
            EditorGUILayout.LabelField("Batch Edit Settings", EditorStyles.boldLabel);

            // 批量编辑字段
            newDisplayName = EditorGUILayout.TextField("New Display Name", newDisplayName);
            newDescription = EditorGUILayout.TextField("New Description", newDescription);
            newPrice = EditorGUILayout.IntField("New Price", newPrice);
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
        // 获取所有选中的对象
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

                selectedItem.Price = newPrice;
                selectedItem.IsStackable = newIsStackable;
                selectedItem.Category = newCategory;

                EditorUtility.SetDirty(selectedItem); // 标记为已修改
            }
        }

        Debug.Log("Batch edits applied to selected items.");
    }
}
