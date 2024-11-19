using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public class WarehouseInteraction : IInteraction
    {
        [SerializeField] private Warehouse m_warehouse = null;

        public bool TryExecute(CharacterBase source, IInteractionTarget target)
        {
            return m_warehouse.TryOpen();
        }
    }
}
