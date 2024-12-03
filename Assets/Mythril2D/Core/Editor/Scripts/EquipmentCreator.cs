using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using Gyvr.Mythril2D;

public class EquipmentCreatorWindow : EditorWindow
{
    private Texture2D spritesheet;
    private EItemCategory category = EItemCategory.Gear;
    private EEquipmentType equipmentType = EEquipmentType.Weapon;
    private int capacity = 0;
    private AudioClipResolver equipAudio;
    private AudioClipResolver unequipAudio;
    private AbilitySheet[] abilitySheets;
    private Stats bonusStats;

    [MenuItem("Tools/Create Equipment from Spritesheet")]
    public static void ShowWindow()
    {
        GetWindow<EquipmentCreatorWindow>("Equipment Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("创建装备（从Spritesheet）", EditorStyles.boldLabel);

        spritesheet = (Texture2D)EditorGUILayout.ObjectField("Spritesheet", spritesheet, typeof(Texture2D), false);
        category = (EItemCategory)EditorGUILayout.EnumPopup("装备类型", category);
        equipmentType = (EEquipmentType)EditorGUILayout.EnumPopup("装备类型", equipmentType);
        equipAudio = (AudioClipResolver)EditorGUILayout.ObjectField("装备音效", equipAudio, typeof(AudioClipResolver), false);
        unequipAudio = (AudioClipResolver)EditorGUILayout.ObjectField("卸下音效", unequipAudio, typeof(AudioClipResolver), false);
        capacity = EditorGUILayout.IntField("容量", capacity);

        SerializedObject so = new SerializedObject(this);
        so.ApplyModifiedProperties();

        if (GUILayout.Button("生成装备"))
        {
            CreateEquipmentFromSpritesheet();
        }
    }

    private void CreateEquipmentFromSpritesheet()
    {
        if (spritesheet == null)
        {
            Debug.LogError("请先选择一个Spritesheet！");
            return;
        }

        // 获取所有切割好的Sprite
        string path = AssetDatabase.GetAssetPath(spritesheet);
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogError("未找到任何切割的Sprite！");
            return;
        }

        string savePath = "Assets/Mythril2D/Demo/Database/Items/Equipments/";
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        foreach (var sprite in sprites)
        {
            Equipment newEquipment = ScriptableObject.CreateInstance<Equipment>();

            newEquipment.DisplayName = sprite.name; // 使用Sprite的名字
            newEquipment.Icon = sprite; // 设置Sprite为图标
            newEquipment.Category = category;
            newEquipment.type = equipmentType;
            newEquipment.capacity = capacity;
            newEquipment.ability = abilitySheets;
            newEquipment.EquipAudio = equipAudio;
            newEquipment.UnequipAudio = unequipAudio;


            string assetPath = $"{savePath}{sprite.name}.asset";
            AssetDatabase.CreateAsset(newEquipment, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"已成功创建 {sprites.Length} 个装备！");
    }
}
