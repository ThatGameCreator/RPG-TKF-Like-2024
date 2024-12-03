using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class Monster : Character<MonsterSheet>
    {
        [Header("Monster Settings")]
        [SerializeField] private bool m_permanentDeath = false;
        [SerializeField] private string m_gameFlagID = "monster_00";
        [SerializeField] private AIController m_aiController;
        [SerializeField] private GameObject m_weaponObject = null;
        [SerializeField] protected float m_lootedTime = 2.0f;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_lootedSound;

        public AIController aiController => m_aiController;

        private bool m_looted = false;
        private int m_randomMaxLootedCount = 0;
        private int m_nowLootedCount = 0;

        protected override void Awake()
        {
            base.Awake();
            UpdateStats();
        }

        private void Update()
        {
            UpdateFieldOfWar();
        }

        protected override void Start()
        {
            base.Start();

            if (m_sheet.lootTable)
            {
                m_randomMaxLootedCount = UnityEngine.Random.Range(0, m_sheet.lootTable.maxLootedCount);
            }

            if (m_permanentDeath && GameManager.GameFlagSystem.Get(m_gameFlagID))
            {
                Destroy(gameObject);
            }
        }

        public override void OnStartInteract(CharacterBase sender, Entity target)
        {
            if (target != this)
            {
                return;
            }

            if (m_nowLootedCount < m_randomMaxLootedCount) // 检查是否可以继续掠夺
            {
                GameManager.Player.OnTryStartLoot(target, m_lootedTime);
            }
        }

        public void SetLevel(int level)
        {
            m_level = level;
            UpdateStats();
        }

        public void UpdateStats()
        {
            m_maxStats.Set(m_sheet.stats[m_level]);
        }

        protected override void Die()
        {
            // close the collider and destory the enemy object
            MonsterDie();
            GameManager.NotificationSystem.monsterKilled.Invoke(m_sheet);
            GameManager.Player.AddExperience(m_sheet.experience[m_level]);
            
            m_sheet.executeOnDeath?.Execute();

            if (m_permanentDeath)
            {
                GameManager.GameFlagSystem.Set(m_gameFlagID, true);
            }
        }

        void MonsterDie()
        {
            GameManager.NotificationSystem.audioPlaybackRequested.Invoke(characterSheet.deathAudio);

            // if play animation fail then destory the object
            if (TryPlayDeathAnimation() == false)
            {
                OnDeath();
            }
            else
            {
                // cancel the playing animation
                m_weaponObject.transform.gameObject.SetActive(false);

                // 检查是否已达到最大掠夺次数
                if (m_nowLootedCount >= m_randomMaxLootedCount)
                {
                    this.gameObject.layer = LayerMask.NameToLayer("Default"); // 设置为不可被掠夺
                }
                else
                {
                    SetLayerRecursively(this.gameObject, LayerMask.NameToLayer("Interaction"));
                }
            }
        }

        private LootTable.LootEntryData GetRandomLootEntry()
        {
            float totalWeight = 0f;

            foreach (var entry in m_sheet.lootTable.entries)
            {
                totalWeight += entry.weight;
            }

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);

            foreach (var entry in m_sheet.lootTable.entries)
            {
                if (randomValue < entry.weight)
                {
                    return entry;
                }

                randomValue -= entry.weight;
            }

            return null;
        }

        public bool LootFinished()
        {
            if (GameManager.InventorySystem.IsBackpackFull())
            {
                GameManager.DialogueSystem.Main.PlayNow("Backpack is full...");

                return false;
            }
            else
            {
                // 增加掠夺次数
                m_nowLootedCount++;

                // 检查是否触发不生成物品的概率
                if (UnityEngine.Random.value <= m_sheet.lootTable.lootRate)
                {
                    // 随机决定掠夺物品还是金钱
                    bool lootItem = UnityEngine.Random.Range(0, 2) == 0;

                    if (lootItem && m_sheet.lootTable.entries != null && m_sheet.lootTable.entries.Length > 0)
                    {
                        //Debug.Log("lootItem");

                        // 使用基于权重的随机选择机制
                        var randomEntry = GetRandomLootEntry();

                        if (randomEntry != null)
                        {
                            int randomQuantity = UnityEngine.Random.Range(1, randomEntry.maxQuantity + 1);
                            GameManager.InventorySystem.AddToBag(randomEntry.item, randomQuantity);
                        }

                        GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_lootedSound);
                    }
                    else if (m_sheet.lootTable.money > 0)
                    {
                        int randomMoney = UnityEngine.Random.Range(1, m_sheet.lootTable.money + 1);
                        GameManager.InventorySystem.AddMoney(randomMoney);

                        GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_lootedSound);

                    }

                    // 检查是否已达到最大掠夺次数
                    if (m_nowLootedCount >= m_randomMaxLootedCount)
                    {
                        this.gameObject.layer = LayerMask.NameToLayer("Default"); // 设置为不可被掠夺
                    }

                    return true; // 表示本次掠夺成功
                }
                else
                {
                    Debug.Log("没有获得任何物品或金钱。");
                    Debug.Log(UnityEngine.Random.value);
                    Debug.Log(m_sheet.lootTable.lootRate);

                    // 检查是否已达到最大掠夺次数
                    if (m_nowLootedCount >= m_randomMaxLootedCount)
                    {
                        this.gameObject.layer = LayerMask.NameToLayer("Default"); // 设置为不可被掠夺
                    }

                    return false;
                }
            }
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
        }

        private void CheckOverlappedObject()
        {
            // get the collider max radius
            CapsuleCollider2D m_capsuleCollider = this.gameObject.GetComponent<CapsuleCollider2D>();
            Vector2 t_size = m_capsuleCollider.size;
            //Debug.Log("hello");
            //Debug.Log("tmp.x = " + tmp.y);
            //Debug.Log("tmp.y = " + tmp.y);

            //Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, 0.8f, LayerMask.GetMask(GameManager.Config.mosterLayer));
            Collider2D[] colliders = Physics2D.OverlapCircleAll(m_capsuleCollider.transform.position, 0.8f, LayerMask.GetMask(GameManager.Config.mosterLayer));

            foreach (Collider2D collider in colliders)
            {
                if((collider.transform == m_capsuleCollider.transform) == false)
                {
                    ColliderDistance2D colliderDistance = m_capsuleCollider.Distance(collider);

                    // draw a line showing the depenetration direction if overlapped
                    if (colliderDistance.isOverlapped)
                    {
                        //Debug.Log("isOverlapped");
                        //Debug.Log("die object = " + m_capsuleCollider.gameObject.transform.name);
                        //Debug.Log("stuck object = " + collider.gameObject.transform.name);
                        //Debug.Log("original stuck object x, y = " + collider.gameObject.transform.position.x.ToString() + collider.gameObject.transform.position.y.ToString());

                        Vector2 resolutionVector = Mathf.Abs(colliderDistance.distance) * colliderDistance.normal;
                        Vector2 t_position = collider.gameObject.transform.position;
                        //collider.gameObject.transform.position.x += resolutionVector.x;
                        //collider.gameObject.transform.position.y += resolutionVector.y;

                        t_position.x += (resolutionVector.x * 2);
                        t_position.y += (resolutionVector.y * 2);

                        //t_position.x += (resolutionVector.x * -2);
                        //t_position.y += (resolutionVector.y * -2);

                        //Debug.Log("t_position x, y = " + t_position.x.ToString() + t_position.y.ToString());

                        //collider.gameObject.transform.position.Set(t_position.x, t_position.y, collider.gameObject.transform.position.z);
                        //collider.gameObject.transform.position = new Vector3(t_position.x, t_position.y, collider.gameObject.transform.position.z);

                        GameObject target = collider.gameObject;
                        target.GetComponent<CharacterBase>().AvoidOverlapped(resolutionVector);

                        //Debug.Log("resolutionVector x, y = " + resolutionVector.x.ToString() + resolutionVector.y.ToString());
                        //Debug.Log("After stuck object x, y = " + collider.gameObject.transform.position.x.ToString() + collider.gameObject.transform.position.y.ToString());

                        // 感觉从游戏效果来看，似乎并没有直观看到怪物有任何移动的变化，感觉并没有真正去移动怪物的位置
                        // 不知道如果用协程去执行会有什么效果
                    }
                }
            }
        }

    }
}
