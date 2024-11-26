using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CreateAssetMenu(fileName = "NewLootTable", menuName = "Loot/Loot Table")]
    public class LootTable : ScriptableObject
    {
        [System.Serializable]
        public class LootEntryData
        {
            public Item item;            // 掠夺的物品
            public int maxQuantity;      // 最大数量
            public float weight;         // 生成权重
        }

        public LootEntryData[] entries;  // 掠夺条目列表
        public int money;                // 金钱奖励最大值
    }
}
