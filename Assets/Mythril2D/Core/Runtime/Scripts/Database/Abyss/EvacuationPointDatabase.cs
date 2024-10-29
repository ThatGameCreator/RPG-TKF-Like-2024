using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Abyss + nameof(EvacuationPoint))]
    public class EvacuationPointDatabase : DatabaseEntry
    {
        public AudioClipResolver getInSound;
    }
}