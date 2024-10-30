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

        [SerializeField] private HoleDatabase m_holeDatabase = null;

        private string[] teleportName = {
            "PS_Small_Corner",
            "PS_Narrow_Tunnel",
            "PS_Centre_Corner",
            "PS_Rightside_Basin",
            "PS_Lower_Corner",
            "PS_Crossroads",
            "PS_Right_Corner",
        };

        public bool TryExecute(CharacterBase source, IInteractionTarget target)
        {
            if (m_holeDatabase != null)
            {
                target.Say(m_dialogueIfWantGetin, (messages) =>
                {
                    if (messages.Contains(EDialogueMessageType.Accept))
                    {
                        GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_holeDatabase.getInSound);

                        GameManager.TeleportLoadingSystem.RequestTransition("That_Abyss", null, null,
                            () =>
                            {
                                //var teleports = GameObject.Find("Player Spawner");

                                //int teleportsLength = teleports.GetComponentsInChildren<Transform>().Length;

                                //int randomNumber = UnityEngine.Random.Range(0, teleportsLength);

                                //OnTransitionComplete(teleports.GetComponentsInChildren<Transform>()[randomNumber].name);

                                GameManager.DayAndNightSystem.OnEnableDayNightSystem();
                            }, 
                            ETeleportType.Normal, teleportName[UnityEngine.Random.Range(0, teleportName.Length)]);

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
