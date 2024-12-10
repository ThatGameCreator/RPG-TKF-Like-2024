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
        [SerializeField] private LootTable lootTable;   // 引用 ScriptableObject 数据表
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

            // 检查是否已达到最大掠夺次数
            if (m_nowLootedCount >= m_randomMaxLootedCount)
            {
                this.gameObject.layer = LayerMask.NameToLayer("Collision D"); // 设置为不可被掠夺
            }

            AssignRandomSprite();
        }

        private void AssignRandomSprite()
        {
            // 检查必要的数组是否有效
            if (m_spriteRenderer == null || m_spriteRenderer.Length == 0 || m_deadBodySprites == null || m_deadBodySprites.Length == 0)
            {
                Debug.LogWarning("SpriteRenderer数组或DeadBodySprites数组未正确设置！");
                return;
            }

            // 从m_deadBodySprites中随机选择一个精灵
            Sprite randomSprite = m_deadBodySprites[Random.Range(0, m_deadBodySprites.Length)];

            // 随机决定是否翻转x轴
            bool flipX = Random.Range(0, 2) == 0; // 生成0或1，决定是否翻转

            // 应用到m_spriteRenderer[0]
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

            if (m_nowLootedCount < m_randomMaxLootedCount) // 检查是否可以继续掠夺
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
            // 增加掠夺次数
            m_nowLootedCount++;

            // 检查是否触发不生成物品的概率
            if (UnityEngine.Random.value <= lootTable.lootRate)
            {
                // 随机决定掠夺物品还是金钱
                bool lootItem = UnityEngine.Random.Range(0, 2) == 0;

                if (lootItem)
                {
                    if (lootTable.entries != null && lootTable.entries.Length > 0)
                    {
                        // 使用基于权重的随机选择机制
                        LootEntryData randomEntry = GetRandomLootEntry();

                        if (GameManager.InventorySystem.IsBackpackFull(randomEntry.item))
                        {
                            GameManager.DialogueSystem.Main.PlayNow
                            (LocalizationSettings.StringDatabase.GetLocalizedString("NPCDialogueTable", "id_dialogue_shop_backpack_full"));

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

            // 检查是否已达到最大掠夺次数
            if (m_nowLootedCount >= m_randomMaxLootedCount)
            {
                this.gameObject.layer = LayerMask.NameToLayer("Collision D"); // 设置为不可被掠夺
            }

            return true; // 表示本次掠夺成功
        }
    }
}
