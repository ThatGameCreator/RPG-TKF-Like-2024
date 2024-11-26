using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace Gyvr.Mythril2D
{
    public class UIDeathMenuBtn : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private TextMeshProUGUI m_text = null;
        [SerializeField] private Image m_image = null;
        [SerializeField] private Button m_button = null;
        [SerializeField] private Color normalColor = new Color(200, 200, 200);
        [SerializeField] private Color highlightedColor = new Color(231, 200, 105);
        [SerializeField] private Color pressedColor = new Color(165, 143, 75);
        private float fadeDuration = 0.2f;

        private Coroutine imageAlphaCoroutine;
        private Coroutine colorCoroutine;

        public Button button => m_button;

        private void Awake()
        {
            // 在这初始化结果就没作用了不知道为什么
            // 因为方法用错了 Color 是 0 到 1 超过1全设为白色了
            normalColor = new Color32(200, 200, 200, 255);
            highlightedColor = new Color32(231, 200, 105, 255);
            pressedColor = new Color32(199, 105, 75, 255);

            if (m_button != null)
            {
                m_button.onClick.AddListener(OnClick);
            }
        }
        public void OnSelect(BaseEventData eventData)
        {
            // Start the fade to highlighted color
            StartColorTransition(highlightedColor);
            StartAlphaTransition(1f); // Image 渐入
        }

        public void OnDeselect(BaseEventData eventData)
        {
            // Start the fade to normal color
            StartColorTransition(normalColor);
            StartAlphaTransition(0f); // Image 渐出
        }

        private void StartColorTransition(Color targetColor)
        {
            if (colorCoroutine != null)
            {
                StopCoroutine(colorCoroutine);
            }

            colorCoroutine = StartCoroutine(FadeTextColor(targetColor));
        }

        private IEnumerator FadeTextColor(Color targetColor)
        {
            Color startColor = m_text.color;
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                m_text.color = Color.Lerp(startColor, targetColor, elapsedTime / fadeDuration);
                yield return null;
            }

            m_text.color = targetColor;
        }
        private void StartAlphaTransition(float targetAlpha)
        {
            if (imageAlphaCoroutine != null)
            {
                StopCoroutine(imageAlphaCoroutine);
            }

            imageAlphaCoroutine = StartCoroutine(FadeImageAlpha(targetAlpha));
        }

        private IEnumerator FadeImageAlpha(float targetAlpha)
        {
            if (m_image == null) yield break;

            Color startColor = m_image.color;
            Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                m_image.color = Color.Lerp(startColor, targetColor, elapsedTime / fadeDuration);
                yield return null;
            }

            m_image.color = targetColor;
        }


        public void OnClick()
        {
            // 没啥用 不晓得为什么
            StartColorTransition(pressedColor);
        }
    }
}
