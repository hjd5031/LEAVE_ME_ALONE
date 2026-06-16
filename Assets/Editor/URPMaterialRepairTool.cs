using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class URPMaterialRepairTool
{
    private const string LitShaderName = "Universal Render Pipeline/Lit";
    private const string UnlitShaderName = "Universal Render Pipeline/Unlit";
    private const string ParticleUnlitShaderName = "Universal Render Pipeline/Particles/Unlit";
    private const string LogTag = "[URPMaterialRepairTool]";

    [MenuItem("Tools/URP/Report Built-in Shader Materials")]
    public static void ReportBuiltInShaderMaterials()
    {
        Material[] materials = LoadProjectMaterials();
        int urpCount = 0;
        int builtInCount = 0;
        int skippedCount = 0;
        int missingCount = 0;
        StringBuilder samples = new StringBuilder();

        foreach (Material material in materials)
        {
            if (material == null)
            {
                continue;
            }

            string assetPath = AssetDatabase.GetAssetPath(material);
            string shaderName = GetShaderName(material);

            if (IsSkippedAsset(assetPath, shaderName))
            {
                skippedCount++;
                continue;
            }

            if (IsMissingShader(shaderName))
            {
                missingCount++;
                AppendSample(samples, assetPath, shaderName);
                continue;
            }

            if (IsUrpShader(shaderName))
            {
                urpCount++;
                continue;
            }

            if (IsLikelyBuiltInShader(shaderName))
            {
                builtInCount++;
                AppendSample(samples, assetPath, shaderName);
                continue;
            }

            skippedCount++;
        }

        Debug.Log($"{LogTag} Materials={materials.Length}, URP={urpCount}, Built-in candidates={builtInCount}, Missing/Error={missingCount}, Skipped/Custom={skippedCount}\n{samples}");
    }

    [MenuItem("Tools/URP/Repair Selected Materials")]
    public static void RepairSelectedMaterials()
    {
        List<Material> selectedMaterials = new List<Material>();
        foreach (Object selectedObject in Selection.objects)
        {
            if (selectedObject is Material material)
            {
                selectedMaterials.Add(material);
            }
        }

        if (selectedMaterials.Count == 0)
        {
            Debug.LogWarning($"{LogTag} Select one or more Material assets first.");
            return;
        }

        int converted = RepairMaterials(selectedMaterials.ToArray(), allowProjectWide: false);
        Debug.Log($"{LogTag} Selected material repair complete. Converted={converted}");
    }

    [MenuItem("Tools/URP/Repair Project Built-in Materials")]
    public static void RepairProjectBuiltInMaterials()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Repair Project Built-in Materials",
            "This will update Built-in shader Material assets in the Assets folder to URP shaders. Commit or back up first.",
            "Repair",
            "Cancel");

        if (!confirmed)
        {
            return;
        }

        int converted = RepairMaterials(LoadProjectMaterials(), allowProjectWide: true);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"{LogTag} Project material repair complete. Converted={converted}");
    }

    private static int RepairMaterials(Material[] materials, bool allowProjectWide)
    {
        Shader litShader = Shader.Find(LitShaderName);
        Shader unlitShader = Shader.Find(UnlitShaderName);
        Shader particleUnlitShader = Shader.Find(ParticleUnlitShaderName);

        if (litShader == null || unlitShader == null)
        {
            Debug.LogError($"{LogTag} URP shaders were not found. Make sure Universal RP is installed and active.");
            return 0;
        }

        int converted = 0;
        foreach (Material material in materials)
        {
            if (material == null)
            {
                continue;
            }

            string assetPath = AssetDatabase.GetAssetPath(material);
            string shaderName = GetShaderName(material);

            if (IsSkippedAsset(assetPath, shaderName) || IsUrpShader(shaderName) || !IsLikelyBuiltInShader(shaderName))
            {
                continue;
            }

            Shader targetShader = PickTargetShader(shaderName, litShader, unlitShader, particleUnlitShader);
            if (targetShader == null)
            {
                continue;
            }

            Undo.RecordObject(material, "Repair material for URP");
            ConvertMaterial(material, targetShader, shaderName);
            EditorUtility.SetDirty(material);
            converted++;

            if (!allowProjectWide)
            {
                Debug.Log($"{LogTag} Converted {assetPath}: {shaderName} -> {targetShader.name}");
            }
        }

        return converted;
    }

    private static void ConvertMaterial(Material material, Shader targetShader, string oldShaderName)
    {
        Texture mainTexture = GetTexture(material, "_MainTex");
        Color color = GetColor(material, "_Color", Color.white);
        Texture bumpMap = GetTexture(material, "_BumpMap");
        Texture metallicGlossMap = GetTexture(material, "_MetallicGlossMap");
        float metallic = GetFloat(material, "_Metallic", 0f);
        float smoothness = GetFloat(material, "_Glossiness", 0.5f);

        material.shader = targetShader;

        SetTexture(material, "_BaseMap", mainTexture);
        SetColor(material, "_BaseColor", color);
        SetTexture(material, "_BumpMap", bumpMap);
        SetTexture(material, "_MetallicGlossMap", metallicGlossMap);
        SetFloat(material, "_Metallic", metallic);
        SetFloat(material, "_Smoothness", smoothness);

        if (IsTransparentShaderName(oldShaderName) || color.a < 0.999f)
        {
            SetFloat(material, "_Surface", 1f);
            material.renderQueue = 3000;
        }
    }

    private static Shader PickTargetShader(string shaderName, Shader litShader, Shader unlitShader, Shader particleUnlitShader)
    {
        if (shaderName.StartsWith("Unlit/") || shaderName.Contains("/Unlit"))
        {
            return unlitShader;
        }

        if (shaderName.StartsWith("Particles/") || shaderName.StartsWith("Legacy Shaders/Particles/"))
        {
            return particleUnlitShader != null ? particleUnlitShader : unlitShader;
        }

        return litShader;
    }

    private static Material[] LoadProjectMaterials()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });
        List<Material> materials = new List<Material>(guids.Length);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                materials.Add(material);
            }
        }

        return materials.ToArray();
    }

    private static bool IsLikelyBuiltInShader(string shaderName)
    {
        if (string.IsNullOrEmpty(shaderName))
        {
            return false;
        }

        return shaderName == "Standard" ||
               shaderName == "Standard (Specular setup)" ||
               shaderName == "Diffuse" ||
               shaderName == "Transparent/Diffuse" ||
               shaderName.StartsWith("Legacy Shaders/") ||
               shaderName.StartsWith("Mobile/") ||
               shaderName.StartsWith("Particles/") ||
               shaderName.StartsWith("Unlit/") ||
               shaderName.StartsWith("Nature/");
    }

    private static bool IsSkippedAsset(string assetPath, string shaderName)
    {
        if (assetPath.Contains("TextMesh Pro") || assetPath.Contains("TextMeshPro"))
        {
            return true;
        }

        return shaderName.Contains("Skybox") ||
               shaderName.Contains("TextMeshPro") ||
               shaderName.StartsWith("GUI/") ||
               shaderName.StartsWith("Sprites/");
    }

    private static bool IsUrpShader(string shaderName)
    {
        return shaderName.StartsWith("Universal Render Pipeline/");
    }

    private static bool IsMissingShader(string shaderName)
    {
        return string.IsNullOrEmpty(shaderName) || shaderName == "Hidden/InternalErrorShader";
    }

    private static bool IsTransparentShaderName(string shaderName)
    {
        return shaderName.Contains("Transparent") || shaderName.Contains("Alpha") || shaderName.Contains("Fade");
    }

    private static string GetShaderName(Material material)
    {
        return material.shader != null ? material.shader.name : string.Empty;
    }

    private static void AppendSample(StringBuilder builder, string assetPath, string shaderName)
    {
        if (builder.Length > 3000)
        {
            return;
        }

        builder.AppendLine($"{assetPath} | {shaderName}");
    }

    private static Texture GetTexture(Material material, string propertyName)
    {
        return material.HasProperty(propertyName) ? material.GetTexture(propertyName) : null;
    }

    private static Color GetColor(Material material, string propertyName, Color fallback)
    {
        return material.HasProperty(propertyName) ? material.GetColor(propertyName) : fallback;
    }

    private static float GetFloat(Material material, string propertyName, float fallback)
    {
        return material.HasProperty(propertyName) ? material.GetFloat(propertyName) : fallback;
    }

    private static void SetTexture(Material material, string propertyName, Texture texture)
    {
        if (texture != null && material.HasProperty(propertyName))
        {
            material.SetTexture(propertyName, texture);
        }
    }

    private static void SetColor(Material material, string propertyName, Color color)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetColor(propertyName, color);
        }
    }

    private static void SetFloat(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }
}
