using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Gyvr.Mythril2D
{
    public enum EEquipmentType
    {
        Weapon,
        Head,
        Torso,
        Hands,
        Feet,
        Backpack
    }

    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Items + nameof(Equipment))]
    public class Equipment : Item
    {
        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_equipAudio;
        [SerializeField] private AudioClipResolver m_unequipAudio;

        [Header("Equipment")]
        [SerializeField] private EEquipmentType m_type;
        [SerializeField] private Stats m_bonusStats;
        [SerializeField] private SpriteLibraryAsset m_visualOverride;
        [SerializeField] private int m_capacity;
        [SerializeField] private AbilitySheet[] m_ability;

        public EEquipmentType type => m_type;
        public Stats bonusStats => m_bonusStats;
        public int capacity => m_capacity;
        public AbilitySheet[] ability => m_ability;
        public SpriteLibraryAsset visualOverride => m_visualOverride;

        public override void Use(CharacterBase user, EItemLocation location)
        {
            if (GameManager.WarehouseSystem.isOpenning == true)
            {
                if (location == EItemLocation.Bag && GameManager.WarehouseSystem.IsWarehouseFull() == false)
                {
                    GameManager.InventorySystem.RemoveFromBag(this);
                    GameManager.WarehouseSystem.AddToWarehouse(this);
                }
                else if(location == EItemLocation.Warehouse && GameManager.InventorySystem.IsBackpackFull() == false)
                {
                    GameManager.InventorySystem.AddToBag(this);
                    GameManager.WarehouseSystem.RemoveFromWarehouse(this);
                }
            }
            else
            {
                if (location == EItemLocation.Bag)
                {
                    GameManager.InventorySystem.Equip(this);
                    GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_equipAudio);
                }
                else
                {
                    GameManager.InventorySystem.UnEquip(type);
                    GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_unequipAudio);
                }
            }
            
        }
    }
}
