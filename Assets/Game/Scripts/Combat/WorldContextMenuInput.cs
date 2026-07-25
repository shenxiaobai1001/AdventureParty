using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// RMB on Teammate/NPC/Enemy opens <see cref="UIFloatOperationPanel"/>.
/// Uses RaycastAll so ground meshes in front of a capsule do not steal the pick.
/// </summary>
[DefaultExecutionOrder(-100)]
public class WorldContextMenuInput : MonoBehaviour
{
    [SerializeField] float rayDistance = 200f;
    [SerializeField] LayerMask rayLayers = ~0;

    static WorldContextMenuInput _instance;
    static readonly RaycastHit[] Hits = new RaycastHit[32];

    public static bool ConsumedRmbThisFrame { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Object.FindFirstObjectByType<WorldContextMenuInput>())
            return;

        var host = Object.FindFirstObjectByType<KenshiCameraController>();
        if (host)
            host.gameObject.AddComponent<WorldContextMenuInput>();
        else
        {
            var go = new GameObject("WorldContextMenuInput");
            go.AddComponent<WorldContextMenuInput>();
        }
    }

    void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(this);
            return;
        }

        _instance = this;
    }

    void Update()
    {
        ConsumedRmbThisFrame = false;

        if (!Input.GetMouseButtonDown(1))
            return;

        // Only block when pointer is over our own floating panels — not every UI graphic
        // (fullscreen / HUD images would otherwise swallow all world RMB).
        if (UIFloatOperationPanel.BlocksWorldInput)
            return;

        if (UIEquipPanel.BlocksInventoryInput)
            return;

        if (IsPointerOverBlockingUi())
            return;

        var cam = Camera.main;
        if (!cam)
            return;

        var ray = cam.ScreenPointToRay(Input.mousePosition);
        var count = Physics.RaycastNonAlloc(ray, Hits, rayDistance, rayLayers, QueryTriggerInteraction.Ignore);
        if (count <= 0)
            return;

        System.Array.Sort(Hits, 0, count, ComparerByDistance.Instance);

        GameObject targetGo = null;
        for (var i = 0; i < count; i++)
        {
            var col = Hits[i].collider;
            if (!col)
                continue;

            var player = col.GetComponentInParent<PlayerController>();
            var go = player ? player.gameObject : col.transform.root.gameObject;
            if (!GameTags.IsContextTarget(go))
                continue;

            targetGo = go;
            break;
        }

        if (!targetGo)
            return;

        var panel = UIFloatOperationPanel.FindOrCreate();
        if (!panel)
            return;

        panel.Open(targetGo.transform);
        ConsumedRmbThisFrame = true;
    }

    static bool IsPointerOverBlockingUi()
    {
        if (!EventSystem.current)
            return false;

        var ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new System.Collections.Generic.List<RaycastResult>(8);
        EventSystem.current.RaycastAll(ped, results);
        for (var i = 0; i < results.Count; i++)
        {
            var go = results[i].gameObject;
            if (!go)
                continue;

            // Inventory / role panels that should keep RMB for themselves.
            if (go.GetComponentInParent<UInventoryGrid.InventoryController>())
                return true;
            if (go.name.IndexOf("Inventory", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }

    sealed class ComparerByDistance : System.Collections.Generic.IComparer<RaycastHit>
    {
        public static readonly ComparerByDistance Instance = new ComparerByDistance();

        public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
    }
}
