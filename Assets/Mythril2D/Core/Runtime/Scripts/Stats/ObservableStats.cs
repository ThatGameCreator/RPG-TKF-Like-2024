using UnityEngine.Events;
using Unity.Mathematics;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class ObservableStats
    {
        public Stats stats => m_stats;
        public float stamina => m_stamina;

        public UnityEvent<Stats> changed => m_changed;
        public UnityEvent<float> staminaChanged => m_staminaChanged;

        private Stats m_stats;
        private float m_stamina;

        private UnityEvent<Stats> m_changed = new UnityEvent<Stats>();
        private UnityEvent<float> m_staminaChanged = new UnityEvent<float>();


        public ObservableStats() : this(new Stats())
        {
            m_stamina = stamina;
        }

        public ObservableStats(float stamina)
        {
            m_stamina = stamina;
        }

        public ObservableStats(Stats stats)
        {
            m_stats = stats;
        }

        public float Stamina
        {
            get => m_stamina;
            set
            {
                // 如果值有变化，则触发事件
                //if (Mathf.Abs(m_stamina - value) > Mathf.Epsilon)
                //{
                //    float previousStamina = m_stamina;
                //    m_stamina = value;
                //    m_staminaChanged.Invoke(previousStamina); // 触发事件，传递旧的 stamina 值
                //    //Debug.Log("m_stamina set" + previousStamina); //active per frame
                //}

                // value 是传入的新值
                float previousStamina = m_stamina;
                m_stamina = value;
                m_staminaChanged.Invoke(previousStamina); // 触发事件，传递旧的 stamina 值
            }
        }

        public int this[EStat stat]
        {
            get => m_stats[stat];
            set
            {
                Stats previous = new Stats(m_stats);
                m_stats[stat] = value;
                m_changed.Invoke(previous);
            }
        }

        public void Set(float stamina)
        {
            // Mathf.Epsilon 是浮点 0 因为初始值为 0 所以跳过了执行
            //if (Mathf.Abs(m_stamina - 0f) > Mathf.Epsilon)

            float previousStamina = m_stamina;
            //m_stamina = 0f; 一直设为 0 没有用方法传入的新值
            m_stamina = stamina;
            m_staminaChanged.Invoke(previousStamina); 

        }

        public void Set(Stats stats)
        {
            Stats previous = new Stats(m_stats);
            m_stats = new Stats(stats);
            m_changed.Invoke(previous);
        }
    }
}
