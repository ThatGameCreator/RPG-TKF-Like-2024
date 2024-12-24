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
        GUILayout.Label("����װ������Spritesheet��", EditorStyles.boldLabel);

        spritesheet = (Texture2D)EditorGUILayout.ObjectField("Spritesheet", spritesheet, typeof(Texture2D), false);
        category = (EItemCategory)EditorGUILayout.EnumPopup("װ������", category);
        equipmentType = (EEquipmentType)EditorGUILayout.EnumPopup("װ������", equipmentType);
        equipAudio = (AudioClipResolver)EditorGUILayout.ObjectField("װ����Ч", equipAudio, typeof(AudioClipResolver), false);
        unequipAudio = (AudioClipResolver)EditorGUILayout.ObjectField("ж����Ч", unequipAudio, typeof(AudioClipResolver), false);
        capacity = EditorGUILayout.IntField("����", capacity);

        SerializedObject so = new SerializedObject(this);
        so.ApplyModifiedProperties();

        if (GUILayout.Button("����װ��"))
        {
            CreateEquipmentFromSpritesheet();
        }
    }

    private void CreateEquipmentFromSpritesheet()
    {
        if (spritesheet == null)
        {
            Debug.LogError("����ѡ��һ��Spritesheet��");
            return;
        }

        // ��ȡ�����и�õ�Sprite
        string path = AssetDatabase.GetAssetPath(spritesheet);
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogError("δ�ҵ��κ��и��Sprite��");
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

            newEquipment.DisplayName = sprite.name; // ʹ��Sprite������
            newEquipment.Icon = sprite; // ����SpriteΪͼ��
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
        Debug.Log($"�ѳɹ����� {sprites.Length} ��װ����");
    }
}
