using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace Gyvr.Mythril2D
{
    public class DeadBody : OtherEntity
    {
        [Header("Dead Body Settings")]
        [SerializeField] private Loot m_loot;
        [SerializeField] private string m_gameFlagID = "DeadBody_00";
        [SerializeField] private Sprite[] m_deadBodySprites = null;
        [SerializeField] private int m_canLootCount = 5;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_openedSound;
        public LootTable lootTable;   // 引用 ScriptableObject 数据表

        private bool m_opened = false;
        private int m_randomMaxLootedCount = 0;
        private int m_nowLootedCount = 0;

        protected override void Start()
        {
            base.Start();

            m_randomMaxLootedCount = Random.Range(0, m_canLootCount);

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

            GameManager.Player.OnTryStartLoot(target, m_lootedTime);
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
            if (m_nowLootedCount < m_randomMaxLootedCount) // 检查是否可以继续掠夺
            {
                // 播放打开声音
                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_openedSound);

                // 随机决定掠夺物品还是金钱
                bool lootItem = Random.Range(0, 2) == 0;

                if (lootItem && lootTable.entries != null && lootTable.entries.Length > 0)
                {
                    // 使用基于权重的随机选择机制
                    var randomEntry = GetRandomLootEntry();

                    if (randomEntry != null)
                    {
                        int randomQuantity = Random.Range(1, randomEntry.maxQuantity + 1);
                        GameManager.InventorySystem.AddToBag(randomEntry.item, randomQuantity);
                        Debug.Log($"玩家获得了 {randomQuantity} 个 {randomEntry.item.name}");
                    }

                }
                else if (lootTable.money > 0)
                {
                    int randomMoney = Random.Range(10, lootTable.money + 1);
                    GameManager.InventorySystem.AddMoney(randomMoney);
                    Debug.Log($"玩家获得了 {randomMoney} 金币");
                }

                // 增加掠夺次数
                m_nowLootedCount++;

                // 检查是否已达到最大掠夺次数
                if (m_nowLootedCount >= m_randomMaxLootedCount)
                {
                    this.gameObject.layer = LayerMask.NameToLayer("Default"); // 设置为不可被掠夺
                }

                return true; // 表示本次掠夺成功
            }

            return false; // 表示已达到最大掠夺次数，无法再掠夺
        }
    }
}
