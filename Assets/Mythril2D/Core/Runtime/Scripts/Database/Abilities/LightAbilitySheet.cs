using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Abilities + nameof(LightAbilitySheet))]
    public class LightAbilitySheet : AbilitySheet
    {
        [Header("Lighting Ability Settings")]
        [SerializeField] private float m_LightingResistance = 120.0f;

        public float lightingResistance => m_LightingResistance;
    }
}
