using Unity.Mathematics;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public enum ETransactionType
    {
        Buy,
        Sell
    }

    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Shops + nameof(Shop))]
    public class Shop : DatabaseEntry
    {

        public EItemCategory[] availableSellTypes = null;

        public Item[] items = null;

        public int GetPrice(Item item, ETransactionType transaction)
        {
            float floatPrice = 0.0f;

            switch (transaction)
            {
                case ETransactionType.Buy:
                    floatPrice = item.buyPrice;
                    break;

                case ETransactionType.Sell:
                    floatPrice = item.sellPrice;
                    break;
            }

            return (int)math.ceil(floatPrice);
        }
    }
}
