using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public class AbyssHoleInteraction : IInteraction
    {
        [Header("Dialogues")]
        [SerializeField] private Hole m_hole = null;
        [SerializeField] private DialogueSequence m_dialogueIfWantGetin = null;
        [SerializeField] private DialogueSequence m_dialogueIfDontWantGetin = null;

        [SerializeField] private HoleDatabase m_holeDatabase = null;

        private string[] teleportNames = {
            "PS_Small_Corner",
            "PS_Narrow_Tunnel",
            "PS_Centre_Corner",
            "PS_Rightside_Basin",
            "PS_Lower_Corner",
            "PS_Crossroads",
            "PS_Right_Corner",
            "PS_Vertical_Tunnel",
        };

        public bool TryExecute(CharacterBase source, IInteractionTarget target)
        {
            if (m_holeDatabase != null)
            {
                target.Say(m_dialogueIfWantGetin, (messages) =>
                {
                    if (messages.Contains(EDialogueMessageType.Accept))
                    {
                        if (m_hole)
                        {
                            m_hole.gameObject.SetActive(false);
                        }

                        GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_holeDatabase.getInSound);

                        string teleportName = teleportNames[UnityEngine.Random.Range(0, teleportNames.Length)];

                        GameManager.TeleportLoadingSystem.RequestTransition("That_Abyss", null,
                            () =>
                            {
                                GameManager.DayNightSystem.OnEnableDayNightSystem();

                                GameManager.NotificationSystem.SetActiveEvacuation.Invoke(teleportName);
                            }, null, 
                            ETeleportType.Normal, teleportName);

                        // 在 lambda 里面居然没有赋值？
                        //Debug.Log("after" + teleportName);
                    }
                });

                return true;
            }

            return false;
        }
    }
}
