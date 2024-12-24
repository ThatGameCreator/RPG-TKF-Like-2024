using Gyvr.Mythril2D;
using UnityEditor;
using UnityEngine;

public class BatchItemEditor : EditorWindow
{
    private Object[] selectedItems; // 存储选中的 Item 对象
    private string newDisplayName;
    private string newDescription;
    private int? newPrice; // 使用可空类型，表示不修改此值
    private bool? newIsStackable;
    private EItemCategory? newCategory;

    [MenuItem("Tools/Batch Item Editor")]
    public static void ShowWindow()
    {
        GetWindow<BatchItemEditor>("Batch Item Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Batch Edit Items", EditorStyles.boldLabel);

        // 显示选中的对象
        if (GUILayout.Button("Refresh Selected Items"))
        {
            RefreshSelectedItems();
        }

        if (selectedItems != null && selectedItems.Length > 0)
        {
            EditorGUILayout.LabelField($"Selected Items: {selectedItems.Length}");
        }
        else
        {
            EditorGUILayout.HelpBox("No Items selected. Please select Items in the Project view.", MessageType.Info);
        }

        EditorGUILayout.Space();

        // 编辑字段
        newDisplayName = EditorGUILayout.TextField("New Display Name", newDisplayName);
        newDescription = EditorGUILayout.TextField("New Description", newDescription);

        if (GUILayout.Button("Clear Display Name and Description"))
        {
            newDisplayName = null;
            newDescription = null;
        }

        newPrice = EditorGUILayout.IntField("New Price", newPrice ?? 0);
        if (GUILayout.Button("Clear Price")) newPrice = null;

        newIsStackable = EditorGUILayout.Toggle("New Stackable Value", newIsStackable ?? false);
        if (GUILayout.Button("Clear Stackable Value")) newIsStackable = null;

        newCategory = (EItemCategory)EditorGUILayout.EnumPopup("New Category", newCategory ?? EItemCategory.Consumable);
        if (GUILayout.Button("Clear Category")) newCategory = null;

        EditorGUILayout.Space();

        // 应用修改
        if (GUILayout.Button("Apply Changes to Selected Items"))
        {
            ApplyChanges();
        }
    }

    private void RefreshSelectedItems()
    {
        selectedItems = Selection.objects; // 获取选中的对象
    }

    private void ApplyChanges()
    {
        if (selectedItems == null || selectedItems.Length == 0)
        {
            Debug.LogWarning("No Items selected for batch editing.");
            return;
        }

        foreach (Object obj in selectedItems)
        {
            if (obj is Item item)
            {
                Undo.RecordObject(item, "Batch Edit Item");

                if (!string.IsNullOrEmpty(newDisplayName)) item.DisplayName = newDisplayName;
                if (!string.IsNullOrEmpty(newDescription)) item.Description = newDescription;
                if (newPrice.HasValue) item.buyPrice = newPrice.Value;
                if (newPrice.HasValue) item.sellPrice = newPrice.Value;
                if (newIsStackable.HasValue) item.IsStackable = newIsStackable.Value;
                if (newCategory.HasValue) item.Category = newCategory.Value;

                EditorUtility.SetDirty(item); // 标记为已修改
            }
        }

        Debug.Log($"Batch edit applied to {selectedItems.Length} items.");
    }
}
