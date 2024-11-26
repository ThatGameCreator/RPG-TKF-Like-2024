using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Gyvr.Mythril2D;
using System;


namespace Gyvr.Mythril2D
{
    public class ItemManager : EditorWindow
    {
        private List<Item> allItems = new List<Item>();
        private Vector2 scrollPosition;
        private string searchQuery = "";
        private EItemCategory? filterCategory = null;
        private EItemLocation? filterLocation = null;
        private EEquipmentType? filterEquipmentType = null;
        private System.Type filterType = typeof(Item);
        private List<Item> filteredItemsCache = null;
        private bool filtersChanged = true;

        // Add refresh button state
        private bool needsRefresh = false;
        private float lastRefreshTime;
        private const float AUTO_REFRESH_INTERVAL = 60f;

        // Icon display settings
        private const float ICON_PREVIEW_SIZE = 64f;
        private const float ICON_BUTTON_SIZE = 80f;
        private GUIStyle iconPreviewStyle;
        private bool stylesInitialized = false;


        [MenuItem("Tools/Item Manager")]
        public static void ShowWindow()
        {
            GetWindow<ItemManager>("Item Manager");
        }

        private void OnEnable()
        {
            LoadAllItems();
            EditorApplication.projectChanged += OnProjectChanged;
        }

        private void InitializeStyles()
        {
            // Only initialize if not already done
            if (!stylesInitialized || iconPreviewStyle == null)
            {
                iconPreviewStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(2, 2, 2, 2),
                    stretchWidth = false,
                    stretchHeight = false
                };
                stylesInitialized = true;
            }
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= OnProjectChanged;
        }

        private void OnProjectChanged()
        {
            needsRefresh = true;
        }

        private void LoadAllItems()
        {
            allItems.Clear();
            string[] guids = AssetDatabase.FindAssets("t:Item", new[] { "Assets" });

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Item item = AssetDatabase.LoadAssetAtPath<Item>(assetPath);

                if (item != null)
                {
                    allItems.Add(item);
                }
                else
                {
                    Debug.LogWarning($"Failed to load Item at path: {assetPath}");
                }
            }

