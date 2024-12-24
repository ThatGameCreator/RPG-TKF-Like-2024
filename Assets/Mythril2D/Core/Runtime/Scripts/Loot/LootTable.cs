using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CreateAssetMenu(fileName = "NewLootTable", menuName = "Loot/Loot Table")]
    public class LootTable : ScriptableObject
    {
        [System.Serializable]
        public class LootEntryData
        {
            public Item item;            // �Ӷ����Ʒ
            public int maxQuantity;      // �������
            public float weight;         // ����Ȩ��
        }

        public LootEntryData[] entries;  // �Ӷ���Ŀ�б�
        public int money;                // ��Ǯ�������ֵ
        public int maxLootedCount;       // �Ӷ�������ֵ
        public int lootRate = 75;             // �Ӷ����
    }
}
