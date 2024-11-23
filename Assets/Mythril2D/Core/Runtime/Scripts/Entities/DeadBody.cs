using System.Collections;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class DeadBody : OtherEntity
    {
        [Header("Dead Body Settings")]
        [SerializeField] private Loot m_loot;
        [SerializeField] private string m_gameFlagID = "DeadBody_00";
        [SerializeField] private Sprite[] m_deadBodySprites = null;
        [SerializeField] private int m_canLootCount = 3;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_openedSound;

        private bool m_opened = false;
        private int m_nowLootedCount = 0;

        protected override void Start()
        {
            base.Start();
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

        public bool TryLooted()
        {
            if (m_nowLootedCount < m_canLootCount) // 检查是否可以继续掠夺
            {
                // 播放打开声音
                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_openedSound);

                // 随机决定掠夺物品还是金钱
                bool lootItem = Random.Range(0, 2) == 0;

                if (lootItem && m_loot.entries != null && m_loot.entries.Length > 0)
                {
                    // 使用基于权重的随机选择机制
                    var randomLoot = m_loot.GetRandomLoot();

                    if (randomLoot.HasValue)
                    {
                        var entry = randomLoot.Value;

                        // 随机生成数量（范围可调整）
                        int randomQuantity = Random.Range(1, entry.quantity + 1);

                        // 添加到玩家背包
                        GameManager.InventorySystem.AddToBag(entry.item, randomQuantity);
                        Debug.Log($"玩家获得了 {randomQuantity} 个 {entry.item.name}");
                    }
                }
                else if (m_loot.money > 0)
                {
                    // 随机分配金钱奖励（范围可调整）
                    int randomMoney = Random.Range(10, m_loot.money + 1);

                    // 添加金钱到玩家
                    GameManager.InventorySystem.AddMoney(randomMoney);
                    Debug.Log($"玩家获得了 {randomMoney} 金币");
                }

                // 增加掠夺次数
                m_nowLootedCount++;

                // 检查是否已达到最大掠夺次数
                if (m_nowLootedCount >= m_canLootCount)
                {
                    this.gameObject.layer = LayerMask.NameToLayer("Default"); // 设置为不可被掠夺
                }

                return true; // 表示本次掠夺成功
            }

            return false; // 表示已达到最大掠夺次数，无法再掠夺
        }
    }
}
