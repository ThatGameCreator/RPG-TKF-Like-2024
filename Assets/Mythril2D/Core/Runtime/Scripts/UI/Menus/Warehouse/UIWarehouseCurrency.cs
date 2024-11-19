using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIWarehouseCurrency : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button m_warehouseIncreaseAllButton;
        [SerializeField] private Button m_warehouseIncreaseTenButton;
        [SerializeField] private Button m_warehouseIncreaseOneButton;
        [SerializeField] private Button m_backpackIncreaseAllButton;
        [SerializeField] private Button m_backpackIncreaseTenButton;
        [SerializeField] private Button m_backpackIncreaseOneButton;

        public void RegisterCallbacks(
            UnityAction<Button> OnWarehouseAllButtonPressed, 
            UnityAction<Button> OnWarehouseTenButtonPressed, 
            UnityAction<Button> OnWarehouseOneButtonPressed, 
            UnityAction<Button> OnBackpackOneButtonPressed, 
            UnityAction<Button> OnBackpackTenButtonPressed, 
            UnityAction<Button> OnBackpackAllButtonPressed)
        {
            m_warehouseIncreaseAllButton.onClick.AddListener(() => OnWarehouseAllButtonPressed(m_warehouseIncreaseAllButton));
            m_warehouseIncreaseTenButton.onClick.AddListener(() => OnWarehouseTenButtonPressed(m_warehouseIncreaseTenButton));
            m_warehouseIncreaseOneButton.onClick.AddListener(() => OnWarehouseOneButtonPressed(m_warehouseIncreaseOneButton));
            m_backpackIncreaseAllButton.onClick.AddListener(() => OnBackpackOneButtonPressed(m_backpackIncreaseAllButton));
            m_backpackIncreaseTenButton.onClick.AddListener(() => OnBackpackTenButtonPressed(m_backpackIncreaseTenButton));
            m_backpackIncreaseOneButton.onClick.AddListener(() => OnBackpackAllButtonPressed(m_backpackIncreaseOneButton));
        }

        // Used for selection
        //public Button GetFirstButton() => m_decreaseButton;
    }
}
