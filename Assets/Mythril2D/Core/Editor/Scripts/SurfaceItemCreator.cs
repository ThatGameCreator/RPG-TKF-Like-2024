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
            // ָ��Item���ݵ�·��
            string itemFolderPath = "Assets/Mythril2D/Demo/Database/Items/Monster Drops/"; // �޸�Ϊ����Item���ݵ��ļ���·��
            string[] itemGuids = AssetDatabase.FindAssets("t:Item", new[] { itemFolderPath });

            foreach (string guid in itemGuids)
            {
                string itemPath = AssetDatabase.GUIDToAssetPath(guid);
                Item item = AssetDatabase.LoadAssetAtPath<Item>(itemPath);

                if (item == null) continue;

                // ���� SurfaceItem ��Ϸ����
                GameObject surfaceItemObject = new GameObject(item.DisplayName);
                SurfaceItem surfaceItem = surfaceItemObject.AddComponent<SurfaceItem>();
                // ���� Layer Ϊ Interaction
                surfaceItemObject.layer = LayerMask.NameToLayer("Interaction");

                // ��Ӳ����� Box Collider 2D
                BoxCollider2D boxCollider = surfaceItemObject.AddComponent<BoxCollider2D>();
                boxCollider.offset = new Vector2(0f, 0f);
                boxCollider.size = new Vector2(0.6f, 0.6f);
                boxCollider.usedByComposite = false;

                // ��Ӳ����� Light Collider 2D���Զ���ű���
                LightCollider2D lightCollider = surfaceItemObject.AddComponent<LightCollider2D>();
                lightCollider.shadowType = LightCollider2D.ShadowType.Collider2D;
                lightCollider.shadowLayer = LayerMask.NameToLayer("Collision D");
                lightCollider.shadowDistance = LightCollider2D.ShadowDistance.Finite;
                lightCollider.shadowTranslucency = 1.0f;

                // ��Ӳ����� Light Event Listener���Զ���ű���
                LightEventListener lightEventListener = surfaceItemObject.AddComponent<LightEventListener>();
                lightEventListener.useDistance = false;
                lightEventListener.visability = 0.0f;
                surfaceItem.LightEventListener = lightEventListener;

                // ��������������
                surfaceItem.IInteraction = new SurfaceItemInteraction
                {
                    SurfaceItem = surfaceItem
                };

                // ���� SpriteRenderer
                SpriteRenderer spriteRenderer = surfaceItemObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = item.Icon;

                // ʹ�� SerializedObject ���� m_spriteRenderer
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
                    money = 0 // �������Ҫ��������ֵ
                };
                surfaceItem.Loot = loot;


                // ����Ԥ����·��
                string prefabPath = $"Assets/Mythril2D/Demo/Prefabs/Entities/Generate/Monster Drops/{item.DisplayName}.prefab";
                string directoryPath = System.IO.Path.GetDirectoryName(prefabPath);
                if (!AssetDatabase.IsValidFolder(directoryPath))
                {
                    System.IO.Directory.CreateDirectory(directoryPath);
                }

                // ����ΪԤ����
                PrefabUtility.SaveAsPrefabAsset(surfaceItemObject, prefabPath);

                // ������ʱ����
                DestroyImmediate(surfaceItemObject);

                Debug.Log($"Created SurfaceItem prefab for {item.DisplayName} at {prefabPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
