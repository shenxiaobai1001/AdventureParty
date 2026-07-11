using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class IconStudioPartLoader
{
    public static List<GameObject> InstantiateParts(IEnumerable<string> partNames, Transform parent, Material overrideMaterial)
    {
        var instances = new List<GameObject>();

        foreach (var partName in partNames)
        {
            if (string.IsNullOrWhiteSpace(partName))
                continue;

            var prefab = LoadStaticPrefab(partName.Trim());
            if (!prefab)
            {
                Debug.LogWarning($"[IconStudio] Static prefab not found for part: {partName}");
                continue;
            }

            var instance = Object.Instantiate(prefab, parent);
            instance.name = partName.Trim();

            if (overrideMaterial)
            {
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
                    renderer.sharedMaterial = overrideMaterial;
            }

            instances.Add(instance);
        }

        return instances;
    }

    public static GameObject LoadStaticPrefab(string partName)
    {
#if UNITY_EDITOR
        var path = $"{IconStudioSettings.StaticPartsRoot}/{partName}_Static.prefab";
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
#else
        Debug.LogWarning("[IconStudio] Static prefab loading is only supported in the Unity Editor.");
        return null;
#endif
    }
}
