using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CreateAssetMenu(fileName = "NewEntityTable", menuName = "Entity/Entity Table")]
    public class EntityTable : ScriptableObject
    {
        [System.Serializable]
        public class EntityData
        {
            public Entity entity;            // 掠夺的物品
            //public int maxQuantity;      // 最大数量
            public float weight;         // 生成权重
        }

        public EntityData[] entries;
        public int generateRate = 50;  // 不生成任何的概率 (0.0 ~ 1.0)
    }
}
