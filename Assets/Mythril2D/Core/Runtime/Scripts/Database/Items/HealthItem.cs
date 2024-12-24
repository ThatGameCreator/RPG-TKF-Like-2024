using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Items + nameof(HealthItem))]
    public class HealthItem : Item
    {
        [Header("Effect")]
        [SerializeField] private float m_healthToRestoreProportion = 1;

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

                    // 计算恢复量：最大生命值乘以恢复比例，然后向上取整
                    int healthToRestore = Mathf.CeilToInt(target.maxStats[EStat.Health] * m_healthToRestoreProportion);

                    // 恢复生命值
                    target.Heal(healthToRestore);

                    int currentHealth = target.currentStats[EStat.Health];
                    int diff = currentHealth - previousHealth;


                    //GameManager.DialogueSystem.Main.PlayNow("You recover {0} <health>", diff);
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
