using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public class AbyssHoleInteraction : IInteraction
    {
        [Header("Dialogues")]
        [SerializeField] private DialogueSequence m_dialogueIfWantGetin = null;
        [SerializeField] private DialogueSequence m_dialogueIfDontWantGetin = null;

        [SerializeField] private Hole m_hole = null;

        public bool TryExecute(CharacterBase source, IInteractionTarget target)
        {
            if (m_hole != null)
            {
                target.Say(m_dialogueIfWantGetin, (messages) =>
                {
                    if (messages.Contains(EDialogueMessageType.Accept))
                    {
                        GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_hole.getInSound);

                        GameManager.TeleportLoadingSystem.RequestTransition("That_Abyss", null, null, null, ETeleportType.Normal, "PS_The_Corner");
                    }
                    
                });

                return true;
            }

            return false;
        }
    }
}
