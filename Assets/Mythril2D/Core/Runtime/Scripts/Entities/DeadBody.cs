using System.Collections;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class DeadBody : OtherEntity
    {
        [Header("Dead Body Settings")]
        [SerializeField] private ChestLoot m_loot;
        [SerializeField] private string m_gameFlagID = "DeadBody_00";
        [SerializeField] private Sprite[] m_deadBodySprites = null;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_openedSound;

        private bool m_opened = false;

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
