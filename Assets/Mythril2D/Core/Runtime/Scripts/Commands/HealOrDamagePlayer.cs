using System;
using Unity.Mathematics;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public class HealOrDamagePlayer : ICommand
    {
        [SerializeField] private EAction m_action = EAction.Add;
        [SerializeField][Min(0)] private int m_amount = 0;

        public void Execute()
        {
            switch (m_action)
            {
                case EAction.Add:
                    GameManager.Player.Heal(m_amount);
                    break;

                case EAction.Remove:
                    // 怎么感觉这个释放者才是玩家
                    //GameManager.Player.CancelLooting();
                    GameManager.Player.Damage(new DamageOutputDescriptor
                    {
                        attacker = null,
                        damage = math.abs(m_amount),
                        flags = EDamageFlag.Default,
                        type = EDamageType.None,
                        source = EDamageSource.Unknown
                    });
                    break;
            }
        }
    }
}
