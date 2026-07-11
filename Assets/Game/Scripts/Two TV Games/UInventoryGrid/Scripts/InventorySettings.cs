using UnityEngine;

namespace UInventoryGrid
{
    [CreateAssetMenu(fileName = "InventorySettings", menuName = "UInventoryGrid/Settings")]
    public class InventorySettings : ScriptableObject
    {
        [Header("Slot Settings")]
        [Tooltip("The size of each inventory slot in pixels.")]
        public Vector2Int slotSize = new Vector2Int(64, 64);

        [Header("Animation Settings")]
        [Tooltip("The speed at which items rotate when animated.")]
        public float rotationAnimationSpeed = 30f;

        [HideInInspector]
        [Tooltip("The scale of the slots, hidden from inspector.")]
        public float slotScale = 1f;
    }
}

