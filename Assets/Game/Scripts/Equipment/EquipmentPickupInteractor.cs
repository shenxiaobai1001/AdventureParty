using System.Collections.Generic;
using UInventoryGrid;
using UnityEngine;

/// <summary>
/// Picks up world equipment via overlap with each pickup's 2x trigger volume, or manual key press.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class EquipmentPickupInteractor : MonoBehaviour
{
    [SerializeField] KeyCode pickupKey = KeyCode.R;
    [SerializeField] float pickupRadius = 2.5f;
    [SerializeField] bool autoPickupOnTouch = true;
    [SerializeField] Inventory targetInventory;

    CharacterController characterController;
    Animator animator;
    readonly HashSet<EquipmentWorldPickup> pickupsInRange = new HashSet<EquipmentWorldPickup>();
    readonly HashSet<EquipmentWorldPickup> previousPickupsInRange = new HashSet<EquipmentWorldPickup>();

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void FixedUpdate()
    {
        RefreshPickupsInRange();

        if (!autoPickupOnTouch)
            return;

        foreach (var pickup in pickupsInRange)
        {
            if (!pickup || !pickup.isActiveAndEnabled)
                continue;

            if (previousPickupsInRange.Contains(pickup))
                continue;

            TryPickup(pickup);
        }

        previousPickupsInRange.Clear();
        foreach (var pickup in pickupsInRange)
            previousPickupsInRange.Add(pickup);
    }

    /// <summary>
    /// Picks up the nearest in-range pickup. Called from PlayerController when R is pressed.
    /// </summary>
    public bool TryManualPickupNearest()
    {
        var pickup = FindNearestInRangePickup();
        if (!pickup)
            return false;

        return TryPickup(pickup);
    }

    public bool HasPickupInRange()
    {
        RefreshPickupsInRange();
        return pickupsInRange.Count > 0;
    }

    public bool TryPickup(EquipmentWorldPickup pickup)
    {
        var inventory = ResolveInventory();
        if (!inventory || !pickup)
            return false;

        var entry = ResolveCharacterEntry();
        if (EquipmentInventoryBridge.TryPickup(pickup, inventory, entry))
        {
            PlayPickupAnimation();
            return true;
        }

        Debug.LogWarning($"[EquipmentPickup] Failed to pick up: {pickup.ItemData?.itemName ?? pickup.name}");
        return false;
    }

    void RefreshPickupsInRange()
    {
        pickupsInRange.Clear();
        var probe = GetProbePosition();

        foreach (var pickup in FindObjectsByType<EquipmentWorldPickup>(FindObjectsSortMode.None))
        {
            if (!pickup || !pickup.isActiveAndEnabled)
                continue;

            if (pickup.ContainsProbePoint(probe, pickupRadius))
                pickupsInRange.Add(pickup);
        }
    }

    EquipmentWorldPickup FindNearestInRangePickup()
    {
        RefreshPickupsInRange();
        EquipmentWorldPickup nearest = null;
        var nearestDistance = float.MaxValue;
        var probe = GetProbePosition();

        foreach (var pickup in pickupsInRange)
        {
            var distance = Vector3.Distance(probe, pickup.GetPickupAnchorPosition());
            if (distance >= nearestDistance)
                continue;

            nearest = pickup;
            nearestDistance = distance;
        }

        return nearest;
    }

    Vector3 GetProbePosition()
    {
        if (characterController)
            return transform.TransformPoint(characterController.center);

        return transform.position;
    }

    CharacterEntry ResolveCharacterEntry()
    {
        var mainPanel = FindFirstObjectByType<UIMainControlPanel>();
        return mainPanel ? mainPanel.GetSelectedCharacterEntry() : null;
    }

    Inventory ResolveInventory()
    {
        if (targetInventory)
            return targetInventory;

        var mainPanel = FindFirstObjectByType<UIMainControlPanel>();
        if (mainPanel)
        {
            var inventory = mainPanel.EnsureRoleInventory();
            if (inventory)
                return inventory;
        }

        var panel = FindFirstObjectByType<UIRolePanelController>(FindObjectsInactive.Include);
        return panel ? panel.GetComponent<Inventory>() : null;
    }

    void PlayPickupAnimation()
    {
        if (!animator)
            animator = GetComponentInChildren<Animator>();

        if (animator)
            RpgAnimParams.TriggerPickup(animator);
    }
}
