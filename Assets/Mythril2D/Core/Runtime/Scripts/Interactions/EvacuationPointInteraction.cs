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

        [SerializeField] private EvacuationPointDatabase m_evacuationPoint = null;

        public bool TryExecute(CharacterBase source, IInteractionTarget target)
        {
            bool isExcuted = false;

            target.Say(m_dialogueIfWantGetOut, (messages) =>
            {
                if (messages.Contains(EDialogueMessageType.Accept))
                {
                    GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_evacuationPoint.getInSound);

                    GameManager.TeleportLoadingSystem.RequestTransition("Pilgrimage_Place", null, null, null, ETeleportType.Normal, "Player_Spawner");

                    isExcuted = true;
                }
                    
            });

            return isExcuted;
        }

        private
    }
}
