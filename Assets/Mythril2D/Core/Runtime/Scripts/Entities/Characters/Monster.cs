using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class Monster : Character<MonsterSheet>
    {
        [Header("Monster Settings")]
        [SerializeField] private bool m_permanentDeath = false;
        [SerializeField] private string m_gameFlagID = "monster_00";

        private bool m_looted = false;

        protected override void Awake()
        {
            base.Awake();
            UpdateStats();
        }

        private void Start()
        {
            if (m_permanentDeath && GameManager.GameFlagSystem.Get(m_gameFlagID))
            {
                Destroy(gameObject);
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
                //Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
                //Array.ForEach(colliders, (collider) => collider.enabled = false);

                // cancel the playing animation
                this.transform.Find("Pivot").gameObject.SetActive(false);

                SetLayerRecursively(this.gameObject, LayerMask.NameToLayer("Interaction"));
            }
        }

        public bool LootFinished()
        {
            if (m_looted == false)
            {
                foreach (Loot loot in m_sheet.potentialLoot)
                {
                    if (GameManager.Player.level >= loot.minimumPlayerLevel && m_level >= loot.minimumMonsterLevel && loot.IsAvailable() && loot.ResolveDrop())
                    {
                        GameManager.InventorySystem.AddToBag(loot.item, loot.quantity);
                    }
                }
                GameManager.InventorySystem.AddMoney(m_sheet.money[m_level]);

                OnDeath();

                m_looted = true;

                return true;
            }
            return false;
        }

        //void OnTriggerStay(Collider collisionInfo)
        void OnTriggerEnter2D(Collider2D other)
        {
            if (String.Equals(other.gameObject.tag, "Player") == true)
            {
                Debug.Log("Monster Interaction");
                Debug.Log("GameManager.Config.collisionContactFilter = " + GameManager.Config.collisionContactFilter);
                //GameManager.Player.GetComponent<PlayerController>().m_interactionTarget = this.gameObject;
                //GameManager.PlayerSystem.PlayerInstance.GetComponent<PlayerController>().GetInteractibleObject();
            }
        }

        //void OnTriggerStay(Collider collisionInfo)
        void OnTriggerExit2D(Collider2D other)
        {
            if (String.Equals(other.gameObject.tag, "Player") == true)
            {
                //GameManager.Player.GetComponent<PlayerController>().m_interactionTarget = null;
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
