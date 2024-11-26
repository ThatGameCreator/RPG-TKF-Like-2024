using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public class PlaceItemTaskProgressDataBlock : QuestTaskProgressDataBlock
    {
        public int itemPlacedNumber;

        public override IQuestTaskProgress CreateInstance() => new PlaceItemTaskProgress(this);
    }

    public class PlaceItemTaskProgress : QuestTaskProgress<PlaceItemTaskProgressDataBlock>
    {
        public int itemPlacedNumber { get; private set; } = 0;

        private PlaceItemTask m_placeItemTask => (PlaceItemTask)m_task;

        public PlaceItemTaskProgress(PlaceItemTask task) : base(task) { }

        public PlaceItemTaskProgress(PlaceItemTaskProgressDataBlock block) : base(block) { }

        public override void OnProgressTrackingStarted()
        {
            GameManager.NotificationSystem.itemPlacedAtLocation.AddListener(OnItemPlacedAtLocation);
        }

        public override void OnProgressTrackingStopped()
        {
            GameManager.NotificationSystem.itemPlacedAtLocation.RemoveListener(OnItemPlacedAtLocation);
        }

        public override bool IsCompleted()
        {
            return itemPlacedNumber >= m_placeItemTask.itemPlacedNumber;
        }

        private void OnItemPlacedAtLocation(Item questObject)
        {
            if (questObject == m_placeItemTask.itemPlaced)
            {
                itemPlacedNumber++;
                UpdateProgression();
            }
        }

        public override PlaceItemTaskProgressDataBlock CreateDataBlock()
        {
            PlaceItemTaskProgressDataBlock block = base.CreateDataBlock();
            block.itemPlacedNumber = itemPlacedNumber;
            return block;
        }

        public override void LoadDataBlock(PlaceItemTaskProgressDataBlock block)
        {
            base.LoadDataBlock(block);
            itemPlacedNumber = block.itemPlacedNumber;
        }
    }

    [CreateAssetMenu(menuName = AssetMenuIndexer.Mythril2D_Quests_Tasks + nameof(PlaceItemTask))]
    public class PlaceItemTask : QuestTask
    {
        public Item itemPlaced = null;
        public int itemPlacedNumber = 1;


        public PlaceItemTask()
        {
            m_title = "Place the item at {0}";
        }

        public override IQuestTaskProgress CreateTaskProgress() => new PlaceItemTaskProgress(this);

        public override string GetCompletedTitle()
        {
            return StringFormatter.Format(m_title, itemPlaced.DisplayName);
        }

        public override string GetInProgressTitle(IQuestTaskProgress progress)
        {
            return StringFormatter.Format(m_title, itemPlaced.DisplayName);
        }
    }
}