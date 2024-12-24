using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Crafting + nameof(Recipe))]
    public class Recipe : DatabaseEntry, INameable
    {
        [Header("Settings")]
        [SerializeField] private int m_craftingFee;

        [Header("Overrides")]
        [SerializeField] private string m_nameOverride = string.Empty;
        [SerializeField] private Sprite m_iconOverride = null;

        [Header("Output")]
        [SerializeField] private Item m_item = null;
        [SerializeField][Min(1)] private int m_quantity = 1;
        [SerializeField] private SerializableDictionary<Item, int> m_additionalOutput = null;

        [Header("Input")]
        [SerializeField] private SerializableDictionary<Item, int> m_ingredients = null;

        public Item item => m_item;
        public Sprite icon => m_iconOverride ? m_iconOverride : (m_item ? m_item.Icon : null);
        public string displayName => GetDisplayName();
        public int quantity => m_quantity;
        public SerializableDictionary<Item, int> ingredients => m_ingredients;
        public SerializableDictionary<Item, int> additionalOutput => m_additionalOutput;

        public int CalculateCraftCost(int flatPrice, float craftingPriceMultiplier)
        {
            return (int)(m_craftingFee * craftingPriceMultiplier) + flatPrice;
        }

        public int CalculateCraftCapacity()
        {
            int maxCapacity = int.MaxValue;

            foreach (var ingredient in m_ingredients)
            {
                if (IsValidIngredient(ingredient))
                {
                    maxCapacity = math.min(maxCapacity, GameManager.InventorySystem.GetItemCount(ingredient.Key) / ingredient.Value);
                }
            }

            return maxCapacity;
        }

        public virtual bool CanCraft(out bool hasMoney, out bool hasIngredients, int flatPrice = 0, float craftPriceMultiplier = 1.0f)
        {
            int craftCost = CalculateCraftCost(flatPrice, craftPriceMultiplier);

            hasMoney = GameManager.InventorySystem.HasSufficientFunds(craftCost);

            hasIngredients = true;

            foreach (var ingredient in m_ingredients)
            {
                if (IsValidIngredient(ingredient) && !HasIngredient(ingredient))
                {
                    hasIngredients = false;
                }
            }

            return hasIngredients && hasMoney;
        }

        private string GetDisplayName()
        {
            if (!string.IsNullOrWhiteSpace(m_nameOverride))
            {
                return m_nameOverride;
            }
            else if (m_item)
            {
                string displayName = m_item.DisplayName;

                if (m_quantity > 1)
                {
                    displayName = $"({m_quantity}) {displayName}";
                }

                return displayName;
            }
            else
            {
                return name;
            }
        }

        private bool HasIngredient(KeyValuePair<Item, int> ingredient)
        {
            return GameManager.InventorySystem.HasItemInBag(ingredient.Key, ingredient.Value);
        }

        public bool IsValidIngredient(KeyValuePair<Item, int> ingredient)
        {
            return ingredient.Key != null && ingredient.Value > 0;
        }
    }
}
