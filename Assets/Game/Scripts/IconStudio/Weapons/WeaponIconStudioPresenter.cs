using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class WeaponIconStudioPresenter
{
    public static GameObject Present(WeaponIconRenderEntry entry, Transform previewStage, Material materialOverride)
    {
        if (entry == null || !previewStage || string.IsNullOrEmpty(entry.syntyPrefabPath))
            return null;

#if UNITY_EDITOR
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(entry.syntyPrefabPath);
#else
        GameObject prefab = null;
#endif
        if (!prefab)
            return null;

        var instance = Object.Instantiate(prefab, previewStage);
        instance.name = prefab.name;
        ApplyMaterial(instance, materialOverride);
        WeaponIconStudioLayout.Apply(instance, entry.category);
        WeaponIconStudioLayout.CenterAtOrigin(instance);
        return instance;
    }

    static void ApplyMaterial(GameObject root, Material materialOverride)
    {
        if (!materialOverride)
            return;

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (!renderer)
                continue;

            var materials = renderer.sharedMaterials;
            for (var i = 0; i < materials.Length; i++)
                materials[i] = materialOverride;

            renderer.sharedMaterials = materials;
        }
    }
}
