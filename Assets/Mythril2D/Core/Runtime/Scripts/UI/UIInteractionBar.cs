using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIInteractionBar : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_spriteRenderer = null;
        [SerializeField] private UIControllerButtonManager.EAction m_action;
        [SerializeField] private Slider m_slider = null;

        [Header("Visual Settings")]
        [SerializeField] private bool m_shakeOnDecrease = false;
        [SerializeField] private float m_shakeAmplitude = 5.0f;
        [SerializeField] private float2 m_shakeFrequency = new float2(30.0f, 25.0f);
        [SerializeField] private float m_shakeDuration = 0.2f;


        // Hack-ish way to make sure we don't start shaking before the UI is fully initialized,
        // which usually take one frame because of Unity's layout system
        const int kFramesToWaitBeforeAllowingShake = 1;
        private int m_elapsedFrames = 0;
        private bool CanShake() => m_elapsedFrames >= kFramesToWaitBeforeAllowingShake;

        private void Update()
        {
            float current, max; current = max = 0;

            current = GameManager.Player.lootingTime;
            max = GameManager.Player.lootingRequiredtTime;

            float previousSliderValue = m_slider.value;

            m_slider.minValue = 0;
            m_slider.maxValue = max;

            m_slider.value = current;

            if (m_slider.value < previousSliderValue && CanShake() && m_shakeOnDecrease)
            {
                Shake();
            }
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
