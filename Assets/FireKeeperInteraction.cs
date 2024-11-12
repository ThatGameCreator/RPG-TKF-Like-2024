using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public class FireKeeperInteraction : IInteraction
    {
        [Header("Dialogues")]
        [SerializeField] private DialogueSequence m_dialogueOption = null;
        [SerializeField] private DialogueSequence m_dialogueIfUpgrade = null;
        [SerializeField] private DialogueSequence m_dialogueIfHeal = null;
        [SerializeField] private DialogueSequence m_dialogueIfCanPayHeal = null;
        [SerializeField] private DialogueSequence m_dialogueIfCannotPayHeal = null;
        [SerializeField] private DialogueSequence m_dialogueIfCancel = null;

        [Header("References")]
        [SerializeField] private Inn m_inn = null;

        public bool TryExecute(CharacterBase source, IInteractionTarget target)
        {
            if (m_inn != null)
            {
                target.Say(m_dialogueOption, (messages) =>
                {
                    if (messages.Contains("Upgrade"))
                    {
                        //Debug.Log("Upgrade");

                        // �ڶԻ����ñ��������װ����ѡ��Ͳ���Ҫ��������ִ�жԻ���
                        // Ҳ��˵��������ݱ�������ѡ��Ͳ���Ҫд���� �����Ҫִ�ж�������ѡ��Ļ� ���ǵ����ô���
                        target.Say(m_dialogueIfUpgrade);

                        GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_inn.healingSound);
                    }

                    else if (messages.Contains("Heal"))
                    {
                        //Debug.Log("Heal");

                        if (GameManager.InventorySystem.HasSufficientFunds(m_inn.price))
                        {
                            target.Say(m_dialogueIfHeal, (messages) =>
                            {
                                //Debug.Log("EDialogueMessageType.Accept");

                                if (messages.Contains(EDialogueMessageType.Accept))
                                {
                                    target.Say(m_dialogueIfCanPayHeal);
                                    GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_inn.healingSound);
                                    GameManager.InventorySystem.RemoveMoney(m_inn.price);
                                    GameManager.Player.Heal(m_inn.healAmount);
                                    GameManager.Player.RecoverMana(m_inn.manaRecoveredAmount);
                                }
                                else
                                {
                                    target.Say(m_dialogueIfCancel);

                                }
                            }, m_inn.price.ToString());
                        }
                        else
                        {
                            target.Say(m_dialogueIfCannotPayHeal);
                        }
                    }

                    else if (messages.Contains("None"))
                    {
                        target.Say(m_dialogueIfCancel);
                    }
                });
                return true;
            }
            return false;
        }
    }
}