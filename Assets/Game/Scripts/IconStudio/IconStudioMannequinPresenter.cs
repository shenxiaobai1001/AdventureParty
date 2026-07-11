using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Dresses equipment on a Synty mannequin for Icon Studio capture.
/// </summary>
public static class IconStudioMannequinPresenter
{
    public static GameObject Present(IconRenderEntry entry, Transform previewStage, Material materialOverride)
    {
        if (entry == null || !previewStage)
            return null;

#if !UNITY_EDITOR
        Debug.LogError("[IconStudio] Mannequin dressing is only supported in the Unity Editor.");
        return null;
#else
        var request = EquipmentDisplayAssembler.FromEntry(entry, materialOverride);
        var mannequin = EquipmentDisplayAssembler.Assemble(request, previewStage);
        if (mannequin)
            mannequin.name = "IconMannequin";

        return mannequin;
#endif
    }
}
