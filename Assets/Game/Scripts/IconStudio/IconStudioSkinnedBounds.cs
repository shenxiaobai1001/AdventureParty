using System.Collections.Generic;
using UnityEngine;

public static class IconStudioSkinnedBounds
{
    public static void PrepareSkinnedMeshes(Transform root)
    {
        if (!root)
            return;

        foreach (var smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (!smr || !smr.gameObject.activeInHierarchy || !smr.enabled)
                continue;

            smr.updateWhenOffscreen = true;
            smr.forceMatrixRecalculationPerRender = true;
        }
    }

    public static bool TryGetWorldBounds(Transform root, out Bounds bounds, bool onlyActiveRenderers = true)
    {
        bounds = default;
        var hasBounds = false;
        var bakeMesh = new Mesh();

        try
        {
            foreach (var smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (!smr)
                    continue;

                if (onlyActiveRenderers && (!smr.enabled || !smr.gameObject.activeInHierarchy))
                    continue;

                smr.BakeMesh(bakeMesh);
                if (bakeMesh.vertexCount == 0)
                    continue;

                var meshBounds = bakeMesh.bounds;
                var worldCenter = smr.transform.TransformPoint(meshBounds.center);
                var worldExtents = Vector3.Scale(meshBounds.extents, smr.transform.lossyScale);
                var worldBounds = new Bounds(worldCenter, worldExtents * 2f);

                if (!hasBounds)
                {
                    bounds = worldBounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(worldBounds);
                }
            }
        }
        finally
        {
            if (Application.isPlaying)
                Object.Destroy(bakeMesh);
            else
                Object.DestroyImmediate(bakeMesh);
        }

        return hasBounds;
    }

    public static void CollectWorldCorners(Transform root, List<Vector3> corners, bool onlyActiveRenderers = true)
    {
        corners.Clear();

        if (!TryGetWorldBounds(root, out var bounds, onlyActiveRenderers))
            return;

        var center = bounds.center;
        var extents = bounds.extents;

        for (var x = -1; x <= 1; x += 2)
        {
            for (var y = -1; y <= 1; y += 2)
            {
                for (var z = -1; z <= 1; z += 2)
                    corners.Add(center + Vector3.Scale(extents, new Vector3(x, y, z)));
            }
        }
    }
}
