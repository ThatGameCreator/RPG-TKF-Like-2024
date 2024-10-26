using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UITimeReminder : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_Text = null;
        private bool m_isTextBeRed = false;

        private void Update()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            int totalCurrentSecond = (int) GameManager.DayAndNightSystem.currentTime;
            int currentMinute = totalCurrentSecond % 3600 / 60;
            int currentSecond = totalCurrentSecond % 3600 % 60;

            // 感觉有点浪费资源？每次都要判断 是不是能够用监听什么
            if (m_isTextBeRed == false && totalCurrentSecond <= GameManager.DayAndNightSystem.maxEmergencyTime)
            {
                m_Text.color = Color.red;
            }

            m_Text.text = StringFormatter.Format("{0:D2} : {1:D2}", currentMinute, currentSecond);
        }
    }
}
