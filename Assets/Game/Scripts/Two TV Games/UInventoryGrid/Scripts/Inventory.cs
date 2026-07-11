using System;
using System.Collections.Generic;
using UnityEngine;

namespace UInventoryGrid
{
    /// <summary>
    /// Structure representing a 2D size with width and height.
    /// </summary>
    [System.Serializable]
    public struct SizeInt
    {
        public int width;
        public int height;

        public SizeInt(int _width, int _height)
        {
            width = _width;
            height = _height;
        }
    }

    public class Inventory : MonoBehaviour
    {
        public event Action<Item, InventoryGrid, InventoryGrid> ItemGridChanged;

        [Header("Inventory Settings")]
        [Tooltip("Settings for the inventory system.")]
        public InventorySettings inventorySettings;

        [Tooltip("Grids where demo (using space bar) items can be added.")]
        [SerializeField] public InventoryGrid[] addItemGrids;

        [Header("Item Settings")]
        [Tooltip("Prefab for the items to be used in the inventory.")]
        public Item itemPrefab;

        [Tooltip("Data for different items available in the inventory.")]
        public ItemData[] itemsData;

        [Header("Sound Settings")]
        [Tooltip("Audio source for playing inventory sounds.")]
        public AudioSource inventoryAudioSource;
        [Tooltip("Audio source for playing search item sounds.")]
        public AudioSource searchItemAudioSource;

        [Tooltip("Sound played when an item is clicked.")]
        public AudioClip clickItemSound;

        [Tooltip("Sound played when an item is moved.")]
        public AudioClip moveItemSound;

        [Tooltip("Sound played when an item is searched.")]
        public AudioClip searchSound;

        [HideInInspector] public Item selectedItem;
        [HideInInspector] public List<Item> items = new List<Item>();
        [HideInInspector] public InventoryGrid gridOnMouse;
        [HideInInspector] public InventoryGrid[] grids;

        private void Awake()
        {
            grids = GetComponentsInChildren<InventoryGrid>(true);
        }

        /// <summary>
        /// Selects an item in the inventory.
        /// </summary>
        /// <param name="item">Item to be selected.</param>
        public void SelectItem(Item item)
        {
            ClearItemReferences(item);
            selectedItem = item;
            selectedItem.SetRaycastTargets(false);
            selectedItem.rectTransform.SetParent(transform);
            selectedItem.rectTransform.SetAsLastSibling();
        }

        /// <summary>
        /// Deselects the currently selected item.
        /// </summary>
        public void DeselectItem()
        {
            if (selectedItem)
                selectedItem.SetRaycastTargets(true);

            selectedItem = null;
        }

