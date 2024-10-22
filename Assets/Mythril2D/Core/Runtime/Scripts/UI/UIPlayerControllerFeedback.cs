using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class UIPlayerControllerFeedback : MonoBehaviour
    {
        [SerializeField] private UIControllerButton m_interactionButtonFeedback = null;
        [SerializeField] private UIInteractionBar m_interactionBarFeedback = null;
        [SerializeField] private SpriteRenderer m_ButtonSpriteRenderer = null;
        [SerializeField] private Vector3 m_offset = Vector3.up;
        [SerializeField] private float m_showAnimationSpeed = 20.0f;
        [SerializeField] private float m_hideAnimationSpeed = 20.0f;

        private Color m_initialButtonSpriteColor = Color.white;
        private Color m_initialBarSpriteColor = Color.white;

        private PlayerController m_playerController = null;

        private void Start()
        {
            m_playerController = GameManager.PlayerSystem.PlayerInstance.GetComponent<PlayerController>();
            m_initialButtonSpriteColor = m_ButtonSpriteRenderer.color;
        }

        private void Update()
        {
            GameObject target = m_playerController.interactionTarget;

            if (target)
            {
                if (!m_interactionButtonFeedback.isActiveAndEnabled)
                {
                    m_interactionButtonFeedback.gameObject.SetActive(true);
                }

                m_interactionButtonFeedback.transform.position = target.transform.position + m_offset;
                m_ButtonSpriteRenderer.color = Color.Lerp(m_ButtonSpriteRenderer.color, m_initialButtonSpriteColor, m_showAnimationSpeed * Time.unscaledDeltaTime);

                Hero hero = GameManager.Player;
                if (hero.isLooting)
                {
                    if (!m_interactionBarFeedback.isActiveAndEnabled)
                    {
                        m_interactionBarFeedback.gameObject.SetActive(true);
                    }

                    m_interactionBarFeedback.transform.position = target.transform.position + m_offset;
                    //m_BarSpriteRenderer.color = Color.Lerp(m_BarSpriteRenderer.color, m_initialBarSpriteColor, m_showAnimationSpeed * Time.unscaledDeltaTime);
                }
                else
                {
                    m_interactionBarFeedback.gameObject.SetActive(false);
                }
            }
            else
            {
                m_ButtonSpriteRenderer.color = Color.Lerp(m_ButtonSpriteRenderer.color, new Color(1.0f, 1.0f, 1.0f, 0.0f), m_hideAnimationSpeed * Time.unscaledDeltaTime);
                m_interactionBarFeedback.gameObject.SetActive(false);
            }
        }
    }
}
