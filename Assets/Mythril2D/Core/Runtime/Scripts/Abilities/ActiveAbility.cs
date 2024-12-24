using UnityEngine;
using UnityEngine.Events;

namespace Gyvr.Mythril2D
{
    public interface ITriggerableAbility
    {
        public void Fire(UnityAction onAbilityEnded);
        public bool CanFire();
        public AbilityBase GetAbilityBase();
    }

    public abstract class ActiveAbility<SheetType> : Ability<SheetType>, ITriggerableAbility where SheetType : AbilitySheet
    {
        private UnityAction m_onAbilityEndedCallback = null;

        public void Fire(UnityAction onAbilityEnded)
        {
            m_onAbilityEndedCallback = onAbilityEnded;
            m_character.DisableActions(m_sheet.disabledActionsWhileCasting);
            Fire();

            // 避免敌人也有魔法值消耗 导致最终无法释放技能
            if (m_character.tag == "Player")
            {
                ConsumeStamina();
                ConsumeMana();
            }
        }

        public virtual bool CanFire()
        {
            if(m_character.tag == "Player")
            {

                //return m_character.Can(EActionFlags.UseAbility) && m_character.currentStats[EStat.Mana] >= m_sheet.manaCost && m_character.currentStats[EStat.Stamina] >= m_sheet.staminaCost;
                // 想了想感觉应该不足精力的时候也能释放，直接扣到0就好
                // 如果不加判断，好像即便0也能释放技能了
                //return m_character.Can(EActionFlags.UseAbility) && m_character.currentStats[EStat.Mana] >= m_sheet.manaCost;

                if (m_sheet.costedItem != null)
                {
                    return m_character.Can(EActionFlags.UseAbility) &&
                    m_character.currentStats[EStat.Mana] >= m_sheet.manaCost &&
                    GameManager.Player.GetStamina() >= m_sheet.staminaCost;

                    //GameManager.InventorySystem.HasItemInBag(m_sheet.costedItem, m_sheet.itemCost);
                }
                else
                {
                    return m_character.Can(EActionFlags.UseAbility) &&
                    m_character.currentStats[EStat.Mana] >= m_sheet.manaCost &&
                    GameManager.Player.GetStamina() >= m_sheet.staminaCost;
                }
                
            }
            else
            {
                return m_character.Can(EActionFlags.UseAbility) && m_character.currentStats[EStat.Mana] >= m_sheet.manaCost;

            }
        }

        protected virtual void ConsumeItem()
        {
            // 先消耗背包的 再消耗装备栏的并脱下
            if (GameManager.InventorySystem.HasItemInBag(m_sheet.costedItem))
            {
                GameManager.InventorySystem.RemoveFromBag(m_sheet.costedItem, m_sheet.itemCost);
            }
            else if (GameManager.Player.equipments[EEquipmentType.Weapon] == m_sheet.costedItem)
            {
                GameManager.Player.Unequip(EEquipmentType.Weapon);
                GameManager.InventorySystem.RemoveFromBag(m_sheet.costedItem, m_sheet.itemCost);

            }
        }

        protected virtual void ConsumeMana()
        {
            GameManager.Player.ConsumeMana(m_sheet.manaCost);
        }

        protected virtual void ConsumeStamina()
        {
            GameManager.Player.ConsumeStamina(m_sheet.staminaCost);
            //GameManager.PlayerSystem.PlayerInstance.ConsumeStamina(m_sheet.staminaCost);
        }

        protected void TerminateCasting()
        {
            // 这个玩家和怪物都会播放
            if (m_character.tag == "Player")
            {
                // 如果只在玩家释放的时候取消搜刮，那么怪物攻击玩家的时候可能会拾取
                //Debug.Log("TerminateCasting");
                GameManager.Player.isExecutingAction = false;

                GameManager.Player.CancelLooting();
            }
                
            m_character.EnableActions(m_sheet.disabledActionsWhileCasting);
            m_onAbilityEndedCallback?.Invoke();
            m_onAbilityEndedCallback = null;
        }

        protected void TerminateDashCasting()
        {
            GameManager.Player.isDashFinishing = false;
            GameManager.Player.isExecutingAction = false;

            m_character.EnableActions(m_sheet.disabledActionsWhileCasting);
            m_onAbilityEndedCallback?.Invoke();
            m_onAbilityEndedCallback = null;
        }

        protected abstract void Fire();

        public AbilityBase GetAbilityBase() => this;
    }
}
