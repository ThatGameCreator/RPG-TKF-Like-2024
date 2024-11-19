using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public class DeadBodyInteraction : IInteraction
    {
        [SerializeField] private DeadBody m_deadBody = null;

        public bool TryExecute(CharacterBase source, IInteractionTarget target)
        {
            return m_deadBody.TryLooted();
        }
    }
}
