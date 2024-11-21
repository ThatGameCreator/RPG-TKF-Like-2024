using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public class PlaceItemInteraction : IInteraction
    {
        [SerializeField] private PlaceItemObject m_placeItemObject = null;

        public bool TryExecute(CharacterBase source, IInteractionTarget target)
        {
            return m_placeItemObject.TryPlaceItem();
        }
    }
}
