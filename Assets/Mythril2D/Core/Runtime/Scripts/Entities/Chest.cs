using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.U2D.Animation;
using static Gyvr.Mythril2D.LootTable;
using static UnityEngine.EventSystems.EventTrigger;

namespace Gyvr.Mythril2D
{
    public class Chest : OtherEntity
    {
        [Header("References")]
        [SerializeField] private Animator m_chestAnimator = null;
        [SerializeField] private SpriteLibrary m_nowSpriteLibrary = null;
        [SerializeField] private SpriteLibraryAsset m_emptySpriteLibraryAsset = null;

        [Header("Chest Settings")]
        //[SerializeField] private Loot m_loot;
        [SerializeField] private LootTable lootTable;   // ���� ScriptableObject ���ݱ�
        [SerializeField] private Item[] m_requiredKeys = null;
        [SerializeField] private bool m_singleUse = false;
        [SerializeField] private string m_gameFlagID = "chest_00";
        [SerializeField] private string m_openedAnimationParameter = "opened";
        [SerializeField] private string m_contentRevealAnimationParameter = "reveal";
        [SerializeField] private float m_contentRevealIconCycleDuration = 1.0f;
        [SerializeField] private bool m_isMonsterChest = false;
        [SerializeField] private bool m_isEmptyChest = false;
        [SerializeField] private int m_damageAmount = 3;
        [SerializeField] private EDamageType m_damageType = EDamageType.Physical;
        [SerializeField] private EDistanceType m_distanceType = EDistanceType.Ranged;
        [SerializeField] private bool m_randomGenerateKey = false;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_openedSound;
        [SerializeField] private AudioClipResolver m_canNotOpenSound;

        private Item m_requiredKey = null;

        private bool m_hasOpeningAnimation = false;
        private bool m_hasRevealAnimation = false;
        public bool opened => m_opened;
        private bool m_opened = false;

        protected void Awake()
        {
            Debug.Assert(m_chestAnimator, ErrorMessages.InspectorMissingComponentReference<Animator>());

            if (m_chestAnimator)
            {
                m_hasOpeningAnimation = AnimationUtils.HasParameter(m_chestAnimator, m_openedAnimationParameter);
            }
        }

        protected override void Start()
        {
            base.Start();

            if (m_randomGenerateKey)
            {
                // ��������Ƿ���ҪԿ��
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

        private LootTable.LootEntryData GetRandomLootEntry()
        {
            float totalWeight = 0f;

            foreach (var entry in lootTable.entries)
            {
                totalWeight += entry.weight;
            }

            float randomValue = Random.Range(0f, totalWeight);

            foreach (var entry in lootTable.entries)
            {
                if (randomValue < entry.weight)
                {
                    return entry;
                }

                randomValue -= entry.weight;
            }

            return null;
        }

        public bool TryOpen()
        {
            if (m_isMonsterChest)
            {
                return MonsterChestOpen();
            }
            else
            {
                return ContentsChestOpen();
            }
        }
        
        private bool MonsterChestOpen()
        {
            if (!m_opened)
            {
                TryPlayOpeningAnimation(true);

                GameManager.Player.Damage(new DamageOutputDescriptor
                {
                    source = EDamageSource.Unknown,
                    attacker = this,
                    damage = m_damageAmount,
                    damageType = m_damageType,
                    distanceType = m_distanceType,
                    flags = EDamageFlag.None
                });

                this.gameObject.layer = LayerMask.NameToLayer("Collision D");

                if (m_requiredKey)
                {
                    GameManager.InventorySystem.RemoveFromBag(m_requiredKey);
                }

                m_opened = true;

                return true;
            }

            return false;
        }

        private bool ContentsChestOpen()
        {
            LootEntryData randomEntry = null;
            if (lootTable.entries != null && lootTable.entries.Length > 0)
            {
                // ʹ�û���Ȩ�ص����ѡ�����
                randomEntry = GetRandomLootEntry();
            }

            // �յ��ǿձ��� ���ܶ�ȡ �ᱨ��
            if (randomEntry != null && GameManager.InventorySystem.IsBackpackFull(randomEntry.item))
            {
                GameManager.DialogueSystem.Main.PlayNow((LocalizationSettings.StringDatabase.GetLocalizedString
                ("NPCDialogueTable", "id_dialogue_shop_backpack_full")));

                return false;
            }
            else
            {
                if (!m_opened)
                {
                    TryPlayOpeningAnimation(true);

                    if (m_isEmptyChest == false)
                    {
                        if (randomEntry != null)
                        {
                            int randomQuantity = Random.Range(1, randomEntry.maxQuantity + 1);
                            GameManager.InventorySystem.AddToBag(randomEntry.item, randomQuantity);
                            GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_openedSound);
                        }
                        if (lootTable.money > 0)
                        {
                            int randomMoney = Random.Range(1, lootTable.money + 1);
                            GameManager.InventorySystem.AddMoney(randomMoney);

                            GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_openedSound);
                        }
                    }

                    this.gameObject.layer = LayerMask.NameToLayer("Collision D");

                    //if (m_emptySpriteLibraryAsset)
                    //{
                    //    m_nowSpriteLibrary.spriteLibraryAsset = m_emptySpriteLibraryAsset;
                    //}

                    if (m_requiredKey)
                    {
                        GameManager.InventorySystem.RemoveFromBag(m_requiredKey);
                    }

                    m_opened = true;

                    return true;
                }

                return false;
            }
        }
    }
}
