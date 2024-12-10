using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class ItemGenerationSystem : AGameSystem
    {
        [SerializeField]
        public SerializableDictionary<Item, SurfaceItem> InstanceObjects = null;

        public List<Entity> instantiateItems = new List<Entity>();

        public void DropItemToPlayer(Item item, int quantity)
        {
            // 获取玩家的位置
            Vector3 playerPosition = GameManager.Player.transform.position;

            // 创建物品对象并设置其位置
            for (int i = 0; i < quantity; i++)
            {
                //Debug.Log("Instantiate");
                // 假设物品有 prefab 引用
                Entity droppedItem = Instantiate(InstanceObjects[item], transform); 
                droppedItem.transform.position = playerPosition;

                instantiateItems.Add(droppedItem);
            }
        }

        public void DestoryAllItemOnTeleport()
        {
            foreach (Entity item in instantiateItems)
            {
                // 好像这样写只是销毁了引用？并没有真的销毁对象？
                // Destroy(item);
                // 不懂得写成gameobject才行
                Destroy(item.gameObject);
                
            }

            instantiateItems.Clear();
        }
    }
}


