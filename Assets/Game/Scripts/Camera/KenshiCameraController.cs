using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Kenshi-style orbit camera: MMB rotate, WASD pan pivot, LMB select Player-tagged unit to follow.
/// Panning detaches from the followed unit; re-select with LMB.
/// </summary>
[RequireComponent(typeof(Camera))]
public class KenshiCameraController : MonoBehaviour
{
    [Header("Orbit")]
    public float orbitDistance = 14f;
    public float minDistance = 5f;
    public float maxDistance = 45f;
    public float rotateSensitivity = 2.5f;
    public float minPitch = 8f;
    public float maxPitch = 80f;
    [Tooltip("Initial camera elevation in degrees (45 = classic RPG angle).")]
    public float initialPitch = 45f;
    public float pivotHeightOffset = 1.1f;

    [Header("Pan (WASD)")]
    public float panSpeed = 10f;

    [Header("Zoom")]
    public float zoomSpeed = 6f;

    [Header("Selection")]
    public string playerTag = "Player";
    public LayerMask selectionLayers = ~0;
    public float selectionRayDistance = 500f;

    [Header("References")]
    public Camera controlledCamera;

    Transform _pivotTransform;
    Transform _followTarget;
    float _yaw;
    float _pitch = 45f;

    public Transform FollowTarget => _followTarget;
    public Vector3 PivotPosition => _pivotTransform ? _pivotTransform.position : transform.position;

    public void SetFollowTarget(Transform target, bool selectPlayer = true)
    {
        _followTarget = target;

        if (!selectPlayer)
            return;

        if (!target)
        {
            PlayerController.ClearSelection();
            return;
        }

        var player = target.GetComponent<PlayerController>();
        if (player)
            PlayerController.Select(player);
        else
            PlayerController.ClearSelection();
    }

    void Awake()
    {
        if (!controlledCamera)
            controlledCamera = GetComponent<Camera>();

        var pivotGo = new GameObject("CameraPivot");
        _pivotTransform = pivotGo.transform;

        var forward = controlledCamera.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;
        forward.Normalize();

        _pivotTransform.position = controlledCamera.transform.position + forward * orbitDistance;
        _pivotTransform.position = new Vector3(_pivotTransform.position.x, pivotHeightOffset, _pivotTransform.position.z);

        var offset = controlledCamera.transform.position - _pivotTransform.position;
        orbitDistance = Mathf.Clamp(offset.magnitude, minDistance, maxDistance);

        var flatOffset = new Vector3(offset.x, 0f, offset.z);
        if (flatOffset.sqrMagnitude > 0.001f)
            _yaw = Mathf.Atan2(flatOffset.x, flatOffset.z) * Mathf.Rad2Deg;

        _pitch = Mathf.Clamp(initialPitch, minPitch, maxPitch);
        ApplyCameraTransform();
    }

    void Start()
    {
        if (_followTarget)
            return;

        var player = FindFirstObjectByType<PlayerController>();
        if (player)
            SetFollowTarget(player.transform);
    }

    void OnDestroy()
    {
        if (_pivotTransform)
            Destroy(_pivotTransform.gameObject);
    }

    void Update()
    {
        HandleZoom();
        HandleRotate();
        HandlePan();
        HandleSelectPlayer();
        SyncFollowTarget();
    }

    void LateUpdate()
    {
        ApplyCameraTransform();
    }

    void SyncFollowTarget()
    {
        if (!_followTarget)
            return;

        var pos = _followTarget.position;
        pos.y += pivotHeightOffset;
        _pivotTransform.position = pos;
    }

    void HandleRotate()
    {
        if (!Input.GetMouseButton(2))
            return;

        _yaw += Input.GetAxis("Mouse X") * rotateSensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * rotateSensitivity;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
    }

    void HandlePan()
    {
        var input = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) input.z += 1f;
        if (Input.GetKey(KeyCode.S)) input.z -= 1f;
        if (Input.GetKey(KeyCode.D)) input.x += 1f;
        if (Input.GetKey(KeyCode.A)) input.x -= 1f;

        if (input.sqrMagnitude < 0.001f)
            return;

        if (_followTarget)
        {
            _followTarget = null;
            PlayerController.ClearSelection();
        }

        input.Normalize();

        var forward = controlledCamera.transform.forward;
        forward.y = 0f;
        forward.Normalize();

        var right = controlledCamera.transform.right;
        right.y = 0f;
        right.Normalize();

        var delta = (forward * input.z + right * input.x) * (panSpeed * Time.deltaTime);
        _pivotTransform.position += delta;
    }

    void HandleZoom()
    {
        var scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.01f)
            return;

        orbitDistance = Mathf.Clamp(orbitDistance - scroll * zoomSpeed, minDistance, maxDistance);
    }

    void HandleSelectPlayer()
    {
        if (!Input.GetMouseButtonDown(0) || IsPointerOverUi())
            return;

        var ray = controlledCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, selectionRayDistance, selectionLayers, QueryTriggerInteraction.Ignore))
            return;

        if (!hit.collider.CompareTag(playerTag))
            return;

        var player = hit.collider.GetComponentInParent<PlayerController>();
        if (!player)
            return;

        SetFollowTarget(player.transform);
    }

    void ApplyCameraTransform()
    {
        var rot = Quaternion.Euler(_pitch, _yaw, 0f);
        var offset = rot * new Vector3(0f, 0f, -orbitDistance);
        controlledCamera.transform.position = _pivotTransform.position + offset;
        controlledCamera.transform.rotation = Quaternion.LookRotation(_pivotTransform.position - controlledCamera.transform.position, Vector3.up);
    }

    static bool IsPointerOverUi()
    {
        return EventSystem.current && EventSystem.current.IsPointerOverGameObject();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!_pivotTransform)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_pivotTransform.position, 0.35f);

        if (controlledCamera)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_pivotTransform.position, controlledCamera.transform.position);
        }
    }
#endif
}
