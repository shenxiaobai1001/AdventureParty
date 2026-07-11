using System.Collections.Generic;
using UnityEngine;

public static class IconStudioLayout
{
    const float ShoulderMeshGap = 0.52f;
    const float ForearmColumnMeshGap = 0.42f;
    const float LegMeshGap = 0.38f;
    const float BodyArmMeshGap = 0.06f;
    const float ForearmStackGap = 0.04f;
    const float BodyArmTiltDegrees = 45f;
    const float ForearmUprightZDegrees = 90f;

    public static void Apply(SyntyEquipmentSlot slot, IList<GameObject> parts)
    {
        if (parts == null || parts.Count == 0)
            return;

        switch (slot)
        {
            case SyntyEquipmentSlot.Forearm:
                LayoutForearm(parts);
                break;
            case SyntyEquipmentSlot.Leg:
                LayoutLeg(parts);
                break;
            case SyntyEquipmentSlot.Body:
                LayoutBody(parts);
                break;
            default:
                LayoutSingle(parts);
                break;
        }

        CenterAtOrigin(parts);
    }

    static void LayoutSingle(IList<GameObject> parts)
    {
        foreach (var part in parts)
        {
            if (!part)
                continue;

            part.transform.localPosition = Vector3.zero;
            part.transform.localRotation = Quaternion.identity;
        }
    }

    static void LayoutBody(IList<GameObject> parts)
    {
        GameObject torso = null;
        GameObject shoulderRight = null;
        GameObject shoulderLeft = null;
        GameObject armRight = null;
        GameObject armLeft = null;

        foreach (var part in parts)
        {
            if (!part)
                continue;

            var name = part.name;
            if (name.Contains("Torso_"))
                torso = part;
            else if (name.Contains("ShoulderAttachRight_"))
                shoulderRight = part;
            else if (name.Contains("ShoulderAttachLeft_"))
                shoulderLeft = part;
            else if (name.Contains("ArmUpperRight_"))
                armRight = part;
            else if (name.Contains("ArmUpperLeft_"))
                armLeft = part;
        }

        if (torso)
        {
            torso.transform.localPosition = Vector3.zero;
            torso.transform.localRotation = Quaternion.identity;
        }

        LayoutBodyShoulders(shoulderRight, shoulderLeft, torso);
        PlaceBodyArm(armRight, torso, isRight: true);
        PlaceBodyArm(armLeft, torso, isRight: false);

        foreach (var part in parts)
        {
            if (!part || part == torso || part == shoulderRight || part == shoulderLeft
                || part == armRight || part == armLeft)
                continue;

            part.transform.localPosition = Vector3.zero;
            part.transform.localRotation = Quaternion.identity;
        }
    }

    static void LayoutBodyShoulders(GameObject shoulderRight, GameObject shoulderLeft, GameObject torso)
    {
        if (!shoulderRight && !shoulderLeft)
            return;

        if (!torso)
        {
            LayoutPairHorizontal(new[] { shoulderRight, shoulderLeft }, "Right", "Left", ShoulderMeshGap);
            return;
        }

        ResetLocalTransform(shoulderRight);
        ResetLocalTransform(shoulderLeft);

        GetLocalBounds(torso, out var torsoMin, out var torsoMax);
        var shoulderY = Mathf.Lerp(torsoMin.y, torsoMax.y, 0.78f);
        var shoulderZ = torsoMax.z + 0.02f;
        var gapHalf = ShoulderMeshGap * 0.5f;

        if (shoulderRight)
        {
            GetLocalBounds(shoulderRight, out var min, out _);
            var attachWorld = torso.transform.TransformPoint(new Vector3(torsoMax.x + gapHalf, shoulderY, shoulderZ));
            var pivotWorld = shoulderRight.transform.TransformPoint(new Vector3(min.x, min.y, min.z));
            shoulderRight.transform.position += attachWorld - pivotWorld;
            shoulderRight.transform.localRotation = Quaternion.identity;
        }

        if (shoulderLeft)
        {
            GetLocalBounds(shoulderLeft, out _, out var max);
            var attachWorld = torso.transform.TransformPoint(new Vector3(torsoMin.x - gapHalf, shoulderY, shoulderZ));
            var pivotWorld = shoulderLeft.transform.TransformPoint(new Vector3(max.x, max.y, max.z));
            shoulderLeft.transform.position += attachWorld - pivotWorld;
            shoulderLeft.transform.localRotation = Quaternion.identity;
        }
    }

    static void PlaceBodyArm(GameObject arm, GameObject torso, bool isRight)
    {
        if (!arm)
            return;

        ResetLocalTransform(arm);

        var tilt = isRight ? -BodyArmTiltDegrees : BodyArmTiltDegrees;
        arm.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);

        if (!torso)
            return;

        GetLocalBounds(torso, out var torsoMin, out var torsoMax);
        var shoulderY = Mathf.Lerp(torsoMin.y, torsoMax.y, 0.72f);
        var shoulderX = isRight ? torsoMax.x + BodyArmMeshGap : torsoMin.x - BodyArmMeshGap;
        var shoulderLocal = new Vector3(shoulderX, shoulderY, 0f);

