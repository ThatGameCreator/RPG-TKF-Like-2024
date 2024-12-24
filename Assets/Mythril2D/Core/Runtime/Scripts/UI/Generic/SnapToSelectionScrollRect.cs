using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    [RequireComponent(typeof(ScrollRect))]
    public class SnapToSelectionScrollRect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // Inspector Settings
        [SerializeField] private float m_snappingSpeed = 20.0f;

        private ScrollRect m_scrollRect = null;
        private GameObject m_selection = null;
        private Vector2 m_destination = Vector2.zero;

        private bool m_hovered = false;
        private float m_scrollFinishDelay = 0.01f; // 延迟时间
        private bool m_isScrolling = false;
        private float m_scrollTimer = 0f;

        public void OnPointerEnter(PointerEventData e)
        {
            m_hovered = true;
        }

        public void OnPointerExit(PointerEventData e)
        {
            m_hovered = false;
        }

        private void Awake()
        {
            m_scrollRect = GetComponent<ScrollRect>();
        }

        private GameObject GetSelectedChild()
        {
            GameObject selection = GameManager.EventSystem.currentSelectedGameObject;

            if (selection != null && selection.transform.IsChildOf(m_scrollRect.content.transform))
            {
                return selection;
            }

            return null;
        }

        private void Update()
        {
            GameObject selection = GetSelectedChild();

            // If the selection changed.
            if (selection != m_selection)
            {
                m_selection = selection;
                // If the new selected child is valid.
                if (m_selection)
                {
                    RectTransform selectionRectTransform = (RectTransform)m_selection.transform;
                    RectTransform contentRectTransform = m_scrollRect.content;
                    RectTransform viewportRectTransform = m_scrollRect.viewport;

                    // 计算选中项目在内容中的位置
                    float itemPositionInContent = -selectionRectTransform.anchoredPosition.y;

                    // 计算视口的高度和内容的高度
                    float viewportHeight = viewportRectTransform.rect.height;
                    float contentHeight = contentRectTransform.rect.height;
                    
                    // 计算滑动目标位置
                    float destinationY = 0;

                    // 如果选中项目完全在视口外
                    if (itemPositionInContent < 0 || itemPositionInContent > viewportHeight)
                    {
                        //Debug.Log("if (itemPositionInContent < 0 || itemPositionInContent > viewportHeight)");

                        // 将选中项目居中
                        destinationY = itemPositionInContent - (viewportHeight / 2) + (selectionRectTransform.rect.height / 2);
                    }

                    // 确保滑动目标在内容范围内
                    destinationY = Mathf.Clamp(destinationY, 0, Mathf.Max(0, contentHeight - viewportHeight));
                    
                    // If the ScrollRect is not hovered, update destination
                    if (!m_hovered)
                    {
                        m_destination.x = contentRectTransform.anchoredPosition.x;
                        m_destination.y = destinationY;

                        m_isScrolling = true; // 开始滚动标记
                        m_scrollTimer = 0f;  // 重置滚动计时器
                    }
                }
            }

            // If the ScrollRect is hovered, lock the destination to the current position
            if (m_hovered)
            {
                m_destination = m_scrollRect.content.anchoredPosition;
            }
            // If something is selected, move towards the destination.
            else if (m_selection)
            {
                m_scrollRect.content.anchoredPosition = Vector2.Lerp(
                    m_scrollRect.content.anchoredPosition,
                    m_destination,
                    Time.unscaledDeltaTime * m_snappingSpeed
                );

                // 检查滚动是否接近目标位置
                if (Vector2.Distance(m_scrollRect.content.anchoredPosition, m_destination) < 0.1f)
                {
                    m_scrollTimer += Time.unscaledDeltaTime; // 增加滚动计时器

                    // 如果滚动完成并达到延迟时间，触发指针更新事件
                    if (m_scrollTimer >= m_scrollFinishDelay)
                    {
                        m_isScrolling = false; // 滚动完成
                        OnScrollCompleted(); // 触发滚动完成事件
                    }
                }
                else
                {
                    m_scrollTimer = 0f; // 如果未接近目标位置，重置计时器
                }
            }
        }

        // 滚动完成事件逻辑
        private void OnScrollCompleted()
        {
            UINavigationCursorTarget selectionCursorTarget = m_selection?.GetComponent<UINavigationCursorTarget>();

            GameManager.NotificationSystem.MoveNavigationCursorAfterScrollRectSnap.Invoke(selectionCursorTarget, true);
        }
    }
}
