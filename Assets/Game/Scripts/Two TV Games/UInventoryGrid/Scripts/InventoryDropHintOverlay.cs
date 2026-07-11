using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UInventoryGrid
{
    /// <summary>
    /// Shows a tinted footprint on the grid under the cursor while an item is being dragged.
    /// </summary>
    [RequireComponent(typeof(Inventory))]
    public class InventoryDropHintOverlay : MonoBehaviour
    {
        [SerializeField] Color validColor = new Color(0.25f, 0.85f, 0.35f, 0.38f);
        [SerializeField] Color invalidColor = new Color(0.92f, 0.22f, 0.18f, 0.38f);

        Inventory inventory;
        readonly Dictionary<InventoryGrid, HintVisual> hints = new Dictionary<InventoryGrid, HintVisual>();
        static Sprite whiteSprite;

        struct HintVisual
        {
            public RectTransform rectTransform;
            public Image image;
        }

        void Awake()
        {
            inventory = GetComponent<Inventory>();
        }

        void LateUpdate()
        {
            if (!inventory)
                return;

            RefreshHints();
        }

        void RefreshHints()
        {
            var item = inventory.selectedItem;
            if (!item)
            {
                HideAllHints();
                return;
            }

            var activeGrid = inventory.gridOnMouse;
            foreach (var grid in inventory.grids)
            {
                if (!grid)
                    continue;

                if (grid != activeGrid)
                {
                    HideHint(grid);
                    continue;
                }

                var slot = inventory.GetSlotAtMouseCoords();
                var width = item.correctedSize.width;
                var height = item.correctedSize.height;
                var valid = IsValidDrop(item, grid, slot, width, height);
                ShowHint(grid, slot, width, height, valid);
            }
        }

        bool IsValidDrop(Item item, InventoryGrid grid, Vector2Int slot, int width, int height)
        {
            if (inventory.ReachedBoundary(slot, grid, width, height))
                return false;

            if (inventory.ExistsItem(slot, grid, width, height))
                return false;

            if (!inventory.IsItemTypeAllowed(item.data))
                return false;

            if (item.inventoryGrid != grid && !item.revealed)
                return false;

            return true;
        }

        void ShowHint(InventoryGrid grid, Vector2Int slot, int width, int height, bool valid)
        {
            var hint = GetOrCreateHint(grid);
            var cell = ItemVisualLayout.GetSlotPixelSize(grid);

            hint.rectTransform.sizeDelta = new Vector2(width * cell.x, height * cell.y);
            hint.rectTransform.localPosition = Inventory.SlotIndexToLocalCenter(slot, width, height, cell);
            hint.image.color = valid ? validColor : invalidColor;
            hint.rectTransform.gameObject.SetActive(true);
        }

        HintVisual GetOrCreateHint(InventoryGrid grid)
        {
            if (hints.TryGetValue(grid, out var hint) && hint.rectTransform)
                return hint;

            var parent = grid.rectTransform ? grid.rectTransform : grid.transform as RectTransform;
            var hintObject = new GameObject("DropHint", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            hintObject.transform.SetParent(parent, false);

            var rectTransform = hintObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localScale = Vector3.one;

            var image = hintObject.GetComponent<Image>();
            image.sprite = GetWhiteSprite();
            image.type = Image.Type.Simple;
            image.raycastTarget = false;

            hint = new HintVisual
            {
                rectTransform = rectTransform,
                image = image,
            };
            hints[grid] = hint;
            return hint;
        }

        static Sprite GetWhiteSprite()
        {
            if (whiteSprite)
                return whiteSprite;

            whiteSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f));

            return whiteSprite;
        }

        void HideHint(InventoryGrid grid)
        {
            if (hints.TryGetValue(grid, out var hint) && hint.rectTransform)
                hint.rectTransform.gameObject.SetActive(false);
        }

        void HideAllHints()
        {
            foreach (var pair in hints)
            {
                if (pair.Value.rectTransform)
                    pair.Value.rectTransform.gameObject.SetActive(false);
            }
        }
    }
}
