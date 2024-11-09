using UnityEngine;

namespace Gyvr.Mythril2D
{
    public enum EItemLocation
    {
        Bag,
        Warehouse,
        Equipment
    }

    public enum EItemCategory
    {
        Consumable,
        Resource,
        Gear,
        Key
    }

    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Items + nameof(Item))]
    public class Item : DatabaseEntry, INameable
    {
        [Header("General")]
        [SerializeField] private EItemCategory m_category = 0;
        [SerializeField] private Sprite m_icon = null;
        [SerializeField] private string m_displayName = string.Empty;
        [SerializeField] private string m_description = string.Empty;
        [SerializeField] private int m_price = 50;

        public virtual void Use(CharacterBase target, EItemLocation location)
        {
            GameManager.DialogueSystem.Main.PlayNow("This item has no effect");
        }

        public EItemCategory category => m_category;
        public Sprite icon => m_icon;
        public string displayName => DisplayNameUtils.GetNameOrDefault(this, m_displayName);
        public string description => StringFormatter.Format(m_description);
        public int price => m_price;
    }
}