            filtersChanged = true;
            filteredItemsCache = null;
            lastRefreshTime = Time.realtimeSinceStartup;
            Debug.Log($"Loaded {allItems.Count} items");
        }

        private void OnGUI()
        {
            InitializeStyles();

            if (needsRefresh || Time.realtimeSinceStartup - lastRefreshTime > AUTO_REFRESH_INTERVAL)
            {
                LoadAllItems();
                needsRefresh = false;
            }

            EditorGUILayout.BeginVertical();

            if (GUILayout.Button("Refresh Items"))
            {
                LoadAllItems();
            }

            EditorGUILayout.Space();
            DrawFilterSection();
            EditorGUILayout.Space();
            DrawBatchEditSection();
            EditorGUILayout.Space();
            DrawItemList();

            HandleKeyboardShortcuts();

            EditorGUILayout.EndVertical();
        }


        private void DrawFilterSection()
        {
            EditorGUILayout.LabelField("Search and Filter", EditorStyles.boldLabel);

            GUI.SetNextControlName("SearchField");
            string newSearchQuery = EditorGUILayout.TextField("Search", searchQuery);
            if (newSearchQuery != searchQuery)
            {
                searchQuery = newSearchQuery;
                filtersChanged = true;
            }

            EItemCategory? newFilterCategory = (EItemCategory?)EditorGUILayout.EnumPopup("Category", filterCategory ?? (EItemCategory)(-1));
            if (newFilterCategory != filterCategory)
            {
                filterCategory = newFilterCategory;
                filtersChanged = true;
            }

            EEquipmentType? newFilterEquipmentType = (EEquipmentType?)EditorGUILayout.EnumPopup("Equipment Type", filterEquipmentType ?? (EEquipmentType)(-1));
            if (newFilterEquipmentType != filterEquipmentType)
            {
                filterEquipmentType = newFilterEquipmentType;
                filtersChanged = true;
            }

            string[] availableTypes = new string[] { "None", "Item", "Equipment" };
            int selectedTypeIndex = Array.IndexOf(availableTypes, filterType?.Name ?? "None");
            int newSelectedTypeIndex = EditorGUILayout.Popup("Type Filter", selectedTypeIndex, availableTypes);

            if (newSelectedTypeIndex != selectedTypeIndex)
            {
                filterType = availableTypes[newSelectedTypeIndex] == "None" ? null :
                            availableTypes[newSelectedTypeIndex] == "Item" ? typeof(Item) : typeof(Equipment);
                filtersChanged = true;
            }

            if (GUILayout.Button("Clear Filters"))
            {
                ClearFilters();
            }

            EditorGUILayout.LabelField($"Showing {GetFilteredItems().Count} of {allItems.Count} items");
        }

        private void DrawBatchEditSection()
        {
            EditorGUILayout.LabelField("Batch Edit", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                string newName = EditorGUILayout.TextField("New Name for All", "");
                GUI.enabled = !string.IsNullOrEmpty(newName);
                if (GUILayout.Button("Apply Name", GUILayout.Width(100)))
                {
                    ApplyBatchOperation(item =>
                    {
                        item.name = newName;
                        return true;
                    });
                }
                GUI.enabled = true;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                int newPrice = EditorGUILayout.IntField("New Price for All", 0);
                if (GUILayout.Button("Apply Price", GUILayout.Width(100)))
                {
                    ApplyBatchOperation(item =>
                    {
                        item.Price = newPrice;
                        return true;
                    });
                }
            }

            // Batch icon modification
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Batch Icon");
                Sprite newIcon = (Sprite)EditorGUILayout.ObjectField(null, typeof(Sprite), false);
                if (newIcon != null && GUILayout.Button("Apply Icon", GUILayout.Width(100)))
                {
                    ApplyBatchOperation(item =>
                    {
                        item.Icon = newIcon;
                        return true;
                    });
                }
            }

            if (GUILayout.Button("Save All Changes"))
            {
                AssetDatabase.SaveAssets();
                Debug.Log("All changes saved successfully");
            }
        }

        private void DrawItemList()
        {
            EditorGUILayout.LabelField("Items", EditorStyles.boldLabel);

            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;

                var items = GetFilteredItems();
                if (items != null && items.Count > 0)
                {
                    foreach (var item in items.Where(i => i != null))
                    {
                        DrawItemEntry(item);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No items found", EditorStyles.centeredGreyMiniLabel);
                }
            }
        }

        private void DrawItemEntry(Item item)
        {
            try
            {
                // Early validation
                if (item == null)
                {
                    Debug.LogWarning("Attempted to draw entry for null item");
                    return;
                }

                // Create and validate SerializedObject early
                SerializedObject serializedItem = new SerializedObject(item);
                if (serializedItem == null)
                {
                    Debug.LogError($"Failed to create SerializedObject for item: {item.name}");
                    return;
                }

                serializedItem.Update();

                // Main container
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    // Header section
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        // Icon section with fixed width
                        using (new EditorGUILayout.VerticalScope(GUILayout.Width(ICON_BUTTON_SIZE)))
                        {
                            DrawItemIcon(item);
                        }

                        // Title and locate button section
                        using (new EditorGUILayout.VerticalScope())
                        {
                            // Null check for name
                            string itemName = string.IsNullOrEmpty(item.name) ? "Unnamed Item" : item.name;
                            EditorGUILayout.LabelField(itemName, EditorStyles.boldLabel);

                            if (GUILayout.Button("Locate", GUILayout.Width(60)))
                            {
                                // Ensure the item is valid before pinging
                                if (item != null)
                                {
                                    EditorGUIUtility.PingObject(item);
                                    Selection.activeObject = item;
                                }
                            }
                        }
                    }

                    // Properties section
                    EditorGUI.BeginChangeCheck();

                    // Draw properties with null checks and error handling
                    DrawSerializedProperty(serializedItem, "m_displayName", "DisplayName");
                    DrawSerializedProperty(serializedItem, "m_description", "Description");
                    DrawSerializedProperty(serializedItem, "m_category", "Category");
                    DrawSerializedProperty(serializedItem, "m_price", "Price");
                    DrawSerializedProperty(serializedItem, "m_isStackable", "Is Stackable");

                    // Equipment-specific properties
                    if (item is Equipment equipment)
                    {
                        if (equipment != null) // Double-check after cast
                        {
                            DrawEquipmentProperties(equipment);
                        }
                    }

                    // Apply modifications if changes were made
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedItem.ApplyModifiedProperties();
                        EditorUtility.SetDirty(item);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in DrawItemEntry: {e.Message}\nStack Trace: {e.StackTrace}");
            }
        }

        // Helper method to safely draw serialized properties
        private void DrawSerializedProperty(SerializedObject serializedItem, string propertyPath, string label)
        {
            try
            {
                var property = serializedItem.FindProperty(propertyPath);
                if (property != null)
                {
                    EditorGUILayout.PropertyField(property, new GUIContent(label));
                }
                else
                {
                    Debug.LogWarning($"Property not found: {propertyPath} on {serializedItem.targetObject.name}");
                    // Draw a disabled field to show there's a missing property
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.LabelField(label, "Property not found");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error drawing property {propertyPath}: {e.Message}");
            }
        }

        private void DrawItemIcon(Item item)
        {
            try
            {
                if (item == null)
                {
                    Debug.LogWarning("Attempted to draw icon for null item");
                    return;
                }

                EditorGUILayout.Space(2);

                if (iconPreviewStyle == null)
                {
                    iconPreviewStyle = new GUIStyle(GUI.skin.box)
                    {
                        padding = new RectOffset(2, 2, 2, 2),
                        stretchWidth = false,
                        stretchHeight = false
                    };
                }

                Rect iconRect = EditorGUILayout.GetControlRect(GUILayout.Width(ICON_PREVIEW_SIZE), GUILayout.Height(ICON_PREVIEW_SIZE));

                if (Event.current != null && Event.current.type == EventType.Repaint)
                {
                    iconPreviewStyle.Draw(iconRect, false, false, false, false);
                }

                SerializedObject serializedItem = new SerializedObject(item);
                if (serializedItem == null)
                {
                    Debug.LogError("Failed to create SerializedObject for item");
                    return;
                }

                // Draw the sprite slice or placeholder
                if (item.Icon != null)
                {
                    Sprite sprite = item.Icon;
                    if (sprite != null && sprite.texture != null)
                    {
                        // Calculate the normalized texture coordinates for the sprite
                        Rect spriteRect = sprite.rect;
                        Texture2D tex = sprite.texture;

                        float texWidth = tex.width;
                        float texHeight = tex.height;

                        // Calculate UV coordinates
                        Rect uvRect = new Rect(
                            spriteRect.x / texWidth,
                            spriteRect.y / texHeight,
                            spriteRect.width / texWidth,
                            spriteRect.height / texHeight
                        );

                        // Calculate the display rect maintaining aspect ratio
                        float aspectRatio = spriteRect.width / spriteRect.height;
                        Rect displayRect = iconRect;

                        if (aspectRatio > 1)
                        {
                            // Wider than tall
                            float newHeight = displayRect.width / aspectRatio;
                            float yOffset = (displayRect.height - newHeight) * 0.5f;
                            displayRect.height = newHeight;
                            displayRect.y += yOffset;
                        }
                        else
                        {
                            // Taller than wide or square
                            float newWidth = displayRect.height * aspectRatio;
                            float xOffset = (displayRect.width - newWidth) * 0.5f;
                            displayRect.width = newWidth;
                            displayRect.x += xOffset;
                        }

                        // Draw the sprite slice
                        GUI.DrawTextureWithTexCoords(displayRect, tex, uvRect);

                        // Optional: Draw border to show sprite bounds
                        Handles.color = new Color(1, 1, 1, 0.5f);
                        Handles.DrawWireDisc(displayRect.center, Vector3.forward, Mathf.Min(displayRect.width, displayRect.height) * 0.5f);
                    }
                    else
                    {
                        EditorGUI.LabelField(iconRect, "Invalid Sprite", EditorStyles.centeredGreyMiniLabel);
                    }
                }
                else
                {
                    EditorGUI.LabelField(iconRect, "No Icon", EditorStyles.centeredGreyMiniLabel);
                }

                // Icon field
                EditorGUI.BeginChangeCheck();
                var iconProperty = serializedItem.FindProperty("m_icon");
                if (iconProperty != null)
                {
                    EditorGUILayout.PropertyField(iconProperty, GUIContent.none, GUILayout.Width(ICON_BUTTON_SIZE));
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedItem.ApplyModifiedProperties();
                        EditorUtility.SetDirty(item);
                    }
                }
                else
                {
                    Debug.LogError($"Could not find property 'm_icon' on item {item.name}");
                }

                EditorGUILayout.Space(2);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in DrawItemIcon: {e.Message}\nStack Trace: {e.StackTrace}");
            }
        }

        private void DrawEquipmentProperties(Equipment equipment)
        {
            SerializedObject serializedEquipment = new SerializedObject(equipment);
            serializedEquipment.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Equipment Properties", EditorStyles.boldLabel);

            SerializedProperty bonusStatsProp = serializedEquipment.FindProperty("m_bonusStats");
            EditorGUILayout.PropertyField(bonusStatsProp, true);

            equipment.type = (EEquipmentType)EditorGUILayout.EnumPopup("Equipment Type", equipment.type);
            equipment.capacity = EditorGUILayout.IntField("Capacity", equipment.capacity);

            SerializedProperty abilityProp = serializedEquipment.FindProperty("m_ability");
            EditorGUILayout.PropertyField(abilityProp, true);

            serializedEquipment.ApplyModifiedProperties();
        }

        private void ApplyBatchOperation(Func<Item, bool> operation)
        {
            var items = GetFilteredItems();
            int modifiedCount = 0;

            Undo.RecordObjects(items.ToArray(), "Batch Modify Items");

            foreach (var item in items)
            {
                if (operation(item))
                {
                    EditorUtility.SetDirty(item);
                    modifiedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Modified {modifiedCount} items");
        }

        private void ClearFilters()
        {
            searchQuery = "";
            filterCategory = null;
            filterLocation = null;
            filterEquipmentType = null;
            filterType = null;
            filtersChanged = true;
        }

        private void HandleKeyboardShortcuts()
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.F && e.control)
                {
                    GUI.FocusControl("SearchField");
                    e.Use();
                }
                else if (e.keyCode == KeyCode.R && e.control)
                {
                    LoadAllItems();
                    e.Use();
                }
            }
        }

        private List<Item> GetFilteredItems()
        {
            if (filtersChanged || filteredItemsCache == null)
            {
                filteredItemsCache = FilterItems();
                filtersChanged = false;
            }
            return filteredItemsCache;
        }

        private List<Item> FilterItems()
        {
            IEnumerable<Item> items = allItems;

            if (!string.IsNullOrEmpty(searchQuery))
            {
                items = items.Where(item =>
                    item != null &&
                    item.name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                );
            }

            if (filterCategory.HasValue && filterCategory.Value != (EItemCategory)(-1))
            {
                items = items.Where(item => item.Category == filterCategory.Value);
            }

            if (filterEquipmentType.HasValue && filterEquipmentType.Value != (EEquipmentType)(-1))
            {
                items = items.OfType<Equipment>()
                            .Where(equipment => equipment.type == filterEquipmentType.Value)
                            .Cast<Item>();
            }

            if (filterType != null)
            {
                items = items.Where(item => item.GetType() == filterType);
            }

            return items.ToList();
        }
    }
}
