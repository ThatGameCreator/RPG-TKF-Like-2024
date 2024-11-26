using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Gyvr.Mythril2D
{
    [CustomEditor(typeof(HeroSheet))]
    public class HeroSheetEditor : DatabaseEntryEditor
    {
        private int m_previewLevel = 1;

        public int GetTotalExperienceRequired(HeroSheet sheet, int level)
        {
            return level > 0 ? sheet.experience[level] + GetTotalExperienceRequired(sheet, level - 1) : 0;
        }

        // Override to display hero-specific information
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI(); // Calls the base class method for shared functionality

            HeroSheet sheet = (HeroSheet)target;

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Hero-Specific Information", EditorStyles.boldLabel);

            // Display hero-specific stats
            EditorGUILayout.IntField("Health", sheet.baseStats[EStat.Health]);
            EditorGUILayout.IntField("Mana", sheet.baseStats[EStat.Mana]);

            // Evolution preview similar to base editor
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Evolution Preview", EditorStyles.boldLabel);

            m_previewLevel = EditorGUILayout.IntSlider("Level", m_previewLevel, Stats.MinLevel, Stats.MaxLevel);

            int experienceRequired = sheet.experience[m_previewLevel];
            int experienceRequiredTotal = GetTotalExperienceRequired(sheet, m_previewLevel);

            GUI.enabled = false;
            EditorGUILayout.IntField("Experience Required", experienceRequired);
            EditorGUILayout.IntField("Experience Required Total", experienceRequiredTotal);
            GUI.enabled = true;

            // Show the abilities preview at the current level
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Abilities at Level", EditorStyles.boldLabel);

            var availableAbilities = sheet.GetAvailableAbilitiesAtLevel(m_previewLevel).ToList();
            foreach (var ability in availableAbilities)
            {
                EditorGUILayout.LabelField(ability.name);
            }

            var unlockedAbilities = sheet.GetAbilitiesUnlockedAtLevel(m_previewLevel).ToList();
            foreach (var ability in unlockedAbilities)
            {
                EditorGUILayout.LabelField("Unlocked at Level " + m_previewLevel + ": " + ability.name);
            }
        }
    }
}
