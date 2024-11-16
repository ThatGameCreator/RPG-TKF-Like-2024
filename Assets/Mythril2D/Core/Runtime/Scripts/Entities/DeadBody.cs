using System.Collections;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class DeadBody : OtherEntity
    {
        [Header("References")]

        [Header("Dead Body Settings")]
        [SerializeField] private ChestLoot m_loot;
        [SerializeField] private string m_gameFlagID = "DeadBody_00";

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_openedSound;

        private bool m_opened = false;


        private void Start()
        {

        }

        public bool TryLooted()
        {
            if (m_opened == false)
            {
                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_openedSound);

                if (m_loot.entries != null)
                {
                    foreach (var entry in m_loot.entries)
                    {
                        GameManager.InventorySystem.AddToBag(entry.item, entry.quantity);
                    }

                    if (m_loot.money != 0)
                    {
                        GameManager.InventorySystem.AddMoney(m_loot.money);
                    }
                }
                this.gameObject.layer = LayerMask.NameToLayer("Default");

                return m_opened = true;
            }

            return false;
        }

    }
}
