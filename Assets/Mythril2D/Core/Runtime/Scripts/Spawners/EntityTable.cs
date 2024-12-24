using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CreateAssetMenu(fileName = "NewEntityTable", menuName = "Entity/Entity Table")]
    public class EntityTable : ScriptableObject
    {
        [System.Serializable]
        public class EntityData
        {
            public Entity entity;            // �Ӷ����Ʒ
            //public int maxQuantity;      // �������
            public float weight;         // ����Ȩ��
        }

        public EntityData[] entries;
        public int generateRate = 50;  // �������κεĸ��� (0.0 ~ 1.0)
    }
}
