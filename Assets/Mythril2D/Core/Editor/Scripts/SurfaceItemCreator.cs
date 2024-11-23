using UnityEngine;
using UnityEditor;
using Gyvr.Mythril2D;
using FunkyCode;

namespace Gyvr.Mythril2D
{

    public class SurfaceItemCreator : MonoBehaviour
    {
        [MenuItem("Tools/Create SurfaceItem Prefabs")]
        public static void CreateSurfaceItemPrefabs()
        {
            // 指定Item数据的路径
            string itemFolderPath = "Assets/Mythril2D/Demo/Database/Items/Equipments"; // 修改为你存放Item数据的文件夹路径
            string[] itemGuids = AssetDatabase.FindAssets("t:Item", new[] { itemFolderPath });

            foreach (string guid in itemGuids)
            {
                string itemPath = AssetDatabase.GUIDToAssetPath(guid);
                Item item = AssetDatabase.LoadAssetAtPath<Item>(itemPath);

                if (item == null) continue;

                // 创建 SurfaceItem 游戏对象
                GameObject surfaceItemObject = new GameObject(item.displayName);
                SurfaceItem surfaceItem = surfaceItemObject.AddComponent<SurfaceItem>();
                // 设置 Layer 为 Interaction
                surfaceItemObject.layer = LayerMask.NameToLayer("Interaction");

                // 添加并配置 Box Collider 2D
                BoxCollider2D boxCollider = surfaceItemObject.AddComponent<BoxCollider2D>();
                boxCollider.offset = new Vector2(0f, 0f);
                boxCollider.size = new Vector2(1f, 1f);
                boxCollider.usedByComposite = false;

                // 添加并配置 Light Collider 2D（自定义脚本）
                LightCollider2D lightCollider = surfaceItemObject.AddComponent<LightCollider2D>();
                lightCollider.shadowType = LightCollider2D.ShadowType.Collider2D;
                lightCollider.shadowLayer = LayerMask.NameToLayer("Default");
                lightCollider.shadowDistance = LightCollider2D.ShadowDistance.Finite;
                lightCollider.shadowTranslucency = 1.0f;

                // 添加并配置 Light Event Listener（自定义脚本）
                LightEventListener lightEventListener = surfaceItemObject.AddComponent<LightEventListener>();
                lightEventListener.useDistance = false;
                lightEventListener.visability = 0.0f;
                surfaceItem.LightEventListener = lightEventListener;

                // 互动方法的引用
                surfaceItem.IInteraction = new SurfaceItemInteraction
                {
                    SurfaceItem = surfaceItem
                };

                // 设置 SpriteRenderer
                SpriteRenderer spriteRenderer = surfaceItemObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = item.icon;

                // 使用 SerializedObject 设置 m_spriteRenderer
                SerializedObject serializedObject = new SerializedObject(surfaceItem);
                SerializedProperty spriteRendererProperty = serializedObject.FindProperty("m_spriteRenderer");
                spriteRendererProperty.arraySize = 1;
                spriteRendererProperty.GetArrayElementAtIndex(0).objectReferenceValue = spriteRenderer;
                serializedObject.ApplyModifiedProperties();

                Loot loot = new Loot
                {
                    entries = new[]
        {
                    new LootEntry { item = item, quantity = 1 }
                },
                    money = 0 // 或根据需要设置其他值
                };
                surfaceItem.Loot = loot;


                // 创建预制体路径
                string prefabPath = $"Assets/Mythril2D/Demo/Prefabs/Entities/Generate/{item.displayName}.prefab";
                string directoryPath = System.IO.Path.GetDirectoryName(prefabPath);
                if (!AssetDatabase.IsValidFolder(directoryPath))
                {
                    System.IO.Directory.CreateDirectory(directoryPath);
                }

                // 保存为预制体
                PrefabUtility.SaveAsPrefabAsset(surfaceItemObject, prefabPath);

                // 清理临时对象
                DestroyImmediate(surfaceItemObject);

                Debug.Log($"Created SurfaceItem prefab for {item.displayName} at {prefabPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
