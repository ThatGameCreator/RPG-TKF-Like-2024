using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Items + nameof(HealthPotion))]
    public class HealthPotion : Item
    {
        [Header("Effect")]
        [SerializeField] private int m_healthToRestore = 1;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_drinkAudio;

        public override void Use(CharacterBase target, EItemLocation location)
        {
            if (GameManager.WarehouseSystem.isOpenning == true)
            {
                if (location == EItemLocation.Bag)
                {
                    GameManager.InventorySystem.RemoveFromBag(this);
                    GameManager.WarehouseSystem.AddToWarehouse(this);
                }
                else
                {
                    GameManager.InventorySystem.AddToBag(this);
                    GameManager.WarehouseSystem.RemoveFromWarehouse(this);
                }
            }
            else
            {
                if (target.currentStats[EStat.Health] < target.stats[EStat.Health])
                {
                    int previousHealth = target.currentStats[EStat.Health];
                    target.Heal(m_healthToRestore);
                    int currentHealth = target.currentStats[EStat.Health];
                    int diff = currentHealth - previousHealth;

                    GameManager.DialogueSystem.Main.PlayNow("You recover {0} <health>", diff);
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
