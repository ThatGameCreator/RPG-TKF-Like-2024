using UnityEngine;

namespace Gyvr.Mythril2D
{
    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Abyss + nameof(Hole))]
    public class HoleDatabase : DatabaseEntry
    {
        public AudioClipResolver getInSound;
    }
}