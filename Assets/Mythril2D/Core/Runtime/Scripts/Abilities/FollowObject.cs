using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gyvr.Mythril2D
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class FollowObject : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator m_animator = null;

        [Header("Settings")]
        [SerializeField] private bool m_reverseRotation = false;
        [SerializeField] private float m_maxDuration = 2.0f;

        [Header("Animation Parameters")]
        [SerializeField] private string m_destroyAnimationParameter = "destroy";

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_collisionSound;

        private DamageOutputDescriptor m_damageOutputDescriptor;
        private Vector2 m_direction;
        private float m_speed;
        private float m_timer;
        private bool m_hasDestroyAnimation;
        private bool m_operating = false;

        private void Awake()
        {
            m_hasDestroyAnimation = m_animator && AnimationUtils.HasParameter(m_animator, m_destroyAnimationParameter);
        }

        public void Throw(DamageOutputDescriptor damageOutputDescriptor, Vector2 direction, float speed)
        {
            m_damageOutputDescriptor = damageOutputDescriptor;
            m_direction = direction;
            m_speed = speed;
            m_timer = 0.0f;
            transform.rotation = Quaternion.LookRotation(Vector3.forward, direction * (m_reverseRotation ? -1.0f : 1.0f));
            m_operating = true;
        }

        public void OnDestroyAnimationEnd()
        {
            Terminate(true);
        }

        private void Terminate(bool forceNoAnimation = false)
        {
            m_operating = false;

            if (!forceNoAnimation && m_hasDestroyAnimation)
            {
                m_animator?.SetTrigger(m_destroyAnimationParameter);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnBecameInvisible()
        {
            Terminate(true);
        }

        private void Update()
        {
            m_timer += Time.deltaTime;

            if (m_timer >= m_maxDuration)
            {
                Terminate();
            }
        }

    }
}
