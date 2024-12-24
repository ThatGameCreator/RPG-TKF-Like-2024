using UnityEngine;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UISettingsVolume : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected Slider m_slider = null;
        [SerializeField] protected Button m_decreaseButton;
        [SerializeField] protected Button m_increaseButton;

        public void UpdateUI(int volume, int suffix)
        {
            m_slider.minValue = 0;
            m_slider.maxValue = suffix;

            m_slider.value = volume;
        }

        // Used for selection
        public Button GetFirstButton() => m_decreaseButton;
    }
}
