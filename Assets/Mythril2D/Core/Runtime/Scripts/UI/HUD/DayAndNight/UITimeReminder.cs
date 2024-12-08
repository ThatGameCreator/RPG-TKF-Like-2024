using System.Collections.Generic;
using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UITimeReminder : MonoBehaviour
    {
        [Serializable]
        public class KeyValueMapping
        {
            public string Key;
            public List<string> Value;

            public KeyValueMapping(string key, List<string> value)
            {
                Key = key;
                Value = value;
            }
        }

        public List<string> Get(string key)
        {
            var mapping = evacuationTextRules.Find(m => m.Key == key);
            return mapping?.Value;
        }
        [SerializeField] private TextMeshProUGUI m_timeText = null;


        [SerializeField]
        private List<KeyValueMapping> evacuationTextRules = new List<KeyValueMapping>();
        [SerializeField] private TextMeshProUGUI[] m_evacuationTexts = null;
        private bool m_isTextBeRed = false;
        private bool m_setEvacuationTextActive = false;

        private void OnEnable()
        {
            GameManager.NotificationSystem.SetActiveEvacuation.AddListener(SetEvacuationText);
        }

        private void OnDestroy()
        {
            GameManager.NotificationSystem.SetActiveEvacuation.RemoveListener(SetEvacuationText);
        }

        private void SetEvacuationText(string teleportName)
        {
            List<string> tmpEvacuationTextKey = Get(teleportName);

            for (int i = 0; i < m_evacuationTexts.Length; i++) {
                m_evacuationTexts[i].text = GameManager.LocalizationSystem.GetMenuLocalizedString
                    (tmpEvacuationTextKey[i], EMenuStringTableType.TimeReminder);
            }
        }

        private void Update()
        {
            UpdateUI();
        }

        private void UpdateTime()
        {
            if (GameManager.Player.isEvacuating == true)
            {
                int currentMinute = 0;
                int currentSecond = (int)(GameManager.Player.evacuatingRequiredtTime - GameManager.Player.evacuatingTime);

                m_timeText.color = Color.green;

                m_timeText.text = StringFormatter.Format("{0:D2} : {1:D2}", currentMinute, currentSecond);
            }

            else
            {
                m_timeText.color = Color.white;

                // 这里感觉应该传送的时候获取一次就可以了，没必要一直更新计算
                int totalCurrentSecond = (int)GameManager.DayNightSystem.currentTime;
                int currentMinute = totalCurrentSecond % 3600 / 60;
                int currentSecond = totalCurrentSecond % 3600 % 60;

                // 感觉有点浪费资源？每次都要判断 是不是能够用监听什么
                // 或者整个一秒执行一次之类的
                if (m_isTextBeRed == false && totalCurrentSecond <= GameManager.DayNightSystem.maxEmergencyTime)
                {
                    m_timeText.color = Color.red;
                }

                m_timeText.text = StringFormatter.Format("{0:D2} : {1:D2}", currentMinute, currentSecond);
            }
        }

        private void UpdateUI()
        {
            UpdateTime();
        }
    }
}
