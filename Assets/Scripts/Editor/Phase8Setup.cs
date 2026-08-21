using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UI;

[InitializeOnLoad]
public class Phase8Setup
{
    static Phase8Setup()
    {
        EditorApplication.delayCall += RunSetupOnce;
    }

    private static void RunSetupOnce()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        string prefKey = "PhaseFinalSetup_Run_" + SceneManager.GetActiveScene().name;
        if (!EditorPrefs.GetBool(prefKey, false))
        {
            SetupPhaseFinal();
            EditorPrefs.SetBool(prefKey, true);
        }
    }

    [MenuItem("Tools/Phase 8/Run Final Setup")]
    public static void SetupPhaseFinal()
    {
        Debug.Log("--- Running Final Scene Setup ---");
        
        FixMaterials();
        FixLightingAndEnvironment();
        SetupWaypointsAndOutput();
        CreateRejectActuatorAndBin();
        FixMachineLabels();
        
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("--- Final Scene Setup Complete ---");
    }

    private static void FixMaterials()
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null) return;

        Material defaultSafeMat = new Material(urpLit) { color = new Color(0.2f, 0.2f, 0.2f) };
        Material pcbSafeMat = new Material(urpLit) { color = new Color(0.0f, 0.25f, 0.1f) };

        Renderer[] allRenderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (Renderer r in allRenderers)
        {
            if (r.gameObject.name.Contains("UI") || r.gameObject.name.Contains("Canvas")) continue;

            Material[] mats = r.sharedMaterials;
            bool changed = false;

            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null || mats[i].shader == null || 
                    mats[i].shader.name == "Hidden/InternalErrorShader" || 
                    mats[i].shader.name == "Standard")
                {
                    bool isPCB = r.gameObject.name.ToLower().Contains("pcb");
                    mats[i] = isPCB ? pcbSafeMat : defaultSafeMat;
                    changed = true;
                }
            }

            if (changed) r.sharedMaterials = mats;
        }
    }

    private static void FixLightingAndEnvironment()
    {
        // Reduce global light intensities and bloom-inducing lights
        Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light l in allLights)
        {
            if (l.type == LightType.Point || l.type == LightType.Spot)
            {
                l.intensity = Mathf.Min(l.intensity, 2.0f);
            }
        }

        // Change bright yellow floors to dark concrete
        Renderer[] allRenderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (Renderer r in allRenderers)
        {
            if (r.gameObject.name.ToLower().Contains("floor") || r.gameObject.name.ToLower().Contains("ground"))
            {
                foreach (Material m in r.sharedMaterials)
                {
                    if (m != null && m.HasProperty("_BaseColor"))
                    {
                        m.SetColor("_BaseColor", new Color(0.25f, 0.25f, 0.25f)); // Dark gray
                    }
                }
            }
        }
    }

    private static void SetupWaypointsAndOutput()
    {
        ConveyorController conveyor = Object.FindFirstObjectByType<ConveyorController>();
        if (conveyor == null) return;

        Transform waypointRoot = conveyor.transform.Find("Waypoints");
        if (waypointRoot == null)
        {
            waypointRoot = new GameObject("Waypoints").transform;
            waypointRoot.SetParent(conveyor.transform, false);
        }

        // CORRECT WAY to find belt local Y avoiding coordinate offset issues
        float beltLocalY = 0.55f;
        if (conveyor.beltRenderer != null)
        {
            Vector3 topCenter = conveyor.beltRenderer.bounds.center;
            topCenter.y = conveyor.beltRenderer.bounds.max.y;
            Vector3 localTopCenter = conveyor.transform.InverseTransformPoint(topCenter);
            beltLocalY = localTopCenter.y;
        }

        Transform beltSurface = GetOrCreateWaypoint(waypointRoot, "PCB_BeltSurface", new Vector3(0, beltLocalY, 0));

        float startZ = -4.0f;
        float endZ = 4.0f;

        Transform pcbInput = GetOrCreateWaypoint(waypointRoot, "PCB_Input", new Vector3(0, beltLocalY, startZ));
        Transform inspectionApproach = GetOrCreateWaypoint(waypointRoot, "PCB_InspectionApproach", new Vector3(0, beltLocalY, -1.0f));
        Transform inspectionCenter = GetOrCreateWaypoint(waypointRoot, "PCB_InspectionCenter", new Vector3(0, beltLocalY, 0.0f));
        Transform decisionPoint = GetOrCreateWaypoint(waypointRoot, "PCB_DecisionPoint", new Vector3(0, beltLocalY, 2.0f));
        Transform goodOutput = GetOrCreateWaypoint(waypointRoot, "PCB_GoodOutput", new Vector3(0, beltLocalY, endZ));
        
        // New Reject Waypoints
        Transform rejectStart = GetOrCreateWaypoint(waypointRoot, "PCB_RejectStart", new Vector3(1.0f, beltLocalY, 2.0f));
        Transform rejectEnd = GetOrCreateWaypoint(waypointRoot, "PCB_RejectEnd", new Vector3(2.5f, beltLocalY, 2.0f));
        Transform rejectChuteFall = GetOrCreateWaypoint(waypointRoot, "PCB_RejectChuteFall", new Vector3(3.0f, beltLocalY - 0.5f, 2.0f));

        PCBSpawner spawner = UnityEngine.Object.FindFirstObjectByType<PCBSpawner>();
        if (spawner != null)
        {
            spawner.transform.position = pcbInput.position;
            spawner.transform.rotation = pcbInput.rotation;
        }

        // Add Output Area Visuals
        Transform outputVisuals = goodOutput.Find("OutputVisuals");
        if (outputVisuals == null)
        {
            outputVisuals = new GameObject("OutputVisuals").transform;
            outputVisuals.SetParent(goodOutput, false);
            outputVisuals.localPosition = new Vector3(0, 0.1f, 0);

            Light outLight = outputVisuals.gameObject.AddComponent<Light>();
            outLight.type = LightType.Point;
            outLight.color = Color.green;
            outLight.range = 1f;
            outLight.intensity = 1.0f;
        }
    }

    private static void CreateRejectActuatorAndBin()
    {
        ConveyorController conveyor = UnityEngine.Object.FindFirstObjectByType<ConveyorController>();
        if (conveyor == null) return;
        Transform waypointRoot = conveyor.transform.Find("Waypoints");
        if (waypointRoot == null) return;
        Transform decisionPoint = waypointRoot.Find("PCB_DecisionPoint");
        if (decisionPoint == null) return;

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        Material darkMetal = new Material(urpLit) { color = new Color(0.15f, 0.15f, 0.15f) };
        Material beltMat = new Material(urpLit) { color = new Color(0.1f, 0.1f, 0.1f) };

        // 1. Actuator
        Transform actuatorObj = conveyor.transform.Find("RejectActuator");
        if (actuatorObj == null)
        {
            actuatorObj = new GameObject("RejectActuator").transform;
            actuatorObj.SetParent(conveyor.transform, false);
            
            Vector3 actPos = decisionPoint.localPosition;
            actPos.x = -0.5f; 
            actPos.y += 0.05f; 
            actuatorObj.localPosition = actPos;
            
            GameObject baseCyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseCyl.transform.SetParent(actuatorObj, false);
            baseCyl.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            baseCyl.transform.localEulerAngles = new Vector3(0, 0, 90);
            UnityEngine.Object.DestroyImmediate(baseCyl.GetComponent<Collider>());
            baseCyl.GetComponent<Renderer>().material = darkMetal;
            
            GameObject rod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rod.name = "PusherRod";
            rod.transform.SetParent(actuatorObj, false);
            rod.transform.localScale = new Vector3(0.1f, 0.4f, 0.1f);
            rod.transform.localEulerAngles = new Vector3(0, 0, 90);
            rod.transform.localPosition = new Vector3(0.4f, 0, 0);
            UnityEngine.Object.DestroyImmediate(rod.GetComponent<Collider>());
            rod.GetComponent<Renderer>().material = darkMetal;
            
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.transform.SetParent(rod.transform, false);
            plate.transform.localScale = new Vector3(2f, 0.2f, 2f);
            plate.transform.localPosition = new Vector3(0, 1f, 0);
            UnityEngine.Object.DestroyImmediate(plate.GetComponent<Collider>());
            plate.GetComponent<Renderer>().material = darkMetal;

            var script = actuatorObj.gameObject.AddComponent<RejectActuatorController>();
            script.pusherRod = rod.transform;
            script.startLocalPos = rod.transform.localPosition;
            script.extendedLocalPos = rod.transform.localPosition + new Vector3(1.2f, 0, 0); // push all the way
        }

        // 2. Transfer Bridge
        Transform bridge = conveyor.transform.Find("RejectTransferBridge");
        if (bridge == null)
        {
            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
            b.name = "RejectTransferBridge";
            b.transform.SetParent(conveyor.transform, false);
            // Bridge from main conveyor (x=0) to reject conveyor (x=1.0)
            b.transform.localPosition = new Vector3(0.5f, decisionPoint.localPosition.y - 0.01f, decisionPoint.localPosition.z);
            b.transform.localScale = new Vector3(0.8f, 0.02f, 0.5f);
            b.GetComponent<Renderer>().material = darkMetal;
        }

        // 3. Reject Conveyor
        Transform rConveyor = conveyor.transform.Find("RejectConveyorBody");
        if (rConveyor == null)
        {
            GameObject rc = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rc.name = "RejectConveyorBody";
            rc.transform.SetParent(conveyor.transform, false);
            // From x=1.0 to x=2.5
            rc.transform.localPosition = new Vector3(1.75f, decisionPoint.localPosition.y - 0.02f, decisionPoint.localPosition.z);
            rc.transform.localScale = new Vector3(1.5f, 0.04f, 0.5f);
            rc.GetComponent<Renderer>().material = beltMat;
        }

        // 4. Reject Chute
        Transform chute = conveyor.transform.Find("RejectChute");
        if (chute == null)
        {
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.name = "RejectChute";
            c.transform.SetParent(conveyor.transform, false);
            // From x=2.5 to x=3.0, sloping down
            c.transform.localPosition = new Vector3(2.75f, decisionPoint.localPosition.y - 0.25f, decisionPoint.localPosition.z);
            c.transform.localScale = new Vector3(0.6f, 0.02f, 0.5f);
            c.transform.localEulerAngles = new Vector3(0, 0, -45f); // slope down to the right
            c.GetComponent<Renderer>().material = darkMetal;
        }

        // 5. Bin
        Transform binObj = conveyor.transform.Find("RejectBin");
        if (binObj == null)
        {
            binObj = new GameObject("RejectBin").transform;
            binObj.SetParent(conveyor.transform, false);
            
            Vector3 binPos = new Vector3(3.3f, decisionPoint.localPosition.y - 0.8f, decisionPoint.localPosition.z);
            binObj.localPosition = binPos;

            GameObject binBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            binBase.transform.SetParent(binObj, false);
            binBase.transform.localScale = new Vector3(1.0f, 0.8f, 1.0f);
            binBase.GetComponent<Renderer>().material = darkMetal;
            
            Collider col = binBase.GetComponent<Collider>();
            col.isTrigger = true; // allow falling inside
        }
    }

    private static void FixMachineLabels()
    {
        // Find all TextMesh/TextMeshPro and apply MachineFocusController
        // Ensure no giant floating text
        TextMesh[] tms = Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None);
        foreach (var tm in tms)
        {
            if (tm.gameObject.name.Contains("Label") || tm.gameObject.name.Contains("Title"))
            {
                // Ensure text is small
                tm.characterSize = 0.05f;
                tm.fontSize = 48;
                
                // Add focus controller if missing
                Transform parentMachine = tm.transform.parent;
                if (parentMachine != null)
                {
                    MachineFocusController mfc = parentMachine.GetComponentInParent<MachineFocusController>();
                    if (mfc == null)
                    {
                        mfc = parentMachine.gameObject.AddComponent<MachineFocusController>();
                        mfc.focusCanvas = tm.gameObject;
                        mfc.interactionDistance = 5.0f;
                    }
                    else
                    {
                        mfc.focusCanvas = tm.gameObject;
                    }
                }
            }
        }
    }

    private static Transform GetOrCreateWaypoint(Transform root, string name, Vector3 localPosition)
    {
        Transform wp = root.Find(name);
        if (wp == null)
        {
            wp = new GameObject(name).transform;
            wp.SetParent(root, false);
        }
        wp.localPosition = localPosition;
        return wp;
    }
}
