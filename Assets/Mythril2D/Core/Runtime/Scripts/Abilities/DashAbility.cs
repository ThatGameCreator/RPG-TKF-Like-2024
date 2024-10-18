using System.Collections;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class DashAbility : ActiveAbility<DashAbilitySheet>
    {
        [Header("Reference")]
        [SerializeField] private ParticleSystem m_particleSystem = null;

        private Vector2 m_dirction = Vector2.zero;

        public override void Init(CharacterBase character, AbilitySheet settings)
        {
            base.Init(character, settings);
        }

        public override bool CanFire()
        {
            //Debug.Log("CanFire Dash");
            return base.CanFire() && !m_character.IsBeingPushed();
        }

        protected override void Fire()
        {
            //Debug.Log("Fire Dash");
            m_dirction =
                m_character.IsMoving() ?
                m_character.movementDirection :
                (m_character.GetLookAtDirection() == EDirection.Right ? Vector2.right : Vector2.left);


            m_character.Push(m_dirction, m_sheet.dashStrength, m_sheet.dashResistance, faceOppositeDirection: true);

            m_particleSystem.Play();

            m_character.TryPlayDashAnimation();

            // 没有粒子效果
            //if (m_dashParticleSystem != null)
            //{
            //    m_dashParticleSystem.Play();
            //}

            // 在主方法执行不需要在这里再扣一次
            //ConsumeStamina();

            TerminateCasting();
        }
    }
}
