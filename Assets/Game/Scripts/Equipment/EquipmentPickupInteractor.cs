using System.Collections.Generic;
using UInventoryGrid;
using UnityEngine;

/// <summary>
/// Picks up world equipment and weapons via overlap with pickup trigger volumes, or manual key press.
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
    readonly HashSet<IWorldItemPickup> pickupsInRange = new HashSet<IWorldItemPickup>();
    readonly HashSet<IWorldItemPickup> previousPickupsInRange = new HashSet<IWorldItemPickup>();

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
            if (pickup == null || pickup is not MonoBehaviour behaviour || !behaviour.isActiveAndEnabled)
                continue;

            if (previousPickupsInRange.Contains(pickup))
                continue;

            TryPickup(pickup);
        }

        previousPickupsInRange.Clear();
        foreach (var pickup in pickupsInRange)
            previousPickupsInRange.Add(pickup);
    }

    public bool TryManualPickupNearest()
    {
        var pickup = FindNearestInRangePickup();
        if (pickup == null)
            return false;

        return TryPickup(pickup);
    }

    public bool HasPickupInRange()
    {
        RefreshPickupsInRange();
        return pickupsInRange.Count > 0;
    }

    public bool TryPickup(IWorldItemPickup pickup)
    {
        var inventory = ResolveInventory();
        if (!inventory || pickup == null)
            return false;

        if (pickup.TryPickup(inventory))
        {
            PlayPickupAnimation();
            return true;
        }

        Debug.LogWarning($"[Pickup] Failed to pick up: {pickup.ItemData?.itemName ?? "Unknown"}");
        return false;
    }

    void RefreshPickupsInRange()
    {
        pickupsInRange.Clear();
        var probe = GetProbePosition();

        foreach (var pickup in FindAllPickups())
        {
            if (pickup == null || pickup is not MonoBehaviour behaviour || !behaviour.isActiveAndEnabled)
                continue;

            if (pickup.ContainsProbePoint(probe, pickupRadius))
                pickupsInRange.Add(pickup);
        }
    }

    static IEnumerable<IWorldItemPickup> FindAllPickups()
    {
        foreach (var pickup in Object.FindObjectsByType<EquipmentWorldPickup>(FindObjectsSortMode.None))
            yield return pickup;

        foreach (var pickup in Object.FindObjectsByType<WeaponWorldPickup>(FindObjectsSortMode.None))
            yield return pickup;
    }

    IWorldItemPickup FindNearestInRangePickup()
    {
        RefreshPickupsInRange();
        IWorldItemPickup nearest = null;
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
        var mainPanel = Object.FindFirstObjectByType<UIMainControlPanel>();
        return mainPanel ? mainPanel.GetSelectedCharacterEntry() : null;
    }

    Inventory ResolveInventory()
    {
        if (targetInventory)
            return targetInventory;

        var mainPanel = Object.FindFirstObjectByType<UIMainControlPanel>();
        if (mainPanel)
        {
            var inventory = mainPanel.EnsureRoleInventory();
            if (inventory)
                return inventory;
        }

        var panel = Object.FindFirstObjectByType<UIRolePanelController>(FindObjectsInactive.Include);
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
