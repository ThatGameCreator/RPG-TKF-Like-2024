using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T m_instance;
        protected bool m_isAwaken = false;

        public static T Instance
        {
            get
            {
                return m_instance;
            }
        }

        public virtual void Awake()
        {
            if (m_instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                m_isAwaken = true;
                m_instance = (T)this;
                GameObject.DontDestroyOnLoad(gameObject);
            }
        }

        public virtual void OnDestroy()
        {
            if (m_instance == this)
            {
                m_instance = null;
            }
        }
    }
}