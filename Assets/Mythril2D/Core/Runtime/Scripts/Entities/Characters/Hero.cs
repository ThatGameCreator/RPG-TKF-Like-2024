using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Events;
using System;
using FunkyCode;

namespace Gyvr.Mythril2D
{
    public class Hero : Character<HeroSheet>
    {
        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_levelUpSound;

        [Header("Hero")]
        [SerializeField] private bool m_restoreHealthOnLevelUp = true;
        [SerializeField] private bool m_restoreManaOnLevelUp = true;

        [Header("Particle")]
        [SerializeField] private ParticleSystem m_dashParticleSystem = null;
        [SerializeField] private ParticleSystem m_runParticleSystem = null;
        public ParticleSystem dashParticleSystem => m_dashParticleSystem;
        public ParticleSystem runParticleSystem => m_runParticleSystem;

        [Header("Stamina Paramenters")]
        // Ã¿ÃEûºÄ¶àÉÙÄÍÁ¦
        [SerializeField] private float staminaMultiplier = 7;
        // ÄÍÁ¦»Ø¸´Ç°£¬µÈ´ıÊ±¼E
        [SerializeField] private float timeBeforeStaminaRengeStarts = 1.25f;
        // Ã¿´Îµ¥Î»ÄÍÁ¦»Ø¸´Á¿
        [SerializeField] private float staminaValueIncrement = 2;
        // µ¥Î»ÄÍÁ¦»Ø¸´¼ä¸ôÊ±¼E
        [SerializeField] private float staminaTimeIncrement = 0.05f;
        // µ±Ç°ÄÍÁ¦
        public float currentStamina => m_currentStats.Stamina;
        public float maxStamina => m_maxStats.Stamina;

        public bool isNowCanRun = false;
        public bool isExecutingAction = false;
        public bool isRunning = false;
        private bool useStamina = true;
        private Coroutine regeneratingStamina;
        public UnityEvent<float> currentStaminaChanged => m_currentStats.staminaChanged;
        public UnityEvent<float> maxStaminaChanged => m_maxStats.staminaChanged;

        [Header("Loot")]
        [SerializeField] private AudioClipResolver m_lootingSound;
        [SerializeField] private AudioClipResolver m_lootedSound;

        public bool isLooting => m_isLooting;
        private bool m_isLooting = false;
        private Entity m_lootingObject = null;
        public float lootingTime => m_lootingTime;
        public float lootingRequiredtTime => m_lootingRequiredtTime;
        private float m_lootingTime = 0f;
        private float m_lootingRequiredtTime = 2f;

        [Header("Evacuation")]
        [SerializeField] private AudioClipResolver m_evacuatingSound;
        [SerializeField] private AudioClipResolver m_evacuatedSound;

        private float m_evacuatingTime = 0f;
        private bool m_isEvacuating = false;
        private float m_evacuatingRequiredtTime = 3f;
        public float evacuatingTime => m_evacuatingTime;
        public bool isEvacuating => m_isEvacuating;
        public float evacuatingRequiredtTime => m_evacuatingRequiredtTime;

        [Header("Light")]
        [SerializeField] private Light2D m_heroSightLight = null;
        [SerializeField] private Light2D m_heroAbilityLight = null;
        [SerializeField] private float m_abilityLightDurationTime = 5f;
        [SerializeField] private SpriteRenderer abilityLightSprite = null;

        public Light2D heroSightLight => m_heroSightLight;
        public Light2D heroAbilityLight => m_heroAbilityLight;
        private bool isUseAbilityLighting = false;
        private float m_nowAbilityLightTime = 0f;

