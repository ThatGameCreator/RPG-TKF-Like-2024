using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public class EvacuationPointInteraction : IInteraction
    {
        [Header("Dialogues")]
        [SerializeField] private DialogueSequence m_dialogueIfWantGetOut = null;
        [SerializeField] private DialogueSequence m_dialogueIfDontWantGetOut = null;

        [Header("Reference")]
        [SerializeField] private EvacuationPointDatabase m_evacuationPointDatabase = null;
        [SerializeField] private EvacuationPoint m_evacuationPoint = null;

        public bool TryExecute(CharacterBase source, IInteractionTarget target)
        {
            bool isExcuted = false;

            target.Say(m_dialogueIfWantGetOut, (messages) =>
            {
                if (messages.Contains(EDialogueMessageType.Accept))
                {
                    GameManager.Player.OnStarEvacuate();

                    isExcuted = true;
                }
                    
            });

            return isExcuted;
        }

    }
}
