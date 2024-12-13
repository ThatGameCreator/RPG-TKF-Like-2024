using System;
using System.Linq;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public struct PlayerDataBlock
    {
        public DatabaseEntryReference<PlayerProfile> profile;
        public bool firstAwake;
        public int usedPoints;
        public int experience;
        public Stats missingCurrentStats;
        public float missingCurrentStamina;
        public Stats customStats;
        public DatabaseEntryReference<Equipment>[] equipments;
        public DatabaseEntryReference<AbilitySheet>[] bonusAbilities;
        public DatabaseEntryReference<AbilitySheet>[] equippedAbilities;
        public DashAbilitySheet dashAbility;
        public Vector3 position;
    }

    public class PlayerSystem : AGameSystem, IDataBlockHandler<PlayerDataBlock>
    {
        [Header("Settings")]
        [SerializeField] private GameObject m_dummyPlayerPrefab = null;

        public Hero PlayerInstance => m_playerInstance;
        public PlayerProfile PlayerProfile => m_profile;

        private PlayerProfile m_profile = null;
        private Hero m_playerInstance = null;

        public override void OnSystemStart()
        {
            m_playerInstance = InstantiatePlayer(m_dummyPlayerPrefab);
        }

        private Hero InstantiatePlayer(GameObject prefab)
        {
            GameObject playerInstance = Instantiate(prefab, transform);
            Hero hero = playerInstance.GetComponent<Hero>();
            Debug.Assert(hero != null, "The player instance specified doesn't have a Hero component");
            return hero;
        }

        private Hero CreatePlayer(PlayerProfile profile, PlayerDataBlock? block = null, bool invokePlayerSpawnedEvent = true)
        {
            Hero hero = InstantiatePlayer(profile.prefab);
            m_profile = profile;

            if (block != null)
            {
                hero.Initialize(block.Value);
            }

            if (invokePlayerSpawnedEvent)
            {
                GameManager.NotificationSystem.playerSpawned.Invoke(hero);
            }

            return hero;
        }

        public void LoadDataBlock(PlayerDataBlock block)
        {
            if (m_playerInstance)
            {
                Destroy(m_playerInstance.gameObject);
            }

            PlayerProfile profile = GameManager.Database.LoadFromReference(block.profile);
            m_playerInstance = CreatePlayer(profile, block);
        }

        public PlayerDataBlock CreateDataBlock()
        {
            return new PlayerDataBlock
            {
                profile = GameManager.Database.CreateReference(m_profile),
                firstAwake = m_playerInstance.firstAwake,
                usedPoints = m_playerInstance.usedPoints,
                experience = m_playerInstance.experience,
                equipments = m_playerInstance.equipments.Values.Select(equipment => GameManager.Database.CreateReference(equipment)).ToArray(),
                missingCurrentStats = m_playerInstance.maxStats - m_playerInstance.currentStats,
                missingCurrentStamina = m_playerInstance.maxStamina - m_playerInstance.currentStamina,
                customStats = m_playerInstance.customStats,
                equippedAbilities = m_playerInstance.equippedAbilities.Where(ability => ability != null).Select(ability => GameManager.Database.CreateReference(ability)).ToArray(),
                bonusAbilities = m_playerInstance.bonusAbilities.Select(ability => GameManager.Database.CreateReference(ability)).ToArray(),
                position = m_playerInstance.transform.position
            };
        }
    }
}