        public int experience => m_experience;
        public int totalNextLevelExperience => GetTotalExpRequirement(m_level + 1);
        public int nextLevelExperience => GetTotalExpRequirement(m_level + 1) - experience;
        public int availablePoints => m_sheet.pointsPerLevel * (m_level - Stats.MinLevel) - m_usedPoints;
        public SerializableDictionary<EEquipmentType, Equipment> equipments => m_equipments;
        public AbilitySheet[] equippedAbilities => m_equippedAbilities;
        public DashAbilitySheet dashAbility => m_dashAbility;
        public HashSet<AbilitySheet> bonusAbilities => m_bonusAbilities;
        public UnityEvent<AbilitySheet[]> equippedAbilitiesChanged => m_equippedAbilitiesChanged;
        public UnityEvent<int> experienceChanged => m_experienceChanged;
        public Stats customStats => m_customStats;
        public Stats missingCurrentStats => m_missingCurrentStats;
        public float missingCurrentStamina => m_missingCurrentStamina;
        public bool firstAwake => m_firstAwake;
        public int usedPoints => m_usedPoints;

        public const int MaxEquipedAbilityCount = 4;

        private Stats m_customStats = new Stats();
        private Stats m_missingCurrentStats = new Stats();
        private float m_missingCurrentStamina = 0;
        private bool m_firstAwake = true;
        private int m_usedPoints = 0;
        private int m_experience = 0;
        private SerializableDictionary<EEquipmentType, Equipment> m_equipments = new SerializableDictionary<EEquipmentType, Equipment>();
        private HashSet<AbilitySheet> m_bonusAbilities = new HashSet<AbilitySheet>();

        private AbilitySheet[] m_equippedAbilities = new AbilitySheet[MaxEquipedAbilityCount];

        // ÕâÀEÃµÄ»°£¬µÃÔÚÈËÎEheetÏÂÃæµ¼ÈEÔÓ¦µÄ abilitysheet
        //ScriptableObject.ctor is not allowed to be called from a MonoBehaviour constructor (or instance field initializer), call it in Awake or Start instead. 
        //private DashAbilitySheet m_dashAbility = ScriptableObject.CreateInstance<DashAbilitySheet>();
        private DashAbilitySheet m_dashAbility = null;
        public bool isDashFinishing = false;

        private UnityEvent<AbilitySheet[]> m_equippedAbilitiesChanged = new UnityEvent<AbilitySheet[]>();
        private UnityEvent<int> m_experienceChanged = new UnityEvent<int>();

        public bool isStartGameRevival = false;

        // Ã»¼Ì³Ğ¸¸Àà»á³öÊÂ ÎŞ·¨»ñµÃ¶ÔÏE
        // private new void Awake(){}
        protected override void Awake()
        {
            isPlayer = true;

            m_maxStats.staminaChanged.AddListener(OnMaxStaminaChanged);

            base.Awake();
            //m_currentStats.changed.AddListener(HandleStamina);
        }

        protected override void Start()
        {
            // Õâ¸ö¸Ğ¾õÓ¦¸Ã²»ĞèÒªÖ´ĞĞÒ»´Î¼àÌı tryexcute


            // ´«ËÍºóËäÈ»Ö´ĞĞÁË²¥·Å¶¯»­º¯Êı£¬µ«È´²¢Ã»ÓĞÖ´ĞĞUpdate£¿
            // Õâ·ÅAwakeÖ´ĞĞ²»ÁË
            if (isStartGameRevival == false)
            {
                // Ò²Ğúï¦¸Ã¸Ä³ÉÖ»ÄÜ »¥¶¯£¿
                DisableActions(EActionFlags.All);

                TryPlayRevivalAnimation();

                isStartGameRevival = true;
            }

            // ¸Ğ¾õÈç¹ûÁ£×ÓĞ§¹ûÉèÖÃÎª isloop Õâ¸öÆô¶¯ÓÎÏ·Ê±ºò¾Í»áÆôÓÃ ËùÒÔÊÔ×ÅÔÚ Awake ÖĞ¹Ø±Õ
            // ºÃÏEAwake µÄÊ±ºò»¹Ã»ĞÂ½¨ºÃÈËÎE¨´úİË£¬ ÊÔÊÔÔÚ Start ÖĞ
            GameManager.Player.runParticleSystem.Stop();

            UpdateMaxStats();

            m_currentStats.changed.AddListener(OnCurrentStatsChanged);
        }

