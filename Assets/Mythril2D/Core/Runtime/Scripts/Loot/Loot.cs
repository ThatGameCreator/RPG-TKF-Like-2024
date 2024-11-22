using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public struct Loot
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
    public struct ChestLootEntry
    {
        public Item item;
        public int quantity;
    }

    [Serializable]
    public struct ChestLoot
    {
        public ChestLootEntry[] entries;
        [Min(5)] public int money;

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
                    sprites.Add(entries[i].item.icon);
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
