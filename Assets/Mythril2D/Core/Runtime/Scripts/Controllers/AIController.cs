using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class AIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterBase m_character = null;
        [SerializeField] private Rigidbody2D m_rigidbody = null;

        [Header("Chase Settings")]
        [SerializeField, Min(1.0f)] private float m_detectionRadius = 5.0f;
        [SerializeField, Min(1.0f)] private float m_resetFromInitialPositionRadius = 10.0f;
        [SerializeField, Min(1.0f)] private float m_resetFromTargetDistanceRadius = 10.0f;
        [SerializeField, Min(0.5f)] private float m_targetOutOfRangeRetargetCooldown = 3.0f;
        [SerializeField, Min(0.1f)] private float m_soughtDistanceFromTarget = 1.0f;

        [Header("Steering Settings")]
        [SerializeField, Min(0.1f)] private float m_steeringDriftResponsiveness = 3.0f;
        [SerializeField, Min(0.1f)] private float m_timeBeforeResetAfterTargetSightLost = 3.0f;
        [SerializeField, Min(0.1f)] private float m_cannotSeeTargetRetargetCooldown = 1.0f;

        [Header("Attack Settings")]
        [SerializeField] public float m_attackTriggerRadius = 1.0f;
        [SerializeField] public float m_attackCooldown = 1.0f;

        private Vector2 m_initialPosition;
        private Transform m_target = null;
        private float m_retargetCooldownTimer = 0.0f;
        private float m_attackCooldownTimer = 0.0f;
        private List<RaycastHit2D> m_castCollisions = new List<RaycastHit2D>();

        private float[] m_interests = new float[8];
        private float[] m_dangers = new float[8];
        private float[] m_steering = new float[8];
        private Vector2 m_steeringAverageOutput = Vector2.zero;
        private Vector2 m_targetPosition = Vector2.zero;
        private Vector2 m_lerpedTargetDirection = Vector2.zero;
        private float m_timeSinceTargetLastSeen = 0.0f;

        private Vector2[] m_directions = new Vector2[8]
        {
            Vector2.up,
            new Vector2(0.5f, 0.5f).normalized,
            Vector3.right,
            new Vector2(0.5f, -0.5f).normalized,
            Vector2.down,
            new Vector2(-0.5f, -0.5f).normalized,
            Vector2.left,
            new Vector2(-0.5f, 0.5f).normalized,
        };

        private void Awake()
        {
            Debug.Assert(m_rigidbody, ErrorMessages.InspectorMissingComponentReference<Rigidbody2D>());
            m_initialPosition = transform.position;
        }

        // 获取技能是在Awake所以得在Start后面执行这个更改
        private void Start()
        {
            // Find the first triggerable ability available on the character and set distance
            foreach (AbilityBase ability in m_character.abilityInstances)
            {
                if (ability is ITriggerableAbility)
                {
                    // 获取 AbilitySheet
                    AbilitySheet abilitySheet = ability.abilitySheet;

                    // 确认是否为 DamageAbilitySheet 类型
                    if (abilitySheet is DamageAbilitySheet damageAbilitySheet)
                    {
                        // 访问 DamageDescriptor
                        DamageDescriptor damageDescriptor = damageAbilitySheet.damageDescriptor;

                        // 获取 distanceType 属性
                        EDistanceType distanceType = damageDescriptor.distanceType;

                        if (distanceType == EDistanceType.Melee)
                        {
                            // 0.25时候 好像太短会让玩家进入到碰撞体中，导致检测不到对象
                            m_attackTriggerRadius = 0.75f;
                        }
                        else
                        {
                            m_attackTriggerRadius = 6.0f;
                            m_attackCooldown = 3.0f;
                        }
                    }
                    break; // 如果只需要处理第一个符合条件的技能，可以在这里中断循环
                }
            }
        }

        private void OnEnable()
        {
            m_character.provokedEvent.AddListener(OnProvoked);
        }

        private void OnDisable()
        {
            m_character.provokedEvent.RemoveListener(OnProvoked);
        }

        private void OnProvoked(CharacterBase source)
        {
            if (source && !m_target && m_retargetCooldownTimer == 0.0f && source.CanBeAttackedBy(m_character))
            {
                m_target = source.transform;
                GameManager.NotificationSystem.targetDetected.Invoke(this, m_target);
            }
        }

        private bool CanSee(Transform other)
        {
            Vector2 targetPosition = other.position;
            Vector2 currentPosition = transform.position;
            Vector2 directionToTarget = targetPosition - currentPosition;
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, directionToTarget, Vector2.Distance(currentPosition, targetPosition), GameManager.Config.collisionContactFilter.layerMask);
            return hit.collider == null;
        }

        private Transform FindTarget()
        {
            RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, m_detectionRadius, Vector2.zero, 0.0f);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.transform.TryGetComponent(out CharacterBase character) &&
                    character.CanBeAttackedBy(m_character) &&
                    CanSee(hit.transform))
                {
                    return hit.transform;
                }
            }

            return null;
        }

        private void UpdateCooldowns()
        {
            if (m_retargetCooldownTimer > 0.0f)
            {
                m_retargetCooldownTimer = Math.Max(m_retargetCooldownTimer - Time.fixedDeltaTime, 0.0f);
            }

            if (m_attackCooldownTimer > 0.0f)
            {
                m_attackCooldownTimer = Math.Max(m_attackCooldownTimer - Time.fixedDeltaTime, 0.0f);
            }
        }

        private void TryToAttackTarget(float distanceToTarget)
        {
            if (m_attackCooldownTimer == 0.0f && distanceToTarget < m_attackTriggerRadius)
            {
                // Find the first triggerable ability available on the character and fire it
                foreach (AbilityBase ability in m_character.abilityInstances)
                {
                    if (ability is ITriggerableAbility)
                    {
                        m_character.FireAbility((ITriggerableAbility)ability);
                        m_attackCooldownTimer = m_attackCooldown;
                        break;
                    }
                }
            }
        }

        private void CheckIfTargetOutOfRange(float distanceToTarget)
        {
            float distanceToInitialPosition = Vector2.Distance(m_initialPosition, this.transform.position);

            bool isTooFarFromInitialPosition = distanceToInitialPosition > m_resetFromInitialPositionRadius;

            bool isTooFarFromTarget = distanceToTarget > m_resetFromTargetDistanceRadius;

            if (isTooFarFromInitialPosition || isTooFarFromTarget)
            {
                StopChase(m_targetOutOfRangeRetargetCooldown);
            }
        }

        private void StopChase(float retargetCooldown)
        {
            m_retargetCooldownTimer = retargetCooldown;
            m_target = null;
        }

        private void ProcessChaseBehaviour(int index)
        {
            Vector2 direction = m_directions[index];

            Vector2 targetPosition = m_targetPosition;
            Vector2 currentPosition = transform.position;
            Vector2 directionToTarget = targetPosition - currentPosition;
            directionToTarget.Normalize();

            float angleToTargetDirection = Vector2.Angle(direction, directionToTarget);

            m_interests[index] = Math.Max(1.0f - (angleToTargetDirection / 90.0f), 0.0f);
        }

        private void ProcessAvoidBehaviour(int index)
        {
            Vector2 direction = m_directions[index];

            int count = m_rigidbody.Cast(
                    direction, // X and Y values between -1 and 1 that represent the direction from the body to look for collisions
                    GameManager.Config.collisionContactFilter, // The settings that determine where a collision can occur on such as layers to collide with
                    m_castCollisions, // List of collisions to store the found collisions into after the Cast is finished
                    1.0f
            ); // The amount to cast equal to the movement plus an offset

            m_dangers[index] = count > 0 ? 1.0f - m_castCollisions[0].distance : 0.0f;
        }

        private void ProcessSteeringBehaviour(int index)
        {
            ProcessChaseBehaviour(index);
            ProcessAvoidBehaviour(index);

            m_steering[index] = m_interests[index] - m_dangers[index];
        }

        private void UpdateTargetPosition()
        {
            if (m_target)
            {
                // While we can see the target, store its last position, so if the AI looses sight of its target,
                // it will go to the last position the target was seen at.
                if (CanSee(m_target))
                {
                    m_targetPosition = (Vector2)m_target.position;
                    m_timeSinceTargetLastSeen = 0.0f;
                }
                else
                {
                    m_timeSinceTargetLastSeen += Time.deltaTime;

                    if (m_timeSinceTargetLastSeen > m_timeBeforeResetAfterTargetSightLost)
                    {
                        StopChase(m_cannotSeeTargetRetargetCooldown);
                    }
                }
            }
            else
            {
                m_targetPosition = m_initialPosition;
            }
        }

        private void FixedUpdate()
        {
            UpdateCooldowns();

            if (!m_target)
            {
                if (m_retargetCooldownTimer == 0.0f)
                {
                    m_target = FindTarget();
                    if (m_target)
                    {
                        GameManager.NotificationSystem.targetDetected.Invoke(this, m_target);
                    }
                }
            }
            else
            {
                float distanceToTarget = Vector2.Distance(m_target.position, transform.position);

                // 如果目标在攻击范围内，则尝试攻击
                if (distanceToTarget < m_attackTriggerRadius)
                {
                    TryToAttackTarget(distanceToTarget);
                }
                else // 目标不在攻击范围内时才检查追逐逻辑
                {
                    CheckIfTargetOutOfRange(distanceToTarget);
                }
            }

            UpdateTargetPosition();

            if (Vector2.Distance(transform.position, m_targetPosition) > m_soughtDistanceFromTarget)
            {
                m_steeringAverageOutput = Vector2.zero;

                // Process the steering behaviour for each direction
                for (int i = 0; i < 8; ++i)
                {
                    ProcessSteeringBehaviour(i);
                    m_steeringAverageOutput += m_directions[i] * m_steering[i];
                }

                m_steeringAverageOutput.Normalize();

                m_lerpedTargetDirection =
                    !m_character.IsMoving() ?
                    m_steeringAverageOutput :
                    Vector2.Lerp(m_lerpedTargetDirection, m_steeringAverageOutput, Time.fixedDeltaTime * m_steeringDriftResponsiveness);

                m_character.SetMovementDirection(m_lerpedTargetDirection.normalized);
            }
            else
            {
                m_character.SetMovementDirection(Vector2.zero);

                if (m_character.Can(EActionFlags.Move))
                {
                    m_character.SetLookAtDirection(m_targetPosition.x - transform.position.x); // Make sure the AI face its target
                }
            }
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < 8; ++i)
            {
                Gizmos.color = m_steering[i] > 0.0f ? Color.green : Color.red;
                Gizmos.DrawRay(transform.position, m_directions[i] * Mathf.Abs(m_steering[i]));
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, m_steeringAverageOutput);

            if (m_target)
            {
                Gizmos.color = CanSee(m_target) ? Color.cyan : Color.magenta;
                Gizmos.DrawLine(transform.position, m_target.transform.position);
            }
        }
    }
}
