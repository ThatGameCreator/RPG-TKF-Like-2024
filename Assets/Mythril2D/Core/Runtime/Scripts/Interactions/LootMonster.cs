using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public class MonsterInteraction : IInteraction
    {
        [SerializeField] private Monster monster;

        public bool TryExecute(CharacterBase source, IInteractionTarget target)
        {
            return monster.LootFinished();
        }
    }
}
