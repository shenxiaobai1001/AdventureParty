using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class WeaponIconStudioCapture
{
    public static bool CaptureToPng(
        Camera camera,
        Transform stageRoot,
        WeaponIconRenderEntry entry,
        out string savedPath)
    {
        savedPath = null;

#if !UNITY_EDITOR
        Debug.LogError("[WeaponIconStudio] PNG export is only supported in the Unity Editor.");
        return false;
#else
        if (!camera || !stageRoot || entry == null)
            return false;

        var pixelSize = WeaponIconStudioSettings.GetOutputPixelSize(entry.gridSize);
        var outputDirectory = WeaponIconStudioSettings.OutputRoot;
        EnsureDirectory(outputDirectory);

        savedPath = Path.Combine(outputDirectory, entry.FileName).Replace('\\', '/');
        var previousTarget = camera.targetTexture;
        var previousBackground = camera.backgroundColor;
        var previousClearFlags = camera.clearFlags;
        var previousOrtho = camera.orthographic;
        var previousOrthoSize = camera.orthographicSize;
        var previousPosition = camera.transform.position;
        var previousRotation = camera.transform.rotation;
        var previousAspect = camera.aspect;
        var previousNear = camera.nearClipPlane;
        var previousFar = camera.farClipPlane;

        var renderTexture = new RenderTexture(pixelSize.x, pixelSize.y, 24, RenderTextureFormat.ARGB32);
        renderTexture.antiAliasing = 4;

        try
        {
            var exportAspect = pixelSize.x / (float)pixelSize.y;
            FitCamera(camera, stageRoot, entry, exportAspect, forExport: true);

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            camera.targetTexture = renderTexture;
            camera.aspect = exportAspect;
            camera.Render();

            var previousActive = RenderTexture.active;
            RenderTexture.active = renderTexture;

            var texture = new Texture2D(pixelSize.x, pixelSize.y, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, pixelSize.x, pixelSize.y), 0, 0);
            texture.Apply();

            RenderTexture.active = previousActive;

            File.WriteAllBytes(savedPath, texture.EncodeToPNG());
            Object.Destroy(texture);

            AssetDatabase.ImportAsset(savedPath);
            ConfigureImportedSprite(savedPath);

            Debug.Log($"[WeaponIconStudio] Saved icon: {savedPath} ({pixelSize.x}x{pixelSize.y})");
            return true;
        }
        finally
        {
            camera.targetTexture = previousTarget;
            camera.backgroundColor = previousBackground;
            camera.clearFlags = previousClearFlags;
            camera.orthographic = previousOrtho;
            camera.orthographicSize = previousOrthoSize;
            camera.transform.position = previousPosition;
            camera.transform.rotation = previousRotation;
            camera.aspect = previousAspect;
            camera.nearClipPlane = previousNear;
            camera.farClipPlane = previousFar;

            if (renderTexture)
                renderTexture.Release();

            Object.Destroy(renderTexture);
        }
#endif
    }

    public static void FitCameraForPreview(Camera camera, Transform stageRoot, WeaponIconRenderEntry entry)
    {
        if (!camera || !stageRoot || entry == null)
            return;

        var previewAspect = camera.pixelWidth > 0 && camera.pixelHeight > 0
            ? camera.pixelWidth / (float)camera.pixelHeight
            : (float)Screen.width / Screen.height;

        FitCamera(camera, stageRoot, entry, previewAspect, forExport: false);
    }

    static void FitCamera(Camera camera, Transform stageRoot, WeaponIconRenderEntry entry, float targetAspect, bool forExport)
    {
        var corners = CollectRendererCorners(stageRoot);
        if (corners.Count == 0)
            return;

        camera.orthographic = true;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 100f;
        camera.transform.rotation = Quaternion.identity;

        var worldBounds = EncapsulatePoints(corners);
        var standOff = Mathf.Max(
            WeaponIconStudioSettings.MinCameraStandOff,
            worldBounds.extents.z + WeaponIconStudioSettings.NearPlaneMargin);

        camera.transform.position = worldBounds.center + Vector3.back * standOff;

        ComputeCameraLocalBounds(camera, corners, out var minX, out var maxX, out var minY, out var maxY, out var minZ, out _);

        var centerOffsetX = (minX + maxX) * 0.5f;
        var centerOffsetY = (minY + maxY) * 0.5f;
        camera.transform.position += new Vector3(centerOffsetX, centerOffsetY, 0f);

        ComputeCameraLocalBounds(camera, corners, out minX, out maxX, out minY, out maxY, out minZ, out _);

        var nearMargin = camera.nearClipPlane + WeaponIconStudioSettings.NearPlaneMargin;
        if (minZ < nearMargin)
            camera.transform.position += Vector3.back * (nearMargin - minZ);

        ComputeCameraLocalBounds(camera, corners, out minX, out maxX, out minY, out maxY, out _, out _);

        var halfWidth = (maxX - minX) * 0.5f;
        var halfHeight = (maxY - minY) * 0.5f;
        var padding = WeaponIconStudioSettings.GetFramePadding(entry.category);
        if (!forExport)
            padding += WeaponIconStudioSettings.PreviewExtraPadding;

        var sizeForHeight = halfHeight * padding;
        var sizeForWidth = (halfWidth / targetAspect) * padding;
        camera.orthographicSize = Mathf.Max(sizeForHeight, sizeForWidth, 0.05f);
        camera.aspect = targetAspect;
    }

    static List<Vector3> CollectRendererCorners(Transform stageRoot)
    {
        var corners = new List<Vector3>(64);

        foreach (var renderer in stageRoot.GetComponentsInChildren<Renderer>(true))
        {
            if (!renderer || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                continue;

            foreach (var corner in GetBoundsCorners(renderer.bounds))
                corners.Add(corner);
        }

        return corners;
    }

    static void ComputeCameraLocalBounds(
        Camera camera,
        List<Vector3> worldCorners,
        out float minX,
        out float maxX,
        out float minY,
        out float maxY,
        out float minZ,
        out float maxZ)
    {
        minX = float.MaxValue;
        maxX = float.MinValue;
        minY = float.MaxValue;
        maxY = float.MinValue;
        minZ = float.MaxValue;
        maxZ = float.MinValue;

        foreach (var corner in worldCorners)
        {
            var local = camera.transform.InverseTransformPoint(corner);
            minX = Mathf.Min(minX, local.x);
            maxX = Mathf.Max(maxX, local.x);
            minY = Mathf.Min(minY, local.y);
            maxY = Mathf.Max(maxY, local.y);
            minZ = Mathf.Min(minZ, local.z);
            maxZ = Mathf.Max(maxZ, local.z);
        }
    }

    static Bounds EncapsulatePoints(List<Vector3> points)
    {
        var bounds = new Bounds(points[0], Vector3.zero);
        for (var i = 1; i < points.Count; i++)
            bounds.Encapsulate(points[i]);

        return bounds;
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

#if UNITY_EDITOR
    static void EnsureDirectory(string assetPath)
    {
        if (AssetDatabase.IsValidFolder(assetPath))
            return;

        var parts = assetPath.Split('/');
        var current = parts[0];
        for (var i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
    }

    static void ConfigureImportedSprite(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (!importer)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }
#endif
}
