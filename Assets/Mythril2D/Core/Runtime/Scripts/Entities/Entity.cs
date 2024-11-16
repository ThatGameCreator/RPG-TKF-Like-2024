using FunkyCode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

namespace Gyvr.Mythril2D
{
    public class Entity : MonoBehaviour, IInteractionTarget
    {
        [Header("Entity Settings")]
        [SerializeReference, SubclassSelector] private IInteraction m_interaction = null;

        public virtual string GetSpeakerName() => string.Empty;

        [Header("Lighting")]
        [SerializeField] protected SpriteRenderer m_spriteRenderer = null;
        [SerializeField] protected LightEventListener m_lightEventListener = null;
        [SerializeField] private float m_showAnimationSpeed = 10f;
        [SerializeField] private float m_hideAnimationSpeed = 10f;

        private Color m_initialSpriteColor = Color.white;

        protected void UpdateFieldOfWar()
        {
            Debug.Log("UpdateFieldOfWar");

            if (m_spriteRenderer != null)
            {
                // 缓存材质的颜色
                var materialColor = m_spriteRenderer.material.color;

                // 目标颜色
                Color targetColor = m_lightEventListener.visability >= 0.7f
                    ? m_initialSpriteColor
                    : new Color(materialColor.r, materialColor.g, materialColor.b, 0f);

                // 插值更新颜色
                float animationSpeed = m_lightEventListener.visability >= 0.7f
                    ? m_showAnimationSpeed
                    : m_hideAnimationSpeed;

                m_spriteRenderer.material.color = Color.Lerp(materialColor, targetColor, animationSpeed * Time.unscaledDeltaTime);
            }
        }

        public virtual void Say(DialogueSequence sequence, UnityAction<DialogueMessageFeed> onDialogueEnded = null, params string[] args)
        {
            string speaker = GetSpeakerName();

            DialogueTree dialogueTree = sequence.ToDialogueTree(speaker, args);

            if (onDialogueEnded != null)
            {
                dialogueTree.dialogueEnded.AddListener(onDialogueEnded);
            }

            GameManager.DialogueSystem.Main.PlayNow(dialogueTree);
        }

        public virtual void OnInteract(CharacterBase sender)
        {
            m_interaction?.TryExecute(sender, this);
        }
    }
}
