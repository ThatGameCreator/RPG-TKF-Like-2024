using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

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
        private int m_dropIndex = -1;



        public int dropIndex
        {
            get => m_dropIndex;
            set => m_dropIndex = value;
        }
        

        public Loot Loot
        {
            get => m_loot;
            set => m_loot = value;
        }

        public bool TryLooted()
        {
            // ��������ֻ�е�һ������Ӧ�ÿ������±�0 �Ȼ�ȡ��������
            LootEntry lootItem = m_loot.entries[0];

            if (GameManager.InventorySystem.IsBackpackFull(lootItem.item))
            {
                GameManager.DialogueSystem.Main.PlayNow
                    (LocalizationSettings.StringDatabase.GetLocalizedString("NPCDialogueTable", "id_dialogue_shop_backpack_full"));

                return false;
            }
            else
            {
                if (m_looted == false)
                {
                    GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_lootedSound);

                    if (m_loot.entries != null)
                    {
                        if (GameManager.InventorySystem.IsBackpackFull(lootItem.item) == false)
                        {
                            GameManager.InventorySystem.AddToBag(lootItem.item, lootItem.quantity);
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

                    // �ж�һ�±�����ǲ���֮ǰ�ӹ��Ķ��� ���⴫�͵�ͼʱ�����������
                    GameManager.ItemGenerationSystem.DeleteNullItem(this);

                    return m_looted = true;

                }

                return false;
            }
        }

    }
}
