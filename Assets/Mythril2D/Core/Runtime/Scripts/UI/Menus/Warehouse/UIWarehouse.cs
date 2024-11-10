using log4net.Core;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIWarehouse : MonoBehaviour, IUIMenu
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI m_backpackMoney = null;
        [SerializeField] private TextMeshProUGUI m_warehouseMoney = null;
        [SerializeField] private UIInventoryBag m_bag = null;
        [SerializeField] private UIWarehouseBag m_warehouse = null;
        [SerializeField] private UIWarehouseCurrency m_uiCurrency = null;

        public void Init()
        {
            m_bag.Init();
            m_warehouse.Init();
            m_uiCurrency.RegisterCallbacks(
                OnWarehouseAllButtonPressed, OnWarehouseTenButtonPressed, OnWarehouseOneButtonPressed,
                OnBackpackOneButtonPressed, OnBackpackTenButtonPressed, OnBackpackAllButtonPressed);
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
            UpdateInfoSection();
            m_bag.UpdateSlots();
            m_warehouse.UpdateSlots();
        }
        private void UpdateInfoSection()
        {
            m_backpackMoney.text = StringFormatter.Format("{0}", GameManager.InventorySystem.backpackMoney.ToString());
            m_warehouseMoney.text = StringFormatter.Format("{0}", GameManager.WarehouseSystem.warehouseMoney.ToString());
        }

        private void OnItemClicked(Item item, EItemLocation location)
        {
            item.Use(GameManager.Player, location);
            UpdateUI();
        }

        private void OnBagItemClicked(Item item) => OnItemClicked(item, EItemLocation.Bag);
        private void OnWarehouseItemClicked(Item item) => OnItemClicked(item, EItemLocation.Warehouse);

        public void OnWarehouseAllButtonPressed(Button button)
        {
            GameManager.WarehouseSystem.AddMoney(GameManager.InventorySystem.backpackMoney);

            GameManager.InventorySystem.RemoveMoney(GameManager.InventorySystem.backpackMoney);

            UpdateUI();
        }

        public void OnWarehouseTenButtonPressed(Button button)
        {
            if (GameManager.InventorySystem.HasSufficientFunds(10))
            {
                GameManager.WarehouseSystem.AddMoney(10);

                GameManager.InventorySystem.RemoveMoney(10);

                UpdateUI();
            }
        }

        public void OnWarehouseOneButtonPressed(Button button)
        {
            if (GameManager.InventorySystem.HasSufficientFunds(1))
            {
                GameManager.WarehouseSystem.AddMoney(1);

                GameManager.InventorySystem.RemoveMoney(1);

                UpdateUI();
            }
        }

        public void OnBackpackAllButtonPressed(Button button)
        {
            GameManager.InventorySystem.AddMoney(GameManager.WarehouseSystem.warehouseMoney);

            GameManager.WarehouseSystem.RemoveMoney(GameManager.WarehouseSystem.warehouseMoney);

            UpdateUI();
        }

        public void OnBackpackTenButtonPressed(Button button)
        {
            if (GameManager.WarehouseSystem.HasSufficientFunds(10)){
                GameManager.InventorySystem.AddMoney(10);

                GameManager.WarehouseSystem.RemoveMoney(10);

                UpdateUI();
            }
        }

        public void OnBackpackOneButtonPressed(Button button)
        {
            if (GameManager.WarehouseSystem.HasSufficientFunds(1))
            {
                GameManager.InventorySystem.AddMoney(1);

                GameManager.WarehouseSystem.RemoveMoney(1);

                UpdateUI();
            }
        }
    }
}
