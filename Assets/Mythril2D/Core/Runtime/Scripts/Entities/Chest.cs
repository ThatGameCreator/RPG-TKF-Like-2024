using System.Collections;
using UnityEngine;
using UnityEngine.U2D.Animation;
using static UnityEngine.EventSystems.EventTrigger;

namespace Gyvr.Mythril2D
{
    public class Chest : OtherEntity
    {
        [Header("References")]
        [SerializeField] private Animator m_chestAnimator = null;
        [SerializeField] private Animator m_contentAnimator = null;
        [SerializeField] private SpriteRenderer m_contentSpriteRenderer = null;
        [SerializeField] private SpriteLibrary m_nowSpriteLibrary = null;
        [SerializeField] private SpriteLibraryAsset m_emptySpriteLibraryAsset = null;

        [Header("Chest Settings")]
        [SerializeField] private Loot m_loot;
        [SerializeField] private Item[] m_requiredKeys = null;
        [SerializeField] private bool m_singleUse = false;
        [SerializeField] private string m_gameFlagID = "chest_00";
        [SerializeField] private string m_openedAnimationParameter = "opened";
        [SerializeField] private string m_contentRevealAnimationParameter = "reveal";
        [SerializeField] private float m_contentRevealIconCycleDuration = 1.0f;
        [SerializeField] private DialogueSequence m_noItemDialogue = null;
        [SerializeField] private DialogueSequence m_hasItemDialogue = null;
        [SerializeField] private bool is_monsterChest = false;
        [SerializeField] private int m_damageAmount = 3;
        [SerializeField] private EDamageType m_damageType = EDamageType.Physical;
        [SerializeField] private EDistanceType m_distanceType = EDistanceType.Ranged;
        [SerializeField] private bool m_randomGenerateKey = false;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_openingSound;
        [SerializeField] private AudioClipResolver m_canNotOpenSound;

        private Item m_requiredKey = null;

        private bool m_hasOpeningAnimation = false;
        private bool m_hasRevealAnimation = false;
        public bool opened => m_opened;
        private bool m_opened = false;

        protected void Awake()
        {
            Debug.Assert(m_chestAnimator, ErrorMessages.InspectorMissingComponentReference<Animator>());
            Debug.Assert(m_contentAnimator, ErrorMessages.InspectorMissingComponentReference<Animator>());
            Debug.Assert(m_contentSpriteRenderer, ErrorMessages.InspectorMissingComponentReference<SpriteRenderer>());

            if (m_chestAnimator)
            {
                m_hasOpeningAnimation = AnimationUtils.HasParameter(m_chestAnimator, m_openedAnimationParameter);
            }

            if (m_contentAnimator)
            {
                m_hasRevealAnimation = AnimationUtils.HasParameter(m_contentAnimator, m_contentRevealAnimationParameter);
            }
        }

        protected override void Start()
        {
            base.Start();

            if (m_randomGenerateKey)
            {
                // 随机决定是否需要钥匙
                bool isNeedKey = Random.Range(0, 2) == 0;

                if (isNeedKey && m_requiredKeys.Length != 0)
                {
                   m_requiredKey = m_requiredKeys[Random.Range(0, m_requiredKeys.Length)];
                }
            }
            
            if (m_singleUse && GameManager.GameFlagSystem.Get(m_gameFlagID))
            {
                m_opened = true;
                TryPlayOpeningAnimation(m_opened);
            }
        }

        public override void OnStartInteract(CharacterBase sender, Entity target)
        {
            if (target != this)
            {
                return;
            }

            if (m_requiredKey != null)
            {
                if (GameManager.InventorySystem.HasItemInBag(m_requiredKey))
                {
                    if (this.opened == false)
                    {
                        GameManager.Player.OnTryStartLoot(target, m_lootedTime);
                    }
                    else
                    {
                        base.OnEndInteract(sender, target);
                    }
                }
                else
                {
                    GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_canNotOpenSound);

                }
            }
            else
            {
                if (this.opened == false)
                {
                    GameManager.Player.OnTryStartLoot(target, m_lootedTime);
                }
                else
                {
                    base.OnEndInteract(sender, target);
                }
            }
        }

