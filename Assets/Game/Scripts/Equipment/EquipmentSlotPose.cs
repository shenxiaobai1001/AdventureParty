using UnityEngine;

/// <summary>
/// Shared bone pose for equipment display (Icon Studio icons and world pickup visuals).
/// </summary>
public static class EquipmentSlotPose
{
    const float BodyShoulderZ = 50f;

    static readonly Vector3 ForearmShoulderLeftPosition = new Vector3(0.228f, -0.004f, -0.191f);
    static readonly Vector3 ForearmShoulderRightPosition = new Vector3(-0.228f, 0.004f, 0.191f);
    static readonly Vector3 ForearmShoulderRotation = new Vector3(0.343f, 27.433f, 4.072f);
    const float ForearmElbowLocalZ = 85f;

    public static void Apply(Transform root, SyntyEquipmentSlot slot)
    {
        if (!root)
            return;

        switch (slot)
        {
            case SyntyEquipmentSlot.Body:
                ApplyBodyPose(root);
                break;
            case SyntyEquipmentSlot.Forearm:
                ApplyForearmPose(root);
                break;
        }
    }

    static void ApplyBodyPose(Transform root)
    {
        RotateBoneLocalZ(root, "Shoulder_R", BodyShoulderZ);
        RotateBoneLocalZ(root, "Shoulder_L", BodyShoulderZ);
    }

    static void ApplyForearmPose(Transform root)
    {
        AssignBoneLocalTransform(root, "Shoulder_L", ForearmShoulderLeftPosition, ForearmShoulderRotation);
        AssignBoneLocalTransform(root, "Shoulder_R", ForearmShoulderRightPosition, ForearmShoulderRotation);
        AssignBoneLocalEulerZ(root, "Elbow_L", ForearmElbowLocalZ);
        AssignBoneLocalEulerZ(root, "Elbow_R", ForearmElbowLocalZ);
    }

    static void AssignBoneLocalTransform(Transform root, string boneName, Vector3 localPosition, Vector3 localEuler)
    {
        var bone = FindBone(root, boneName);
        if (!bone)
            return;

        bone.localPosition = localPosition;
        bone.localRotation = Quaternion.Euler(localEuler);
    }

    static void AssignBoneLocalEulerZ(Transform root, string boneName, float localZ)
    {
        var bone = FindBone(root, boneName);
        if (!bone)
            return;

        var euler = bone.localEulerAngles;
        bone.localRotation = Quaternion.Euler(euler.x, euler.y, localZ);
    }

    static void RotateBoneLocalZ(Transform root, string boneName, float zDegrees)
    {
        var bone = FindBone(root, boneName);
        if (!bone)
            return;

        bone.localRotation *= Quaternion.Euler(0f, 0f, zDegrees);
    }

    static Transform FindBone(Transform root, string boneName)
    {
        foreach (var transform in root.GetComponentsInChildren<Transform>(true))
        {
            if (transform.name == boneName)
                return transform;
        }

        return null;
    }
}
