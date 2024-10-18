using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    enum SaveFileActionType
    {
        Save,
        Load
    }

    struct SaveFileActionDesc
    {
        public SaveFileActionType action;
        public string filename;
    }

    public class UISaveFile : MonoBehaviour, IPointerEnterHandler
    {
        [Header("Settings")]
        [SerializeField] private SaveFileActionType m_action = SaveFileActionType.Load;
        [SerializeField] private string m_saveFileName = null;

        [Header("References")]
        [SerializeField] private TextMeshProUGUI m_details = null;
        [SerializeField] private Button m_button = null;
        [SerializeField] private Button m_cancelButton = null;
        [SerializeField] private UIMainMenu m_UIMainMenu = null;
        [SerializeField] private SaveFile m_saveFile = null;


        public string saveFileName => m_saveFileName;
        public Button button => m_button;
        public bool isEmpty => m_isEmpty;

        private bool m_isEmpty;

        private void Awake()
        {
            m_button.onClick.AddListener(OnClick);
        }

        public void UpdateUI()
        {
            SaveFileData saveFile;

            // Each button try to check their Save Data
            // The button will show the Character name of Save data
            if (SaveSystem.TryExtractingSaveData(m_saveFileName, out saveFile))
            {
                m_details.text = saveFile.header;
                m_isEmpty = false;
            }
            // 
            else
            {
                m_details.text = "New Game";
                m_isEmpty = true;
                m_cancelButton.gameObject.SetActive(false);
                //if (m_action == SaveFileActionType.Load)
                //{
                //    m_button.interactable = false;
                //}
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_button.Select();
        }

        public void OnClick()
        {
            // 没有存档尝试新建
            if (m_isEmpty == true)
            {
                m_UIMainMenu.StartNewGameFromDefaultSaveFile(m_saveFile);
            }
            else
            {
                SendMessageUpwards("OnSaveFileClicked", new SaveFileActionDesc
                {
                    action = m_action,
                    filename = m_saveFileName

                }, SendMessageOptions.RequireReceiver);
            }
        }
    }
}
