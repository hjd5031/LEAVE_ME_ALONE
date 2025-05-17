using UnityEngine;
using UnityEditor;

public class FixPinkMaterials : EditorWindow
{
    [MenuItem("Tools/Fix Pink Materials")]
    public static void FixMaterials()
    {
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        int fixedCount = 0;

        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat != null && mat.shader.name == "Hidden/InternalErrorShader")
            {
                mat.shader = Shader.Find("Standard");
                fixedCount++;
            }
        }

        Debug.Log($"✔ Fixed {fixedCount} pink materials.");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}