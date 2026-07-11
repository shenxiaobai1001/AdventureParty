using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UInventoryGrid
{
    [RequireComponent(typeof(Inventory))]
    public class InventoryController : MonoBehaviour
    {
        [Header("Controller Settings")]
        [Tooltip("Reference to the Inventory component.")]
        [SerializeField] public Inventory inventory;
        [Tooltip("Reference to the item panel UI element.")]
        public ItemPanelUI itemPanel;
        private Item currentItemWithPanel;

        private void Awake()
        {
            // Check if the Inventory component is present on the current GameObject
            if (inventory == null)
            {
                inventory = GetComponent<Inventory>();
                if (inventory == null)
                {
                    Debug.LogError("Inventory component is missing. Please add an Inventory component to the GameObject.");
                }
            }

            // Check if the itemPanel is assigned directly or found within the Inventory GameObject
            if (itemPanel == null)
            {
                // Try to find the Transform of the item panel within the Inventory GameObject
                Transform itemPanelTransform = inventory.transform.Find("Item Panel");

                if (itemPanelTransform != null)
                {
                    // Try to access the ItemPanelUI component on the found Transform
                    itemPanel = itemPanelTransform.gameObject.GetComponent<ItemPanelUI>();

                    if (itemPanel == null)
                    {
                        Debug.LogError("ItemPanelUI component is missing on the assigned item panel. Please add an ItemPanelUI component to the item panel GameObject.");
                    }
                }
                else
                {
                    Debug.LogWarning("[InventoryController] Item panel is not assigned. Right-click item menus will be disabled.");
                }
            }

            if (!GetComponent<InventoryDropHintOverlay>())
                gameObject.AddComponent<InventoryDropHintOverlay>();
        }

        private void Update()
        {
            if (inventory == null)
                return;

            HandleMouseInput();
            HandleKeyboardInput();
            UpdateSelectedItemPosition();
            UpdateGridOnMouse();
        }

        /// <summary>
        /// Updates the grid currently under the mouse cursor.
        /// </summary>
        private void UpdateGridOnMouse()
        {
            Item item = inventory.GetItemAtMouseCoords();

            if (item != null)
            {
                inventory.gridOnMouse = item.inventoryGrid;
            }
            else
            {
                foreach (var grid in inventory.grids)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(grid.rectTransform, Input.mousePosition))
                    {
                        inventory.gridOnMouse = grid;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Handles mouse input for item interactions.
        /// </summary>
        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Prevent clicking on items when inspect panel is active.
                if (itemPanel != null && itemPanel.inspectPanel != null && itemPanel.inspectPanel.activeSelf)
                {
                    return;
                }

                Vector2Int slotPosition = inventory.GetSlotAtMouseCoords();

                if (inventory.gridOnMouse == null)
                    return;

                // Check if the mouse position is within the inventory boundaries.
                if (!inventory.ReachedBoundary(slotPosition, inventory.gridOnMouse))
                {
                    CloseInspectUI();

                    // Handle item interactions based on selection state.
                    if (inventory.selectedItem != null)
                    {
                        if (inventory.selectedItem.isSearching)
                            return;

                        HandleSelectedItemClick(slotPosition);
                    }
                    else if (itemPanel == null || !itemPanel.gameObject.activeSelf)
                    {
                        Item item = GetItemAtSlot(slotPosition);
                        if(item != null)
                        {
                            if (item.isSearching)
                                return;
                        }

                        SelectItemWithMouse(slotPosition);
                    }
                }
            }
            else if (Input.GetMouseButtonDown(2))
            {
                Vector2Int slotPosition = inventory.GetSlotAtMouseCoords();

                // Get item at slot position
                Item item = GetItemAtSlot(slotPosition);

                if(item != null)
                {
                    // Handle starting item search with middle mouse button click.
                    if (!item.revealed)
                    {
                        Debug.Log($"Call search item function for: {item.data.itemName}");
                        item.StartSearch();               
                    }
                }
            }

            // Handle mouse click events on the item panel.
            if (itemPanel != null && itemPanel.gameObject.activeSelf && Input.GetMouseButtonDown(0))
            {
                HandlePanelMouseClick();
            }
        }

        /// <summary>
        /// Handles keyboard input for inventory actions.
        /// </summary>
        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (inventory.selectedItem != null)
                    return;

                inventory.AddItem(inventory.itemsData[Random.Range(0, inventory.itemsData.Length)], 1, false, true);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseCurrentPanel();
                CloseInspectUI();
            }

            if (Input.GetKeyDown(KeyCode.R) && inventory.selectedItem != null)
            {
                inventory.selectedItem.Rotate();
            }
        }

        /// <summary>
        /// Handles click events when an item is selected.
        /// </summary>
        /// <param name="slotPosition">The position of the slot clicked.</param>
        private void HandleSelectedItemClick(Vector2Int slotPosition)
        {
            Item selectedItem = inventory.selectedItem;
            Item clickedItem = GetItemAtSlot(slotPosition);

            // Check if the item is being moved to another inventory
            if (clickedItem != null && clickedItem != selectedItem && clickedItem.inventoryGrid != inventory.gridOnMouse)
            {
                if (inventory.IsItemTypeAllowed(selectedItem.data))
                {
                    TransferItemToInventory(selectedItem, clickedItem.inventoryGrid);
                }
                else
                {
                    Debug.Log("Item type not allowed");
                }
                return;
            }

            // Check if the item is being stacked
            if (clickedItem != null && clickedItem.data == selectedItem.data && clickedItem.data.stackable)
            {
                StackItems(selectedItem, clickedItem);
            }
            else
            {
                inventory.MoveItem(selectedItem);
            }
        }

        /// <summary>
        /// Transfers an item to another inventory.
        /// </summary>
        /// <param name="item">The item to transfer.</param>
        /// <param name="targetGrid">The target inventory grid.</param>
        public void TransferItemToInventory(Item item, InventoryGrid targetGrid)
        {
            if (targetGrid == null || item == null) return;

            // Remove the item from the current grid
            item.inventory.RemoveItem(item);

            // Add the item to the target grid
            bool added = targetGrid.inventory.AddItem(item.data, item.stackCount, true, false);

            // If the item cannot be added to the target grid, re-add the item to its original grid
            if (!added)
            {
                targetGrid.inventory.AddItem(item.data, item.stackCount, true, false);
            }
        }

        /// <summary>
        /// Handles mouse click events on the panel.
        /// </summary>
        private void HandlePanelMouseClick()
        {
            if (itemPanel == null)
                return;

            if (!IsPointerOverUIObject(itemPanel.gameObject))
            {
                ClosePanel();
            }
        }

        /// <summary>
        /// Updates the position of the selected item to follow the mouse cursor.
        /// </summary>
        private void UpdateSelectedItemPosition()
        {
            if (inventory.selectedItem != null)
            {
                MoveSelectedItemToMouse();
            }
        }

        /// <summary>
        /// Checks if the pointer is over a UI object.
        /// </summary>
        /// <param name="obj">The UI object to check against.</param>
        /// <returns>True if the pointer is over the UI object, false otherwise.</returns>
        private bool IsPointerOverUIObject(GameObject obj)
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
            {
                position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject == obj || result.gameObject.transform.IsChildOf(obj.transform))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Toggles the item panel visibility.
        /// </summary>
        /// <param name="item">The item to toggle the panel for.</param>
        /// <param name="forceClose">If true, forces the panel to close.</param>
        /// <param name="forceCloseInspectUI">If true, forces the inspect UI to close.</param>
        private void TogglePanel(Item item, bool forceClose = false, bool forceCloseInspectUI = false)
        {
            if (itemPanel == null)
                return;

            if (forceClose || (currentItemWithPanel == item && itemPanel.gameObject.activeSelf))
            {
                ClosePanel(forceCloseInspectUI);
            }
            else
            {
                OpenPanel(item);
            }
        }

        /// <summary>
        /// Opens the item panel for the specified item.
        /// </summary>
        /// <param name="item">The item to open the panel for.</param>
        private void OpenPanel(Item item)
        {
            if (itemPanel == null)
                return;

            ResetCurrentPanelItem();

            currentItemWithPanel = item;
            SetItemsRaycast(false);

            item.icon.raycastTarget = false;
            item.background.raycastTarget = false;

            itemPanel.PositionPanel(item);

            SetPanelListeners(item);

            itemPanel.gameObject.SetActive(true);

            if (inventory.selectedItem == item)
            {
                InspectCurrentItem();
            }
        }

        /// <summary>
        /// Sets the panel button listeners for the specified item.
        /// </summary>
        /// <param name="item">The item to set the listeners for.</param>
        private void SetPanelListeners(Item item)
        {
            itemPanel.dropButton.onClick.RemoveAllListeners();
            itemPanel.inspectButton.onClick.RemoveAllListeners();
            itemPanel.splitButton.onClick.RemoveAllListeners();
            itemPanel.useButton.onClick.RemoveAllListeners();

            itemPanel.dropButton.onClick.AddListener(RemoveCurrentItem);
            itemPanel.inspectButton.onClick.AddListener(InspectCurrentItem);
            itemPanel.splitButton.onClick.AddListener(SplitItem);
            itemPanel.useButton.onClick.AddListener(UseItem);

            itemPanel.equipButton.enabled = CanEquipItem(item);
            itemPanel.equipButton.onClick.AddListener(EquipItem);

            itemPanel.splitButton.enabled = item.data.stackable;
            itemPanel.inspectButton.enabled = item.data.itemType != ItemType.Currency;
            itemPanel.useButton.enabled = CanUseSelectedItem(item) ? true : false;
        }

        /// <summary>
        /// Closes the item panel.
        /// </summary>
        /// <param name="forceCloseInspectUI">If true, forces the inspect UI to close.</param>
        private void ClosePanel(bool forceCloseInspectUI = false)
        {
            if (itemPanel == null)
                return;

            if (forceCloseInspectUI)
            {
                CloseInspectUI();
            }

            itemPanel.gameObject.SetActive(false);
            currentItemWithPanel = null;
            SetItemsRaycast(true);
        }

        /// <summary>
        /// Resets the current panel item by enabling raycast targets.
        /// </summary>
        private void ResetCurrentPanelItem()
        {
            if (currentItemWithPanel != null)
            {
                currentItemWithPanel.icon.raycastTarget = true;
                currentItemWithPanel.background.raycastTarget = true;
            }
        }

        /// <summary>
        /// Sets the raycast targets for all items in the inventory.
        /// </summary>
        /// <param name="state">The state to set the raycast targets to.</param>
        private void SetItemsRaycast(bool state)
        {
            foreach (var item in inventory.items)
            {
                item.icon.raycastTarget = state;
                item.background.raycastTarget = state;
            }
        }

        /// <summary>
        /// Selects an item at the specified slot position with the mouse.
        /// </summary>
        /// <param name="slotPosition">The position of the slot to select.</param>
        private void SelectItemWithMouse(Vector2Int slotPosition)
        {
            Item item = GetItemAtSlot(slotPosition);

            if (item != null)
            {
                inventory.PlayInventoryAudioClip(inventory.clickItemSound);

                if (currentItemWithPanel != null && currentItemWithPanel != item)
                {
                    ClosePanel();
                }

                inventory.SelectItem(item);
            }
        }

        /// <summary>
        /// Gets the item at the specified slot position.
        /// </summary>
        /// <param name="slotPosition">The position of the slot.</param>
        /// <returns>The item at the slot position.</returns>
        private Item GetItemAtSlot(Vector2Int slotPosition)
        {
            return inventory.gridOnMouse?.items[slotPosition.x, slotPosition.y];
        }

        /// <summary>
        /// Handles item click events.
        /// </summary>
        /// <param name="item">The item clicked.</param>
        /// <param name="eventData">The event data for the click.</param>
        public void OnItemClick(Item item, PointerEventData eventData)
        {
            if (itemPanel == null)
                return;

            if (!itemPanel.gameObject.activeSelf && eventData.button == PointerEventData.InputButton.Right)
            {
                if (itemPanel.inspectPanel != null && itemPanel.inspectPanel.activeSelf)
                    CloseInspectUI();

                if (inventory.selectedItem != null)
                    return;

                TogglePanel(item);
            }
        }

        /// <summary>
        /// Moves the selected item to follow the mouse cursor.
        /// </summary>
        private void MoveSelectedItemToMouse()
        {
            var item = inventory.selectedItem;
            if (!item)
                return;

            item.rectTransform.position = Input.mousePosition;
        }

        /// <summary>
        /// Stacks two items together if possible.
        /// </summary>
        /// <param name="selectedItem">The selected item.</param>
        /// <param name="clickedItem">The clicked item.</param>
        private void StackItems(Item selectedItem, Item clickedItem)
        {
            int totalQuantity = selectedItem.stackCount + clickedItem.stackCount;

            if (totalQuantity <= selectedItem.data.maxStack)
            {
                clickedItem.stackCount = totalQuantity;
                inventory.RemoveItem(selectedItem);
            }
            else
            {
                int overflow = totalQuantity - selectedItem.data.maxStack;
                clickedItem.stackCount = selectedItem.data.maxStack;
                selectedItem.stackCount = overflow;
            }

            clickedItem.UpdateStack();
            selectedItem.UpdateStack();
        }

        /// <summary>
        /// Removes the current item with the panel.
        /// </summary>
        public void RemoveCurrentItem()
        {
            if (currentItemWithPanel != null)
            {
                inventory.RemoveItem(currentItemWithPanel);
                if (itemPanel != null)
                    itemPanel.gameObject.SetActive(false);
                currentItemWithPanel = null;
                SetItemsRaycast(true);
            }
        }

        /// <summary>
        /// Removes the item under the mouse cursor.
        /// </summary>
        public void RemoveItemWithMouse()
        {
            Vector2Int slotPosition = inventory.GetSlotAtMouseCoords();
            Item item = GetItemAtSlot(slotPosition);

            if (item != null)
            {
                inventory.RemoveItem(item);
            }
        }

        /// <summary>
        /// Inspects the current item with the panel.
        /// </summary>
        public void InspectCurrentItem()
        {
            if (itemPanel == null || itemPanel.inspectPanel == null)
                return;

            if (currentItemWithPanel != null && currentItemWithPanel.revealed)
            {
                var inspectPanel = itemPanel.inspectPanel;
                var inspectUI = inspectPanel.GetComponent<ItemInspectUI>();

                inspectPanel.SetActive(true);
                inspectUI.itemName.text = $"Name: {currentItemWithPanel.data.itemName}";
                inspectUI.itemDescription.text = $"Description: {currentItemWithPanel.data.description}";
                inspectUI.itemPrice.text = $"Price: {currentItemWithPanel.data.price}";
                inspectUI.closeButton.onClick.AddListener(CloseInspectUI);

                ClosePanel();
            }
            else
            {
                Debug.LogWarning("No item selected to inspect or is hidden.");
            }
        }

        /// <summary>
        /// Closes the inspect UI.
        /// </summary>
        public void CloseInspectUI()
        {
            if (itemPanel == null || itemPanel.inspectPanel == null)
                return;

            itemPanel.inspectPanel.SetActive(false);
        }

        /// <summary>
        /// Splits the current item with the panel into two stacks.
        /// </summary>
        private void SplitItem()
        {
            if (currentItemWithPanel != null && currentItemWithPanel.revealed)
            {
                if (currentItemWithPanel.stackCount <= 1)
                {
                    Debug.Log("Item cannot be split, it has a stack count of " + currentItemWithPanel.stackCount);
                    return;
                }

                int splitQuantity = Mathf.CeilToInt(currentItemWithPanel.stackCount / 2f);

                bool itemAddStatus = inventory.AddItem(currentItemWithPanel.data, splitQuantity, true);

                if (itemAddStatus)
                {
                    currentItemWithPanel.stackCount -= splitQuantity;
                    currentItemWithPanel.UpdateStack();
                }

                ClosePanel();
            }
            else
            {
                Debug.LogWarning("No item selected to split or is hidden.");
            }
        }

        /// <summary>
        /// Uses the currently selected item, reducing its stack count if stackable and removing it if depleted.
        /// </summary>
        private void UseItem()
        {
            if (currentItemWithPanel != null && currentItemWithPanel.revealed)
            {
                // Check if the item is stackable and if it has enough count to be used
                if (currentItemWithPanel.data.stackable)
                {
                    if (currentItemWithPanel.stackCount <= 1)
                    {
                        Debug.Log("Item cannot be used, it has a stack count of " + currentItemWithPanel.stackCount);
                        return;
                    }
                }

                // Check if the item can be used based on game-specific logic
                if (!CanUseSelectedItem(currentItemWithPanel))
                {
                    Debug.Log("Item cannot be used");
                    return;
                }

                // Reduce stack count for stackable items and remove the item if stack count is zero
                if (currentItemWithPanel.data.stackable)
                {
                    currentItemWithPanel.stackCount--;
                    currentItemWithPanel.UpdateStack();

                    if (currentItemWithPanel.stackCount <= 0)
                    {
                        inventory.RemoveItem(currentItemWithPanel);
                    }
                }
                else
                {
                    inventory.RemoveItem(currentItemWithPanel);
                }

                // Close any open panel related to the item usage
                ClosePanel();
            }
            else
            {
                Debug.LogWarning("No item selected to use or is hidden.");
            }
        }

        /// <summary>
        /// Checks if the selected item can be used based on its itemType.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the item can be used, false otherwise.</returns>
        private bool CanUseSelectedItem(Item item)
        {
            if (item == null)
            {
                return false;
            }

            // Check if the item type is FoodAndDrink or Medical
            if (item.data.itemType == ItemType.FoodAndDrink || item.data.itemType == ItemType.Medical)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Equips the current item if it can be equipped based on its type.
        /// </summary>
        private void EquipItem()
        {
            Item item = currentItemWithPanel;

            if (item == null || !item.revealed)
                return;

            if (CanEquipItem(item))
            {
                Debug.Log($"Equipping item: {item.data.itemName}");

                inventory.EquipItem(item);

                ClosePanel();
            }
            else
            {
                Debug.Log($"Cannot equip item: {item.data.itemName} with current item type: {item.data.itemType}");
            }
        }

        /// <summary>
        /// Checks if the item can be equipped based on its type.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the item can be equipped, otherwise False.</returns>
        private bool CanEquipItem(Item item)
        {
            return item.data.itemType == ItemType.Weapon ||
                   item.data.itemType == ItemType.MeleeWeapon ||
                   item.data.itemType == ItemType.Holster ||
                   item.data.itemType == ItemType.Backpack ||
                   item.data.itemType == ItemType.TacticalArmor ||
                   item.data.itemType == ItemType.TacticalHeadset ||
                   item.data.itemType == ItemType.TacticalHelmet ||
                   item.data.itemType == ItemType.TacticalRig ||
                   item.data.itemType == ItemType.Head ||
                   item.data.itemType == ItemType.Body ||
                   item.data.itemType == ItemType.Shoulder ||
                   item.data.itemType == ItemType.Forearm ||
                   item.data.itemType == ItemType.Hips ||
                   item.data.itemType == ItemType.Leg ||
                   item.data.itemType == ItemType.BackSlot ||
                   item.data.itemType == ItemType.WeaponPrimary ||
                   item.data.itemType == ItemType.WeaponSecondary;
        }

        /// <summary>
        /// Closes the current panel.
        /// </summary>
        private void CloseCurrentPanel()
        {
            if (currentItemWithPanel != null)
            {
                TogglePanel(currentItemWithPanel, true, true);
            }
        }
    }
}