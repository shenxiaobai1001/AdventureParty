using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Kenshi-style unit control: selected via camera LMB, move to floor point with RMB.
/// Casual Relax locomotion with walk/run toggle (R). Combat uses fixed combat speed.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public enum CasualLocomotionMode
    {
        Walk,
        Run,
    }

    public static PlayerController Selected { get; private set; }

    [Header("Movement")]
    [Tooltip("Casual walk speed.")]
    public float walkSpeed = 3.5f;

    [Tooltip("Casual run speed.")]
    public float runSpeed = 6f;

    [Tooltip("Combat move speed (no walk/run toggle).")]
    public float combatMoveSpeed = 5f;

    public float stoppingDistance = 0.2f;
    public float rotationSpeed = 540f;

    [Header("Animation")]
    [Tooltip("Local-space Z velocity scale while walking in casual mode.")]
    public float walkAnimSpeed = RpgAnimParams.WalkBlendSpeed;

    [Header("Ground")]
    public LayerMask groundLayers = ~0;
    public float groundRayMaxDistance = 200f;

    CharacterController _characterController;
    Animator _animator;
    PlayerStanceController _stance;
    PlayerActivityController _activity;
    Vector3 _destination;
    bool _hasDestination;
    bool _isSelected;
    Vector3 _currentPlanarVelocity;
    CasualLocomotionMode _casualLocomotion = CasualLocomotionMode.Walk;

    public bool IsSelected => _isSelected;
    public CasualLocomotionMode CasualLocomotion => _casualLocomotion;

    public bool IsStandingStill =>
        !_hasDestination &&
        _currentPlanarVelocity.sqrMagnitude < 0.01f &&
        (!_characterController || _characterController.velocity.magnitude < 0.15f);

    public static void Select(PlayerController player)
    {
        if (Selected == player)
            return;

        if (Selected)
            Selected.SetSelected(false);

        Selected = player;
        if (player)
            player.SetSelected(true);
    }

    public static void ClearSelection()
    {
        if (!Selected)
            return;

        Selected.SetSelected(false);
        Selected = null;
    }

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _stance = GetComponent<PlayerStanceController>();
        _activity = GetComponent<PlayerActivityController>();

        if (!_activity)
            _activity = gameObject.AddComponent<PlayerActivityController>();

        if (_animator && !_animator.GetComponent<RpgAnimatorEvents>())
            _animator.gameObject.AddComponent<RpgAnimatorEvents>();
    }

    void Start()
    {
        if (Selected)
            return;

        var players = FindObjectsOfType<PlayerController>();
        if (players.Length == 1 && players[0] == this)
            Select(this);
    }

    void Update()
    {
        if (!_isSelected)
        {
            StopLocomotion();
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            var pickupInteractor = GetComponent<EquipmentPickupInteractor>();
            if (pickupInteractor != null && pickupInteractor.HasPickupInRange())
                pickupInteractor.TryManualPickupNearest();
            else
                TryToggleCasualLocomotion();
        }

        if (Input.GetMouseButtonDown(1) && !IsPointerOverUi() && !IsMovementBlocked())
            TrySetDestinationFromMouse();

        UpdateMovement();
        UpdateAnimatorLocomotion();
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;

        if (!selected)
        {
            _hasDestination = false;
            StopLocomotion();
        }
    }

    public void CancelMovement()
    {
        _hasDestination = false;
        _currentPlanarVelocity = Vector3.zero;
    }

    public void SetCasualLocomotion(CasualLocomotionMode mode)
    {
        _casualLocomotion = mode;
    }

    void TryToggleCasualLocomotion()
    {
        if (_stance && (_stance.CurrentStance != PlayerStanceController.StanceMode.Casual || _stance.IsSwitching))
            return;

        SetCasualLocomotion(_casualLocomotion == CasualLocomotionMode.Walk
            ? CasualLocomotionMode.Run
            : CasualLocomotionMode.Walk);
    }

    bool IsMovementBlocked()
    {
        if (_activity && _activity.IsResting)
            return true;

        return _stance && _stance.IsSwitching;
    }

    float CurrentMoveSpeed
    {
        get
        {
            if (_activity && _activity.IsStealthing)
                return _activity.GetCrouchWalkSpeed();

            if (!_stance || _stance.CurrentStance == PlayerStanceController.StanceMode.Casual)
                return _casualLocomotion == CasualLocomotionMode.Run ? runSpeed : walkSpeed;

            return combatMoveSpeed;
        }
    }

    void TrySetDestinationFromMouse()
    {
        if (_activity && _activity.IsResting)
            return;

        var cam = Camera.main;
        if (!cam)
            return;

        var ray = cam.ScreenPointToRay(Input.mousePosition);
        var hits = Physics.RaycastAll(ray, groundRayMaxDistance, groundLayers, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (hit.collider.GetComponentInParent<PlayerController>() == this)
                continue;

            if (hit.normal.y < 0.5f)
                continue;

            _destination = hit.point;
            _hasDestination = true;
            return;
        }
    }

    void UpdateMovement()
    {
        if (IsMovementBlocked())
        {
            _currentPlanarVelocity = Vector3.zero;
            return;
        }

        if (!_hasDestination)
        {
            _currentPlanarVelocity = Vector3.zero;
            return;
        }

        var toTarget = _destination - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude <= stoppingDistance * stoppingDistance)
        {
            _hasDestination = false;
            _currentPlanarVelocity = Vector3.zero;
            return;
        }

        var direction = toTarget.normalized;
        var moveSpeed = CurrentMoveSpeed;

        var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);

        _currentPlanarVelocity = direction * moveSpeed;
        _characterController.SimpleMove(_currentPlanarVelocity);
    }

    void UpdateAnimatorLocomotion()
    {
        if (!_animator)
            return;

        if (IsMovementBlocked())
        {
            _animator.SetFloat(RpgAnimParams.VelocityX, 0f);
            _animator.SetFloat(RpgAnimParams.VelocityZ, 0f);
            _animator.SetBool(RpgAnimParams.Moving, false);
            _animator.SetBool(RpgAnimParams.Sprint, false);
            return;
        }

        var speed = _currentPlanarVelocity.magnitude;
        var isMoving = speed > 0.05f;
        var isCasual = !_stance || _stance.CurrentStance == PlayerStanceController.StanceMode.Casual;
        var isStealthing = _activity && _activity.IsStealthing;
        var isCasualRun = isCasual && !isStealthing && _casualLocomotion == CasualLocomotionMode.Run;
        var blendSpeed = isCasual
            ? (isCasualRun ? RpgAnimParams.RunBlendSpeed : walkAnimSpeed)
            : RpgAnimParams.RunBlendSpeed;
        var moveSpeed = CurrentMoveSpeed;

        _animator.SetBool(RpgAnimParams.Crouch, isStealthing);

        if (isMoving)
        {
            var localVelocity = transform.InverseTransformDirection(_currentPlanarVelocity);
            var normalized = localVelocity / Mathf.Max(moveSpeed, 0.01f);
            _animator.SetFloat(RpgAnimParams.VelocityX, normalized.x * blendSpeed);
            _animator.SetFloat(RpgAnimParams.VelocityZ, normalized.z * blendSpeed);
        }
        else
        {
            _animator.SetFloat(RpgAnimParams.VelocityX, 0f);
            _animator.SetFloat(RpgAnimParams.VelocityZ, 0f);
        }

        _animator.SetBool(RpgAnimParams.Moving, isMoving);
        _animator.SetBool(RpgAnimParams.Sprint, isCasualRun);
    }

    void StopLocomotion()
    {
        _currentPlanarVelocity = Vector3.zero;

        if (!_animator)
            return;

        _animator.SetFloat(RpgAnimParams.VelocityX, 0f);
        _animator.SetFloat(RpgAnimParams.VelocityZ, 0f);
        _animator.SetBool(RpgAnimParams.Moving, false);
        _animator.SetBool(RpgAnimParams.Sprint, false);
    }

    static bool IsPointerOverUi()
    {
        return EventSystem.current && EventSystem.current.IsPointerOverGameObject();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!_hasDestination)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_destination, 0.25f);
        Gizmos.DrawLine(transform.position, _destination);
    }
#endif
}
