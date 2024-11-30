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
        Key,
        MonsterDrop,
    }

    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Items + nameof(Item))]
    public class Item : DatabaseEntry
    {
        [Header("General")]
        [SerializeField] private EItemCategory m_category = 0;
        [SerializeField] private Sprite m_icon = null;
        [SerializeField] private string m_displayName = string.Empty;
        [SerializeField] private string m_description = string.Empty;
        [SerializeField] private int m_buyPrice = 50;
        [SerializeField] private int m_sellPrice = 50;
        [SerializeField] private bool m_isStackable = false; // 默认设置为不可堆叠


        public virtual void Use(CharacterBase target, EItemLocation location)
        {
            if (GameManager.WarehouseSystem.isOpenning == true)
            {
                if (location == EItemLocation.Bag && GameManager.WarehouseSystem.IsWarehouseFull() == false)
                {
                    GameManager.InventorySystem.RemoveFromBag(this);
                    GameManager.WarehouseSystem.AddToWarehouse(this);
                }
                else if (location == EItemLocation.Warehouse && GameManager.InventorySystem.IsBackpackFull() == false)
                {
                    GameManager.InventorySystem.AddToBag(this);
                    GameManager.WarehouseSystem.RemoveFromWarehouse(this);
                }
            }
            else
            {
                GameManager.DialogueSystem.Main.PlayNow("This item has no effect");
            }
        }

        // 公共访问器
        public EItemCategory Category
        {
            get => m_category;
            set => m_category = value;
        }

        public Sprite Icon
        {
            get => m_icon;
            set => m_icon = value;
        }

        public string DisplayName
        {
            get => m_displayName;
            set => m_displayName = value;
        }

        public string Description
        {
            get => m_description;
            set => m_description = value;
        }

        public int buyPrice
        {
            get => m_buyPrice;
            set => m_buyPrice = value;
        }

        public int sellPrice
        {
            get => m_sellPrice;
            set => m_sellPrice = value;
        }

        public bool IsStackable
        {
            get => m_isStackable;
            set => m_isStackable = value;
        }
    }
}
