using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class ContactDamageAbility : Ability<DamageAbilitySheet>
    {
        [Header("Settings")]
        [SerializeField] private ContactFilter2D m_contactFilter2D;
        private float m_detectionInterval = 0.1f; // 每隔 0.1 秒检测一次
        private float m_lastDetectionTime = 0f;

        private Collider2D m_hitbox = null;

        public override void Init(CharacterBase character, AbilitySheet settings)
        {
            base.Init(character, settings);

            // Use the character collider as the contact damage hitbox
            m_hitbox = character.GetComponent<Collider2D>();
        }

        private void Awake()
        {
            m_hitbox = GetComponentInParent<Collider2D>();
        }

        private void FixedUpdate()
        {
            Debug.Log(this);

            if (Time.time >= m_lastDetectionTime + m_detectionInterval)
            {
                m_lastDetectionTime = Time.time;
                DetectAndApplyDamage();
            }
        }

        private void DetectAndApplyDamage()
        {
            List<Collider2D> colliders = new List<Collider2D>();
            Physics2D.OverlapCollider(m_hitbox, m_contactFilter2D, colliders);

            foreach (Collider2D collider in colliders)
            {
                // 只对具有 CharacterBase 组件的对象生效
                // 他这个原来会对全部找到的对象发生伤害广播，而我们又在广播里面写了禁止掠夺
                // 就导致如果有怪物挂载了接触伤害脚本，就会导致虽然没有接触到玩家，但就是会取消互动
                CharacterBase characterBase = collider.GetComponent<CharacterBase>();
                if (characterBase != null && collider.gameObject != gameObject)
                {
                    DamageOutputDescriptor damageOutput = DamageSolver.SolveDamageOutput(m_character, m_sheet.damageDescriptor);
                    DamageDispatcher.Send(characterBase.gameObject, damageOutput);
                }
            }
        }

    }
}
