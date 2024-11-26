using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class ContactDamageAbility : Ability<DamageAbilitySheet>
    {
        [Header("Settings")]
        [SerializeField] private ContactFilter2D m_contactFilter2D;
        [SerializeField] private float m_detectionInterval = 0.1f; // 每隔 0.1 秒检测一次
        private float m_elapsedTime = 0f; // 用于累计时间

        private Collider2D m_hitbox = null;
        private HashSet<Collider2D> m_processedColliders = new HashSet<Collider2D>(); // 避免重复广播
        [SerializeField] private float m_cooldownTime = 1.0f; // 冷却时间
        private Dictionary<Collider2D, float> m_cooldownTimers = new Dictionary<Collider2D, float>();

        public override void Init(CharacterBase character, AbilitySheet settings)
        {
            base.Init(character, settings);

            // 使用角色的碰撞体作为接触伤害的碰撞区域
            m_hitbox = character.GetComponent<Collider2D>();
        }

        private void Awake()
        {
            m_hitbox = GetComponentInParent<Collider2D>();
        }

        private void FixedUpdate()
        {
            m_elapsedTime += Time.fixedDeltaTime;

            // 创建一个待处理的键值对列表
            var copyOfTimers = new Dictionary<Collider2D, float>(m_cooldownTimers);

            foreach (var kvp in copyOfTimers)
            {
                // 在原字典中更新时间
                if (m_cooldownTimers.ContainsKey(kvp.Key))
                {
                    m_cooldownTimers[kvp.Key] -= Time.fixedDeltaTime;

                    // 检查是否需要移除
                    if (m_cooldownTimers[kvp.Key] <= 0f)
                    {
                        m_cooldownTimers.Remove(kvp.Key);
                        m_processedColliders.Remove(kvp.Key);
                    }
                }
            }

            if (m_elapsedTime >= m_detectionInterval)
            {
                // 搞不懂为什么没玩家也要执行
                // 难道身边有怪物也要判断下？
                m_elapsedTime -= m_detectionInterval;
                DetectAndApplyDamage();
            }
        }

        private void DetectAndApplyDamage()
        {
            List<Collider2D> colliders = new List<Collider2D>();
            Physics2D.OverlapCollider(m_hitbox, m_contactFilter2D, colliders);

            foreach (Collider2D collider in colliders)
            {
                // 检测是否符合伤害条件
                if (collider == null || m_processedColliders.Contains(collider) || collider.gameObject == gameObject)
                    continue;

                // 只对具有 CharacterBase 组件的对象生效
                // 他这个原来会对全部找到的对象发生伤害广播，而我们又在广播里面写了禁止掠夺
                // 就导致如果有怪物挂载了接触伤害脚本，就会导致虽然没有接触到玩家，但就是会取消互动
                // 而关于为什么开启关闭背景音乐后就不一样 是因为我们这个音乐对象挂了一个几乎整个地图的碰撞体
                // 这就导致怪物拿到了音乐的碰撞体，并且向音乐给发送了伤害广播
                CharacterBase characterBase = collider.GetComponent<CharacterBase>();
                if (characterBase != null && collider.gameObject != gameObject)
                {
                    DamageOutputDescriptor damageOutput = DamageSolver.SolveDamageOutput(m_character, m_sheet.damageDescriptor);
                    DamageDispatcher.Send(characterBase.gameObject, damageOutput);

                    // 记录已处理的碰撞体
                    m_processedColliders.Add(collider);
                    m_cooldownTimers[collider] = m_cooldownTime;
                }
            }

            // 清理无效的碰撞体记录
            m_processedColliders.RemoveWhere(collider => !collider || !collider.enabled);
        }
    }
}
