using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class GenerateZcoolHappyFontAsset
{
    const string FontPath = "Assets/Game/Font/站酷快乐体2016修订版.ttf";
    const string CharsetPath = "Assets/Game/Font/7000汉字加所有特殊符号.txt";
    const string OutputPath = "Assets/Game/Font/ZcoolHappy-2016 SDF.asset";

    const int AtlasSize = 4096;
    const int Padding = 5;

    [MenuItem("Game/Font/Generate ZcoolHappy SDF 4096")]
    public static void GenerateFromMenu()
    {
        if (!EditorUtility.DisplayDialog(
                "Generate TMP Font",
                "Generate ZcoolHappy-2016 SDF (4096) from 站酷快乐体 + charset file?",
                "Generate",
                "Cancel"))
            return;

        TMP_EditorCoroutine.StartCoroutine(GenerateCoroutine());
    }

    public static void GenerateBatch()
    {
        TMP_EditorCoroutine.StartCoroutine(GenerateCoroutine());
    }

    static IEnumerator GenerateCoroutine()
    {
        var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        var charsetFile = AssetDatabase.LoadAssetAtPath<TextAsset>(CharsetPath);

        if (!font || !charsetFile)
        {
            Debug.LogError("[GenerateZcoolHappyFontAsset] Missing font or charset file.");
            yield break;
        }

        var characters = BuildCharacterSet(charsetFile.text);
        Debug.Log($"[GenerateZcoolHappyFontAsset] Unique characters: {characters.Length}");

        yield return null;

        var fontAsset = GenerateFontAsset(font, characters);
        if (!fontAsset)
        {
            Debug.LogError("[GenerateZcoolHappyFontAsset] Failed to generate font asset.");
            yield break;
        }

        var pointSize = fontAsset.faceInfo.pointSize;
        SaveFontAsset(fontAsset, characters);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[GenerateZcoolHappyFontAsset] Created {OutputPath} (point size {pointSize}).");
    }

    static TMP_FontAsset GenerateFontAsset(Font font, string characters)
    {
        for (var pointSize = 90; pointSize >= 18; pointSize -= 2)
        {
            var fontAsset = TMP_FontAsset.CreateFontAsset(
                font,
                pointSize,
                Padding,
                GlyphRenderMode.SDFAA,
                AtlasSize,
                AtlasSize,
                AtlasPopulationMode.Static,
                true);

            if (!fontAsset)
                continue;

            var success = fontAsset.TryAddCharacters(characters, out var missing);
            if (success && string.IsNullOrEmpty(missing))
                return fontAsset;

            if (!string.IsNullOrEmpty(missing))
                Debug.Log($"[GenerateZcoolHappyFontAsset] Point size {pointSize}: missing {missing.Length} chars, retrying...");

            Object.DestroyImmediate(fontAsset);
        }

        return null;
    }

    static void SaveFontAsset(TMP_FontAsset fontAsset, string characters)
    {
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OutputPath))
            AssetDatabase.DeleteAsset(OutputPath);

        fontAsset.name = Path.GetFileNameWithoutExtension(OutputPath);
        AssetDatabase.CreateAsset(fontAsset, OutputPath);

        var atlas = fontAsset.atlasTexture;
        if (atlas)
        {
            atlas.name = fontAsset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(atlas, fontAsset);
        }

        if (fontAsset.material)
        {
            fontAsset.material.name = fontAsset.name + " Material";
            EnsureDistanceFieldMaterial(fontAsset);
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        fontAsset.creationSettings = new FontAssetCreationSettings
        {
            sourceFontFileGUID = AssetDatabase.AssetPathToGUID(FontPath),
            pointSize = fontAsset.faceInfo.pointSize,
            pointSizeSamplingMode = 1,
            padding = Padding,
            packingMode = 4,
            atlasWidth = AtlasSize,
            atlasHeight = AtlasSize,
            characterSetSelectionMode = 7,
            characterSequence = characters,
            renderMode = (int)GlyphRenderMode.SDFAA
        };

        EditorUtility.SetDirty(fontAsset);
    }

    static void EnsureDistanceFieldMaterial(TMP_FontAsset fontAsset)
    {
        var shader = Shader.Find("TextMeshPro/Distance Field");
        if (!shader || !fontAsset.material || !fontAsset.atlasTexture)
            return;

        fontAsset.material.shader = shader;
        fontAsset.material.SetTexture(ShaderUtilities.ID_MainTex, fontAsset.atlasTexture);
        fontAsset.material.SetFloat(ShaderUtilities.ID_TextureWidth, fontAsset.atlasWidth);
        fontAsset.material.SetFloat(ShaderUtilities.ID_TextureHeight, fontAsset.atlasHeight);
        fontAsset.material.SetFloat(ShaderUtilities.ID_GradientScale, fontAsset.atlasPadding + 1f);
    }

    static string BuildCharacterSet(string raw)
    {
        var unique = new HashSet<char>();
        var builder = new StringBuilder(raw.Length);

        foreach (var character in raw)
        {
            if (char.IsControl(character))
                continue;

            if (unique.Add(character))
                builder.Append(character);
        }

        for (var character = ' '; character <= '~'; character++)
        {
            if (unique.Add(character))
                builder.Append(character);
        }

        return builder.ToString();
    }
}
