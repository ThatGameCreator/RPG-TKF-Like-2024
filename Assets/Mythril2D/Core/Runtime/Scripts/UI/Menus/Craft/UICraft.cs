using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UICraft : MonoBehaviour, IUIMenu
    {
        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_craftAudio;

        [Header("Dialogues")]
        [SerializeField] private DialogueSequence m_cannotUseItem = null;
        [SerializeField] private DialogueSequence m_cannotCraftBecauseIngredients = null;
        [SerializeField] private DialogueSequence m_cannotCraftBecauseMoney = null;
        [SerializeField] private DialogueSequence m_craftSucceeded = null;

        [Header("References")]
        [SerializeField] private UIInventoryBag m_inventoryBag = null;
        [SerializeField] private GameObject m_recipeEntryPrefab = null;
        [SerializeField] private GameObject m_recipeEntriesRoot = null;
        [SerializeField] private TextMeshProUGUI m_money = null;
        [SerializeField] private GameObject m_noRecipeSelectedIndicator = null;
        [SerializeField] private InstancePool m_ingredientListInstancePool = null;

        private UIRecipeEntry[] m_entries = null;
        private CraftingStation m_craftingStation = null;

        public void Init()
        {
            m_inventoryBag.UpdateSlots();
        }

        public void Show(params object[] args)
        {
            Debug.Assert(args.Length == 1 && args[0] is CraftingStation, "Craft panel invoked with incorrect arguments");

            m_craftingStation = (CraftingStation)args[0];
            gameObject.SetActive(true);

            UpdateUI();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void EnableInteractions(bool enable)
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup)
            {
                canvasGroup.interactable = enable;
            }
        }

        public GameObject FindSomethingToSelect()
        {
            if (m_entries.Length > 0)
            {
                return m_entries[0].GetComponentInChildren<Button>().gameObject;
            }

            UINavigationCursorTarget bagNavigationTarget = m_inventoryBag.FindNavigationTarget();

            if (bagNavigationTarget && bagNavigationTarget.gameObject.activeInHierarchy)
            {
                return bagNavigationTarget.gameObject;
            }

            return null;
        }

        // Message called by children using SendMessageUpward when the bag or equipment changed
        private void UpdateUI(Recipe selectedRecipe = null, bool skipItemSlots = false)
        {
            m_inventoryBag.UpdateSlots();

            if (!skipItemSlots)
            {
                ClearEntries();
                FillEntries();
                RewireNavigation();
            }

            foreach (UIRecipeEntry entry in m_entries)
            {
                entry.UpdateUI();
            }

            UpdatePlayerMoneyDisplay(selectedRecipe);
            UpdateRecipeDetails(selectedRecipe);
        }

        private void UpdateRecipeDetails(Recipe selectedRecipe)
        {
            m_ingredientListInstancePool.DisableAll();

            m_noRecipeSelectedIndicator.SetActive(!selectedRecipe);

            if (selectedRecipe)
            {
                // TODO make sure all the requirements are valid
                foreach (var requirement in selectedRecipe.ingredients)
                {
                    GameObject instance = m_ingredientListInstancePool.GetAvailableInstance();
                    UIIngredientEntry ingredientEntry = instance.GetComponent<UIIngredientEntry>();
                    ingredientEntry.Initialize(requirement.Key, requirement.Value);
                    instance.SetActive(true);
                }
            }
        }

        private void UpdatePlayerMoneyDisplay(Recipe selectedRecipe)
        {
            int selectedRecipeCost = 0;

            if (selectedRecipe)
            {
                selectedRecipeCost = m_craftingStation.GetCraftingCost(selectedRecipe);
            }
            
            if (selectedRecipeCost == 0)
            {
                m_money.text = GameManager.InventorySystem.backpackMoney.ToString();
            }
            else
            {
                m_money.text = string.Format("{0}\n({1}{2})",
                    GameManager.InventorySystem.backpackMoney,
                    selectedRecipeCost > 0 ? "-" : "+",
                    math.abs(selectedRecipeCost));
            }
        }

        private void FillEntries()
        {
            int recipeCount = m_craftingStation.recipes.Length;
            m_entries = new UIRecipeEntry[recipeCount];

            for (int i = 0; i < recipeCount; ++i)
            {
                Recipe recipe = m_craftingStation.recipes[i];

                GameObject recipeEntryInstance = Instantiate(m_recipeEntryPrefab, m_recipeEntriesRoot.transform);
                UIRecipeEntry recipeEntry = recipeEntryInstance.GetComponent<UIRecipeEntry>();

                if (recipeEntry)
                {
                    recipeEntry.Initialize(recipe, m_craftingStation);
                    m_entries[i] = recipeEntry;
                }
            }
        }

        private void ClearEntries()
        {
            for (int i = 0; i < m_recipeEntriesRoot.transform.childCount; ++i)
            {
                Transform child = m_recipeEntriesRoot.transform.GetChild(i);
                Destroy(child.gameObject);
            }
        }

        private void OnRecipeEntrySelected(Recipe recipe) => UpdateUI(recipe, true);

        private void OnRecipeEntryDeselected(Recipe recipe) => UpdateUI(recipe, true);

        private void OnRecipeEntryClicked(Recipe recipe)
        {
            if (m_craftingStation.CanCraft(recipe, out bool hasMoney, out bool hasIngredients))
            {
                m_craftingStation.Craft(recipe);
                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_craftAudio);
                UpdateUI(recipe, true);
                GameManager.DialogueSystem.Main.PlayNow(DialogueUtils.CreateDialogueTree(m_craftSucceeded, null, recipe.displayName));
            }
            else if (!hasIngredients)
            {
                GameManager.DialogueSystem.Main.PlayNow(DialogueUtils.CreateDialogueTree(m_cannotCraftBecauseIngredients, null, recipe.displayName));
            }
            else if (!hasMoney)
            {
                GameManager.DialogueSystem.Main.PlayNow(DialogueUtils.CreateDialogueTree(m_cannotCraftBecauseMoney, null));
            }
        }

        private void OnBagItemClicked(Item item)
        {
            GameManager.DialogueSystem.Main.PlayNow(DialogueUtils.CreateDialogueTree(m_cannotUseItem, null, item.displayName));
        }

        private void RewireNavigation()
        {
            UIInventoryBagSlot firstBagSlot = m_inventoryBag.GetFirstSlot();

            for (int i = 0; i < m_entries.Length; ++i)
            {
                UIRecipeEntry current = m_entries[i];
                UIRecipeEntry previous = i > 0 ? m_entries[i - 1] : null;
                UIRecipeEntry next = i < m_entries.Length - 1 ? m_entries[i + 1] : null;

                current.button.navigation = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = previous?.button,
                    selectOnDown = next?.button,
                    selectOnRight = firstBagSlot?.button
                };
            }
        }
    }
}
