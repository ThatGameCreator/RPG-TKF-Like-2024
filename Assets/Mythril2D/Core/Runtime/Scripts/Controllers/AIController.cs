using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.AI;

namespace Gyvr.Mythril2D
{
    public class AIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterBase m_character = null;
        [SerializeField] private Rigidbody2D m_rigidbody = null; // 怪物的刚体组件

        [Header("Chase Settings")]
        [SerializeField, Min(1.0f)] private float m_detectionRadius = 5.0f;
        [SerializeField, Min(1.0f)] private float m_resetFromInitialPositionRadius = 10.0f;
        [SerializeField, Min(1.0f)] private float m_resetFromTargetDistanceRadius = 10.0f;
        [SerializeField, Min(0.5f)] private float m_targetOutOfRangeRetargetCooldown = 3.0f;
        [SerializeField, Min(0.1f)] private float m_soughtDistanceFromTarget = 1.0f; // 怪物与玩家的保持距离

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
        private List<RaycastHit2D> m_castCollisions = new List<RaycastHit2D>(); // 存储碰撞检测结果

        private float[] m_interests = new float[8]; // 兴趣权重数组
        private float[] m_dangers = new float[8]; // 危险权重数组
        private float[] m_steering = new float[8]; // 最终权重数组
        private Vector2 m_steeringAverageOutput = Vector2.zero; // 平均权重方向
        private Vector2 m_targetPosition = Vector2.zero; // 目标玩家位置
        private Vector2 m_lerpedTargetDirection = Vector2.zero;
        private float m_timeSinceTargetLastSeen = 0.0f;

        private Vector2 m_lastPosition;
        private Vector2 m_lastMoveDirection = Vector2.zero;  // Tracks the previous frame's movement direction
        private int m_sameDirectionCount = 0;               // Counter for consecutive similar movement directions
        private float m_stuckTime = 0f;
        private const float STUCK_THRESHOLD_TIME = 0.5f;
        private const float STUCK_DISTANCE_THRESHOLD = 0.05f;
        private Vector2 m_wallFollowDirection = Vector2.zero;
        private float m_wallFollowTimer = 0f;
        private const float WALL_FOLLOW_DURATION = 1.0f;
        private Vector2 m_lastWallNormal = Vector2.zero;
        private float m_directionChangeTimer = 0f;
        private const float DIRECTION_CHANGE_COOLDOWN = 1.5f; // Prevent rapid direction changes

        // 当前计算的方向数组
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

        public float m_weightMemoryFactor = 0.5f; // 历史路径记忆权重
        public float m_randomExplorationWeight = 0.1f; // 随机探索权重
        public int initialDirectionCount = 8; // 初始方向数量

        private Vector2 m_previousDirection = Vector2.zero; // 上一次移动方向

        private void Awake()
        {
            Debug.Assert(m_rigidbody, ErrorMessages.InspectorMissingComponentReference<Rigidbody2D>());
            m_initialPosition = transform.position;
        }

        // 获取技能是在Awake所以得在Start后面执行这个更改
        private void Start()
        {
            m_lastPosition = transform.position;

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
                            m_attackTriggerRadius = 1.0f;
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
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, directionToTarget, Vector2.Distance(currentPosition, targetPosition), GameManager.Config.monsterCollisionContactFilter.layerMask);
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
            // 动态调整兴趣范围
            float dynamicRange = Mathf.Lerp(90.0f, 30.0f, Vector2.Distance(currentPosition, targetPosition) / 10.0f);

            m_interests[index] = Mathf.Max(1.0f - (angleToTargetDirection / dynamicRange), 0.0f);
            //m_interests[index] = Math.Max(1.0f - (angleToTargetDirection / 90.0f), 0.0f);
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

            float totalDanger = 0.0f;
            for (int i = 0; i < count; ++i)
            {
                totalDanger += 1.0f - m_castCollisions[i].distance;
            }

