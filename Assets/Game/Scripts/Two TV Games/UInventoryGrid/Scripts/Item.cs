using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UInventoryGrid
{
    /// <summary>
    /// Represents an item in the inventory.
    /// </summary>
    public class Item : MonoBehaviour, IPointerClickHandler
    {
        // The data of the item
        [HideInInspector] public ItemData data;

        [Tooltip("The icon representing the item.")]
        public Image icon;

        [Tooltip("The background image for the item.")]
        public Image background;

        [Tooltip("Sliced slot grid overlay matching the role panel.")]
        public Image slotGrid;

        private Vector3 rotateTarget;

        [Tooltip("Indicates whether the item is rotated.")]
        public bool isRotated;

        [Tooltip("Legacy rotation index kept for save compatibility (0 = vertical, 1 = horizontal).")]
        public int rotateIndex;

        [Tooltip("The stack count for stackable items.")]
        public int stackCount;

        [Tooltip("The icon representing the search.")]
        public Image searchIcon;

        [Tooltip("Indicates whether the item is revealed or hidden.")]
        public bool revealed;

        private float currentSearchTime;
        [HideInInspector] public bool isSearching;

        /// <summary>
        /// The position of the item in the inventory grid.
        /// </summary>
        public Vector2Int indexPosition { get; set; }

        /// <summary>
        /// The inventory that contains the item.
        /// </summary>
        public Inventory inventory { get; set; }

        /// <summary>
        /// The RectTransform component of the item.
        /// </summary>
        public RectTransform rectTransform { get; set; }

        /// <summary>
        /// The inventory grid containing the item.
        /// </summary>
        public InventoryGrid inventoryGrid { get; set; }

        [Tooltip("The size of the item after correcting for rotation.")]
        public SizeInt correctedSize
        {
            get
            {
                return new SizeInt(!isRotated ? data.size.width : data.size.height,
                                   !isRotated ? data.size.height : data.size.width);
            }
        }

        [Tooltip("The text displaying the stack count.")]
        public Text stackCountText;

        private void Start()
        {
            if (searchIcon)
                searchIcon.sprite = transform.Find("Search Icon")?.GetComponent<Image>()?.sprite;

            isRotated = rotateIndex == 1 || rotateIndex == 3;
            rotateIndex = isRotated ? 1 : 0;

            RefreshVisualLayout();

            currentSearchTime = 0f;
            isSearching = false;
        }

        public void RefreshVisualLayout()
        {
            ItemVisualLayout.Apply(this);
        }

        public void SetRaycastTargets(bool enabled)
        {
            if (icon)
                icon.raycastTarget = enabled;

            if (background)
                background.raycastTarget = enabled;
        }

        private void LateUpdate()
        {
            if (rectTransform && rectTransform.localRotation != Quaternion.identity)
                rectTransform.localRotation = Quaternion.identity;

            UpdateSearchTimer();
        }

        /// <summary>
        /// Toggle between vertical and horizontal orientation.
        /// </summary>
        public void Rotate()
        {
            isRotated = !isRotated;
            rotateIndex = isRotated ? 1 : 0;
            RefreshVisualLayout();
        }

        /// <summary>
        /// Reset the item's rotation.
        /// </summary>
        public void ResetRotate()
        {
            isRotated = false;
            rotateIndex = 0;
            RefreshVisualLayout();
        }

        void UpdateSearchTimer()
        {
            if (!isSearching)
                return;

            currentSearchTime += Time.deltaTime;
            if (currentSearchTime >= data.searchTime)
                OnSearchComplete();
        }

        /// <summary>
        /// Handle pointer click events on the item.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            var controller = inventory ? inventory.GetComponent<InventoryController>() : null;
            if (controller != null)
                controller.OnItemClick(this, eventData);
        }

        /// <summary>
        /// Update the stack count text.
        /// </summary>
        public void UpdateStack()
        {
            if (data.stackable)
            {
                if (!stackCountText.gameObject.activeSelf)
                {
                    stackCountText.gameObject.SetActive(true);
                }

                stackCountText.text = stackCount.ToString();
            }
            else
            {
                if (stackCountText.gameObject.activeSelf)
                {
                    stackCountText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Hides the item by changing its icon color to hidden color.
        /// </summary>
        public void HideItem()
        {
            icon.color = data.hiddenIconColor;
            revealed = false;
            Debug.Log($"Item hidden: {data.itemName}");
        }

        /// <summary>
        /// Reveal the item by changing its icon color to normal color.
        /// </summary>
        public void RevealItem()
        {
            icon.color = data.normalIconColor;
            revealed = true;
            searchIcon.gameObject.SetActive(false);
            Debug.Log($"Item revealed: {data.itemName}");
        }

        /// <summary>
        /// Start the search process for the item.
        /// </summary>
        public void StartSearch()
        {
            currentSearchTime = 0f;
            isSearching = true;
            searchIcon.gameObject.SetActive(true);

            if (inventory.searchItemAudioSource != null)
                inventory.AddToItemSearchQueue(this);

            Debug.Log($"Searching item: {data.itemName}");
        }

        /// <summary>
        /// Handle actions when the item search is complete.
        /// </summary>
        private void OnSearchComplete()
        {
            isSearching = false;

            RevealItem();

            inventory.RemoveItemFromSearchQueue();

            Debug.Log($"Search complete for item: {data.itemName}");
        }
    }
}