        public bool TryPlayOpeningAnimation(bool open)
        {
            if (m_chestAnimator && m_hasOpeningAnimation)
            {
                m_chestAnimator.SetBool(m_openedAnimationParameter, open);
                return true;
            }

            return false;
        }

        public bool TryPlayContentRevealAnimation()
        {
            if (m_contentSpriteRenderer && m_contentAnimator && m_hasRevealAnimation)
            {
                Sprite[] sprites = m_loot.GetLootSprites();

                if (sprites.Length > 0)
                {
                    StartCoroutine(UpdateContentSprite(sprites, m_contentRevealIconCycleDuration));
                    m_contentAnimator.SetTrigger(m_contentRevealAnimationParameter);
                    return true;
                }

                return false;
            }

            return false;
        }

        private IEnumerator UpdateContentSprite(Sprite[] sprites, float duration)
        {
            if (sprites.Length == 0) yield break;

            float interval = duration / sprites.Length;

            for (int index = 0; index < sprites.Length; ++index)
            {
                m_contentSpriteRenderer.sprite = sprites[index];
                yield return new WaitForSeconds(interval);
            }
        }

        public bool TryOpen()
        {
            if (!m_opened)
            {
                TryPlayOpeningAnimation(true);
                
                if (is_monsterChest)
                {
                    GameManager.Player.Damage(new DamageOutputDescriptor
                     {
                         source = EDamageSource.Unknown,
                         attacker = this,
                         damage = m_damageAmount,
                         damageType = m_damageType,
                         distanceType = m_distanceType,
                         flags = EDamageFlag.None
                     });

                    this.gameObject.layer = LayerMask.NameToLayer("Default");

                    return true;
                }

                if (!m_loot.IsEmpty())
                {
                    TryPlayContentRevealAnimation();

                    GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_openingSound);

                    if (m_loot.entries != null)
                    {
                        foreach (var entry in m_loot.entries)
                        {
                            if (m_hasItemDialogue)
                                {
                                    GameManager.DialogueSystem.Main.AddToQueue(
                                    m_hasItemDialogue.ToDialogueTree(
                                        string.Empty, $"{entry.item.DisplayName} x{entry.quantity}"
                                    )
                                );
                            }

                            GameManager.InventorySystem.AddToBag(entry.item, entry.quantity);
                        }

                        if (m_loot.money != 0)
                        {
                            if (m_hasItemDialogue)
                            {
                                GameManager.DialogueSystem.Main.AddToQueue(
                                    m_hasItemDialogue.ToDialogueTree(
                                        string.Empty, $"{m_loot.money} <currency.fullName>"
                                    )
                                );
                            }


                            GameManager.InventorySystem.AddMoney(m_loot.money);
                        }
                    }
                }
                else
                {
                    if (m_hasItemDialogue)
                    {
                        GameManager.DialogueSystem.Main.AddToQueue(
                            m_noItemDialogue.ToDialogueTree(string.Empty)
                        );
                    }
                }

                this.gameObject.layer =  LayerMask.NameToLayer("Default");

                if (m_emptySpriteLibraryAsset)
                {
                    m_nowSpriteLibrary.spriteLibraryAsset = m_emptySpriteLibraryAsset;
                }

                if (m_requiredKey)
                {
                    GameManager.InventorySystem.RemoveFromBag(m_requiredKey);
                }

                m_opened = true;

                if (m_singleUse)
                {
                    if (string.IsNullOrWhiteSpace(m_gameFlagID))
                    {
                        Debug.LogError("No ChestID provided while SingleUse is checked. Make sure to provide this chest with an ID");
                    }
                    else
                    {
                        GameManager.GameFlagSystem.Set(m_gameFlagID, true);
                    }
                }

                return true;
            }

            return false;
        }
    }
}
