using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.GraphicsBuffer;

namespace Gyvr.Mythril2D
{
    public class NotificationSystem : AGameSystem
    {
        // 如果事件是统一定义在这的？那么我们有些放在其他地方自己定义的事件是不是也得放在这？
        [Header("Gameplay Events")]
        public UnityEvent<MonsterSheet> monsterKilled = new UnityEvent<MonsterSheet>();
        public UnityEvent<CharacterBase, DamageInputDescriptor> damageApplied = new UnityEvent<CharacterBase, DamageInputDescriptor>();
        public UnityEvent<CharacterBase, int> healthRecovered = new UnityEvent<CharacterBase, int>();
        public UnityEvent<CharacterBase, int> manaConsumed = new UnityEvent<CharacterBase, int>();
        public UnityEvent<CharacterBase, int> manaRecovered = new UnityEvent<CharacterBase, int>();
        public UnityEvent<Hero, float> staminaConsumed = new UnityEvent<Hero, float>();
        public UnityEvent<Hero, float> staminaRecovered = new UnityEvent<Hero, float>();
        public UnityEvent<int> experienceGained = new UnityEvent<int>();
        public UnityEvent<int> levelUp = new UnityEvent<int>();
        public UnityEvent<AIController, Transform> targetDetected = new UnityEvent<AIController, Transform>();
        public UnityEvent<int> moneyAdded = new UnityEvent<int>();
        public UnityEvent<int> moneyRemoved = new UnityEvent<int>();
        public UnityEvent<Item, int> itemAdded = new UnityEvent<Item, int>();
        public UnityEvent<Item, int> itemRemoved = new UnityEvent<Item, int>();
        public UnityEvent<Item> itemPlacedAtLocation = new UnityEvent<Item>();
        public UnityEvent<Equipment> itemEquipped = new UnityEvent<Equipment>();
        public UnityEvent<Equipment> itemUnequipped = new UnityEvent<Equipment>();
        public UnityEvent<AbilitySheet> abilityAdded = new UnityEvent<AbilitySheet>();
        public UnityEvent<AbilitySheet> abilityRemoved = new UnityEvent<AbilitySheet>();
        public UnityEvent<Quest> questProgressionUpdated = new UnityEvent<Quest>();
        public UnityEvent<Quest> questStarted = new UnityEvent<Quest>();
        public UnityEvent<Quest> questUnlocked = new UnityEvent<Quest>();
        public UnityEvent<Quest, bool> questAvailabilityChanged = new UnityEvent<Quest, bool>();
        public UnityEvent<Quest> questFullfilled = new UnityEvent<Quest>();
        public UnityEvent<Quest> questCompleted = new UnityEvent<Quest>();
        public UnityEvent<string, bool> gameFlagChanged = new UnityEvent<string, bool>();
        public UnityEvent mapTransitionStarted = new UnityEvent();
        public UnityEvent mapTransitionCompleted = new UnityEvent();
        public UnityEvent mapLoaded = new UnityEvent();
        public UnityEvent mapUnloaded = new UnityEvent();
        public UnityEvent saveStart = new UnityEvent();
        public UnityEvent saveEnd = new UnityEvent();
        public UnityEvent<Hero> playerSpawned = new UnityEvent<Hero>();
        public UnityEvent<Hero, Entity> playerTryInteracte = new UnityEvent<Hero, Entity>();
        public UnityEvent<Hero, Entity> playerEndInteracte = new UnityEvent<Hero, Entity>();
        public UnityEvent<string> SetActiveEvacuation = new UnityEvent<string>();

        public UnityEvent<Item> OnItemDiscarded = new UnityEvent<Item>();

        public UnityEvent<TeleportLoadingDelegationParams> mapTransitionDelegationRequested = new UnityEvent<TeleportLoadingDelegationParams>();

        [Header("User Interface")]
        public UnityEvent<Shop> shopRequested = new UnityEvent<Shop>();
        public UnityEvent<CraftingStation> craftRequested = new UnityEvent<CraftingStation>();
        public UnityEvent gameMenuRequested = new UnityEvent();
        public UnityEvent statsRequested = new UnityEvent();
        public UnityEvent warehouseRequested = new UnityEvent();
        public UnityEvent inventoryRequested = new UnityEvent();
        public UnityEvent journalRequested = new UnityEvent();
        public UnityEvent spellBookRequested = new UnityEvent();
        public UnityEvent settingsRequested = new UnityEvent();
        public UnityEvent saveMenuRequested = new UnityEvent();
        public UnityEvent deathScreenRequested = new UnityEvent();
        public UnityEvent<IUIMenu> menuShowed = new UnityEvent<IUIMenu>();
        public UnityEvent<IUIMenu> menuHid = new UnityEvent<IUIMenu>();
        public UnityEvent<Item> itemDetailsOpened = new UnityEvent<Item>();
        public UnityEvent itemDetailsClosed = new UnityEvent();

        [Header("Audio")]
        public UnityEvent<AudioClipResolver> audioPlaybackRequested = new UnityEvent<AudioClipResolver>();
        public UnityEvent<AudioClipResolver> audioStopPlaybackRequested = new UnityEvent<AudioClipResolver>();

        [Header("UI")]
        public UnityEvent<EItemCategory> UICategorySelected = new UnityEvent<EItemCategory>();
        public UnityEvent UIWarehouseClosed = new UnityEvent();


    }
}
