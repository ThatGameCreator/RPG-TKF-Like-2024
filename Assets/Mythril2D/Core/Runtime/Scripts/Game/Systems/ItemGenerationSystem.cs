using Codice.Client.BaseCommands;
using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class ItemGenerationSystem : AGameSystem
    {
        [SerializeField]
        public SerializableDictionary<Item, SurfaceItem> InstanceObjects = null;

        public void DropItemToPlayer(Item item, int quantity)
        {
            // 获取玩家的位置
            Vector3 playerPosition = GameManager.Player.transform.position;

            // 创建物品对象并设置其位置
            for (int i = 0; i < quantity; i++)
            {
                //Debug.Log("Instantiate");

                Entity droppedItem = Instantiate(InstanceObjects[item]); // 假设物品有 prefab 引用
                droppedItem.transform.position = playerPosition;
            }
        }

    }
}


