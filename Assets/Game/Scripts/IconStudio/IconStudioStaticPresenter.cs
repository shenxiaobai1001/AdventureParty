using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static prefab presentation for slots where icon layout is easier without a skinned mannequin.
/// </summary>
public static class IconStudioStaticPresenter
{
    public static List<GameObject> Present(IconRenderEntry entry, Transform previewStage, Material materialOverride)
    {
        var parts = IconStudioPartLoader.InstantiateParts(entry.parts, previewStage, materialOverride);
        IconStudioLayout.Apply(entry.slot, parts);
        return parts;
    }
}