        /// <summary>
        /// Adds an item to the inventory.
        /// </summary>
        /// <param name="itemData">Data of the item to add.</param>
        /// <param name="quantity">Quantity of the item to add.</param>
        /// <param name="splitedItem">If true, adds the item as a split stack.</param>
        /// <param name="hidden">If true, adds the item as hidden (not visible).</param>
        /// <returns>True if the item was added successfully, false otherwise.</returns>
        public bool AddItem(ItemData itemData, int quantity = 1, bool splitedItem = false, bool hidden = false)
        {
            Debug.Log("Adding item: " + itemData.itemName + " with quantity: " + quantity);

            if (itemData.stackable)
            {
                Item stackableItem = GetStackableItem(itemData);

                if (stackableItem != null)
                {
                    if (stackableItem.stackCount < itemData.maxStack)
                    {
                        if (splitedItem)
                        {
                            return AddNewItem(itemData, quantity, hidden);
                        }
                        else
                        {
                            int spaceAvailable = itemData.maxStack - stackableItem.stackCount;
                            int addedQuantity = Mathf.Min(spaceAvailable, quantity);

                            stackableItem.stackCount += addedQuantity;
                            stackableItem.UpdateStack();

                            quantity -= addedQuantity;

                            if (quantity <= 0)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            if (!splitedItem)
            {
                for (int i = 0; i < quantity;)
                {
                    return AddNewItem(itemData, 1, hidden); // Adiciona apenas um item por vez
                }
            }

            return false;
        }


        /// <summary>
        /// Creates a new item in the inventory based called from AddItem function
        /// </summary>
        /// <param name="itemData">Data of the item to create.</param>
        /// <param name="stackCount">Initial stack count of the item.</param>
        /// <param name="hidden">If true, the item will be created hidden.</param>
        /// <returns>True if the item was created successfully, false otherwise.</returns>
        private bool AddNewItem(ItemData itemData, int stackCount = 1, bool hidden = false)
        {
            Debug.Log("Creating new item: " + itemData.itemName + ", stack: " + stackCount);

            if (addItemGrids == null || addItemGrids.Length == 0)
            {
                Debug.LogWarning("[Inventory] addItemGrids is not configured.");
                return false;
            }

            for (int gridIndex = 0; gridIndex < addItemGrids.Length; gridIndex++)
            {
                InventoryGrid currentGrid = addItemGrids[gridIndex];
                if (!currentGrid)
                    continue;

                if (currentGrid.items == null)
                    currentGrid.InitializeGrid();

                if (currentGrid.items == null)
                    continue;

                for (int y = 0; y < currentGrid.gridSize.y; y++)
                {
                    for (int x = 0; x < currentGrid.gridSize.x; x++)
                    {
                        Vector2Int slotPosition = new Vector2Int(x, y);

                        // Check for both orientations: original and rotated
                        if (TryPlaceItemInGrid(currentGrid, slotPosition, itemData, stackCount, false, hidden))
                        {
                            return true;
                        }

                        if (TryPlaceItemInGrid(currentGrid, slotPosition, itemData, stackCount, true, hidden))
                        {
                            return true;
                        }
                    }
                }
            }

            Debug.Log("Not enough slots found to add the item!");
            return false;
        }

        /// <summary>
        /// Attempts to place an item at a specific position within an inventory grid.
        /// </summary>
        /// <param name="grid">The inventory grid where the item will be placed.</param>
        /// <param name="slotPosition">The initial slot position where the item placement will be attempted.</param>
        /// <param name="itemData">The data of the item to be placed in the grid.</param>
        /// <param name="stackCount">The number of items in the stack to be added.</param>
        /// <param name="rotate">Indicates whether the item should be rotated before being placed.</param>
        /// <param name="hidden">Indicates whether the item should be added hidden.</param>
        /// <returns>Returns true if the item was successfully placed; otherwise, returns false.</returns>
        public bool PlaceItemInGrid(InventoryGrid grid, Vector2Int slotPosition, ItemData itemData, int stackCount, bool rotate, bool hidden = false)
        {
            return TryPlaceItemInGrid(grid, slotPosition, itemData, stackCount, rotate, hidden);
        }

        bool TryPlaceItemInGrid(InventoryGrid grid, Vector2Int slotPosition, ItemData itemData, int stackCount, bool rotate, bool hidden)
        {
            // Determine the item dimensions based on rotation.
            int itemWidth = rotate ? itemData.size.height : itemData.size.width;
            int itemHeight = rotate ? itemData.size.width : itemData.size.height;

            // Check if there is enough space available in the grid for the item with the specified dimensions.
            if (!ExistsItem(slotPosition, grid, itemWidth, itemHeight))
            {
                // Instantiate a new item.
                Item newItem = Instantiate(itemPrefab);

                // Rotate the item if necessary.
                if (rotate)
                {
                    newItem.Rotate();
                }

                // Set up the RectTransform properties for the item.
                newItem.rectTransform = newItem.GetComponent<RectTransform>();
                newItem.rectTransform.SetParent(grid.rectTransform);
                newItem.rectTransform.localScale = Vector3.one;


                // Set the item's position and other properties.
                newItem.indexPosition = slotPosition;
                newItem.inventory = this;

                // Assign the item to the corresponding slots in the grid.
                for (int xx = 0; xx < itemWidth; xx++)
                {
                    for (int yy = 0; yy < itemHeight; yy++)
                    {
                        int slotX = slotPosition.x + xx;
                        int slotY = slotPosition.y + yy;

                        grid.items[slotX, slotY] = newItem;
                        grid.items[slotX, slotY].data = itemData;
                    }
                }

                // Adjust the RectTransform's local position in the grid.
                newItem.inventoryGrid = grid;
                newItem.rectTransform.localPosition = IndexToInventoryPosition(newItem);
                newItem.stackCount = stackCount;
                newItem.UpdateStack();

                // Add the item to the inventory's item list.
                items.Add(newItem);

                // Hide the item if requested.
                if (hidden)
                {
                    newItem.HideItem();
                }
                else
                {
                    newItem.RevealItem();
                }

                newItem.RefreshVisualLayout();

                NotifyItemGridChanged(newItem, null, grid);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Retrieves a stackable item that matches the given item data.
        /// </summary>
        /// <param name="itemData">Data of the item to match.</param>
        /// <returns>The stackable item if found, null otherwise.</returns>
        public Item GetStackableItem(ItemData itemData)
        {
            foreach (Item item in items)
            {
                if (item.data == itemData && item.stackCount < itemData.maxStack)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// Moves the specified item within the inventory.
        /// </summary>
        /// <param name="item">The item to move.</param>
        /// <param name="deselectItemInEnd">If true, the item will be deselected after moving.</param>
        public void MoveItem(Item item, bool deselectItemInEnd = true)
        {
            var previousGrid = item.inventoryGrid;
            Vector2Int slotPosition = GetSlotAtMouseCoords();

            if (ReachedBoundary(slotPosition, gridOnMouse, item.correctedSize.width, item.correctedSize.height))
            {
                return;
            }

            if (ExistsItem(slotPosition, gridOnMouse, item.correctedSize.width, item.correctedSize.height))
            {
                return;
            }

            if (!IsItemTypeAllowed(item.data))
            {
                return;
            }

            if (item.inventoryGrid != gridOnMouse)
            {
                if (!item.revealed)
                {
                    Debug.Log("Cannot move an unrevealed item from another grid or equipment slot.");
                    return;
                }
            }

            item.icon.raycastTarget = true;
            item.background.raycastTarget = true;

            item.indexPosition = slotPosition;
            item.rectTransform.SetParent(gridOnMouse.rectTransform);

            for (int x = 0; x < item.correctedSize.width; x++)
            {
                for (int y = 0; y < item.correctedSize.height; y++)
                {
                    int slotX = item.indexPosition.x + x;
                    int slotY = item.indexPosition.y + y;

                    gridOnMouse.items[slotX, slotY] = item;
                }
            }

            item.inventoryGrid = gridOnMouse;
            item.rectTransform.localPosition = IndexToInventoryPosition(item);
            item.RefreshVisualLayout();
            NotifyItemGridChanged(item, previousGrid, gridOnMouse);

            if (deselectItemInEnd)
            {
                DeselectItem();
            }

            PlayInventoryAudioClip(moveItemSound);
        }

        /// <summary>
        /// Clears item references in the grid.
        /// </summary>
        /// <param name="item">The item to clear references for.</param>
        public void ClearItemReferences(Item item)
        {
            int gridWidth = item.inventoryGrid.items.GetLength(0);
            int gridHeight = item.inventoryGrid.items.GetLength(1);

            for (int x = 0; x < item.correctedSize.width; x++)
            {
                for (int y = 0; y < item.correctedSize.height; y++)
                {
                    int slotX = item.indexPosition.x + x;
                    int slotY = item.indexPosition.y + y;

                    if (slotX >= 0 && slotX < gridWidth && slotY >= 0 && slotY < gridHeight)
                    {
                        item.inventoryGrid.items[slotX, slotY] = null;
                    }
                }
            }
        }

        /// <summary>
        /// Reverts item references in the grid.
        /// </summary>
        /// <param name="item">The item to revert references for.</param>
        public void RevertItemReferences(Item item)
        {
            for (int x = 0; x < item.correctedSize.width; x++)
            {
                for (int y = 0; y < item.correctedSize.height; y++)
                {
                    int slotX = item.indexPosition.x + x;
                    int slotY = item.indexPosition.y + y;

                    item.inventoryGrid.items[slotX, slotY] = item;
                }
            }
        }

        /// <summary>
        /// Checks if an item exists at the given slot position.
        /// </summary>
        /// <param name="slotPosition">Position of the slot to check.</param>
        /// <param name="grid">Reference to the grid.</param>
        /// <param name="width">Width of the item.</param>
        /// <param name="height">Height of the item.</param>
        /// <returns>True if an item exists at the position, false otherwise.</returns>
        public bool ExistsItem(Vector2Int slotPosition, InventoryGrid grid, int width = 1, int height = 1)
        {
            if (ReachedBoundary(slotPosition, grid, width, height))
            {
                return true;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int slotX = slotPosition.x + x;
                    int slotY = slotPosition.y + y;

                    if (grid.items[slotX, slotY] != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the slot position is within the grid boundaries.
        /// </summary>
        /// <param name="slotPosition">Position of the slot to check.</param>
        /// <param name="gridReference">Reference to the grid.</param>
        /// <param name="width">Width of the item.</param>
        /// <param name="height">Height of the item.</param>
        /// <returns>True if the position is within boundaries, false otherwise.</returns>
        public bool ReachedBoundary(Vector2Int slotPosition, InventoryGrid gridReference, int width = 1, int height = 1)
        {
            if (slotPosition.x + width > gridReference.gridSize.x || slotPosition.x < 0)
            {
                return true;
            }

            if (slotPosition.y + height > gridReference.gridSize.y || slotPosition.y < 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts an index position to a local position in the inventory.
        /// </summary>
        /// <param name="item">The item to get the position for.</param>
        /// <returns>Local position in the inventory.</returns>
        public Vector3 IndexToInventoryPosition(Item item)
        {
            var cell = ItemVisualLayout.GetSlotPixelSize(item);
            return SlotIndexToLocalCenter(item.indexPosition, item.correctedSize.width, item.correctedSize.height, cell);
        }

        /// <summary>
        /// Gets the slot position at the mouse coordinates.
        /// When an item is selected, the mouse is treated as the item center and the top-left slot is returned.
        /// </summary>
        /// <returns>Slot position at the mouse coordinates.</returns>
        public Vector2Int GetSlotAtMouseCoords()
        {
            if (gridOnMouse == null)
            {
                return Vector2Int.zero;
            }

            var cell = ItemVisualLayout.GetSlotPixelSize(gridOnMouse);
            var localPoint = GetMouseLocalPointOnGrid(gridOnMouse);

            if (selectedItem != null)
            {
                var footprintWidth = selectedItem.correctedSize.width * cell.x;
                var footprintHeight = selectedItem.correctedSize.height * cell.y;
                localPoint.x -= footprintWidth * 0.5f;
                localPoint.y += footprintHeight * 0.5f;
            }

            return LocalPointToSlotIndex(localPoint, cell);
        }

        public Vector2 GetMouseLocalPointOnGrid(InventoryGrid grid)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                grid.rectTransform,
                Input.mousePosition,
                null,
                out var localPoint);

            localPoint.x += grid.rectTransform.pivot.x * grid.rectTransform.rect.width;
            localPoint.y -= (1f - grid.rectTransform.pivot.y) * grid.rectTransform.rect.height;
            return localPoint;
        }

        public static Vector2Int LocalPointToSlotIndex(Vector2 localPoint, Vector2 cell)
        {
            return new Vector2Int(
                Mathf.FloorToInt(localPoint.x / cell.x),
                Mathf.FloorToInt(-localPoint.y / cell.y));
        }

        public static Vector3 SlotIndexToLocalCenter(Vector2Int slot, int width, int height, Vector2 cell)
        {
            return new Vector3(
                slot.x * cell.x + cell.x * width / 2f,
                -(slot.y * cell.y + cell.y * height / 2f),
                0f);
        }

        /// <summary>
        /// Gets the item at the mouse coordinates.
        /// </summary>
        /// <returns>The item at the mouse coordinates.</returns>
        public Item GetItemAtMouseCoords()
        {
            if (gridOnMouse == null)
            {
                return null;
            }

            Vector2Int slotPosition = GetSlotAtMouseCoords();

            if (!ReachedBoundary(slotPosition, gridOnMouse))
            {
                return GetItemFromSlotPosition(slotPosition);
            }

            return null;
        }

        /// <summary>
        /// Gets the item at the specified slot position.
        /// </summary>
        /// <param name="slotPosition">Position of the slot to check.</param>
        /// <returns>The item at the slot position.</returns>
        public Item GetItemFromSlotPosition(Vector2Int slotPosition)
        {
            return gridOnMouse.items[slotPosition.x, slotPosition.y];
        }

        /// <summary>
        /// Transfers an item to another inventory.
        /// </summary>
        /// <param name="item">Item to transfer.</param>
        /// <param name="targetInventory">Target inventory to transfer the item to.</param>
        public void TransferItemToInventory(Item item, Inventory targetInventory)
        {
            if (targetInventory == null || item == null) return;

            if (RemoveItem(item))
            {
                bool added = targetInventory.AddItem(item.data, item.stackCount, true);

                if (!added)
                {
                    AddItem(item.data, item.stackCount, true); // Re-add item to the original inventory if transfer fails
                }

                item.icon.raycastTarget = true;
                item.background.raycastTarget = true;
            }

            PlayInventoryAudioClip(moveItemSound);
        }

        /// <summary>
        /// Removes an item from the inventory.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        /// <returns>True if the item was removed successfully, false otherwise.</returns>
        public bool RemoveItem(Item item)
        {
            if (item != null)
            {
                var previousGrid = item.inventoryGrid;
                items.Remove(item);

                ClearItemReferences(item);
                NotifyItemGridChanged(item, previousGrid, null);
                Destroy(item.gameObject);

                return true;
            }
            return false;
        }

        void NotifyItemGridChanged(Item item, InventoryGrid previousGrid, InventoryGrid currentGrid)
        {
            ItemGridChanged?.Invoke(item, previousGrid, currentGrid);
        }

        /// <summary>
        /// Checks if the item type is allowed in the current grid.
        /// </summary>
        /// <param name="itemData">Data of the item to check.</param>
        /// <returns>True if the item type is allowed, false otherwise.</returns>
        public bool IsItemTypeAllowed(ItemData itemData)
        {
            if (gridOnMouse.allowedItemTypes.Contains(ItemType.All) || gridOnMouse.allowedItemTypes.Contains(itemData.itemType))
            {
                Debug.Log($"Item type {itemData.itemType} allowed.");
                return true;
            }
            else
            {
                Debug.Log($"Item type {itemData.itemType} not allowed in this grid.");
                return false;
            }
        }

        public void ClearAllItems()
        {
            for (var i = items.Count - 1; i >= 0; i--)
                RemoveItem(items[i]);
        }

        public InventoryGrid FindGridByName(string gridName)
        {
            if (string.IsNullOrEmpty(gridName) || grids == null)
                return null;

            foreach (var grid in grids)
            {
                if (grid && grid.name == gridName)
                    return grid;
            }

            return null;
        }

        /// <summary>
        /// Plays an audio clip with random pitch and volume variations.
        /// </summary>
        /// <param name="clip">The audio clip to play.</param>
        public void PlayInventoryAudioClip(AudioClip clip)
        {
        }

        /// <summary>
        /// Equips an item in the allowed grid based on the item type.
        /// </summary>
        /// <param name="item">The item to be equipped.</param>
        public void EquipItem(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning("No item to equip.");
                return;
            }

            foreach (InventoryGrid grid in grids)
            {
                if (grid.allowedItemTypes.Count == 1 && grid.allowedItemTypes.Contains(item.data.itemType))
                {
                    if (IsItemEquipped(item, grid))
                    {
                        Debug.Log("Item already eqquiped");
                        continue;
                    }

                    if (!item.revealed)
                    {
                        Debug.Log("Please reveal the item first before attempting to equip.");
                        continue;
                    }

                    if (TryEquipItemInGrid(grid, item.data, 1))
                    {
                        Debug.Log($"Item {item.data.itemName} equipped in grid: {grid.name}");
                        RemoveItem(item);
                        PlayInventoryAudioClip(moveItemSound);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to place an item at a specific position within an inventory grid.
        /// </summary>
        /// <param name="grid">The inventory grid where the item will be placed.</param>
        /// <param name="itemData">The data of the item to be placed in the grid.</param>
        /// <param name="stackCount">The number of items in the stack to be added.</param>
        /// <returns>Returns true if the item was successfully placed; otherwise, returns false.</returns>
        private bool TryEquipItemInGrid(InventoryGrid grid, ItemData itemData, int stackCount)
        {
            for (int y = 0; y < grid.gridSize.y; y++)
            {
                for (int x = 0; x < grid.gridSize.x; x++)
                {
                    Vector2Int slotPosition = new Vector2Int(x, y);

                    if (TryPlaceItemInGrid(grid, slotPosition, itemData, stackCount, false, false))
                    {
                        return true;
                    }

                    if (TryPlaceItemInGrid(grid, slotPosition, itemData, stackCount, true, false))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Checks if the item is currently equipped in the specified grid.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <param name="grid">The grid to check within.</param>
        /// <returns>True if the item is equipped in the specified grid, otherwise False.</returns>
        public bool IsItemEquipped(Item item, InventoryGrid grid)
        {
            Transform insideGrid = grid.transform.Find("Grid").transform;

            if (insideGrid != null)
            {
                for (int i = 0; i < insideGrid.childCount; i++)
                {
                    Item equippedItem = insideGrid.GetChild(i).GetComponent<Item>();
                    if (equippedItem != null && equippedItem.data.itemType == item.data.itemType)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Add an item to the search queue.
        /// </summary>
        public void AddToItemSearchQueue(Item item)
        {
        }

        /// <summary>
        /// Remove an item from the search queue.
        /// </summary>
        public void RemoveItemFromSearchQueue()
        {
        }
    }
}