using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;
using static Gyvr.Mythril2D.LootTable;
using static UnityEngine.EventSystems.EventTrigger;

namespace Gyvr.Mythril2D
{
    public class DeadBody : OtherEntity
    {
        [Header("Dead Body Settings")]
        [SerializeField] private LootTable lootTable;   // ���� ScriptableObject ���ݱ�
        [SerializeField] private string m_gameFlagID = "DeadBody_00";
        [SerializeField] private Sprite[] m_deadBodySprites = null;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_lootedSound;

        private bool m_looted = false;
        private int m_randomMaxLootedCount = 0;
        private int m_nowLootedCount = 0;

        protected override void Start()
        {
            base.Start();

            m_randomMaxLootedCount = Random.Range(0, lootTable.maxLootedCount);

            // ����Ƿ��Ѵﵽ����Ӷ����
            if (m_nowLootedCount >= m_randomMaxLootedCount)
            {
                this.gameObject.layer = LayerMask.NameToLayer("Collision D"); // ����Ϊ���ɱ��Ӷ�
            }

            AssignRandomSprite();
        }

        private void AssignRandomSprite()
        {
            // ����Ҫ�������Ƿ���Ч
            if (m_spriteRenderer == null || m_spriteRenderer.Length == 0 || m_deadBodySprites == null || m_deadBodySprites.Length == 0)
            {
                Debug.LogWarning("SpriteRenderer�����DeadBodySprites����δ��ȷ���ã�");
                return;
            }

            // ��m_deadBodySprites�����ѡ��һ������
            Sprite randomSprite = m_deadBodySprites[Random.Range(0, m_deadBodySprites.Length)];

            // ��������Ƿ�תx��
            bool flipX = Random.Range(0, 2) == 0; // ����0��1�������Ƿ�ת

            // Ӧ�õ�m_spriteRenderer[0]
            m_spriteRenderer[0].sprite = randomSprite;
            m_spriteRenderer[0].flipX = flipX;
        }

        public override void OnStartInteract(CharacterBase sender, Entity target)
        {
            if (target != this)
            {
                return;
            }

            //Debug.Log("OnStartInteract");

            if (m_nowLootedCount < m_randomMaxLootedCount) // ����Ƿ���Լ����Ӷ�
            {
                GameManager.Player.OnTryStartLoot(target, m_lootedTime);
            }
        }

        private LootTable.LootEntryData GetRandomLootEntry()
        {
            float totalWeight = 0f;

            foreach (var entry in lootTable.entries)
            {
                totalWeight += entry.weight;
            }

            float randomValue = Random.Range(0f, totalWeight);

            foreach (var entry in lootTable.entries)
            {
                if (randomValue < entry.weight)
                {
                    return entry;
                }

                randomValue -= entry.weight;
            }

            return null;
        }

        public bool TryLooted()
        {
            // �����Ӷ����
            m_nowLootedCount++;

            // ����Ƿ񴥷���������Ʒ�ĸ���
            if (UnityEngine.Random.value <= lootTable.lootRate)
            {
                // ��������Ӷ���Ʒ���ǽ�Ǯ
                bool lootItem = UnityEngine.Random.Range(0, 2) == 0;

                if (lootItem)
                {
                    if (lootTable.entries != null && lootTable.entries.Length > 0)
                    {
                        // ʹ�û���Ȩ�ص����ѡ�����
                        LootEntryData randomEntry = GetRandomLootEntry();

                        if (GameManager.InventorySystem.IsBackpackFull(randomEntry.item))
                        {
                            GameManager.DialogueSystem.Main.PlayNow
                            (LocalizationSettings.StringDatabase.GetLocalizedString("NPCDialogueTable", "id_dialogue_shop_backpack_full"));

                            // �����˵Ļ�����ȥ
                            m_nowLootedCount--;
                            
                            return false;
                        }
                        else if (randomEntry != null)
                        {
                            //Debug.Log("lootItem");

                            int randomQuantity = UnityEngine.Random.Range(1, randomEntry.maxQuantity + 1);
                            GameManager.InventorySystem.AddToBag(randomEntry.item, randomQuantity);
                            GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_lootedSound);
                        }
                    }
                }
                else if (lootTable.money > 0)
                {
                    int randomMoney = UnityEngine.Random.Range(1, lootTable.money + 1);
                    GameManager.InventorySystem.AddMoney(randomMoney);

                    GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_lootedSound);

                }
            }

            // ����Ƿ��Ѵﵽ����Ӷ����
            if (m_nowLootedCount >= m_randomMaxLootedCount)
            {
                this.gameObject.layer = LayerMask.NameToLayer("Collision D"); // ����Ϊ���ɱ��Ӷ�
            }

            return true; // ��ʾ�����Ӷ�ɹ�
        }
    }
}
