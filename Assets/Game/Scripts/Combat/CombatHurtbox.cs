using UnityEngine;

/// <summary>
/// Receives melee sweeps. Place on each combatant (auto-added with CombatHealth).
/// </summary>
[RequireComponent(typeof(CombatHealth))]
public class CombatHurtbox : MonoBehaviour
{
    [SerializeField] CapsuleCollider bodyCollider;
    [SerializeField] Vector3 capsuleCenter = new Vector3(0f, 0.9f, 0f);
    [SerializeField] float capsuleRadius = 0.45f;
    [SerializeField] float capsuleHeight = 1.9f;

    public CombatHealth Health { get; private set; }
    public Transform OwnerRoot => Health ? Health.transform : transform.root;

    void Awake()
    {
        Health = GetComponent<CombatHealth>();
        EnsureCollider();
    }

    public void EnsureCollider()
    {
        var hurtLayer = CombatLayers.Hurt;
        if (hurtLayer < 0)
            hurtLayer = 0;

        Transform host = transform.Find("CombatHurtbox");
        if (!host)
        {
            var go = new GameObject("CombatHurtbox");
            go.transform.SetParent(transform, false);
            host = go.transform;
        }

        bodyCollider = host.GetComponent<CapsuleCollider>();
        if (!bodyCollider)
            bodyCollider = host.gameObject.AddComponent<CapsuleCollider>();

        bodyCollider.isTrigger = true;
        bodyCollider.center = capsuleCenter;
        bodyCollider.radius = capsuleRadius;
        bodyCollider.height = capsuleHeight;
        bodyCollider.direction = 1; // Y
        host.gameObject.layer = hurtLayer;
    }
}
