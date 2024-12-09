using System;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public struct SaveFileData
    {
        public string header;
        public string map;
        public JournalDataBlock journal;
        public WarehouseDataBlock warehouse;
        public InventoryDataBlock inventory;
        public LocalizationDataBlock localization;
        public GameFlagsDataBlock gameFlags;
        public PlayerDataBlock player;
    }
}
