using UnityEngine;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UINavigationCursor : MonoBehaviour
    {
        [SerializeField] private Image m_image = null;
        [SerializeField] private RectTransform m_rectTransform = null;

        private UINavigationCursorTarget m_target = null;

        private Vector3 GetTargetPosition()
        {
            return m_target.transform.position + m_target.totalPositionOffset;
        }

        private Vector2 GetTargetSize()
        {
            return ((RectTransform)m_target.transform).sizeDelta + m_target.totalSizeOffset;
        }

        private void OnTargetDestroyed()
        {
            SetTarget(null);
        }

        private void SetTarget(UINavigationCursorTarget currentTarget)
        {
            UINavigationCursorTarget previousTarget = m_target;

            // If target changed
            if (currentTarget != previousTarget)
            {
                if (previousTarget != null)
                {
                    previousTarget.destroyed.RemoveListener(OnTargetDestroyed);
                }

                m_target = currentTarget;


                // New valid target found
                if (m_target != null)
                {
                    m_target.destroyed.AddListener(OnTargetDestroyed);

                    ((RectTransform)transform).sizeDelta = GetTargetSize();
                    transform.position = GetTargetPosition();

                    m_image.enabled = true;
                    // 匹配slot格子 长宽 64 + 边缘 4 的长宽
                    m_rectTransform.sizeDelta = new Vector2(68, 68);
                    m_image.sprite = m_target.navigationCursorStyle.sprite;
                    m_image.color = m_target.navigationCursorStyle.color;
                }
                else
                // No target
                {
                    m_image.enabled = false;
                }
            }
        }

        private void Update()
        {
            UINavigationCursorTarget currentTarget = null;
            
            if (GameManager.EventSystem.currentSelectedGameObject != null)
            {
                currentTarget = GameManager.EventSystem.currentSelectedGameObject?.GetComponent<UINavigationCursorTarget>();
            }

            // Ignore disabled targets
            if (currentTarget == null || !currentTarget.isActiveAndEnabled)
            {
                currentTarget = null;
            }

            SetTarget(currentTarget);
        }
    }
}