            m_dangers[index] = count > 0 ? totalDanger / count : 0.0f;
            //m_dangers[index] = count > 0 ? 1.0f - m_castCollisions[0].distance : 0.0f;
        }

        private void ProcessSteeringBehaviour(int index)
        {
            ProcessChaseBehaviour(index);
            ProcessAvoidBehaviour(index);

            // 原来
            //m_steering[index] = m_interests[index] - m_dangers[index];

            //// 考虑历史移动方向的影响
            //float alignment = Vector2.Dot(m_directions[index], m_previousDirection);
            //float alignmentWeight = Mathf.Max(0.0f, alignment) * m_weightMemoryFactor;
            //// 考虑历史计算最终权重
            //m_steering[index] = m_interests[index] - m_dangers[index] + alignmentWeight;

            float interest = m_interests[index];
            float danger = m_dangers[index];

            // Exponential danger scaling to create stronger avoidance
            danger = danger > 0 ? Mathf.Pow(danger, 1.5f) : 0;

            // Reduce alignment influence when near obstacles
            float alignmentWeight = (1.0f - danger) * m_weightMemoryFactor;
            float alignment = Vector2.Dot(m_directions[index], m_previousDirection);
            alignment = Mathf.Max(0.0f, alignment) * alignmentWeight;

            // Final weight calculation with higher emphasis on avoiding obstacles
            m_steering[index] = interest - (danger * 1.5f) + alignment;
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
                    FaceTarget();

                    TryToAttackTarget(distanceToTarget);
                }
                else // 目标不在攻击范围内时才检查追逐逻辑
                {
                    CheckIfTargetOutOfRange(distanceToTarget);
                }
            }

            // 计算到目标的距离
            UpdateTargetPosition();

            OriginalNavigationFunction();
        }

        public void FaceTarget()
        {
            if (m_character.Can(EActionFlags.Move) && m_target)
            {
                // 计算目标的方向向量
                Vector3 directionToTarget = m_target.position - transform.position;

                //Debug.Log("directionToTarget = " + directionToTarget);

                // 使方向向量标准化，避免速度放大
                directionToTarget.Normalize();

                /* 计算水平方向和垂直方向的角度
                    Mathf.Asin 接受的是一个范围在 [-1, 1] 之间的数值，返回该值对应的角度（以弧度为单位）
                    其返回值范围是 [-π/2, π/2]（对应角度为 [-90°, 90°]）。
                    使用 Mathf.Asin(directionToTarget.x)，它只会计算向量的 x 分量投影 与完整向量长度的比值的反三角函数，并不是水平角度。
                    水平角度通常表示向量在水平面（如 X-Z 平面）上的投影，与 X 轴之间的夹角。*/
                float xAngle = Mathf.Atan2(directionToTarget.z, directionToTarget.x) * Mathf.Rad2Deg;
                float yAngle = Mathf.Asin(directionToTarget.y) * Mathf.Rad2Deg;

                // 使用 SetLookAtDirection 来设置怪物的朝向，传递两个角度
                m_character.SetLookAtDirection(xAngle, yAngle);
            }
        }

        private void FaceTargetOld()
        {
            if (m_character.Can(EActionFlags.Move))
            {
                //m_character.SetLookAtDirection(m_targetPosition.x - transform.position.x); // Make sure the AI face its target
            }
        }

        private void UpdateDirections(int count)
        {
            m_directions = new Vector2[count];
            m_interests = new float[count];
            m_dangers = new float[count];
            m_steering = new float[count];

            float angleStep = 360.0f / count;
            for (int i = 0; i < count; ++i)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                m_directions[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }
        }

        private void ImprovedNavigationFunction()
        {
            float distanceToTarget = Vector2.Distance(transform.position, m_targetPosition);

            if (distanceToTarget > m_soughtDistanceFromTarget)
            {
                m_steeringAverageOutput = Vector2.zero;
                Vector2 dirToTarget = (m_targetPosition - (Vector2)transform.position).normalized;

                // First, check if we have a clear path to target
                bool hasDirectPath = CheckDirectPathToTarget();

                if (hasDirectPath)
                {
                    // If we have a clear path, go directly to target while maintaining obstacle avoidance
                    ProcessDirectApproach(dirToTarget);
                }
                else
                {
                    // If no clear path, use the improved navigation logic
                    ProcessObstacleNavigation(dirToTarget);
                }

                // Update tracking variables
                m_lastPosition = transform.position;
                m_lastMoveDirection = ((Vector2)transform.position - m_lastPosition).normalized;
            }
            else
            {
                m_character.SetMovementDirection(Vector2.zero);
                FaceTarget();
            }
        }

        private bool CheckDirectPathToTarget()
        {
            Vector2 dirToTarget = (m_targetPosition - (Vector2)transform.position).normalized;
            float distanceToTarget = Vector2.Distance(transform.position, m_targetPosition);

            // Cast a ray to check if there are obstacles between us and the target
            int hitCount = m_rigidbody.Cast(
                dirToTarget,
                GameManager.Config.collisionContactFilter,
                m_castCollisions,
                distanceToTarget
            );

            return hitCount == 0;
        }

        private void ProcessDirectApproach(Vector2 dirToTarget)
        {
            int dynamicDirectionCount = 12; // Reduced number of directions for direct approach
            UpdateDirections(dynamicDirectionCount);

            // Process directions with heavy bias towards target
            for (int i = 0; i < dynamicDirectionCount; ++i)
            {
                ProcessSteeringBehaviour(i);

                // Add strong bias towards target direction
                float targetAlignment = Vector2.Dot(m_directions[i], dirToTarget);
                m_steering[i] += targetAlignment * 2.0f; // Increased target bias

                m_steeringAverageOutput += m_directions[i] * m_steering[i];
            }

            if (m_steeringAverageOutput.magnitude > 0.1f)
            {
                m_steeringAverageOutput.Normalize();

                // Heavily bias towards target direction
                Vector2 finalDirection = Vector2.Lerp(m_steeringAverageOutput, dirToTarget, 0.7f);
                m_lerpedTargetDirection = Vector2.Lerp(m_lerpedTargetDirection, finalDirection,
                    Time.fixedDeltaTime * m_steeringDriftResponsiveness);
            }
            else
            {
                m_lerpedTargetDirection = dirToTarget;
            }

            // Reset wall following and stuck detection
            m_wallFollowDirection = Vector2.zero;
            m_stuckTime = 0f;
            m_sameDirectionCount = 0;

            m_character.SetMovementDirection(m_lerpedTargetDirection.normalized);
        }

        private void ProcessObstacleNavigation(Vector2 dirToTarget)
        {
            int dynamicDirectionCount = Mathf.Clamp((int)(Vector2.Distance(transform.position, m_targetPosition) * 2), 12, 24);
            UpdateDirections(dynamicDirectionCount);

            Vector2 bestDirection = Vector2.zero;
            float bestScore = float.MinValue;

            // Process the steering behaviour for each direction
            for (int i = 0; i < dynamicDirectionCount; ++i)
            {
                ProcessSteeringBehaviour(i);

                if (m_steering[i] > bestScore)
                {
                    bestScore = m_steering[i];
                    bestDirection = m_directions[i];
                }

                m_steeringAverageOutput += m_directions[i] * m_steering[i];
            }

            // Check if we're stuck or circling
            float distanceMoved = Vector2.Distance(transform.position, m_lastPosition);
            bool isMovingVeryLittle = distanceMoved < STUCK_DISTANCE_THRESHOLD;

            float directionDifference = Vector2.Dot(
                ((Vector2)transform.position - m_lastPosition).normalized,
                m_lastMoveDirection);

            if (directionDifference > 0.9f)
            {
                m_sameDirectionCount++;
            }
            else
            {
                m_sameDirectionCount = 0;
            }

            bool isCircling = m_sameDirectionCount > 20;

            if (isMovingVeryLittle || isCircling)
            {
                m_stuckTime += Time.fixedDeltaTime;
            }
            else
            {
                m_stuckTime = 0f;
                m_wallFollowTimer = 0f;
                m_wallFollowDirection = Vector2.zero;
                m_lastWallNormal = Vector2.zero;
            }

            bool isStuck = m_stuckTime >= STUCK_THRESHOLD_TIME;

            if (isStuck)
            {
                HandleStuckState(bestDirection, dirToTarget);
            }
            else
            {
                HandleNormalNavigation(bestDirection, dirToTarget);
            }
        }

        private void HandleStuckState(Vector2 bestDirection, Vector2 dirToTarget)
        {
            Vector2 wallNormal = GetWallNormal();

            if (wallNormal != Vector2.zero)
            {
                if (m_wallFollowDirection == Vector2.zero ||
                    Vector2.Dot(wallNormal, m_lastWallNormal) < 0.9f)
                {
                    Vector2 clockwiseDir = Quaternion.Euler(0, 0, 90) * wallNormal;
                    Vector2 counterClockwiseDir = Quaternion.Euler(0, 0, -90) * wallNormal;

                    float clockwiseDot = Vector2.Dot(clockwiseDir, dirToTarget);
                    float counterClockwiseDot = Vector2.Dot(counterClockwiseDir, dirToTarget);

                    m_wallFollowDirection = (clockwiseDot > counterClockwiseDot) ? clockwiseDir : counterClockwiseDir;
                    m_wallFollowDirection = (m_wallFollowDirection * 0.8f + dirToTarget * 0.2f).normalized;

                    m_lastWallNormal = wallNormal;
                    m_sameDirectionCount = 0;
                }
            }
            else
            {
                m_wallFollowDirection = Vector2.Lerp(bestDirection, dirToTarget, 0.3f).normalized;
            }

            m_lerpedTargetDirection = m_wallFollowDirection;
            m_previousDirection = m_wallFollowDirection;
            m_weightMemoryFactor = 0.2f;

            m_character.SetMovementDirection(m_lerpedTargetDirection.normalized);
        }

        private void HandleNormalNavigation(Vector2 bestDirection, Vector2 dirToTarget)
        {
            if (m_steeringAverageOutput.magnitude > 0.1f)
            {
                m_steeringAverageOutput.Normalize();

                Vector2 combinedDirection = m_steeringAverageOutput * 0.8f +
                                          m_previousDirection * (m_weightMemoryFactor * 0.2f);
                combinedDirection.Normalize();

                // Add moderate bias towards target
                combinedDirection = Vector2.Lerp(combinedDirection, dirToTarget, 0.3f);

                m_previousDirection = Vector2.Lerp(m_previousDirection, combinedDirection, 0.3f);

                m_lerpedTargetDirection = !m_character.IsMoving() ?
                    combinedDirection :
                    Vector2.Lerp(m_lerpedTargetDirection, combinedDirection,
                        Time.fixedDeltaTime * m_steeringDriftResponsiveness);
            }
            else
            {
                m_lerpedTargetDirection = bestDirection;
            }

            m_character.SetMovementDirection(m_lerpedTargetDirection.normalized);
        }

        private Vector2 GetWallNormal()
        {
            const float rayDistance = 1.0f;
            Vector2 wallNormal = Vector2.zero;
            int hitCount = 0;

            // Cast rays in multiple directions to find nearby walls
            for (int i = 0; i < 8; i++)
            {
                float angle = i * (360f / 8);
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                if (m_rigidbody.Cast(direction, GameManager.Config.collisionContactFilter, m_castCollisions, rayDistance) > 0)
                {
                    wallNormal += -direction * (1.0f - m_castCollisions[0].distance / rayDistance);
                    hitCount++;
                }
            }

            if (hitCount > 0)
            {
                wallNormal /= hitCount;
                wallNormal.Normalize();
            }

            return wallNormal;
        }

        private void OriginalNavigationFunction()
        {

            float distanceToTarget = Vector2.Distance(transform.position, m_targetPosition);

            if (distanceToTarget > m_soughtDistanceFromTarget)
            {
                m_steeringAverageOutput = Vector2.zero;

                // 计算动态方向数量
                int dynamicDirectionCount = Mathf.Clamp((int)(distanceToTarget * 2), 8, 16);
                UpdateDirections(dynamicDirectionCount);

                // Process the steering behaviour for each direction
                for (int i = 0; i < 8; ++i)
                {
                    ProcessSteeringBehaviour(i);
                    m_steeringAverageOutput += m_directions[i] * m_steering[i];
                }

                // 归一化当前的引导方向
                m_steeringAverageOutput.Normalize();

                // 计算结合历史记忆后的方向
                Vector2 combinedDirection = m_steeringAverageOutput + m_previousDirection * m_weightMemoryFactor;

                // 更新 m_previousDirection 为最新的综合方向
                m_previousDirection = combinedDirection.normalized;

                // 如果 m_steeringAverageOutput 太小（表示没有找到有效路径），则进行随机探索
                if (m_steeringAverageOutput.magnitude < 0.1f)
                {
                    // 增加随机探索行为
                    float randomAngle = UnityEngine.Random.Range(0f, 360f);
                    m_steeringAverageOutput = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));

                    // 降低历史记忆的影响，避免过度依赖过去路径
                    m_weightMemoryFactor = 0.2f;
                }
                else
                {
                    // 恢复正常的历史记忆权重
                    m_weightMemoryFactor = 0.8f;
                }

                // 平滑过渡最终目标方向
                m_lerpedTargetDirection = !m_character.IsMoving() ?
                    combinedDirection : // 如果怪物没有在移动，直接使用组合方向
                    Vector2.Lerp(m_lerpedTargetDirection, combinedDirection, Time.fixedDeltaTime * m_steeringDriftResponsiveness);


                // 设置刚体移动
                m_character.SetMovementDirection(m_lerpedTargetDirection.normalized);
            }
            // 停止移动并面对目标
            else
            {
                m_character.SetMovementDirection(Vector2.zero);

                FaceTarget(); // 使怪物面向目标
            }
        }

        private void OnDrawGizmos()
        {
            // 绘制每个方向的引导射线（根据每个方向的权重来决定颜色）
            for (int i = 0; i < 8; ++i)
            {
                Gizmos.color = m_steering[i] > 0.0f ? Color.green : Color.red;
                Gizmos.DrawRay(transform.position, m_directions[i] * Mathf.Abs(m_steering[i]));
            }

            // 绘制综合的引导方向（即 m_steeringAverageOutput）
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, m_steeringAverageOutput);

            // 绘制带有历史路径记忆的方向（即 newDirection 和 m_steeringAverageOutput 的结合）
            Vector2 combinedDirection = m_steeringAverageOutput + m_previousDirection * m_weightMemoryFactor;
            Gizmos.color = Color.blue;  // 使用蓝色显示组合后的方向
            Gizmos.DrawRay(transform.position, combinedDirection);

            // 如果目标存在，绘制目标的视线
            if (m_target)
            {
                Gizmos.color = CanSee(m_target) ? Color.cyan : Color.magenta;
                Gizmos.DrawLine(transform.position, m_target.transform.position);
            }
        }

        private void saveFunction()
        {

            //// 归一化当前的引导方向
            //m_steeringAverageOutput.Normalize();

            //// 计算结合历史记忆后的方向
            //Vector2 combinedDirection = m_steeringAverageOutput + m_previousDirection * m_weightMemoryFactor;

            //// 更新 m_previousDirection 为最新的综合方向
            //m_previousDirection = combinedDirection.normalized;

            //// 平滑过渡最终目标方向
            //m_lerpedTargetDirection = !m_character.IsMoving() ?
            //    combinedDirection : // 如果怪物没有在移动，直接使用组合方向
            //    Vector2.Lerp(m_lerpedTargetDirection, combinedDirection, Time.fixedDeltaTime * m_steeringDriftResponsiveness);


            //// 归一化结果
            //m_steeringAverageOutput.Normalize();

            //// 计算最终目标方向（带有历史记忆的平滑过渡）
            //m_steeringAverageOutput = m_steeringAverageOutput + m_previousDirection * m_weightMemoryFactor;
            //m_previousDirection = m_steeringAverageOutput.normalized;

            //// 平滑过渡方向
            //m_lerpedTargetDirection = !m_character.IsMoving() ?
            //    m_steeringAverageOutput :
            //    Vector2.Lerp(m_lerpedTargetDirection, m_steeringAverageOutput, Time.fixedDeltaTime * m_steeringDriftResponsiveness);

            //// 更新移动方向
            //m_lerpedTargetDirection =
            //    !m_character.IsMoving() ?
            //    m_steeringAverageOutput :
            //    Vector2.Lerp(m_lerpedTargetDirection, m_steeringAverageOutput, Time.fixedDeltaTime * m_steeringDriftResponsiveness);
        }
    }
}
