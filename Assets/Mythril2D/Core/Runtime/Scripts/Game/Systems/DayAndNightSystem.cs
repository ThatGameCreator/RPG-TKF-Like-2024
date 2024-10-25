using FunkyCode;
using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class DayAndNightSystem : AGameSystem
    {
        [SerializeField] private LightingManager2D m_lightingManager = null;
        [SerializeField] private float m_maxBrightness = 0.5f;
        [SerializeField] private float m_remainDayAndNightTime = 60f;
        private float m_currentTime = 60f;

        public LightingManager2D lightingManager => m_lightingManager;

        private void Update()
        {
            Debug.Log("DarknessColor" + m_lightingManager.profile.DarknessColor);

            m_currentTime -= Time.deltaTime;

            if(m_currentTime > 0f)
            {
                float newBrightness = m_lightingManager.profile.DarknessColor.r;

                //  maxWhite = 0.5 -> black = 0
                newBrightness = (float) (0.5 * (m_currentTime / m_remainDayAndNightTime));

                m_lightingManager.profile.DarknessColor = new Color(newBrightness, newBrightness, newBrightness, 1);

                Debug.Log("newBrightness" + newBrightness);
            }

            else if (Mathf.Approximately(1.0f, m_currentTime / m_remainDayAndNightTime))
            {
                m_lightingManager.profile.DarknessColor = new Color(0, 0, 0, 1);
            }
        }
    }
}
