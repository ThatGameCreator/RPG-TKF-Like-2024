using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIStatBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI m_label = null;
        [SerializeField] private Slider m_slider = null;
        [SerializeField] private TextMeshProUGUI m_sliderText = null;

        [Header("General Settings")]
        [SerializeField] private bool useStamina = false;
        [SerializeField] private EStat m_stat;

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

            // 如果我这样写 target 却是 CharacterBase 会不会怪物的 UI Bar 更新的时候就会报错
            // 这获取的是 player 那么应该只对应玩家
            m_target.maxStatsChanged.AddListener(OnStatsChanged);
            m_target.currentStatsChanged.AddListener(OnStatsChanged);

            // 基本属性写在了 Base 基类 精力属性只写在了 Hero 子类
            m_target.maxStaminaChanged.AddListener(OnStaminaChanged);
            m_target.currentStaminaChanged.AddListener(OnStaminaChanged);

            m_elapsedFrames = 0;

            UpdateUI();
        }

        // 原来没有接受 float 参数的方法 所以 invoke 调用的时候并没有对应的方法来接受参数来监听对象
        private void OnStaminaChanged(float previousStamina)
        {
            UpdateUI();
        }

        private void OnStatsChanged(Stats previous)
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
            float current, max; current = max = 0;

            if (useStamina && m_stat == EStat.Stamina)
            {
                current = GameManager.Player.GetStamina();
                max = GameManager.Player.maxStamina;
                m_label.text = "Stamina";
            }
            else
            {
                current = m_target.currentStats[m_stat];
                max = m_target.stats[m_stat];
                m_label.text = GameManager.Config.GetTermDefinition(m_stat).shortName;
            }

            float previousSliderValue = m_slider.value;

            m_slider.minValue = 0;
            m_slider.maxValue = max;

            // 取个整数 避免精力值有小数点
            current = math.floor(current);

            m_slider.value = current;

            if (m_slider.value < previousSliderValue && CanShake() && m_shakeOnDecrease)
            {
                Shake();
            }

            m_sliderText.text = StringFormatter.Format("{0}/{1}", current, max);
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
