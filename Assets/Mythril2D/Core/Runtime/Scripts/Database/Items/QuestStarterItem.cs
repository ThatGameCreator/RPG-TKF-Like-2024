using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Items + nameof(QuestStarterItem))]
    public class QuestStarterItem : Item
    {
        [Header("Settings")]
        [SerializeField] private string m_dialogueLine;
        [SerializeField] private Quest m_questToStart;
        [SerializeField] private bool m_destroyAfterUse;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_useAudio;

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
                bool canPlayQuest =
                !GameManager.JournalSystem.IsQuestActive(m_questToStart) &&
                !GameManager.JournalSystem.IsQuestFullfilled(m_questToStart) &&
                (!GameManager.JournalSystem.IsQuestCompleted(m_questToStart) || m_questToStart.repeatable);

                if (canPlayQuest)
                {
                    GameManager.DialogueSystem.Main.PlayNow(m_dialogueLine);
                    GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_useAudio);
                    GameManager.JournalSystem.StartQuest(m_questToStart);

                    if (m_destroyAfterUse)
                    {
                        GameManager.InventorySystem.RemoveFromBag(this);
                    }
                }
                else
                {
                    base.Use(target, location);
                }
            }
        }
    }
}
