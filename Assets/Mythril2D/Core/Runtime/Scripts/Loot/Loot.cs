using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public struct MonsterLoot
    {
        [SerializeReference, SubclassSelector] public ICondition condition;
        public Item item;
        public int quantity;
        public int dropRate;
        public int minimumMonsterLevel;
        public int minimumPlayerLevel;

        public bool IsAvailable() => condition?.Evaluate() ?? true;
        public bool ResolveDrop() => UnityEngine.Random.Range(1, 101) <= dropRate;
    }

    [Serializable]
    public struct LootEntry
    {
        public Item item;
        public int quantity;
        public float weight;     // 此物品的生成权重
    }

    [Serializable]
    public struct Loot
    {
        public LootEntry[] entries;
        [Min(5)] public int money;

        // 根据权重随机生成掠夺物品
        public LootEntry? GetRandomLoot()
        {
            if (entries == null || entries.Length == 0)
                return null;

            // 计算总权重
            float totalWeight = 0f;
            foreach (var entry in entries)
            {
                totalWeight += entry.weight;
            }

            // 生成一个0到totalWeight之间的随机值
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);

            // 根据权重选择对应的LootEntry
            foreach (var entry in entries)
            {
                if (randomValue < entry.weight)
                {
                    return entry;
                }
                randomValue -= entry.weight;
            }

            return null; // 理论上不会到达这里
        }

        public bool HasMoney() => money != 0;
        public bool HasItems() => entries != null && entries.Length > 0;
        public bool IsEmpty() => !(HasItems() || HasMoney());

        public Sprite[] GetLootSprites()
        {
            List<Sprite> sprites = new List<Sprite>();

            if (HasItems())
            {
                for (int i = 0; i < entries.Length; ++i)
                {
                    sprites.Add(entries[i].item.Icon);
                }
            }

            if (HasMoney())
            {
                sprites.Add(GameManager.Config.GetTermDefinition("currency").icon);
            }

            sprites.RemoveAll((sprite) => sprite == null);

            return sprites.ToArray();
        }
    }
}
