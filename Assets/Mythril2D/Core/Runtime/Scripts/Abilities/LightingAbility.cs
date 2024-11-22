using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class LightingAbility : ActiveAbility<LightAbilitySheet>
    {
        [Header("Reference")]
        [SerializeField] private Animator m_animator = null;

        [Header("Animation Parameters")]
        [SerializeField] private string m_fireAnimationParameter = "fire";
        [SerializeField] private string m_lightingAnimationParameter = "lighting";
        [SerializeField] private string m_exitAnimationParameter = "exit";

        public override void Init(CharacterBase character, AbilitySheet settings)
        {
            base.Init(character, settings);

            Debug.Assert(m_animator, ErrorMessages.InspectorMissingComponentReference<Animator>());
            Debug.Assert(m_animator.GetBehaviour<StateMessageDispatcher>(), string.Format("{0} not found on the melee attack animator controller", typeof(StateMessageDispatcher).Name));
        }

        protected override void Fire()
        {
            m_animator?.SetTrigger(m_fireAnimationParameter);
        }

        public void OnTryLightingAnimationEnd()
        {
            if (!m_character.dead)
            {
                GameManager.Player.OnEnableAbilityLighting();
                TerminateCasting();
            }
        }

        public void OnLightingAnimationStart()
        {
            if (!m_character.dead) {
                GameManager.Player.heroSightLight.enabled = false;
                GameManager.Player.heroAbilityLight.enabled = true;
            }
        }

        public void OnLightingAnimationEnd()
        {
            if (!m_character.dead)
            {
                GameManager.Player.heroSightLight.enabled = true;
                GameManager.Player.heroAbilityLight.enabled = false;
                m_animator?.SetTrigger(m_exitAnimationParameter);
            }
        }
    }
}
