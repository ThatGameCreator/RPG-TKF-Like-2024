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
            // ɵ�ˣ�����Ϊ��ʲô�������⣬ԭ��ֻ���߼�����
            // target != this�϶���true�� �������û��Ѫƿ����ֱ��������������� ��Ȼ��ִ�к���Ĵ�����
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
                    this.gameObject.layer = LayerMask.NameToLayer("Collision D");
                }
            }

            return true;
        }
    }
}
