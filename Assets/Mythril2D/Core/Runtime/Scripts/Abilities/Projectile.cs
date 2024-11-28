using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gyvr.Mythril2D
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody2D m_rigidbody = null;
        [SerializeField] private Collider2D m_collider = null;
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
        private HashSet<Collider2D> m_processedColliders = new HashSet<Collider2D>(); // 避免重复广播
        private bool m_hasCollided = false; // 标记是否已发生碰撞
        [SerializeField] private LayerMask m_validLayers; // 可通过 Inspector 配置的有效层级

        private void Awake()
        {
            m_hasDestroyAnimation = m_animator && AnimationUtils.HasParameter(m_animator, m_destroyAnimationParameter);
        }

        public void Throw(DamageOutputDescriptor damageOutputDescriptor, Vector2 direction, float speed)
        {
            ResetState(); // 重置投掷物的状态

            m_damageOutputDescriptor = damageOutputDescriptor;
            m_direction = direction;
            m_speed = speed;
            m_rigidbody.velocity = m_direction * m_speed;
            m_collider.enabled = true;
            m_timer = 0.0f;
            transform.rotation = Quaternion.LookRotation(Vector3.forward, direction * (m_reverseRotation ? -1.0f : 1.0f));
            m_operating = true;
        }

        private void ResetState()
        {
            // 完全重置状态
            m_hasCollided = false;
            m_processedColliders.Clear();
            m_operating = false;
            m_rigidbody.velocity = Vector3.zero;
            m_timer = 0.0f;
            m_collider.enabled = true;
        }


        public void OnDestroyAnimationEnd()
        {
            Terminate(true);
        }

        private void Terminate(bool forceNoAnimation = false)
        {
            m_operating = false;
            m_rigidbody.velocity = Vector3.zero;
            m_collider.enabled = false;

            // 好像一个是碰到墙壁一个是碰到玩家（下面）
            if (!forceNoAnimation && m_hasDestroyAnimation)
            {
                m_collider.enabled = false;

                m_animator?.SetTrigger(m_destroyAnimationParameter);
            }
            else
            {
                m_collider.enabled = false;

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

        private void OnCollision()
        {
            if (m_hasCollided)
                return;

            m_hasCollided = true; // 标记碰撞已处理
            GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_collisionSound);
            Terminate();
        }

        private void HandleCollision(GameObject target)
        {
            if (m_hasCollided || m_processedColliders.Contains(target.GetComponent<Collider2D>()))
                return;

            m_processedColliders.Add(target.GetComponent<Collider2D>());
            CharacterBase character = target.GetComponentInParent<CharacterBase>();
            if (character)
            {
                if (DamageDispatcher.Send(character.gameObject, m_damageOutputDescriptor))
                {
                    // 成功命中有效角色目标
                    OnCollision();
                }
            }
            else
            {
                // 命中非角色目标
                OnCollision();
            }
        }

        private bool TryColliding(GameObject target)
        {
            if (!m_operating || target == gameObject || m_hasCollided)
                return false;

            Collider2D targetCollider = target.GetComponent<Collider2D>();
            if (targetCollider == null)
            {
                Debug.LogWarning($"Target {target.name} has no Collider2D!");
                return false;
            }

            if (m_processedColliders.Contains(targetCollider))
            {
                Debug.Log($"Target {target.name} already processed.");
                return false;
            }

            if (IsProperCollider(target.layer))
            {
                Debug.Log($"Target {target.name} is a valid collider.");
                HandleCollision(target);
                return true;
            }

            Debug.Log($"Target {target.name} is not a proper collider.");
            return false;
        }


        private bool IsProperCollider(int layer)
        {
            return (m_validLayers & (1 << layer)) != 0;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!m_hasCollided && TryColliding(collision.gameObject))
            {
                // 碰撞已处理
                return;
            }

            Debug.Log($"Collision Enter: {collision.gameObject.name}");
            if (!TryColliding(collision.gameObject) && IsProperCollider(collision.gameObject.layer))
            {
                OnCollision();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            TryColliding(collision.gameObject);
        }
    }
}
