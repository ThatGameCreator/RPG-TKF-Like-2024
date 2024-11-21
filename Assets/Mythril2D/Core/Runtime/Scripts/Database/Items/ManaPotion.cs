using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Items + nameof(ManaPotion))]
    public class ManaPotion : Item
    {
        [Header("Effect")]
        [SerializeField] private int m_manaToRestore = 1;

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
                else if (location == EItemLocation.Warehouse && GameManager.InventorySystem.IsBackpackFull() == false)
                {
                    GameManager.InventorySystem.AddToBag(this);
                    GameManager.WarehouseSystem.RemoveFromWarehouse(this);
                }
            }
            else
            {
                if (target.currentStats[EStat.Mana] < target.stats[EStat.Mana])
                {
                    int previousMana = target.currentStats[EStat.Mana];
                    target.RecoverMana(m_manaToRestore);
                    int currentMana = target.currentStats[EStat.Mana];
                    int diff = currentMana - previousMana;

                    GameManager.DialogueSystem.Main.PlayNow("You recover {0} <mana>", diff);
                    GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_drinkAudio);
                    GameManager.GetSystem<InventorySystem>().RemoveFromBag(this);
                }
                else
                {
                    base.Use(target, location);
                }
            }
        }
    }
}
