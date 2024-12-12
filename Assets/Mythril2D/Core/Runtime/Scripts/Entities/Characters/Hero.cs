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
        // 每秒消耗多少耐力
        [SerializeField] private float staminaMultiplier = 7;
        // 耐力回复前，等待时间
        [SerializeField] private float timeBeforeStaminaRengeStarts = 1.25f;
        // 每次单位耐力回复量
        [SerializeField] private float staminaValueIncrement = 2;
        // 单位耐力回复间隔时间
        [SerializeField] private float staminaTimeIncrement = 0.05f;
        // 当前耐力
        public float currentStamina => m_currentStats.Stamina;
        public float maxStamina => m_sheet.GetMaxStamina();

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
        public int usedPoints => m_usedPoints;

        public const int MaxEquipedAbilityCount = 4;

        private Stats m_customStats = new Stats();
        private Stats m_missingCurrentStats = new Stats();
        private float m_missingCurrentStamina = 0;
        private int m_usedPoints = 0;
        private int m_experience = 0;
        private SerializableDictionary<EEquipmentType, Equipment> m_equipments = new SerializableDictionary<EEquipmentType, Equipment>();
        private HashSet<AbilitySheet> m_bonusAbilities = new HashSet<AbilitySheet>();

        private AbilitySheet[] m_equippedAbilities = new AbilitySheet[MaxEquipedAbilityCount];

        // 这里用的话，得在人物sheet下面导入对应的 abilitysheet
        //ScriptableObject.ctor is not allowed to be called from a MonoBehaviour constructor (or instance field initializer), call it in Awake or Start instead. 
        //private DashAbilitySheet m_dashAbility = ScriptableObject.CreateInstance<DashAbilitySheet>();
        private DashAbilitySheet m_dashAbility = null;
        public bool isDashFinishing = false;

        private UnityEvent<AbilitySheet[]> m_equippedAbilitiesChanged = new UnityEvent<AbilitySheet[]>();
        private UnityEvent<int> m_experienceChanged = new UnityEvent<int>();

        public bool isStartGameRevival = false;


        protected override void OnDeathAnimationEnd()
        {
            base.OnDeathAnimationEnd();
        }

        private void OnDeadAnimationStart()
         {
        }

        public void RecoverPlayerStats()
        {
            // 朝向篝火
            SetLookAtDirection(Vector2.right);

            // 恢复血量 
            m_currentStats[EStat.Health] = 1;
            m_currentStats[EStat.Mana] = 1;
            RecoverStamina((int)GameManager.Player.maxStamina);

            // 恢复碰撞体
            Collider2D[] colliders = GameManager.Player.GetComponentsInChildren<Collider2D>();
            Array.ForEach(colliders, (collider) => collider.enabled = true);
        }

        private void OnDeadAnimationEnd()
        {
            StartCoroutine(SaveWithDelay());
        }

        private IEnumerator SaveWithDelay()
        {
            GameManager.SaveSystem.SaveToFile(GameManager.SaveSystem.saveFileName); // 保存数据

            yield return new WaitForSeconds(1f); // 等待一秒
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
            UpdateStats();
        }

        public void LogUsedPoints(int points)
        {
            m_usedPoints += points;
        }

        // 没继承父类会出事 无法获得对象
        // private new void Awake(){}
        private new void Awake()
        {

            isPlayer = true;

            m_maxStats.staminaChanged.AddListener(OnStaminaChanged);

            base.Awake();
            //m_currentStats.changed.AddListener(HandleStamina);
        }

        protected override void Start()
        {
            // 这个感觉应该不需要执行一次监听 tryexcute


            // 传送后虽然执行了播放动画函数，但却并没有执行Update？
            // 这放Awake执行不了
            if (isStartGameRevival == false)
            {
                // 也许应该改成只能 互动？
                DisableActions(EActionFlags.All);

                TryPlayRevivalAnimation();

                isStartGameRevival = true;
            }

            // 感觉如果粒子效果设置为 isloop 这个启动游戏时候就会启用 所以试着在 Awake 中关闭
            // 好像 Awake 的时候还没新建好人物报错了， 试试在 Start 中
            GameManager.Player.runParticleSystem.Stop();

            UpdateStats();
        }

        private void Update()
        {
            if (isUseAbilityLighting == true) HandleAbilityLighting();

            if (useStamina == true) HandleStamina();

            if (m_isLooting == true) OnTryLooting();

            if (m_isEvacuating == true) OnTryEvacuating();
        }


        public void OnEnableAbilityLighting()
        {
            //Debug.Log("OnEnableAbilityLighting");

            // 缓存材质的颜色
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

        // 感觉在这个框架下，要么做成技能，放到玩家下，要么得用个监听去监听其他行为来取消/
        // 不然一个拓展性不会，再者不停在Update中检测也不好
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

        private void OnStaminaChanged(float previous)
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
            // 感觉这到处用布尔值判断，确实不如直接把这个b方法放到技能里面
            // 但想了想，这个也没有动画，如果要做到技能里面似乎也得拓展一点其他东西
            if (isDashFinishing == false && isExecutingAction == false)
            {
                // 冲刺状态，且有移动输入，处理耐力
                if (isRunning == true && movementDirection != Vector2.zero)
                {
                    // 如果耐力回复协程开启，中断
                    if (regeneratingStamina != null)
                    {
                        StopCoroutine(regeneratingStamina);
                        regeneratingStamina = null;
                    }

                    // 如果其他技能也用同一个bool isExecutingAction来开启 则会导致不是跑步也会扣精力值
                    m_currentStats.Stamina -= staminaMultiplier * Time.deltaTime;

                    if (m_currentStats.Stamina < 0) m_currentStats.Stamina = 0;

                    // 耐力值归零，禁止使用冲刺
                    if (m_currentStats.Stamina <= 0)
                    {
                        isNowCanRun = false;
                        EndPlayRunAnimation();
                    }
                }

                // 好像当时忘记修一个bug，现在只能检测 run 的时候的状态 其他比如攻击和冲刺等并不能检测，导致可以一边做动作一边恢复耐力
                // 耐力值不满，且没有冲刺，且耐力回复未开启
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
                // 大于0，可以使用冲刺
                if (m_currentStats.Stamina > 0f) isNowCanRun = true;

                RecoverStamina((int)(staminaValueIncrement));

                if (m_currentStats.Stamina > maxStamina)
                    m_currentStats.Stamina = maxStamina;

                yield return timeToWait;
            }
            // 耐力回复完毕，引用置空
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
            // 卸下当前类型的装备
            Equipment previousEquipment = Unequip(equipment.type);

            // 穿戴新装备
            m_equipments[equipment.type] = equipment;

            // 如果装备有技能属性，将其依次分配到空闲的技能槽
            if (equipment.ability != null)
            {
                AssignAbilitiesToSlots(equipment.ability, 2);
            }

            GameManager.NotificationSystem.itemEquipped.Invoke(equipment);
            UpdateStats();
            return previousEquipment;
        }

        public void DeathUnequipAll()
        {
            // 好像这个自己写的字典不能这么遍历
            //foreach(EEquipmentType equipmentType in m_equipments.Keys)

            Array eEquipmentType = Enum.GetValues(typeof(EEquipmentType));
            foreach (EEquipmentType equipmentType in eEquipmentType)
            {
                m_equipments.TryGetValue(equipmentType, out Equipment toUnequip);

                if (toUnequip)
                {
                    if (toUnequip.ability != null)
                    {
                        RemoveAbilitiesFromSlots(toUnequip.ability, 2);
                    }

                    // 检查容量并调整
                    if (toUnequip.capacity != 0)
                    {
                        GameManager.InventorySystem.DecreaseBackpackCapacity(toUnequip.capacity);
                        GameManager.UIManagerSystem.UIMenu.inventory.Init(); // 更新 UI
                    }

                    m_equipments.Remove(equipmentType);
                    GameManager.NotificationSystem.itemUnequipped.Invoke(toUnequip);
                    UpdateStats();
                }
            }
        }

        public Equipment Unequip(EEquipmentType type)
        {
            m_equipments.TryGetValue(type, out Equipment toUnequip);

            if (toUnequip)
            {
                // 如果装备有技能属性，将其从技能槽中移除
                if (toUnequip.ability != null)
                {
                    RemoveAbilitiesFromSlots(toUnequip.ability, 2);
                }

                m_equipments.Remove(type);
                GameManager.NotificationSystem.itemUnequipped.Invoke(toUnequip);
                UpdateStats();
            }

            return toUnequip;
        }

        // 为技能分配到从 startIndex 开始的槽
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

        // 从技能槽中移除从 startIndex 开始的技能
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

        private void UpdateStats()
        {
            Stats equipmentStats = CalculateEquipmentStats();
            Stats totalStats = m_sheet.baseStats + m_customStats + equipmentStats;

            m_maxStats.Set(totalStats);
            m_maxStats.Set(maxStamina);

            ApplyMissingCurrentStats();
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
