using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public static class DamageSolver
    {
        public static int CalculateDamageOut(int flatDamages, int scaledDamages, float scale, int stat)
        {
            return flatDamages + (int)math.round((scaledDamages + stat) * scale);
        }

        public static int CalculateDamageIn(int damage, int stat)
        {
            return (int)math.floor(damage * (100.0f / (100.0f + stat)));
        }

        public static int CalculateCriticalDamage(int damage)
        {
            return damage * 2;
        }

        public static int CalculateMissDamage(int damage)
        {
            return 0;
        }

        public static bool EvaluateCritical(int luck)
        {
            return UnityEngine.Random.Range(1, 100) < luck;
        }

        public static bool EvaluateMiss(int attackerAgility, int defenderAgility)
        {
            float missChance = math.clamp((int)math.floor((90.0f + attackerAgility) * (100.0f / (100.0f + defenderAgility))), 0, 100);
            return UnityEngine.Random.Range(1, 100) > missChance;
        }

        public static int GetOffensiveStat(Stats stats, EDamageType type)
        {
            switch (type)
            {
                default:
                case EDamageType.None: return 0;
                case EDamageType.Physical: return stats[EStat.PhysicalAttack];
                case EDamageType.Magical: return stats[EStat.MagicalAttack];
            }
        }

        public static int GetDefensiveStat(Stats stats, EDamageType type)
        {
            switch (type)
            {
                default:
                case EDamageType.None: return 0;
                case EDamageType.Physical: return stats[EStat.PhysicalDefense];
                case EDamageType.Magical: return stats[EStat.MagicalDefense];
            }
        }

        public static DamageOutputDescriptor SolveDamageOutput(CharacterBase attacker, DamageDescriptor input)
        {
            if (attacker)
            {
                EDamageFlag damageFlags = EDamageFlag.None;

                int damage = CalculateDamageOut(
                    input.flatDamages,
                    input.scaledDamages,
                    input.scale,
                    GetOffensiveStat(attacker.maxStats, input.damageType));

                if (GameManager.Config.canCriticalHit && EvaluateCritical(attacker.maxStats[EStat.Luck]))
                {
                    damage = CalculateCriticalDamage(damage);
                    damageFlags |= EDamageFlag.Critical;
                }

                return new DamageOutputDescriptor
                {
                    source = EDamageSource.Character,
                    attacker = attacker,
                    damage = damage,
                    damageType = input.damageType,
                    distanceType = input.distanceType,
                    flags = damageFlags
                };
            }
            else
            {
                return new DamageOutputDescriptor
                {
                    source = EDamageSource.Unknown,
                    attacker = null,
                    damage = input.flatDamages,
                    damageType = input.damageType,
                    distanceType = input.distanceType,
                    flags = EDamageFlag.None
                };
            }
        }

        public static DamageInputDescriptor SolveDamageInput(CharacterBase defender, DamageOutputDescriptor output)
        {
            if (output.source == EDamageSource.Character)
            {
                CharacterBase attacker = output.attacker as CharacterBase;

                int damage = CalculateDamageIn(output.damage, GetDefensiveStat(defender.maxStats, output.damageType));
                bool missed =
                    GameManager.Config.canMissHit ?
                    EvaluateMiss(attacker.maxStats[EStat.Agility], defender.maxStats[EStat.Agility]) :
                    false;

                if (missed) UnityEngine.Debug.Log("Damage missed");

                return new DamageInputDescriptor
                {
                    
                    attacker = output.attacker,
                    damage = missed ? CalculateMissDamage(damage) : damage,
                    flags = missed ? output.flags | EDamageFlag.Miss : output.flags
                };
            }
            else
            {
                return new DamageInputDescriptor
                {
                    attacker = output.attacker,
                    damage = output.damage,
                    flags = output.flags
                };
            }
        }
    }
}
