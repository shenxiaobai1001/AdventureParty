using UnityEngine;
using UnityEngine.UI;

namespace UInventoryGrid
{
    public class ItemPanelUI : MonoBehaviour
    {
        [Tooltip("Button to inspect the selected item.")]
        public Button inspectButton;

        [Tooltip("Button to use the selected item.")]
        public Button useButton;

        [Tooltip("Button to split the stack of the selected item.")]
        public Button splitButton;

        [Tooltip("Button to drop the selected item.")]
        public Button dropButton;

        [Tooltip("Button to equip the selected item into an empty slot for its type.")]
        public Button equipButton;

        [Tooltip("Panel to display item inspection details.")]
        public GameObject inspectPanel;

        /// <summary>
        /// Positions the item panel relative to the specified item.
        /// </summary>
        /// <param name="item">The item to position the panel for.</param>
        public void PositionPanel(Item item)
        {
            transform.SetParent(item.transform.root, false);

            RectTransform itemRectTransform = item.GetComponent<RectTransform>();
            RectTransform panelRectTransform = GetComponent<RectTransform>();

            Vector3 panelPosition = itemRectTransform.position + new Vector3(itemRectTransform.rect.width / 2 + panelRectTransform.rect.width / 2, 0, 0);
            panelRectTransform.position = panelPosition;

            // Adjust the panel position to keep it within screen bounds
            AdjustPanelPosition(panelRectTransform);
        }

        /// <summary>
        /// Adjusts the panel position to ensure it stays within screen bounds.
        /// </summary>
        /// <param name="panelRectTransform">The RectTransform of the panel to adjust.</param>
        private void AdjustPanelPosition(RectTransform panelRectTransform)
        {
            Vector3[] panelCorners = new Vector3[4];
            panelRectTransform.GetWorldCorners(panelCorners);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            Vector3 adjustedPosition = panelRectTransform.position;

            // Check if the panel is out of bounds on the right side of the screen
            if (panelCorners[2].x > screenWidth)
            {
                adjustedPosition.x -= panelCorners[2].x - screenWidth;
            }

            // Check if the panel is out of bounds on the left side of the screen
            if (panelCorners[0].x < 0)
            {
                adjustedPosition.x -= panelCorners[0].x;
            }

            // Check if the panel is out of bounds on the top side of the screen
            if (panelCorners[2].y > screenHeight)
            {
                adjustedPosition.y -= panelCorners[2].y - screenHeight;
            }

            // Check if the panel is out of bounds on the bottom side of the screen
            if (panelCorners[0].y < 0)
            {
                adjustedPosition.y -= panelCorners[0].y;
            }

            panelRectTransform.position = adjustedPosition;
        }
    }
}
