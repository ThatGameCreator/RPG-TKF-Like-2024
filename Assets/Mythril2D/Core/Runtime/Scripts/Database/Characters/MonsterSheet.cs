using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Characters + nameof(MonsterSheet))]
    public class MonsterSheet : CharacterSheet
    {
        [Header("Monster")]
        public LevelScaledStats stats = new LevelScaledStats();

        [Header("Rewards")]
        public LevelScaledInteger experience = new LevelScaledInteger();
        public LootTable lootTable;

        [Header("Commands")]
        [SerializeReference, SubclassSelector]
        public ICommand executeOnDeath;

        public MonsterSheet() : base(EAlignment.Evil) { }
    }
}