        GetLocalBounds(arm, out var armMin, out var armMax);
        var attachLocal = isRight
            ? new Vector3(armMin.x, armMax.y, 0f)
            : new Vector3(armMax.x, armMax.y, 0f);

        var shoulderWorld = torso.transform.TransformPoint(shoulderLocal);
        var attachWorld = arm.transform.TransformPoint(attachLocal);
        arm.transform.position += shoulderWorld - attachWorld;
    }

    static void LayoutForearm(IList<GameObject> parts)
    {
        GameObject armLowerRight = null;
        GameObject armLowerLeft = null;
        GameObject handRight = null;
        GameObject handLeft = null;

        foreach (var part in parts)
        {
            if (!part)
                continue;

            var name = part.name;
            if (name.Contains("ArmLowerRight_"))
                armLowerRight = part;
            else if (name.Contains("ArmLowerLeft_"))
                armLowerLeft = part;
            else if (name.Contains("HandRight_"))
                handRight = part;
            else if (name.Contains("HandLeft_"))
                handLeft = part;
        }

        var rightColumn = BuildForearmColumn(armLowerRight, handRight, isLeft: false);
        var leftColumn = BuildForearmColumn(armLowerLeft, handLeft, isLeft: true);

        var rightHalf = GetColumnHorizontalHalf(rightColumn);
        var leftHalf = GetColumnHorizontalHalf(leftColumn);
        var centerOffset = (ForearmColumnMeshGap + rightHalf + leftHalf) * 0.5f;

        OffsetParts(rightColumn, Vector3.right * centerOffset);
        OffsetParts(leftColumn, Vector3.left * centerOffset);

        foreach (var part in parts)
        {
            if (!part || part == armLowerRight || part == armLowerLeft || part == handRight || part == handLeft)
                continue;

            part.transform.localPosition = Vector3.zero;
            part.transform.localRotation = Quaternion.identity;
        }
    }

    static List<GameObject> BuildForearmColumn(GameObject armLower, GameObject hand, bool isLeft)
    {
        var columnRoots = new List<GameObject>(1);
        if (!armLower)
            return columnRoots;

        columnRoots.Add(armLower);
        ResetLocalTransform(armLower);

        if (hand)
        {
            hand.transform.SetParent(armLower.transform, false);
            ResetLocalTransform(hand);
            GetLocalBounds(armLower, out var armMin, out _);
            GetLocalBounds(hand, out _, out var handMax);
            hand.transform.localPosition = new Vector3(0f, armMin.y - ForearmStackGap - handMax.y, 0f);
            hand.transform.localRotation = Quaternion.identity;
        }

        var uprightDegrees = isLeft ? -ForearmUprightZDegrees : ForearmUprightZDegrees;
        RotatePartsAroundZ(columnRoots, uprightDegrees, Vector3.zero);
        return columnRoots;
    }

    static void LayoutLeg(IList<GameObject> parts)
    {
        var primary = new List<GameObject>();
        var extras = new List<GameObject>();

        foreach (var part in parts)
        {
            if (!part)
                continue;

            if (part.name.Contains("LegRight_") || part.name.Contains("LegLeft_"))
                primary.Add(part);
            else
                extras.Add(part);
        }

        LayoutPairHorizontal(primary, "LegRight", "LegLeft", LegMeshGap);

        foreach (var extra in extras)
        {
            if (!extra)
                continue;

            var isRight = extra.name.Contains("Right");
            var isLeft = extra.name.Contains("Left");
            if (!isRight && !isLeft)
            {
                extra.transform.localPosition = Vector3.zero;
                extra.transform.localRotation = Quaternion.identity;
                continue;
            }

            AttachLegExtra(extra, parts, isRight);
        }

        AlignLegFeetBottom(parts);
    }

    static void AlignLegFeetBottom(IList<GameObject> allParts)
    {
        if (allParts == null || allParts.Count == 0)
            return;

        var targetBottom = float.MaxValue;
        var bottoms = new List<(GameObject part, float bottom)>();

        foreach (var part in allParts)
        {
            if (!part)
                continue;

            GetLocalBounds(part, out var min, out _);
            bottoms.Add((part, min.y));
            targetBottom = Mathf.Min(targetBottom, min.y);
        }

        if (targetBottom == float.MaxValue)
            return;

        foreach (var (part, bottom) in bottoms)
        {
            var deltaY = targetBottom - bottom;
            if (Mathf.Abs(deltaY) > 0.0001f)
                part.transform.localPosition += new Vector3(0f, deltaY, 0f);
        }
    }

    static void AttachLegExtra(GameObject extra, IList<GameObject> parts, bool isRight)
    {
        GameObject leg = null;
        foreach (var part in parts)
        {
            if (!part)
                continue;

            if (isRight && part.name.Contains("LegRight_"))
                leg = part;
            else if (!isRight && part.name.Contains("LegLeft_"))
                leg = part;
        }

        if (!leg)
        {
            extra.transform.localPosition = Vector3.zero;
            extra.transform.localRotation = Quaternion.identity;
            return;
        }

        ResetLocalTransform(extra);
        GetLocalBounds(leg, out var legMin, out var legMax);
        GetLocalBounds(extra, out var extraMin, out var extraMax);

        var attachY = Mathf.Lerp(legMin.y, legMax.y, 0.35f);
        var extraCenterY = (extraMin.y + extraMax.y) * 0.5f;

        extra.transform.SetParent(leg.transform, false);
        extra.transform.localPosition = new Vector3(0f, attachY - extraCenterY, 0f);
        extra.transform.localRotation = Quaternion.identity;
    }

    static void LayoutPairHorizontal(IList<GameObject> parts, string rightKey, string leftKey, float meshGap)
    {
        GameObject right = null;
        GameObject left = null;

        foreach (var part in parts)
        {
            if (!part)
                continue;

            if (part.name.Contains(rightKey))
                right = part;
            else if (part.name.Contains(leftKey))
                left = part;
        }

        ResetLocalTransform(right);
        ResetLocalTransform(left);

        var rightInnerHalf = GetHalfExtentTowardNegativeX(right);
        var leftInnerHalf = GetHalfExtentTowardPositiveX(left);
        var centerOffset = (meshGap + rightInnerHalf + leftInnerHalf) * 0.5f;

        if (right)
        {
            right.transform.localPosition = new Vector3(centerOffset, 0f, 0f);
            right.transform.localRotation = Quaternion.identity;
        }

        if (left)
        {
            left.transform.localPosition = new Vector3(-centerOffset, 0f, 0f);
            left.transform.localRotation = Quaternion.identity;
        }
    }

    static void RotatePartsAroundZ(IList<GameObject> parts, float degrees, Vector3 pivot)
    {
        var rotation = Quaternion.Euler(0f, 0f, degrees);
        foreach (var part in parts)
        {
            if (!part)
                continue;

            var offset = part.transform.localPosition - pivot;
            part.transform.localPosition = pivot + rotation * offset;
            part.transform.localRotation = rotation * part.transform.localRotation;
        }
    }

    static void OffsetParts(IEnumerable<GameObject> parts, Vector3 offset)
    {
        foreach (var part in parts)
        {
            if (!part)
                continue;

            part.transform.localPosition += offset;
        }
    }

    static float GetColumnHorizontalHalf(IList<GameObject> parts)
    {
        var maxHalf = 0f;
        foreach (var part in parts)
            maxHalf = Mathf.Max(maxHalf, GetHorizontalHalfSize(part));

        return maxHalf;
    }

    static void ResetLocalTransform(GameObject part)
    {
        if (!part)
            return;

        part.transform.localPosition = Vector3.zero;
        part.transform.localRotation = Quaternion.identity;
    }

    static float GetHorizontalHalfSize(GameObject part)
    {
        if (!part)
            return 0f;

        GetLocalBounds(part, out var min, out var max);
        return Mathf.Max(Mathf.Abs(min.x), Mathf.Abs(max.x));
    }

    static float GetHalfExtentTowardNegativeX(GameObject part)
    {
        if (!part)
            return 0f;

        GetLocalBounds(part, out var min, out _);
        return Mathf.Max(0f, -min.x);
    }

    static float GetHalfExtentTowardPositiveX(GameObject part)
    {
        if (!part)
            return 0f;

        GetLocalBounds(part, out _, out var max);
        return Mathf.Max(0f, max.x);
    }

    static void GetLocalBounds(GameObject part, out Vector3 min, out Vector3 max)
    {
        min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        var hasBounds = false;

        foreach (var renderer in part.GetComponentsInChildren<Renderer>(true))
        {
            foreach (var corner in GetBoundsCorners(renderer.bounds))
            {
                var local = part.transform.InverseTransformPoint(corner);
                min = Vector3.Min(min, local);
                max = Vector3.Max(max, local);
                hasBounds = true;
            }
        }

        if (!hasBounds)
        {
            min = Vector3.zero;
            max = Vector3.zero;
        }
    }

    static IEnumerable<Vector3> GetBoundsCorners(Bounds bounds)
    {
        var center = bounds.center;
        var extents = bounds.extents;

        for (var x = -1; x <= 1; x += 2)
        {
            for (var y = -1; y <= 1; y += 2)
            {
                for (var z = -1; z <= 1; z += 2)
                    yield return center + Vector3.Scale(extents, new Vector3(x, y, z));
            }
        }
    }

    static void CenterAtOrigin(IList<GameObject> parts)
    {
        var bounds = CalculateBounds(parts);
        if (bounds.size.sqrMagnitude <= 0f)
            return;

        var offset = -bounds.center;
        foreach (var part in parts)
        {
            if (!part)
                continue;

            part.transform.position += offset;
        }
    }

    public static Bounds CalculateBounds(IEnumerable<GameObject> parts)
    {
        var bounds = new Bounds(Vector3.zero, Vector3.zero);
        var hasBounds = false;

        foreach (var part in parts)
        {
            if (!part)
                continue;

            foreach (var renderer in part.GetComponentsInChildren<Renderer>(true))
            {
                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
        }

        if (!hasBounds)
            bounds = new Bounds(Vector3.zero, Vector3.one * 0.1f);

        return bounds;
    }
}
