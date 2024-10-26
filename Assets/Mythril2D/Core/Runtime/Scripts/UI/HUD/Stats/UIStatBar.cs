using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using static Codice.CM.Common.CmCallContext;

namespace Gyvr.Mythril2D
{
    public class UIStatBar : MonoBehaviour
    {
        [Header("References")]
        //[SerializeField] private TextMeshProUGUI m_label = null;
        [SerializeField] private Slider m_slider = null;
        // [实际血量]-瞬间掉血,  Slider组件里自带的那个Fill
        [SerializeField] private RectTransform m_topFillRect;
        // [缓动血量]-缓慢掉血,  自己复制出来的Fill_1
        [SerializeField] private RectTransform m_midtFillRect;
        [SerializeField] private TextMeshProUGUI m_sliderText = null;
        [SerializeField] private float m_aniamtionSpeed = 0.001f; // 掉血速度

        [Header("General Settings")]
        [SerializeField] private bool useStamina = false;
        [SerializeField] private EStat m_stat;

        [Header("Visual Settings")]
        [SerializeField] private bool m_shakeOnDecrease = false;
        [SerializeField] private float m_shakeAmplitude = 5.0f;
        [SerializeField] private float2 m_shakeFrequency = new float2(30.0f, 25.0f);
        [SerializeField] private float m_shakeDuration = 0.2f;


        private bool isStartSettingEnd = false;     // 初始设置
        private bool isTurnMidAnimationOn = false;  // 是否可以开始掉血
        private bool isStillChange = false; 
        private float nowWaitingTime = 0f;
        private float shouldWaitingTime = 1f;
        private float statTotalChangedValue = 0f;
        private float statStartChangedValue = 0f;     // 缓慢掉血的血条-缓动起点
        private float statEndChangedValue = 0f;       // 缓慢掉血的血条-缓动终点

        //private CharacterBase m_target = null;
        // 可能是因为我们把怪物的血条关了？所以现在这个基类没有报错
        // 如果要开启怪物血量，可能得把这个类型给改回来
        private Hero m_target = null;

        // Hack-ish way to make sure we don't start shaking before the UI is fully initialized,
        // which usually take one frame because of Unity's layout system
        const int kFramesToWaitBeforeAllowingShake = 1;
        private int m_elapsedFrames = 0;
        private bool CanShake() => m_elapsedFrames >= kFramesToWaitBeforeAllowingShake;

        private void Start()
        {
            m_target = GameManager.Player;

            // 分开监听
            if(useStamina == false)
            {
                // 如果我这样写 target 却是 CharacterBase 会不会怪物的 UI Bar 更新的时候就会报错
                // 这获取的是 player 那么应该只对应玩家
                m_target.maxStatsChanged.AddListener(OnStatsChanged);
                m_target.currentStatsChanged.AddListener(OnStatsChanged);

            }
            else
            {
                // 基本属性写在了 Base 基类 精力属性只写在了 Hero 子类
                m_target.maxStaminaChanged.AddListener(OnStaminaChanged);
                m_target.currentStaminaChanged.AddListener(OnStaminaChanged);
            }
            
            m_elapsedFrames = 0;;

            UpdateUI();
        }

        // 原来没有接受 float 参数的方法 所以 invoke 调用的时候并没有对应的方法来接受参数来监听对象
        private void OnStaminaChanged(float previousStamina)
        {
            UpdateUI();
        }

        private void OnStatsChanged(Stats previous)
        {
            // 感觉这个监听似乎也有问题，把精力值和其他状态都堆到一起写的结果就是
            // 每次精力值被调用的时候，就会把其他状态也都记录和判断一遍
            // 既精力变化和状态变化都会执行同样的方法
            // 要么得把 Stats 放在一个基类里面，要么就得分开写，不然也太别扭了
            UpdateUI();
        }

