using UnityEngine;
using UnityEditor;

public class FixPCBMaterials : EditorWindow
{
    [InitializeOnLoadMethod]
    public static void AutoRun()
    {
        if (SessionState.GetBool("FixPCBMaterialsDone", false)) return;
        SessionState.SetBool("FixPCBMaterialsDone", true);
        EditorApplication.delayCall += FixMaterials;
    }

    [MenuItem("Tools/Fix PCB Materials")]
    public static void FixMaterials()
    {
        Debug.Log("Fixing PCB Materials...");
        
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            Debug.LogError("Could not find Universal Render Pipeline/Lit shader!");
            return;
        }

        // Fix Base
        FixMaterial("Assets/Materials/PCBGreen.mat", urpLit, new Color(0.0f, 0.25f, 0.05f), 0.2f, 0.3f);
        FixMaterial("Assets/Materials/SafePCB_Green.mat", urpLit, new Color(0.0f, 0.25f, 0.05f), 0.2f, 0.3f);
        
        // Fix Traces
        FixMaterial("Assets/Materials/Copper.mat", urpLit, new Color(0.72f, 0.45f, 0.2f), 0.8f, 0.8f);
        
        // Fix ICs
        FixMaterial("Assets/Materials/BlackPlastic.mat", urpLit, new Color(0.1f, 0.1f, 0.1f), 0.1f, 0.4f);
        
        // Fix Solder
        FixMaterial("Assets/Materials/Silver.mat", urpLit, new Color(0.75f, 0.75f, 0.75f), 1.0f, 0.8f);
        FixMaterial("Assets/Materials/IndustrialMetal.mat", urpLit, new Color(0.6f, 0.6f, 0.6f), 0.9f, 0.7f);
        FixMaterial("Assets/Materials/IRON.mat", urpLit, new Color(0.5f, 0.5f, 0.5f), 0.9f, 0.6f);
        FixMaterial("Assets/Materials/DarkMetal.mat", urpLit, new Color(0.3f, 0.3f, 0.3f), 0.8f, 0.5f);

        Debug.Log("Finished fixing materials. Re-applying to prefab...");
        
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PCB/InspectionPCB.prefab");
        if (prefab != null)
        {
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                string rName = r.gameObject.name.ToLower();
                if (rName.Contains("base") || rName.Contains("board") || rName.Contains("pcb"))
                {
                    r.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/PCBGreen.mat");
                }
                else if (rName.Contains("trace") || rName.Contains("copper"))
                {
                    r.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Copper.mat");
                }
                else if (rName.Contains("solder") || rName.Contains("pad") || rName.Contains("pin"))
                {
                    r.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/IndustrialMetal.mat");
                }
                else if (rName.Contains("ic") || rName.Contains("chip") || rName.Contains("processor"))
                {
                    r.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BlackPlastic.mat");
                }
                else
                {
                    // Default small components
                    r.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/DarkMetal.mat");
                }
            }
            EditorUtility.SetDirty(prefab);
            AssetDatabase.SaveAssets();
            Debug.Log("Successfully assigned materials to prefab.");
        }
        else
        {
            Debug.LogWarning("Could not find prefab at Assets/Prefabs/PCB/InspectionPCB.prefab");
        }
    }

    private static void FixMaterial(string path, Shader shader, Color color, float metallic, float smoothness)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.shader = shader;
        }
        
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(mat);
    }
}
