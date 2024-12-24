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
        public float weight;     // ����Ʒ������Ȩ��
    }

    [Serializable]
    public struct Loot
    {
        public LootEntry[] entries;
        [Min(5)] public int money;

        // ����Ȩ����������Ӷ���Ʒ
        public LootEntry? GetRandomLoot()
        {
            if (entries == null || entries.Length == 0)
                return null;

            // ������Ȩ��
            float totalWeight = 0f;
            foreach (var entry in entries)
            {
                totalWeight += entry.weight;
            }

            // ����һ��0��totalWeight֮������ֵ
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);

            // ����Ȩ��ѡ���Ӧ��LootEntry
            foreach (var entry in entries)
            {
                if (randomValue < entry.weight)
                {
                    return entry;
                }
                randomValue -= entry.weight;
            }

            return null; // �����ϲ��ᵽ������
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
