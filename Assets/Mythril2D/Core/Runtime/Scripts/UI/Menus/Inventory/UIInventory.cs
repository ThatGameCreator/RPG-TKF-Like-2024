using TMPro;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class UIInventory : MonoBehaviour, IUIMenu
    {
        [SerializeField] private UIInventoryEquipment m_equipment = null;
        [SerializeField] private UIInventoryBag m_bag = null;
        [SerializeField] private TextMeshProUGUI m_backpackMoney = null;

        public UIInventoryBag bag => m_bag;

        private void Awake()
        {
            GameManager.NotificationSystem.OnBagItemDiscarded?.AddListener(OnItemDiscarded);
            GameManager.NotificationSystem.OnEquipmentDiscarded?.AddListener(OnEquipmentDiscarded);

        }

        private void OnDestroy()
        {
            GameManager.NotificationSystem.OnBagItemDiscarded?.RemoveListener(OnItemDiscarded);
            GameManager.NotificationSystem.OnEquipmentDiscarded?.RemoveListener(OnEquipmentDiscarded);
        }

        public void Init()
        {
            m_bag.UpdateSlots();
        }

        public void Show(params object[] args)
        {
            UpdateUI();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
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
                UINavigationCursorTarget equipmentNavigationTarget = m_equipment.FindNavigationTarget();

                if (equipmentNavigationTarget && equipmentNavigationTarget.isActiveAndEnabled)
                {
                    return equipmentNavigationTarget.gameObject;
                }
            }

            return null;
        }

        // Message called by children using SendMessageUpward when the bag or equipment changed
        private void UpdateUI()
        {
            m_bag.UpdateSlots();
            m_equipment.UpdateSlots();
            m_backpackMoney.text = StringFormatter.Format("{0}", GameManager.InventorySystem.backpackMoney.ToString());

            // 如果在仓库中扔背包东西，slot不更新能够继续扔
            if (GameManager.WarehouseSystem.isOpenning == true) 
            {
                GameManager.UIManagerSystem.UIMenu.warehouse.bag.UpdateSlots();
            }

            if(GameManager.UIManagerSystem.UIMenu.shop.isActiveAndEnabled == true)
            {
                GameManager.UIManagerSystem.UIMenu.shop.bag.UpdateSlots();
            }
        }

        private void OnItemClicked(Item item, EItemLocation location)
        {
            item.Use(GameManager.Player, location);
            UpdateUI();
        }

        private void OnEquipmentDiscarded(Equipment equipment, EItemLocation location)
        {
            if(location == EItemLocation.Equipment)
            {
                equipment.Drop(equipment, GameManager.Player, location);
                UpdateUI();
            }
        }

        private void OnItemDiscarded(ItemInstance itemInstance, EItemLocation location)
        {
            // 感觉这分开写是不是有点傻逼
            // 要么就弄两个委托，要么就合成一个？
            if(location == EItemLocation.Bag)
            {
                itemInstance.GetItem().Drop(itemInstance, GameManager.Player, location);
                UpdateUI();
            }
        }

        private void OnBagItemClicked(Item item) => OnItemClicked(item, EItemLocation.Bag);
        private void OnBagItemDiscarded(ItemInstance itemInstance) => OnItemDiscarded(itemInstance, EItemLocation.Bag);
        private void OnEquipmentItemClicked(Item item) => OnItemClicked(item, EItemLocation.Equipment);
    }
}
