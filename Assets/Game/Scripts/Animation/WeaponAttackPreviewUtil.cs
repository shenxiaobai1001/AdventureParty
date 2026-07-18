using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shared cleanup / weapon-mesh helpers for the attack preview scene.
/// </summary>
public static class WeaponAttackPreviewUtil
{
    static void Kill(UnityEngine.Object obj)
    {
        if (!obj)
            return;

        // Always immediate while stripping freshly instantiated prefabs so
        // PerfectLookAt / SuperCharacterController never tick an Update.
        UnityEngine.Object.DestroyImmediate(obj);
    }

    public static void StripGameplayScripts(GameObject actor)
    {
        // Destroy behaviours BEFORE NavMeshAgent / CharacterController — several RPG pack
        // scripts use [RequireComponent] and block agent removal otherwise.
        var behaviours = actor.GetComponentsInChildren<MonoBehaviour>(true);
        for (var i = behaviours.Length - 1; i >= 0; i--)
        {
            var behaviour = behaviours[i];
            if (behaviour)
                Kill(behaviour);
        }

        foreach (var agent in actor.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>(true))
            Kill(agent);

        foreach (var rb in actor.GetComponentsInChildren<Rigidbody>(true))
            Kill(rb);

        foreach (var cc in actor.GetComponentsInChildren<CharacterController>(true))
            Kill(cc);

        foreach (var col in actor.GetComponentsInChildren<Collider>(true))
            Kill(col);

        foreach (var audio in actor.GetComponentsInChildren<AudioSource>(true))
            Kill(audio);
    }

    public static void SoftenRendererCost(GameObject actor)
    {
        foreach (var renderer in actor.GetComponentsInChildren<Renderer>(true))
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }

        foreach (var smr in actor.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            smr.updateWhenOffscreen = false;
    }

    public static void ApplyWeaponKit(GameObject actor, WeaponAttackPreviewKit kit)
    {
        var weapons = CollectWeaponTransforms(actor);
        foreach (var t in weapons)
            t.gameObject.SetActive(false);

        switch (kit)
        {
            case WeaponAttackPreviewKit.None:
                break;
            case WeaponAttackPreviewKit.TwoHandSword:
                EnableNamed(weapons, "2Hand-Sword");
                break;
            case WeaponAttackPreviewKit.TwoHandSpear:
                EnableNamed(weapons, "2Hand-Spear");
                break;
            case WeaponAttackPreviewKit.TwoHandAxe:
                EnableNamed(weapons, "2Hand-Axe");
                break;
            case WeaponAttackPreviewKit.TwoHandStaff:
                EnableNamed(weapons, "Staff");
                break;
            case WeaponAttackPreviewKit.TwoHandBow:
                EnableNamed(weapons, "2Hand-Bow");
                break;
            case WeaponAttackPreviewKit.TwoHandCrossbow:
                EnableNamed(weapons, "2Hand-Crossbow");
                break;
            case WeaponAttackPreviewKit.TwoHandRifle:
                EnableNamed(weapons, "2Hand-Rifle");
                break;
            case WeaponAttackPreviewKit.SwordRight:
                EnableRightHandWeapon(weapons, "Sword");
                break;
            case WeaponAttackPreviewKit.SwordDual:
                EnableNamed(weapons, "Sword");
                break;
            case WeaponAttackPreviewKit.MaceRight:
                EnableRightHandWeapon(weapons, "Mace");
                break;
            case WeaponAttackPreviewKit.DaggerRight:
                EnableRightHandWeapon(weapons, "Dagger");
                break;
            case WeaponAttackPreviewKit.SpearRight:
                EnableNamed(weapons, "Spear");
                break;
            case WeaponAttackPreviewKit.ShieldSword:
                EnableNamed(weapons, "Shield");
                EnableRightHandWeapon(weapons, "Sword");
                break;
            case WeaponAttackPreviewKit.ItemRight:
                EnableRightHandWeapon(weapons, "Knife");
                break;
            case WeaponAttackPreviewKit.PistolRight:
                EnableRightHandWeapon(weapons, "Pistol");
                break;
        }
    }

    static List<Transform> CollectWeaponTransforms(GameObject actor)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "2Hand-Sword", "2Hand-Spear", "2Hand-Axe", "2Hand-Bow", "2Hand-Crossbow", "2Hand-Rifle",
            "Staff", "Sword", "Mace", "Dagger", "Knife", "Shield", "Spear", "Pistol",
        };

        var list = new List<Transform>();
        foreach (var t in actor.GetComponentsInChildren<Transform>(true))
        {
            if (names.Contains(t.name))
                list.Add(t);
        }

        return list;
    }

    static void EnableNamed(List<Transform> weapons, string name)
    {
        foreach (var t in weapons)
        {
            if (string.Equals(t.name, name, StringComparison.OrdinalIgnoreCase))
                t.gameObject.SetActive(true);
        }
    }

    static void EnableRightHandWeapon(List<Transform> weapons, string name)
    {
        Transform best = null;
        var bestScore = int.MinValue;

        foreach (var t in weapons)
        {
            if (!string.Equals(t.name, name, StringComparison.OrdinalIgnoreCase))
                continue;

            var score = 0;
            var p = t.parent;
            while (p)
            {
                var n = p.name;
                if (n.IndexOf("Right", StringComparison.OrdinalIgnoreCase) >= 0
                    || n.IndexOf("Hand_R", StringComparison.OrdinalIgnoreCase) >= 0
                    || n.EndsWith("_R", StringComparison.OrdinalIgnoreCase)
                    || n.IndexOf("R_Hand", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    score += 10;
                }

                if (n.IndexOf("Left", StringComparison.OrdinalIgnoreCase) >= 0
                    || n.IndexOf("Hand_L", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    score -= 10;
                }

                p = p.parent;
            }

            score += t.GetSiblingIndex();

            if (score > bestScore)
            {
                bestScore = score;
                best = t;
            }
        }

        if (best)
            best.gameObject.SetActive(true);
        else
            EnableNamed(weapons, name);
    }
}
