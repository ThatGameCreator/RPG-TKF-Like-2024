using FunkyCode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using static UnityEngine.GraphicsBuffer;

namespace Gyvr.Mythril2D
{
    public class  Entity : MonoBehaviour, IInteractionTarget
    {
        [Header("Entity Settings")]
        [SerializeReference, SubclassSelector] private IInteraction m_interaction = null;

        public virtual string GetSpeakerName() => string.Empty;

        [Header("Lighting")]
        // 第一个是自己的精灵 其他的是阴影
        [SerializeField] protected SpriteRenderer[] m_spriteRenderer = null;
        [SerializeField] protected LightEventListener m_lightEventListener = null;
        [SerializeField] private float m_showAnimationSpeed = 10f;
        [SerializeField] private float m_hideAnimationSpeed = 10f;

        private Color m_initialSpriteColor = Color.white;

        protected virtual void Start()
        {
            GameManager.NotificationSystem.playerTryInteracte.AddListener(OnStartInteract);

            GameManager.NotificationSystem.playerEndInteracte.AddListener(OnEndInteract);
        }

        protected virtual void OnDestroy()
        {
            GameManager.NotificationSystem.playerTryInteracte.RemoveListener(OnStartInteract);

            GameManager.NotificationSystem.playerEndInteracte.RemoveListener(OnEndInteract);
        }

        protected void UpdateFieldOfWar()
        {
            //Debug.Log("UpdateFieldOfWar");

            if (m_spriteRenderer != null)
            {
                foreach (SpriteRenderer spriteRenderer in m_spriteRenderer)
                {
                    // 缓存材质的颜色
                    Color materialColor = spriteRenderer.material.color;

                    // 目标颜色
                    Color targetColor = m_lightEventListener.visability >= 0.5f
                        ? m_initialSpriteColor
                        : new Color(materialColor.r, materialColor.g, materialColor.b, 0f);

                    // 插值更新颜色
                    float animationSpeed = m_lightEventListener.visability >= 0.5f
                        ? m_showAnimationSpeed
                        : m_hideAnimationSpeed;

                    spriteRenderer.material.color = Color.Lerp(materialColor, targetColor, animationSpeed * Time.unscaledDeltaTime);
                }
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

        public virtual void OnStartInteract(CharacterBase sender, Entity target)
        {
            if (target != this)
            {
                return;
            }

            // 左边是发起互动对象 右边是被互动对象
            m_interaction?.TryExecute(sender, this);
        }

        public virtual void OnEndInteract(CharacterBase sender, Entity target)
        {
            if (target != this)
            {
                return;
            }

            // 左边是发起互动对象 右边是被互动对象
            m_interaction?.TryExecute(sender, this);
        }
    }
}
