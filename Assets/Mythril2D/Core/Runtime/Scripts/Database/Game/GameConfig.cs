using UnityEngine;

namespace Gyvr.Mythril2D
{
    [System.Flags]
    public enum EOptionalCharacterStatistics
    {
        None = 0,
        Mana = 1 << 0,
        MagicalAttack = 1 << 1,
        MagicalDefense = 1 << 2,
        Agility = 1 << 3,
        Luck = 1 << 4,
    }

    [System.Flags]
    public enum ECameraShakeSources
    {
        None = 0,
        PlayerReceiveDamage = 1 << 0,
        AnyCharacterReceiveDamageFromPlayer = 1 << 1
    }

    public enum EGameTerm
    {
        Currency,
        Level,
        Experience
    }

    [System.Serializable]
    public struct StatSettings
    {
        public string name;
        public string shortened;
        public string description;
        public Sprite icon;
        public bool hide;
    }

    [System.Serializable]
    public struct TermDefinition
    {
        public string fullName;
        public string shortName;
        public string description;
        public Sprite icon;
    }

    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Game + nameof(GameConfig))]
    public class GameConfig : DatabaseEntry
    {
        [Header("General Settings")]
        public DatabaseRegistry databaseRegistry = null;
        public string gameplayScene = "Gameplay";
        public string mainMenuSceneName = "Main Menu";
        public string interactionLayer = "Interaction";
        public string mosterLayer = "Default";
        public string projectileLayer = "Projectile";
        public string hitboxLayer = "Hitbox";
        public string[] layersIgnoredByProjectiles;
        public ContactFilter2D collisionContactFilter;

        [Header("Visual Settings")]
        public ECameraShakeSources cameraShakeSources = ECameraShakeSources.None;

        [Header("Gameplay Settings")]
        public CraftingStation onTheGoCraftingStation = null;
        public bool canCriticalHit = true;
        public bool canMissHit = true;

        [Header("Game Terms")]
        [SerializeField] private SerializableDictionary<string, TermDefinition> m_gameTerms = new SerializableDictionary<string, TermDefinition>();

        [Header("Game Terms Bindings (Advanced Settings)")]
        [SerializeField] private SerializableDictionary<EStat, string> m_statTermsBinding = new SerializableDictionary<EStat, string>();
        [SerializeField] private SerializableDictionary<EItemCategory, string> m_itemCategoryTermsBinding = new SerializableDictionary<EItemCategory, string>();

        private TermDefinition m_defaultTermDefinition = new TermDefinition
        {
            fullName = "[INVALID_FULLNAME]",
            shortName = "[INVALID_SHORTNAME]",
            description = "[INVALID_DESCRIPTION]",
            icon = null
        };

        public TermDefinition GetTermDefinition(string termID)
        {
            if (m_gameTerms.ContainsKey(termID))
            {
                return m_gameTerms[termID];
            }

            return m_defaultTermDefinition;
        }

        public TermDefinition GetTermDefinition(EStat stat)
        {
            if (m_statTermsBinding.ContainsKey(stat))
            {
                return GetTermDefinition(m_statTermsBinding[stat]);
            }

            return m_defaultTermDefinition;
        }

        public TermDefinition GetTermDefinition(EItemCategory category)
        {
            if (m_itemCategoryTermsBinding.ContainsKey(category))
            {
                return GetTermDefinition(m_itemCategoryTermsBinding[category]);
            }

            return m_defaultTermDefinition;
        }
    }
}
