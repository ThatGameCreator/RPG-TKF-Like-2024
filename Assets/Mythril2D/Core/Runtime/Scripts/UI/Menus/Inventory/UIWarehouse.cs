using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class UIWarehouse : MonoBehaviour, IUIMenu
    {
        [SerializeField] private UIInventoryBag m_bag = null;
        [SerializeField] private UIWarehouseBag m_warehouse = null;

        public void Init()
        {
            m_bag.Init();
            m_warehouse.Init();
        }

        public void Show(params object[] args)
        {
            GameManager.WarehouseSystem.isOpenning = true;
            UpdateUI();
            gameObject.SetActive(true);

        }

        public void Hide()
        {
            GameManager.WarehouseSystem.isOpenning = false;

            gameObject.SetActive(false);

            GameManager.NotificationSystem.UIWarehouseClosed.Invoke();
        }

        public void EnableInteractions(bool enable)
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup)
            {
                canvasGroup.interactable = enable;
            }
        }

        public GameObject FindSomethingToSelect()
        {
            UINavigationCursorTarget bagNavigationTarget = m_bag.FindNavigationTarget();

            if (bagNavigationTarget && bagNavigationTarget.gameObject.activeInHierarchy)
            {
                return bagNavigationTarget.gameObject;
            }
            else
            {
                UINavigationCursorTarget warehouseNavigationTarget = m_warehouse.FindNavigationTarget();

                if (warehouseNavigationTarget && warehouseNavigationTarget.gameObject.activeInHierarchy)
                {
                    return warehouseNavigationTarget.gameObject;
                }
            }

            return null;
        }

        // Message called by children using SendMessageUpward when the bag or equipment changed
        private void UpdateUI()
        {
            m_bag.UpdateSlots();
            m_warehouse.UpdateSlots();
        }

        private void OnItemClicked(Item item, EItemLocation location)
        {
            item.Use(GameManager.Player, location);
            UpdateUI();
        }

        private void OnBagItemClicked(Item item) => OnItemClicked(item, EItemLocation.Bag);
        private void OnWarehouseItemClicked(Item item) => OnItemClicked(item, EItemLocation.Warehouse);
    }
}
