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

                    Debug.Log(itemPositionInContent);

                    // 计算视口的高度和内容的高度
                    float viewportHeight = viewportRectTransform.rect.height;
                    float contentHeight = contentRectTransform.rect.height;
                    
                    // 计算滑动目标位置
                    float destinationY = 0;

                    // 如果选中项目完全在视口外
                    if (itemPositionInContent < 0 || itemPositionInContent > viewportHeight)
                    {
                        Debug.Log("if (itemPositionInContent < 0 || itemPositionInContent > viewportHeight)");

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
            }
        }
    }
}
