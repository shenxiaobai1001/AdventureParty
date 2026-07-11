using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UInventoryGrid
{
    [RequireComponent(typeof(RectTransform))]
    public class InventoryGrid : MonoBehaviour, IPointerEnterHandler
    {
        [Header("Inventory")]
        [Tooltip("Reference to the Inventory component.")]
        [SerializeField] public Inventory inventory;
        [Tooltip("List of allowed item types in this grid.")]
        public List<ItemType> allowedItemTypes = new List<ItemType>();

        [Header("Stored Items")]
        [Tooltip("2D array to store items in the grid.")]
        [SerializeField] public Item[,] items;

        [Header("Grid Config")]
        [Tooltip("Size of the grid in terms of slots (width, height).")]
        public Vector2Int gridSize = new(5, 5);
        [Tooltip("Reference to the RectTransform component.")]
        public RectTransform rectTransform;
        [Tooltip("When enabled, keeps stretch-anchored grid visuals sized by the UI layout instead of forcing slotSize * gridSize.")]
        [SerializeField] bool preserveVisualLayout = true;

        private void Awake()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                InitializeGrid();
            }
            else
            {
                Debug.LogError("Inventory Grid RectTransform not found!");
            }
        }

        /// <summary>
        /// Initializes the grid by setting the size and creating the items array.
        /// </summary>
        public void InitializeGrid()
        {
            items = new Item[gridSize.x, gridSize.y];

            if (rectTransform == null || inventory == null || inventory.inventorySettings == null)
                return;

            if (preserveVisualLayout && UsesStretchAnchors(rectTransform))
                return;

            Vector2 size = new Vector2(
                gridSize.x * inventory.inventorySettings.slotSize.x,
                gridSize.y * inventory.inventorySettings.slotSize.y
            );
            rectTransform.sizeDelta = size;
        }

        static bool UsesStretchAnchors(RectTransform target)
        {
            return !Mathf.Approximately(target.anchorMin.x, target.anchorMax.x)
                || !Mathf.Approximately(target.anchorMin.y, target.anchorMax.y);
        }

        public Vector2 GetCellSize()
        {
            if (!rectTransform || gridSize.x <= 0 || gridSize.y <= 0)
                return Vector2.one * 64f;

            var rect = rectTransform.rect;
            return new Vector2(rect.width / gridSize.x, rect.height / gridSize.y);
        }

        /// <summary>
        /// Handles the pointer enter event to set the current grid on mouse.
        /// </summary>
        /// <param name="eventData">The event data for the pointer enter event.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            inventory.gridOnMouse = this;
        }
    }

}
