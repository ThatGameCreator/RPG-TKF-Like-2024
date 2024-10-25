using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UITimeReminder : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_Text = null;

        private void Update()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            int totalCurrentSecond = (int) GameManager.DayAndNightSystem.currentTime;
            int currentMinute = totalCurrentSecond % 3600 / 60;
            int currentSecond = totalCurrentSecond % 3600 % 60;

            m_Text.text = StringFormatter.Format("{0} : {1}", currentMinute, currentSecond);
        }
    }
}
