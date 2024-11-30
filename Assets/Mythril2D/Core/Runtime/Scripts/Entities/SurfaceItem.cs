using System.Collections;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class SurfaceItem : OtherEntity
    {
        [Header("References")]

        [Header("SurfaceItem Settings")]
        [SerializeField] private Loot m_loot;
        [SerializeField] private string m_gameFlagID = "SurfaceItem_00";

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_lootedSound;

        private bool m_looted = false;

        public Loot Loot
        {
            get => m_loot;
            set => m_loot = value;
        }

        public bool TryLooted()
        {
            if (GameManager.InventorySystem.IsBackpackFull())
            {
                GameManager.DialogueSystem.Main.PlayNow("Backpack is full...");

                return false;
            }
            else
            {
                if (m_looted == false)
                {
                    GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_lootedSound);

                    if (m_loot.entries != null)
                    {
                        if (GameManager.InventorySystem.IsBackpackFull() == false)
                        {
                            foreach (var entry in m_loot.entries)
                            {
                                GameManager.InventorySystem.AddToBag(entry.item, entry.quantity);
                            }
                        }
                        else
                        {
                            return false;
                        }

                        if (m_loot.money != 0)
                        {
                            GameManager.InventorySystem.AddMoney(m_loot.money);
                        }
                    }
                    Destroy(this.gameObject);

                    return m_looted = true;
                }

                return false;
            }
        }

    }
}
