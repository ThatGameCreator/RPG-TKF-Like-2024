using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public class SurfaceItemInteraction : IInteraction
    {
        [SerializeField] private SurfaceItem m_surfaceItem = null;

        public bool TryExecute(CharacterBase source, IInteractionTarget target)
        {
            return m_surfaceItem.TryLooted();
        }
    }
}