        private void Update()
        {
            if (isUseAbilityLighting == true) HandleAbilityLighting();

            if (useStamina == true) HandleStamina();

            if (m_isLooting == true) OnTryLooting();

            if (m_isEvacuating == true) OnTryEvacuating();
        }


        protected override void OnDeathAnimationEnd()
        {
            base.OnDeathAnimationEnd();
        }

        private void OnDeadAnimationStart()
        {
        }

        private void OnRevivalAnimationEnd()
        {
            //Debug.Log("OnRevivalAnimationEnd");

            EnableActions(EActionFlags.All);
        }

        public void OnDashAnimationEnd()
        {
            if (!dead)
            {
                isDashFinishing = false;
            }
        }

        private void OnDeadAnimationEnd()
        {
            StartCoroutine(SaveWithDelay());
        }

        public void RecoverPlayerStats(bool fullRecover = false)
        {
            // ³¯Ïòóô»E
            SetLookAtDirection(Vector2.right);

            // »Ö¸´ÑªÁ¿ 
            if (fullRecover)
            {
                m_currentStats[EStat.Health] = m_maxStats[EStat.Health];
                m_currentStats[EStat.Mana] = m_maxStats[EStat.Mana];
            }
            else
            {
                
                m_currentStats[EStat.Health] = 1;
                m_currentStats[EStat.Mana] = 1;
            }
            
            RecoverStamina((int)GameManager.Player.maxStamina);

            // »Ö¸´Åö×²ÌE
            Collider2D[] colliders = GameManager.Player.GetComponentsInChildren<Collider2D>();
            Array.ForEach(colliders, (collider) => collider.enabled = true);
        }

        private IEnumerator SaveWithDelay()
        {
            GameManager.SaveSystem.SaveToFile(GameManager.SaveSystem.saveFileName); // ±£´æÊı¾İ

            yield return new WaitForSeconds(1f); // µÈ´ıÒ»ÃE
        }

        public int GetTotalExpRequirement(int level)
        {
            int total = 0;

            for (int i = 1; i < level; i++)
            {
                total += m_sheet.experience[i];
            }
            
            return total;
        }

        public void AddExperience(int experience, bool silentMode = false)
        {
            Debug.Assert(experience > 0, "Cannot add a negative amount of experience.");

            GameManager.NotificationSystem.experienceGained.Invoke(experience);

            m_experience += experience;

            m_experienceChanged.Invoke(m_experience);

            while (m_experience >= GetTotalExpRequirement(m_level + 1))
            {
                OnLevelUp(silentMode);
            }
        }

        public void AddCustomStats(Stats customStats)
        {
            m_customStats += customStats;
            UpdateMaxStats();
        }

        public void LogUsedPoints(int points)
        {
            m_usedPoints += points;
        }


        public void OnEnableAbilityLighting()
        {
            //Debug.Log("OnEnableAbilityLighting");

            // »º´æ²ÄÖÊµÄÑÕÉ«
            //Color materialColor = abilityLightSprite.material.color;
            //abilityLightSprite.material.color = new Color(materialColor.a, materialColor.b, materialColor.g, 0.3f);

            isUseAbilityLighting = true;
            m_heroSightLight.transform.gameObject.SetActive(false);
            m_heroAbilityLight.transform.gameObject.SetActive(true);
        }


        private void CancelAbilityLighting()
        {
            m_heroSightLight.transform.gameObject.SetActive(true);
            m_heroAbilityLight.transform.gameObject.SetActive(false);
            isUseAbilityLighting = false;
            m_nowAbilityLightTime = 0f;
        }

        private void HandleAbilityLighting()
        {
            if (!dead)
            {
                m_nowAbilityLightTime += Time.deltaTime;

                if(m_nowAbilityLightTime > m_abilityLightDurationTime)
                {
                    CancelAbilityLighting();
                }
            }
        }

