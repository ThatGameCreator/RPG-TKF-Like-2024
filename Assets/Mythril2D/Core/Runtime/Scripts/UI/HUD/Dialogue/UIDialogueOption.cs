using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIDialogueOption : MonoBehaviour
    {
        // Inspector Settings
        [SerializeField] private TextMeshProUGUI m_text = null;
        [SerializeField] private int m_optionID = 0;
        [SerializeField] private Button m_button = null;
        [SerializeField] private Image m_background = null;


        private void Awake()
        {
            Debug.Log("OnClicked");

            if (m_button == null)
            {
                Debug.Log("GetComponent");

                m_button = GetComponent<Button>();
            }
            m_button.onClick.AddListener(OnClicked);
        }

        public void OnClicked()
        {
            Debug.Log("OnClicked");

            SendMessageUpwards("OnOptionClicked", m_optionID);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
            m_background.gameObject.SetActive(visible);
        }

        public void SetText(string text)
        {
            m_text.text = text;
        }
    }
}
