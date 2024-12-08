using System.Collections.Generic;
using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Gyvr.Mythril2D
{
    public class UITimeReminder : MonoBehaviour
    {
        [Serializable]
        public class EvacuationToTextMapping
        {
            public string Key;
            public List<string> Value;
        }

        public List<string> GetEvacuationValueMapping(string key)
        {
            var mapping = evacuationTextRules.Find(m => m.Key == key);
            return mapping?.Value;
        }

        [Serializable]
        public class EvacuationToPositionMapping
        {
            public string Key;
            public List<Vector2> Value;
        }

        public List<Vector2> GetEvacuationPosition(string key)
        {
            var mapping = evacuationPositionRules.Find(m => m.Key == key);
            return mapping?.Value;
        }

        [SerializeField] private TextMeshProUGUI m_timeText = null;
        [SerializeField] private Image[] m_compassImgae = null;


        [SerializeField] private List<EvacuationToTextMapping> evacuationTextRules = new List<EvacuationToTextMapping>();
        [SerializeField] private List<EvacuationToPositionMapping> evacuationPositionRules = new List<EvacuationToPositionMapping>();
        [SerializeField] private TextMeshProUGUI[] m_evacuationTexts = null;
        private bool m_isTextBeRed = false;
        private bool m_setEvacuationTextActive = false;
        private string m_teleportName = string.Empty;

        private void OnEnable()
        {
            GameManager.NotificationSystem.SetActiveEvacuation.AddListener(SetEvacuationInfo);
        }

        private void OnDestroy()
        {
            GameManager.NotificationSystem.SetActiveEvacuation.RemoveListener(SetEvacuationInfo);
        }

        private void SetEvacuationInfo(string teleportName)
        {
            m_teleportName = teleportName;
            List<string> tmpEvacuationTextKey = GetEvacuationValueMapping(m_teleportName);

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

        private void UpdateCompass()
        {
            // 获取撤离点位置
            List<Vector2> evacuationPositions = GetEvacuationPosition(m_teleportName);
            if (evacuationPositions == null || evacuationPositions.Count == 0) return;


            // 获取玩家位置
            // 这他妈gpt原来写了个z 我他妈晕死
            Vector2 playerPosition = new Vector2(GameManager.Player.transform.position.x, GameManager.Player.transform.position.y);
            
            for (int i = 0; i < evacuationPositions.Count; i++)
            {
                Vector2 targetPosition = evacuationPositions[i];
                Vector2 direction = targetPosition - playerPosition;

                Vector3 eulerAngles = Quaternion.FromToRotation(Vector3.forward, direction).eulerAngles;
                // 计算角度 (从方向向量到角度，注意Unity的Z轴为0°)
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                //Debug.Log(i+ ", " + targetPosition + " - " + playerPosition);
                //Debug.Log(i+ ", " + direction);
                //Debug.Log(i+ ", " + angle);

                //angle = (angle + 360f) % 360f; // 确保角度在0-360范围内

                // 更新每个图片的旋转

                float currentAngle = m_compassImgae[i].transform.eulerAngles.z;
                // 平滑旋转，注意负号
                // 没有负号 被忽悠了
                float smoothAngle = Mathf.LerpAngle(currentAngle, angle, 0.1f); 
                m_compassImgae[i].transform.eulerAngles = new Vector3(0, 0, smoothAngle);
            }
        }

        private IEnumerator UpdateCompassCoroutine()
        {
            while (true)
            {
                UpdateCompass();
                yield return new WaitForSeconds(1f); // 每秒更新一次
            }
        }

        private void UpdateUI()
        {
            UpdateTime();

            StartCoroutine(UpdateCompassCoroutine());
        }
    }
}
