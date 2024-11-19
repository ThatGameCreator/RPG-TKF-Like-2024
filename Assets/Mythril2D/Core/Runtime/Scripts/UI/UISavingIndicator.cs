using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UISavingIndicator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_spriteRenderer = null;
        [SerializeField] private Image m_image = null;
        [SerializeField] private TextMeshProUGUI m_text = null;
        [SerializeField] private float m_showAnimationSpeed = 1.0f;
        [SerializeField] private float m_hideAnimationSpeed = 1.0f;
        private Color m_indicatorSpriteColor;
        private Color m_textColor;

        // 原来不启用的话，无论是start还是awake都不会执行
        private void Start()
        {
            m_indicatorSpriteColor = m_image.color;
            m_textColor = m_text.color;

            GameManager.NotificationSystem.saveStart.AddListener(ActivateAndShowIndicator);

            gameObject.SetActive(false);  // 关闭对象
        }

        private void ActivateAndShowIndicator()
        {
            gameObject.SetActive(true);  // 启用对象
            StartCoroutine(FadeInOutCycle());  // 开始渐入动画
        }

        // 渐入渐出循环，执行3次渐入渐出
        private IEnumerator FadeInOutCycle()
        {
            for (int i = 0; i < 3; i++)  // 执行3次
            {
                // 渐入
                yield return StartCoroutine(FadeIn());

                // 渐出
                yield return StartCoroutine(FadeOut());
            }

            gameObject.SetActive(false);  // 完成后禁用该对象
        }

        // 渐入动画（使对象渐显）
        private IEnumerator FadeIn()
        {
            float elapsedTime = 0.0f;

            // 渐入过程
            while (elapsedTime < m_showAnimationSpeed)
            {
                float alpha = Mathf.Lerp(0.0f, 1.0f, elapsedTime / m_showAnimationSpeed);
                SetAlpha(alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            SetAlpha(1.0f);  // 确保最终达到完全不透明
        }


        // 你可以通过外部逻辑调用来触发渐出效果
        public void HideIndicator()
        {
            StartCoroutine(FadeOut());  // 开始渐出动画
        }

        // 渐出动画（使对象渐隐）
        private IEnumerator FadeOut()
        {
            float elapsedTime = 0.0f;

            // 渐出过程
            while (elapsedTime < m_hideAnimationSpeed)
            {
                float alpha = Mathf.Lerp(1.0f, 0.0f, elapsedTime / m_hideAnimationSpeed);
                SetAlpha(alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            SetAlpha(0.0f);  // 确保最终完全透明
        }

        // 设置Sprite和Text的透明度
        private void SetAlpha(float alpha)
        {
            m_image.color = new Color(m_indicatorSpriteColor.r, m_indicatorSpriteColor.g, m_indicatorSpriteColor.b, alpha);
            m_text.color = new Color(m_textColor.r, m_textColor.g, m_textColor.b, alpha);
        }
    }
}
