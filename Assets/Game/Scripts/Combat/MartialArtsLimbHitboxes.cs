using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fist / foot sweep sources for martial arts / unarmed hits.
/// </summary>
public class MartialArtsLimbHitboxes : MonoBehaviour
{
    [SerializeField] MeleeSweepSource rightHand;
    [SerializeField] MeleeSweepSource leftHand;
    [SerializeField] MeleeSweepSource rightFoot;
    [SerializeField] MeleeSweepSource leftFoot;

    [SerializeField] float handLength = 0.18f;
    [SerializeField] float handRadius = 0.07f;
    [SerializeField] float footLength = 0.22f;
    [SerializeField] float footRadius = 0.08f;

    void Awake()
    {
        EnsureLimbs();
    }

    public void EnsureLimbs()
    {
        rightHand = EnsureLimb(rightHand, "Hand_R", "LimbHit_Hand_R", handLength, handRadius);
        leftHand = EnsureLimb(leftHand, "Hand_L", "LimbHit_Hand_L", handLength, handRadius);
        rightFoot = EnsureLimb(rightFoot, "Ball_R", "LimbHit_Foot_R", footLength, footRadius)
                    ?? EnsureLimb(rightFoot, "Foot_R", "LimbHit_Foot_R", footLength, footRadius);
        leftFoot = EnsureLimb(leftFoot, "Ball_L", "LimbHit_Foot_L", footLength, footRadius)
                   ?? EnsureLimb(leftFoot, "Foot_L", "LimbHit_Foot_L", footLength, footRadius);
    }

    MeleeSweepSource EnsureLimb(MeleeSweepSource existing, string boneName, string childName, float length, float radius)
    {
        if (existing)
            return existing;

        var bone = FindBone(boneName);
        if (!bone)
            return null;

        var child = bone.Find(childName);
        if (!child)
        {
            var go = new GameObject(childName);
            child = go.transform;
            child.SetParent(bone, false);
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
        }

        var source = child.GetComponent<MeleeSweepSource>();
        if (!source)
            source = child.gameObject.AddComponent<MeleeSweepSource>();

        source.ApplyLimbDefaults(length, radius);
        // Point along bone forward-ish (+Z local). Fine for MVP.
        return source;
    }

    Transform FindBone(string boneName)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name == boneName)
                return t;
        }

        return null;
    }

    public void CollectActiveSources(List<MeleeSweepSource> into)
    {
        if (into == null)
            return;

        EnsureLimbs();
        // MVP: enable both hands + feet during any unarmed Hit window.
        // Later: map attack slot → specific limb.
        Add(into, rightHand);
        Add(into, leftHand);
        Add(into, rightFoot);
        Add(into, leftFoot);
    }

    static void Add(List<MeleeSweepSource> into, MeleeSweepSource source)
    {
        if (source && source.contributesToMelee)
            into.Add(source);
    }
}
