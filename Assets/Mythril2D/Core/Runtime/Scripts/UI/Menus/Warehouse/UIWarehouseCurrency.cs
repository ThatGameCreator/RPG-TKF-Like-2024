using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIWarehouseCurrency : MonoBehaviour
    {
        //[Header("References")]

        public void RegisterCallbacks(Dictionary<Button, UnityAction> buttonCallbacks)
        {
            foreach (var entry in buttonCallbacks)
            {
                entry.Key.onClick.AddListener(() => entry.Value());
            }
        }


        // Used for selection
        //public Button GetFirstButton() => m_decreaseButton;
    }
}
