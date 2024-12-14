using UnityEngine;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UINavigationCursor : MonoBehaviour
    {
        [SerializeField] private Image m_image = null;
        [SerializeField] private RectTransform m_rectTransform = null;

        private UINavigationCursorTarget m_target = null;

        private void Start()
        {
            GameManager.NotificationSystem.MoveNavigationCursorAfterScrollRectSnap.AddListener(SetTarget);
        }

        private void OnDestroy()
        {
            GameManager.NotificationSystem.MoveNavigationCursorAfterScrollRectSnap.RemoveListener(SetTarget);

        }

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

        private void SetTarget(UINavigationCursorTarget currentTarget, bool isNeedUpdate = false)
        {
            if (currentTarget == m_target && isNeedUpdate == false) return;

            if (m_target != null)
            {
                m_target.destroyed.RemoveListener(OnTargetDestroyed);
            }

            m_target = currentTarget;

            if (m_target != null)
            {
                m_target.destroyed.AddListener(OnTargetDestroyed);
                UpdateCursorAppearance();
            }
            else
            {
                m_image.enabled = false;
            }
        }

        private void UpdateCursorAppearance()
        {
            ((RectTransform)transform).sizeDelta = GetTargetSize();
            transform.position = GetTargetPosition();

            m_image.enabled = true;
            m_rectTransform.sizeDelta = new Vector2(68, 68);
            m_image.sprite = m_target.navigationCursorStyle.sprite;
            m_image.color = m_target.navigationCursorStyle.color;
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
