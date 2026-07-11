using System.Collections.Generic;
using UnityEngine;

public static class IconStudioSkinnedMeshBaker
{
    public static List<GameObject> BakeAndDisableSources(IList<GameObject> skinnedParts, Transform parent)
    {
        var bakedParts = new List<GameObject>(skinnedParts.Count);

        foreach (var part in skinnedParts)
        {
            if (!part)
                continue;

            var skinnedRenderer = part.GetComponent<SkinnedMeshRenderer>();
            if (!skinnedRenderer)
            {
                bakedParts.Add(part);
                continue;
            }

            bakedParts.Add(Bake(skinnedRenderer, parent));
            part.SetActive(false);
        }

        return bakedParts;
    }

    static GameObject Bake(SkinnedMeshRenderer source, Transform parent)
    {
        var bakedMesh = new Mesh();
        source.BakeMesh(bakedMesh);

        var bakedObject = new GameObject(source.gameObject.name);
        bakedObject.transform.SetParent(parent, false);
        bakedObject.transform.localPosition = source.transform.localPosition;
        bakedObject.transform.localRotation = source.transform.localRotation;
        bakedObject.transform.localScale = source.transform.localScale;

        var meshFilter = bakedObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = bakedMesh;

        var meshRenderer = bakedObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = source.sharedMaterial;

        return bakedObject;
    }
}