        private void Update()
        {
            if (!CanShake())
            {
                ++m_elapsedFrames;
            }

            // 启动过渡动画
            if (isTurnMidAnimationOn)
            {
                nowWaitingTime -= Time.deltaTime;

                // 如果仍然有变化 得先停一会 不执行后面函数
                if (isStillChange == true)
                {
                    nowWaitingTime = shouldWaitingTime;

                    isStillChange = false;
                }

                if (nowWaitingTime < 0f)
                {
                    // 还以为为什么用局部变量跟上面的speed实际变化速度不一样
                    // 原来是因为编辑器改了那个值 所以实际上并不是 0.001f
                    statTotalChangedValue -= m_aniamtionSpeed;

                    // 锚点赋值 y 是顶部滑动条的值
                    m_midtFillRect.anchorMax = new Vector2(m_midtFillRect.anchorMax.x - m_aniamtionSpeed, m_topFillRect.anchorMax.y);

                    // 如果一直不满会无法回复中间的条
                    // 如果回复的速度比等待速度快，那么先退到 now 和 end 一样，就会导致虚血在中间
                    if (statTotalChangedValue <= 0f || Mathf.Approximately(1.0f, statStartChangedValue / statEndChangedValue))
                    {
                        isTurnMidAnimationOn = false;   // 关闭过渡效果
                    }
                }
            }
        }

        private void UpdateUI()
        {
            float current, max; current = max = 0;

            if (useStamina)
            {
                current = GameManager.Player.GetStamina();
                max = GameManager.Player.maxStamina;
                //m_label.text = "Stamina";
            }
            else
            {
                current = m_target.currentStats[m_stat];
                max = m_target.stats[m_stat];
                //m_label.text = GameManager.Config.GetTermDefinition(m_stat).shortName;
            }

            float previousSliderValue = m_slider.value;

            m_slider.minValue = 0;
            m_slider.maxValue = max;

            // 这个 value 并不是 0 到 1 而是 角色状态的值 0 和 max 在上面有设置
            // 取个整数 避免精力值有小数点
            current = math.floor(current);

            m_slider.value = current;

            ChangeMiddleBar(current, previousSliderValue);

            // 得让顶层更新后再让中间的更新 不然在 start 时候更新的话， 还没有获取角色状态 只有一半
            if(isStartSettingEnd == false)
            {
                isStartSettingEnd = true;

                // 确保[实际血量]显示在最上面(对应在Hierarchy同级的最下面)
                m_topFillRect.SetAsLastSibling();

                // 初始的时候让[实际血量]和[缓动血量]一致
                m_midtFillRect.anchorMax = m_topFillRect.anchorMax;
            }

            if (m_slider.value < previousSliderValue && CanShake() && m_shakeOnDecrease)
            {
                Shake();
            }

            m_sliderText.text = StringFormatter.Format("{0} / {1}", current, max);
        }

        private float StatsValueToAnchor(float convertValue)
        {
            return convertValue / m_slider.maxValue;
        }

        // 启动减血效果(此时Slider的value已经变化过了, [实际血量]已经变化)
        public void ChangeMiddleBar(float current, float previous)
        {
            if (current < previous) {
                statStartChangedValue = StatsValueToAnchor(previous);

                statEndChangedValue = StatsValueToAnchor(current);

                statTotalChangedValue += statStartChangedValue - statEndChangedValue;

                isStillChange = isTurnMidAnimationOn = true;
            }

            // 增加且动画关闭的时候
            // 问题应该是动画并没有关闭 但是增加了
            // 如果是恢复数值 并且当前的虚血小于实际血量则直接赋值
            if (current > previous && m_midtFillRect.anchorMax.x < m_topFillRect.anchorMax.x)
            {
                statTotalChangedValue = 0f;

                statEndChangedValue = statStartChangedValue = StatsValueToAnchor(m_slider.value);

                m_midtFillRect.anchorMax = m_topFillRect.anchorMax;
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
