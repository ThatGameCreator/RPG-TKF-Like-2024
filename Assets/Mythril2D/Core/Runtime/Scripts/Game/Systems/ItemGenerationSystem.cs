using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class ItemGenerationSystem : AGameSystem
    {
        [SerializeField]
        public SerializableDictionary<Item, SurfaceItem> InstanceObjects = null;

        private SerializableDictionary<int, SurfaceItem> m_instantiateItems = new SerializableDictionary<int, SurfaceItem>();
        private int m_currentyItemCount = 0;

        public void DropItemToPlayer(Item item, int quantity)
        {
            // 获取玩家的位置
            Vector3 playerPosition = GameManager.Player.transform.position;

            // 创建物品对象并设置其位置
            for (int i = 0; i < quantity; i++)
            {
                //Debug.Log("Instantiate");
                // 假设物品有 prefab 引用
                SurfaceItem droppedItem = Instantiate(InstanceObjects[item], transform); 
                droppedItem.transform.position = playerPosition;

                m_instantiateItems[++m_currentyItemCount] = droppedItem;
            }
        }

        public void DeleteNullItem(SurfaceItem lootItem)
        {
            if(m_instantiateItems.ContainsKey(lootItem.dropIndex))
            {
                m_instantiateItems.Remove(lootItem.dropIndex);
            }
        }

        public void DestoryAllItemOnTeleport()
        {
            if(m_instantiateItems.Count != 0)
            {
                foreach (var itemKeyToValue in m_instantiateItems)
                {
                    // 好像这样写只是销毁了引用？并没有真的销毁对象？
                    // Destroy(item);
                    // 不懂得写成gameobject才行
                    Destroy(itemKeyToValue.Value.gameObject);

                    m_instantiateItems.Remove(itemKeyToValue.Key);
                }

                m_instantiateItems.Clear();

                m_currentyItemCount = 0;
            }
        }
    }
}


