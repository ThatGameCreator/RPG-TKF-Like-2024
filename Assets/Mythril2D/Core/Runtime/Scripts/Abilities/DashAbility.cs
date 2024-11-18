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

        public void OnDashAnimationEnd()
        {
            //Debug.Log("OnDashAnimationEnd");

            if (!m_character.dead)
            {
                TerminateCasting();
            }
        }

        protected override void Fire()
        {
            //Debug.Log("Fire Dash");

            GameManager.Player.isDashFinished = false;

            m_dirction =
                m_character.IsMoving() ?
                m_character.movementDirection :
                (m_character.GetLookAtDirection() == EDirection.Right ? Vector2.right : Vector2.left);


            m_character.Push(m_dirction, m_sheet.dashStrength, m_sheet.dashResistance, faceOppositeDirection: true);

            m_character.TryPlayDashAnimation();

            if (GameManager.Player.dashParticleSystem != null)
            {
                GameManager.Player.dashParticleSystem.Play();
            }

            // 启动协程等待动画结束
            StartCoroutine(WaitForDashAnimation());

            // 在主方法执行不需要在这里再扣一次
            //ConsumeStamina();

            //TerminateCasting();
        }

        // 协程：等待动画结束后调用 TerminateCasting
        private IEnumerator WaitForDashAnimation()
        {
            // 等待当前动画播放完成
            while (GameManager.Player.isDashFinished == false)
            {
                yield return null;
            }

            TerminateCasting(); // 动画播放完成后调用
        }

    }
}
