using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class WeaponIconResolver
{
    static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

    public static Sprite LoadSprite(string assetPath)
    {
        if (string.IsNullOrWhiteSpace(assetPath))
            return null;

        assetPath = assetPath.Replace('\\', '/');
        if (Cache.TryGetValue(assetPath, out var cached))
            return cached;

#if UNITY_EDITOR
        cached = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
#else
        cached = Resources.Load<Sprite>(ToResourcesPath(assetPath));
#endif

        Cache[assetPath] = cached;
        return cached;
    }

    public static string BuildAssetPath(string iconFileName)
    {
        if (string.IsNullOrWhiteSpace(iconFileName))
            return string.Empty;

        var path = iconFileName.Trim().Replace('\\', '/');
        if (path.StartsWith("Assets/"))
            return path;

        return $"{WeaponIconStudioSettings.OutputRoot}/{path}";
    }

#if !UNITY_EDITOR
    static string ToResourcesPath(string assetPath)
    {
        const string resourcesRoot = "Assets/Game/Resources/";
        if (!assetPath.StartsWith(resourcesRoot))
            return null;

        var relative = assetPath.Substring(resourcesRoot.Length);
        var dot = relative.LastIndexOf('.');
        return dot >= 0 ? relative.Substring(0, dot) : relative;
    }
#endif
}
