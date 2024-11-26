using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public class ItemTaskProgressDataBlock : QuestTaskProgressDataBlock
    {
        public override IQuestTaskProgress CreateInstance() => new ItemTaskProgress(this);
    }

    public class ItemTaskProgress : QuestTaskProgress<ItemTaskProgressDataBlock>
    {
        public int currentQuantity { get; private set; } = 0;

        private ItemTask m_itemTask => (ItemTask)m_task;

        public ItemTaskProgress(ItemTask task) : base(task) {}

        public ItemTaskProgress(ItemTaskProgressDataBlock block) : base(block) {}

        public override void OnProgressTrackingStarted()
        {
            GameManager.NotificationSystem.itemAdded.AddListener(OnItemAdded);
        }

        public override void OnProgressTrackingStopped()
        {
            GameManager.NotificationSystem.itemAdded.RemoveListener(OnItemAdded);
        }

        public override bool IsCompleted()
        {
            return currentQuantity >= m_itemTask.amountToCollect;
        }

        protected override void Update()
        {
            int quantityInInventory = GameManager.InventorySystem.GetItemCount(m_itemTask.item);

            if (quantityInInventory != currentQuantity)
            {
                currentQuantity = GameManager.InventorySystem.GetItemCount(m_itemTask.item);
                UpdateProgression();
            }
        }

        private void OnItemAdded(Item item, int quantity)
        {
            Update();
        }
    }

    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Quests_Tasks + nameof(ItemTask))]
    public class ItemTask : QuestTask
    {
        public Item item = null;
        public int amountToCollect = 1;

        public ItemTask()
        {
            m_title = "Acquire {0} ({1}/{2})";
        }

        public override IQuestTaskProgress CreateTaskProgress() => new ItemTaskProgress(this);

        public override string GetCompletedTitle()
        {
            return StringFormatter.Format(m_title, item.DisplayName, amountToCollect, amountToCollect);
        }

        public override string GetInProgressTitle(IQuestTaskProgress progress)
        {
            return StringFormatter.Format(m_title, item.DisplayName, ((ItemTaskProgress)progress).currentQuantity, amountToCollect);
        }
    }
}