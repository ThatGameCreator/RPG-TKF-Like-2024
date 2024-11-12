using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public class JournalInteraction : IInteraction
    {

        public bool TryExecute(CharacterBase source, IInteractionTarget target)
        {
            GameManager.NotificationSystem.journalRequested.Invoke();

            return true;
        }
    }
}
