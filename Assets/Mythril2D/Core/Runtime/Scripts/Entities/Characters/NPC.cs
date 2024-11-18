using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Gyvr.Mythril2D
{
    public class NPC : Character<NPCSheet>
    {
        [SerializeField] private UINPCIcon m_npcIcon = null;

        private void Update()
        {
            UpdateFieldOfWar();
        }

        private void Start()
        {
            GameManager.NotificationSystem.questUnlocked.AddListener(OnQuestStatusChanged);
            GameManager.NotificationSystem.questAvailabilityChanged.AddListener(OnQuestAvailabilityChanged);
            GameManager.NotificationSystem.questCompleted.AddListener(OnQuestStatusChanged);
            GameManager.NotificationSystem.questFullfilled.AddListener(OnQuestStatusChanged);
            GameManager.NotificationSystem.questProgressionUpdated.AddListener(OnQuestStatusChanged);
            GameManager.NotificationSystem.questStarted.AddListener(OnQuestStatusChanged);
            
            UpdateIndicator();
        }

        private void OnDestroy()
        {
            GameManager.NotificationSystem.questUnlocked.RemoveListener(OnQuestStatusChanged);
            GameManager.NotificationSystem.questAvailabilityChanged.AddListener(OnQuestAvailabilityChanged);
            GameManager.NotificationSystem.questCompleted.RemoveListener(OnQuestStatusChanged);
            GameManager.NotificationSystem.questFullfilled.RemoveListener(OnQuestStatusChanged);
            GameManager.NotificationSystem.questProgressionUpdated.RemoveListener(OnQuestStatusChanged);
            GameManager.NotificationSystem.questStarted.RemoveListener(OnQuestStatusChanged);
        }

        private void OnQuestStatusChanged(Quest quest) => UpdateIndicator();
        private void OnQuestAvailabilityChanged(Quest quest, bool available) => UpdateIndicator();

        private void UpdateIndicator()
        {
            if (GameManager.JournalSystem.GetQuestToComplete(this))
            {
                SetIconType(UINPCIcon.EIconType.QuestCompleted);
            }
            else if (GameManager.JournalSystem.GetQuestToStart(this))
            {
                SetIconType(UINPCIcon.EIconType.QuestAvailable);
            }
            else if (GameManager.JournalSystem.GetNonFullfilledQuestToReportTo(this) != null)
            {
                SetIconType(UINPCIcon.EIconType.QuestInProgress);
            }
            else
            {
                SetIconType(UINPCIcon.EIconType.None);
            }
        }

        public override string GetSpeakerName() => characterSheet.displayName;

        public override void OnInteract(CharacterBase sender)
        {
            SetLookAtDirection(sender.transform);
            base.OnInteract(sender);
        }

        public void SetIconType(UINPCIcon.EIconType iconType)
        {
            if (m_npcIcon != null)
            {
                m_npcIcon.SetIconType(iconType);
            }
        }
    }
}
