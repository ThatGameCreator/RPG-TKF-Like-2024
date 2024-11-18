using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class ProjectileAbility : ActiveAbility<ProjectileAbilitySheet>
    {
        [Header("References")]
        [SerializeField] private Animator m_animator = null;
        [SerializeField] private InstancePool m_projectilePool = null;
        [SerializeField] private Transform m_projectileSpawnPoint = null;

        [Header("Animation Parameters")]
        [SerializeField] private string m_fireAnimationParameter = "fire";

        public override void Init(CharacterBase character, AbilitySheet settings)
        {
            base.Init(character, settings);

            Debug.Assert(m_animator, ErrorMessages.InspectorMissingComponentReference<Animator>());
            Debug.Assert(m_animator.GetBehaviour<StateMessageDispatcher>(), string.Format("{0} not found on the projectile animator controller", typeof(StateMessageDispatcher).Name));
        }

        protected override void Fire()
        {
            m_animator?.SetTrigger(m_fireAnimationParameter);
        }

        public void OnChargeAnimationStart()
        {
            if (m_character.isPlayer == false)
            {
                m_character.GetComponent<Monster>().aiController.FaceTarget();
            }
        }

        public void OnChargeAnimationEnd()
        {
            if (!m_character.dead)
            {
                for (int i = 0; i < m_sheet.projectileCount; ++i)
                {
                    ThrowProjectile(i);
                }

                TerminateCasting();
            }
        }

        private void ThrowProjectileOld(int projectileIndex)
        {
            GameObject projectileInstance = m_projectilePool.GetAvailableInstance();

            projectileInstance.transform.position = m_projectileSpawnPoint.position;

            Quaternion offestRotation = m_projectileSpawnPoint.parent.parent.localRotation;

            projectileInstance.SetActive(true);

            Projectile projectile = projectileInstance.GetComponent<Projectile>();

            projectile.Throw(DamageSolver.SolveDamageOutput(m_character, m_sheet.damageDescriptor), SetPlayerProjectileAngle(projectileIndex, offestRotation), m_sheet.projectileSpeed);
        }

        private Vector3 SetPlayerProjectileAngle(int projectileIndex, Quaternion offestRotation)
        {
            bool lookAtLeft = m_character.GetLookAtDirection() == EDirection.Left;

            Vector3 direction =
                    lookAtLeft ?
                    Vector3.left :
                    Vector3.right;

            float angleOffset = (m_sheet.spread / m_sheet.projectileCount) * (projectileIndex - (int)(m_sheet.projectileCount / 2.0f));

            angleOffset = m_sheet.projectileCount % 2 == 0 ?
                (projectileIndex >= (int)(m_sheet.projectileCount / 2.0f)
                ? angleOffset + m_sheet.spread / m_sheet.projectileCount : angleOffset) : angleOffset;

            Vector3 offestSum = Quaternion.AngleAxis(angleOffset, lookAtLeft ? Vector3.forward : Vector3.back).eulerAngles + offestRotation.eulerAngles;

            direction = Quaternion.Euler(offestSum) * direction;
            //direction = Quaternion.AngleAxis(angleOffset, lookAtLeft ? Vector3.forward : Vector3.back) * direction;

            return direction;
        }

        private Vector3 SetMonsterProjectileAngle(int projectileIndex, Quaternion offestRotation)
        {
            // 攻击前再设置一下
            // 偷懒这样写，但感觉肯定有问题
            // 尝试攻击前重置了一次，这里发射的时候又重置了一次
            // 感觉实际上应该在对应事件中朝向
            // 而且这个用GetComponent也太蠢了
            m_character.GetComponent<Monster>().aiController.FaceTarget();

            // 获取目标的X和Y角度
            float targetXAngle = m_character.FaceToTargetXAngle;
            float targetYAngle = m_character.FaceToTargetYAngle;

            Debug.Log("targetXAngle, targetYAngle" + m_character.FaceToTargetXAngle + " + " + m_character.FaceToTargetYAngle);

            // 使用目标的X和Y角度来计算方向
            Vector3 direction = new Vector3(Mathf.Cos(targetYAngle * Mathf.Deg2Rad) * Mathf.Cos(targetXAngle * Mathf.Deg2Rad),
                                            Mathf.Sin(targetYAngle * Mathf.Deg2Rad),
                                            Mathf.Cos(targetYAngle * Mathf.Deg2Rad) * Mathf.Sin(targetXAngle * Mathf.Deg2Rad));

            // 处理角度偏移
            float angleOffset = (m_sheet.spread / m_sheet.projectileCount) * (projectileIndex - (int)(m_sheet.projectileCount / 2.0f));

            angleOffset = m_sheet.projectileCount % 2 == 0 ?
                (projectileIndex >= (int)(m_sheet.projectileCount / 2.0f)
                ? angleOffset + m_sheet.spread / m_sheet.projectileCount : angleOffset) : angleOffset;

            // 使用偏移角度来调整方向
            direction = Quaternion.Euler(0, 0, angleOffset) * direction;

            return direction;
        }

        private void ThrowProjectile(int projectileIndex)
        {
            GameObject projectileInstance = m_projectilePool.GetAvailableInstance();

            projectileInstance.transform.position = m_projectileSpawnPoint.position;

            Quaternion offestRotation = m_projectileSpawnPoint.parent.parent.localRotation;

            projectileInstance.SetActive(true);

            Projectile projectile = projectileInstance.GetComponent<Projectile>();

            if(m_character.isPlayer == true)
            {
                projectile.Throw(DamageSolver.SolveDamageOutput(m_character, m_sheet.damageDescriptor), 
                    SetPlayerProjectileAngle(projectileIndex, offestRotation), m_sheet.projectileSpeed);
            }
            else
            {
                projectile.Throw(DamageSolver.SolveDamageOutput(m_character, m_sheet.damageDescriptor), 
                    SetMonsterProjectileAngle(projectileIndex, offestRotation), m_sheet.projectileSpeed);
            }
        }
    }
}