        public bool CheckIsPlayerMoving()
        {
            return movementDirection == Vector2.zero;
        }

        private void OnTryLooting()
        {

            if (CheckIsPlayerMoving())
            {
                m_lootingTime += Time.deltaTime;
            }
            else
            {
                CancelLooting();
            }

            if (m_lootingTime > m_lootingRequiredtTime)
            {
                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_lootedSound);

                //Debug.Log("playerEndInteracte.Invoke");

                //m_lootingObject.SendMessageUpwards("OnInteract", GameManager.Player);
                GameManager.NotificationSystem.playerEndInteracte.Invoke(GameManager.Player, m_lootingObject);

                CancelLooting();
            }
        }

        private void OnTryEvacuating()
        {
            if (GameManager.Player.CheckIsPlayerMoving())
            {
                m_evacuatingTime += Time.deltaTime;
            }
            else
            {
                CancelEvacuate();
            }

            if (m_evacuatingTime > m_evacuatingRequiredtTime)
            {
                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_evacuatedSound);

                GameManager.TeleportLoadingSystem.RequestTransition("Pilgrimage_Place", null, null, () => {
                    GameManager.SaveSystem.SaveToFile(GameManager.SaveSystem.saveFileName);
                }, ETeleportType.Normal, "Player_Spawner");

                GameManager.DayNightSystem.OnDisableDayNightSystem();

                CancelAbilityLighting();

                CancelEvacuate();
            }
        }

        // ¸Ğ¾õÔÚÕâ¸ö¿ò¼ÜÏÂ£¬ÒªÃ´×ö³É¼¼ÄÜ£¬·Åµ½Íæ¼ÒÏÂ£¬ÒªÃ´µÃÓÃ¸ö¼àÌıÈ¥¼àÌıÆäËûĞĞÎªÀ´È¡ÏE
        // ²»È»Ò»¸öÍØÕ¹ĞÔ²»»á£¬ÔÙÕß²»Í£ÔÚUpdateÖĞ¼EâÒ²²»ºÃ
        private void CancelEvacuate()
        {
            //TerminateCasting();

            m_evacuatingTime = 0f;
            m_isEvacuating = false;
            GameManager.NotificationSystem.audioStopPlaybackRequested.Invoke(m_evacuatingSound);
        }

        public void OnStarEvacuate()
        {
            GameManager.Player.SetMovementDirection(Vector2.zero);
            m_isEvacuating = true;
            GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_evacuatingSound);
        }

        public void CancelLooting()
        {
            //TerminateCasting();
            //Debug.Log("CancelLooting");

            m_lootingTime = 0f;
            m_isLooting = false;
            m_lootingObject = null;
            GameManager.NotificationSystem.audioStopPlaybackRequested.Invoke(m_lootingSound);
        }

        public void OnStartLooting(Entity lootingObject, float targetLootTime)
        {
            m_lootingRequiredtTime = targetLootTime;

            SetMovementDirection(Vector2.zero);

            m_isLooting = true;

            m_lootingObject = lootingObject;

            GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_lootingSound);
        }

        public void OnTryStartLoot(Entity interactionTargett, float targetLootTime)
        {
            if (GameManager.Player.isLooting == true)
            {
                //Debug.Log("CancelLooting");

                CancelLooting();
            }
            else
            {
                //Debug.Log("OnStartLooting");

                OnStartLooting(interactionTargett, targetLootTime);
            }
        }

        private void OnMaxStaminaChanged(float previous)
        {
            float difference = m_maxStats.stamina - previous;
            float newCurrentStamina = m_currentStats.stamina + difference;

            // Make sure we don't kill the character when updating its maximum stats
            newCurrentStamina = math.max(newCurrentStamina, 1);
            m_currentStats.Set(newCurrentStamina);
        }

        public float GetMaxStamina()
        {
            return maxStamina;
        }

        public float GetStamina()
        {
            return m_currentStats.Stamina;
        }

        public void RecoverStamina(int value)
        {
            float missingStamina = maxStamina - m_currentStats.Stamina;
            //m_sheet.SetStamina(GetStamina() + math.min(value, missingStamina));
            m_currentStats.Stamina += math.min(value, missingStamina);
            GameManager.NotificationSystem.staminaRecovered.Invoke(this, value);
        }

        public void ConsumeStamina(int value)
        {
            //m_sheet.SetStamina(m_currentStats.Stamina - math.min(value, m_currentStats.Stamina));
            m_currentStats.Stamina -= math.min(value, m_currentStats.Stamina);
            GameManager.NotificationSystem.staminaConsumed.Invoke(this, value);

        }

        private void HandleStamina()
        {
            // ¸Ğ¾õÕâµ½´¦ÓÃ²¼¶ûÖµÅĞ¶Ï£¬È·Êµ²»ÈçÖ±½Ó°ÑÕâ¸öb·½·¨·Åµ½¼¼ÄÜÀEE
            // µ«ÏEËÏE¬Õâ¸öÒ²Ã»ÓĞ¶¯»­£¬Èç¹ûÒª×öµ½¼¼ÄÜÀEæËÆºõÒ²µÃÍØÕ¹Ò»µãÆäËû¶«ÎE
            if (isDashFinishing == false && isExecutingAction == false)
            {
                // ³å´Ì×´Ì¬£¬ÇÒÓĞÒÆ¶¯ÊäÈE¬´¦ÀúàÍÁ¦
                if (isRunning == true && movementDirection != Vector2.zero)
                {
                    // Èç¹ûÄÍÁ¦»Ø¸´Ğ­³Ì¿ªÆô£¬ÖĞ¶Ï
                    if (regeneratingStamina != null)
                    {
                        StopCoroutine(regeneratingStamina);
                        regeneratingStamina = null;
                    }

                    // Èç¹ûÆäËû¼¼ÄÜÒ²ÓÃÍ¬Ò»¸öbool isExecutingActionÀ´¿ªÆEÔò»áµ¼ÖÂ²»ÊÇÅÜ²½Ò²»á¿Û¾«Á¦Öµ
                    m_currentStats.Stamina -= staminaMultiplier * Time.deltaTime;

                    if (m_currentStats.Stamina < 0) m_currentStats.Stamina = 0;

                    // ÄÍÁ¦Öµ¹éÁã£¬½ûÖ¹Ê¹ÓÃ³å´Ì
                    if (m_currentStats.Stamina <= 0)
                    {
                        isNowCanRun = false;
                        EndPlayRunAnimation();
                    }
                }

                // ºÃÏñµ±Ê±ÍEÇĞŞÒ»¸öbug£¬ÏÖÔÚÖ»ÄÜ¼EErun µÄÊ±ºòµÄ×´Ì¬ ÆäËû±ÈÈç¹¥»÷ºÍ³å´ÌµÈ²¢²»ÄÜ¼Eâ£¬µ¼ÖÂ¿ÉÒÔÒ»±ß×ö¶¯×÷Ò»±ß»Ö¸´ÄÍÁ¦
                // ÄÍÁ¦Öµ²»Âú£¬ÇÒÃ»ÓĞ³å´Ì£¬ÇÒÄÍÁ¦»Ø¸´Î´¿ªÆE
                if (m_currentStats.Stamina < maxStamina && isRunning == false && regeneratingStamina == null)
                {
                    //Debug.Log("RegenerateStamina");
                    //Debug.Log(isExecutingAction);
                    regeneratingStamina = StartCoroutine(RegenerateStamina());
                }
            }
        }

        private IEnumerator RegenerateStamina()
        {
            yield return new WaitForSeconds(timeBeforeStaminaRengeStarts);

            WaitForSeconds timeToWait = new WaitForSeconds(staminaTimeIncrement);

            while (isDashFinishing == false && isExecutingAction == false && m_currentStats.Stamina < maxStamina)
            {
                // ´óÓÚ0£¬¿ÉÒÔÊ¹ÓÃ³å´Ì
                if (m_currentStats.Stamina > 0f) isNowCanRun = true;

                RecoverStamina((int)(staminaValueIncrement));

                if (m_currentStats.Stamina > maxStamina)
                    m_currentStats.Stamina = maxStamina;

                yield return timeToWait;
            }
            // ÄÍÁ¦»Ø¸´ÍEÏ£¬ÒıÓÃÖÃ¿Õ
            regeneratingStamina = null;
        }

        public void SetPlayerHealthToZero()
        {
            m_currentStats[EStat.Health] = 0;
        }

        public Equipment GetEquipment(EEquipmentType equipmentType)
        {
            return m_equipments[equipmentType];
        }

        public Equipment Equip(Equipment equipment)
        {
            // Ğ¶ÏÂµ±Ç°ÀàĞÍµÄ×°±¸
            Equipment previousEquipment = Unequip(equipment.type);

            // ´©´÷ĞÂ×°±¸
            m_equipments[equipment.type] = equipment;

            // Èç¹û×°±¸ÓĞ¼¼ÄÜÊôĞÔ£¬½«ÆäÒÀ´Î·ÖÅäµ½¿ÕÏĞµÄ¼¼ÄÜ²Û
            if (equipment.ability != null)
            {
                AssignAbilitiesToSlots(equipment.ability, 2);
            }

            GameManager.NotificationSystem.itemEquipped.Invoke(equipment);
            EquipUpdateMaxStats();
            return previousEquipment;
        }

        public void DeathUnequipAll()
        {
            // ºÃÏñÕâ¸ö×Ô¼ºĞ´µÄ×Öµä²»ÄÜÕâÃ´±éÀE
            //foreach(EEquipmentType equipmentType in m_equipments.Keys)

            List<EEquipmentType> keysToRemove = new List<EEquipmentType>(m_equipments.Keys);

            foreach (EEquipmentType equipmentType in keysToRemove)
            {
                m_equipments.TryGetValue(equipmentType, out Equipment toUnequip);

                if (toUnequip)
                {
                    if (toUnequip.ability != null)
                    {
                        RemoveAbilitiesFromSlots(toUnequip.ability, 2);
                    }

                    // ¼EéÈİÁ¿²¢µ÷ÕE
                    if (toUnequip.capacity != 0)
                    {
                        GameManager.InventorySystem.DecreaseBackpackCapacity(toUnequip.capacity);
                        GameManager.UIManagerSystem.UIMenu.inventory.Init(); // ¸EÂ UI
                    }

                    m_equipments.Remove(equipmentType);
                    GameManager.NotificationSystem.itemUnequipped.Invoke(toUnequip);
                    UpdateMaxStats();
                }
            }
        }

        public Equipment Unequip(EEquipmentType type)
        {
            m_equipments.TryGetValue(type, out Equipment toUnequip);

            if (toUnequip)
            {
                // Èç¹û×°±¸ÓĞ¼¼ÄÜÊôĞÔ£¬½«Æä´Ó¼¼ÄÜ²ÛÖĞÒÆ³ı
                if (toUnequip.ability != null)
                {
                    RemoveAbilitiesFromSlots(toUnequip.ability, 2);
                }

                m_equipments.Remove(type);
                GameManager.NotificationSystem.itemUnequipped.Invoke(toUnequip);
                EquipUpdateMaxStats();
            }

            return toUnequip;
        }

        // Îª¼¼ÄÜ·ÖÅäµ½´Ó startIndex ¿ªÊ¼µÄ²Û
        private void AssignAbilitiesToSlots(AbilitySheet[] abilities, int startIndex)
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                int slotIndex = startIndex + i;
                if (slotIndex < m_equippedAbilities.Length)
                {
                    m_equippedAbilities[slotIndex] = abilities[i];
                }
            }
            m_equippedAbilitiesChanged.Invoke(m_equippedAbilities);
        }

        // ´Ó¼¼ÄÜ²ÛÖĞÒÆ³ı´Ó startIndex ¿ªÊ¼µÄ¼¼ÄÜ
        private void RemoveAbilitiesFromSlots(AbilitySheet[] abilities, int startIndex)
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                int slotIndex = startIndex + i;
                if (slotIndex < m_equippedAbilities.Length)
                {
                    m_equippedAbilities[slotIndex] = null;
                }
            }
            m_equippedAbilitiesChanged.Invoke(m_equippedAbilities);
        }


        public void AddBonusAbility(AbilitySheet abilitySheet)
        {
            if (!m_bonusAbilities.Contains(abilitySheet))
            {
                m_bonusAbilities.Add(abilitySheet);
                AddAbility(abilitySheet);
            }
        }

        public void RemoveBonusAbility(AbilitySheet abilitySheet)
        {
            if (!m_bonusAbilities.Contains(abilitySheet))
            {
                Debug.LogAssertion("Cannot remove an ability that hasn't been added in the first place.");
            }
            else
            {
                m_bonusAbilities.Remove(abilitySheet);
                RemoveAbility(abilitySheet);
            }
        }

        private bool AddDashAbility(DashAbilitySheet abilitySheet)
        {
            if (base.AddAbility(abilitySheet))
            {
                if (!IsAbilityEquiped(abilitySheet))
                {
                    for (int i = 0; i < m_equippedAbilities.Length; ++i)
                    {
                        m_dashAbility = abilitySheet;
                    }
                }

                GameManager.NotificationSystem.abilityAdded.Invoke(abilitySheet);
                return true;
            }

            return false;
        }

        protected override bool AddAbility(AbilitySheet abilitySheet)
        {
            if (base.AddAbility(abilitySheet))
            {
                if (!IsAbilityEquiped(abilitySheet))
                {
                    for (int i = 0; i < m_equippedAbilities.Length; ++i)
                    {
                        if (m_equippedAbilities[i] == null)
                        {
                            Equip(abilitySheet, i);
                        }
                    }
                }

                GameManager.NotificationSystem.abilityAdded.Invoke(abilitySheet);
                return true;
            }

            return false;
        }

        protected override bool RemoveAbility(AbilitySheet abilitySheet)
        {
            if (base.RemoveAbility(abilitySheet))
            {
                for (int i = 0; i < m_equippedAbilities.Length; ++i)
                {
                    if (m_equippedAbilities[i] == abilitySheet)
                    {
                        Unequip(i);
                    }
                }

                GameManager.NotificationSystem.abilityRemoved.Invoke(abilitySheet);
                return true;
            }

            return false;
        }

        public void Equip(AbilitySheet ability, int index)
        {
            m_equippedAbilities[index] = ability;
            m_equippedAbilitiesChanged.Invoke(m_equippedAbilities);
        }

        public void Unequip(int index)
        {
            m_equippedAbilities[index] = null;
            m_equippedAbilitiesChanged.Invoke(m_equippedAbilities);
        }

        public bool IsAbilityEquiped(AbilitySheet abilitySheet)
        {
            foreach (AbilitySheet ability in m_equippedAbilities)
            {
                if (ability == abilitySheet) return true;
            }

            return false;
        }

        private Stats CalculateEquipmentStats()
        {
            Stats equipmentStats = new Stats();

            foreach (Equipment piece in m_equipments.Values)
            {
                if (piece)
                {
                    equipmentStats += piece.bonusStats;
                }
            }

            return equipmentStats;
        }

        private int CalculateEquipmentStamina()
        {
            int equipmentStamina = 0;

            foreach (Equipment piece in m_equipments.Values)
            {
                if (piece)
                {
                    equipmentStamina += piece.stamina;
                }
            }

            return equipmentStamina;
        }

        private void EquipUpdateMaxStats()
        {
            Stats equipmentStats = CalculateEquipmentStats();
            Stats newMaxStats = m_sheet.baseStats + m_customStats + equipmentStats;

            int equipmentStamina = CalculateEquipmentStamina();
            int newMaxStamina = m_sheet.maxStamina + equipmentStamina;

            newMaxStats.isEquip = true;

            m_maxStats.Set(newMaxStats);
            m_maxStats.Set(newMaxStamina);

            ApplyMissingCurrentStats();
        }

        private void UpdateMaxStats()
        {
            Stats equipmentStats = CalculateEquipmentStats();
            Stats newMaxStats = m_sheet.baseStats + m_customStats + equipmentStats;

            //Debug.Log(currentStamina);

            int equipmentStamina = CalculateEquipmentStamina();
            int newMaxStamina = m_sheet.maxStamina + equipmentStamina;

            m_maxStats.Set(newMaxStats);
            m_maxStats.Set(newMaxStamina);

            ApplyMissingCurrentStats();
        }

        private void UpdateCurrentStats()
        {
            m_currentStats.Set(m_maxStats.stats);
            m_currentStats.Set(maxStamina);
        }

        private void ApplyMissingCurrentStats()
        {
            m_currentStats.Set(m_currentStats.stamina - m_missingCurrentStamina);
            m_currentStats.Set(m_currentStats.stats - m_missingCurrentStats);

            m_missingCurrentStats.Reset();
            m_missingCurrentStamina = 0f;
        }

        private void OnLevelUp(bool silentMode = false)
        {
            ++m_level;

            if (!silentMode)
            {
                if (m_restoreHealthOnLevelUp)
                {
                    Heal(m_maxStats[EStat.Health] - m_currentStats[EStat.Health]);
                }

                if (m_restoreManaOnLevelUp)
                {
                    RecoverMana(m_maxStats[EStat.Mana] - m_currentStats[EStat.Mana]);
                }

                GameManager.NotificationSystem.levelUp.Invoke(m_level);
                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_levelUpSound);
            }

            foreach (AbilitySheet ability in m_sheet.GetAbilitiesUnlockedAtLevel(m_level))
            {
                AddAbility(ability);
            }
        }

        public void Initialize(PlayerDataBlock block)
        {
            m_firstAwake = block.firstAwake;

            m_usedPoints = block.usedPoints;

            if (block.experience > 0)
            {
                AddExperience(block.experience, true);
            }

            AddDashAbility(m_sheet.GetDashAbility());

            foreach (var ability in block.bonusAbilities)
            {
                AddBonusAbility(GameManager.Database.LoadFromReference(ability));
            }

            m_equipments = new SerializableDictionary<EEquipmentType, Equipment>();

            foreach (var piece in block.equipments)
            {
                Equipment instance = GameManager.Database.LoadFromReference(piece);
                m_equipments[instance.type] = instance;
            }

            m_customStats = block.customStats;

            // Copy missing current stats so block data doesn't get altered
            m_missingCurrentStats = new Stats(block.missingCurrentStats);
            m_missingCurrentStamina = block.missingCurrentStamina;

            // Clear equipped abilities
            for (int i = 0; i < m_equippedAbilities.Length; ++i)
            {
                m_equippedAbilities[i] = i < block.equippedAbilities.Length ? GameManager.Database.LoadFromReference(block.equippedAbilities[i]) : null;
            }

            m_equippedAbilitiesChanged.Invoke(m_equippedAbilities);

            transform.position = block.position;

            //Debug.Log("Initialize");
        }

        protected override void OnDeath()
        {
            //Debug.Log("Hero Death");

            // Prevents the Hero GameObject from being destroyed, so it can be used in the death screen.
            m_destroyOnDeath = false; 
            base.OnDeath();

            CancelAbilityLighting();

            GameManager.InventorySystem.EmptyBag();

            DeathUnequipAll();

            GameManager.DayNightSystem.OnDisableDayNightSystem();

            GameManager.NotificationSystem.deathScreenRequested.Invoke();
        }
    }
}
