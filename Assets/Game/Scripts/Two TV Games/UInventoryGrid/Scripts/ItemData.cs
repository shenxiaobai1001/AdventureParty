using UnityEngine;

namespace UInventoryGrid
{
    public enum ItemType
    {
        All,
        Weapon,
        Ammunition,
        Attachment,
        Medical,
        FoodAndDrink,
        Container,
        QuestItem,
        Valuables,
        Keys,
        Electronic,
        BarterItem,
        Tool,
        MeleeWeapon,
        Throwable,
        Currency,
        Information,
        Clothing,
        Hideout,
        SecureContainer,
        TacticalRig,
        TacticalHeadset,
        TacticalArmor,
        TacticalHelmet,
        Backpack,
        Holster,

        /// <summary>Role equipment slots (UIRolePanel).</summary>
        Head = 26,
        Shoulder = 27,
        Body = 28,
        Hips = 29,
        Leg = 30,
        Forearm = 31,
        BackSlot = 32,
        WeaponPrimary = 33,
        WeaponSecondary = 34,

        /// <summary>Character built-in carry storage (Kenshi-style).</summary>
        CarryStorage = 35,

        /// <summary>Equipped backpack internal storage.</summary>
        BackpackStorage = 36,
    }

    [CreateAssetMenu(fileName = "ItemData", menuName = "UInventoryGrid/ItemData")]
    public class ItemData : ScriptableObject
    {
        [Header("Information")]
        [Tooltip("The name of the item.")]
        public string itemName;

        [Tooltip("Description of the item.")]
        public string description;

        [Tooltip("The price of the item.")]
        public float price;

        [Tooltip("Weight per unit for encumbrance.")]
        public float weight = 1f;

        [Header("Type")]
        [Tooltip("The type/category of the item.")]
        public ItemType itemType;

        [Header("Size")]
        [Tooltip("The size of the item in grid units.")]
        [SerializeField] public SizeInt size;

        [Header("Visual")]
        [Tooltip("The icon representing the item.")]
        public Sprite icon;

        [Tooltip("The background color associated with the item.")]
        public Color backgroundColor;

        [Tooltip("Color of the icon when it is not being searched.")]
        public Color normalIconColor = Color.white;

        [Tooltip("Color of the icon when it is in an hidden state.")]
        public Color hiddenIconColor = Color.black;

        [Header("Stack")]
        [Tooltip("Indicates if the item can be stacked.")]
        public bool stackable;

        [Tooltip("The maximum stack size of the item.")]
        public int maxStack;

        [Header("Search Options")]
        [Tooltip("The time required to search for the item, in seconds.")]
        public float searchTime = 5f;
    }
}
