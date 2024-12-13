using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Items + nameof(AttackPotion))]
    public class AttackPotion : Item
    {
        [Header("Effect")]
        [SerializeField] private int m_AttackPowerToIncrease = 1;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_drinkAudio;

        public override void Use(CharacterBase target, EItemLocation location)
        {
            if (GameManager.WarehouseSystem.isOpenning == true)
            {
                if (location == EItemLocation.Bag && GameManager.WarehouseSystem.IsWarehouseFull(this) == false)
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
                if (target.currentStats[EStat.Health] < target.maxStats[EStat.Health])
                {
                    int previousHealth = target.currentStats[EStat.Health];
                    //target.Heal(m_healthToRestore);
                    int currentHealth = target.currentStats[EStat.Health];
                    int diff = currentHealth - previousHealth;

                    GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_drinkAudio);
                    GameManager.InventorySystem.RemoveFromBag(this);
                }
                else
                {
                    base.Use(target, location);
                }
            }
        }
    }
}
