using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UISoulBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI m_nowExperiencelabel = null;
        [SerializeField] private TextMeshProUGUI m_nextExperiencelabel = null;
        [SerializeField] private Slider m_slider = null;

        [Header("Visual Settings")]
        [SerializeField] private bool m_shakeOnDecrease = false;
        [SerializeField] private float m_shakeAmplitude = 5.0f;
        [SerializeField] private float2 m_shakeFrequency = new float2(30.0f, 25.0f);
        [SerializeField] private float m_shakeDuration = 0.2f;

        //private CharacterBase m_target = null;
        private Hero m_target = null;

        // Hack-ish way to make sure we don't start shaking before the UI is fully initialized,
        // which usually take one frame because of Unity's layout system
        const int kFramesToWaitBeforeAllowingShake = 1;
        private int m_elapsedFrames = 0;
        private bool CanShake() => m_elapsedFrames >= kFramesToWaitBeforeAllowingShake;

        private void Start()
        {
            m_target = GameManager.Player;

            GameManager.Player.experienceChanged.AddListener(OnStatsChanged);
            
            m_elapsedFrames = 0;

            UpdateUI();
        }

        private void OnStatsChanged(int previous)
        {
            UpdateUI();
        }

        private void Update()
        {
            if (!CanShake())
            {
                ++m_elapsedFrames;
            }
        }

        private void UpdateUI()
        {
            float now = GameManager.Player.GetTotalExpRequirement(GameManager.Player.level);
            float max = GameManager.Player.GetTotalExpRequirement(GameManager.Player.level+1);

            float previousSliderValue = m_slider.value;

            m_slider.minValue = now;
            m_slider.maxValue = max;

            // 这个 value 并不是 0 到 1 而是 角色状态的值 0 和 max 在上面有设置
            // 取个整数 避免精力值有小数点
            now = math.floor(now);

            m_slider.value = GameManager.Player.experience;

            if (m_slider.value < previousSliderValue && CanShake() && m_shakeOnDecrease)
            {
                Shake();
            }

            m_nowExperiencelabel.text = StringFormatter.Format("{0}", now);
            m_nextExperiencelabel.text = StringFormatter.Format("{0}", max);
        }

        private void Shake()
        {
            TransformShaker.Shake(
                target: m_slider.transform,
                amplitude: m_shakeAmplitude,
                frequency: m_shakeFrequency,
                duration: m_shakeDuration
            );
        }
    }
}
