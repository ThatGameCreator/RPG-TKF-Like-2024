using System.Collections;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class Warehouse : Entity
    {
        [Header("References")]
        [SerializeField] private Animator m_chestAnimator = null;
        [SerializeField] private Animator m_contentAnimator = null;
        [SerializeField] private SpriteRenderer m_contentSpriteRenderer = null;

        [Header("Chest Settings")]
        [SerializeField] private string m_gameFlagID = "warehouse_00";
        [SerializeField] private string m_openedAnimationParameter = "opened";
        [SerializeField] private string m_contentRevealAnimationParameter = "reveal";
        [SerializeField] private float m_contentRevealIconCycleDuration = 1.0f;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_openingSound;

        private bool m_hasOpeningAnimation = false;
        private bool m_hasRevealAnimation = false;

        protected void Awake()
        {
            Debug.Assert(m_chestAnimator, ErrorMessages.InspectorMissingComponentReference<Animator>());
            Debug.Assert(m_contentAnimator, ErrorMessages.InspectorMissingComponentReference<Animator>());
            Debug.Assert(m_contentSpriteRenderer, ErrorMessages.InspectorMissingComponentReference<SpriteRenderer>());

            if (m_chestAnimator)
            {
                m_hasOpeningAnimation = AnimationUtils.HasParameter(m_chestAnimator, m_openedAnimationParameter);
            }

            if (m_contentAnimator)
            {
                m_hasRevealAnimation = AnimationUtils.HasParameter(m_contentAnimator, m_contentRevealAnimationParameter);
            }
        }

        private void Start()
        {

        }

        public bool TryOpen()
        {
            GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_openingSound);

            GameManager.NotificationSystem.warehouseRequested.Invoke();
            
            return true;
        }
    }
}
