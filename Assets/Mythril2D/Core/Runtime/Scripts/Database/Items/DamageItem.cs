using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Gyvr.Mythril2D
{
    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Items + nameof(DamageItem))]
    public class DamageItem : Item
    {
        [Header("Effect")]
        [SerializeField] private int m_healthToDamage = 1;
        [SerializeField] private int m_damageType = 1;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_drinkAudio;

        public override void Use(CharacterBase target, EItemLocation location)
        {
            if (GameManager.WarehouseSystem.isOpenning == true)
            {
                if (location == EItemLocation.Bag && GameManager.WarehouseSystem.IsWarehouseFull() == false)
                {
                    GameManager.InventorySystem.RemoveFromBag(this);
                    GameManager.WarehouseSystem.AddToWarehouse(this);
                }
                else if (location == EItemLocation.Warehouse && GameManager.InventorySystem.IsBackpackFull(this) == false)
                {
                    GameManager.InventorySystem.AddToBag(this);
                    GameManager.WarehouseSystem.RemoveFromWarehouse(this);
                }
            }
            else
            {
                target.Damage(new DamageOutputDescriptor
                {
                    source = EDamageSource.Unknown,
                    attacker = this,
                    damage = m_healthToDamage,
                    damageType = EDamageType.None,
                    distanceType = EDistanceType.None,
                    flags = EDamageFlag.None
                });

                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_drinkAudio);
                GameManager.InventorySystem.RemoveFromBag(this);
            }
        }
    }
}
