using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class PlaceItemObject : OtherEntity
    {
        [Header("Place Item Object Settings")]
        [SerializeField] private string m_displayName = string.Empty;
        [SerializeField] private PlaceItemTask m_placeTask = null;

        private int m_placedItemCount = 0;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_openedSound;

        private bool m_placed = false;
        public string displayName => DisplayNameUtils.GetNameOrDefault(this, m_displayName);

        public override void OnStartInteract(CharacterBase sender, Entity target)
        {
            // 傻了，还以为是什么世界难题，原来只是逻辑错了
            // target != this肯定是true啊 但后面的没有血瓶导致直接跳过这个方法了 自然就执行后面的代码了
            // if (target != this && GameManager.InventorySystem.HasItemInBag(m_placeTask.itemPlaced))
            if (target != this || GameManager.InventorySystem.HasItemInBag(m_placeTask.itemPlaced) == false)
            {
                return;
            }

            GameManager.Player.OnTryStartLoot(target, m_lootedTime);
        }

        public bool TryPlaceItem()
        {
            if (GameManager.InventorySystem.HasItemInBag(m_placeTask.itemPlaced, 1))
            {
                GameManager.InventorySystem.RemoveFromBag(m_placeTask.itemPlaced, 1);

                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_openedSound);

                GameManager.NotificationSystem.itemPlacedAtLocation.Invoke(m_placeTask.itemPlaced);

                m_placedItemCount++;

                if (m_placedItemCount >= m_placeTask.itemPlacedNumber)
                {
                    this.gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }

            return true;
        }
    }
}
