using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public class Phase2Builder
{
    [MenuItem("Tools/Build Phase 2 PCB Flow")]
    public static void BuildPhase2()
    {
        Debug.Log("Starting Phase 2 PCB Flow Build...");

        // Ensure directories exist
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/PCB"))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "PCB");
        }

        // 1. Create Materials
        Material pcbGreen = CreateMaterial("Assets/Materials/PCBGreen.mat", new Color(0.0f, 0.4f, 0.1f));
        Material pcbCopper = CreateMaterial("Assets/Materials/Copper.mat", new Color(0.72f, 0.45f, 0.2f));
        pcbCopper.SetFloat("_Metallic", 1.0f);
        pcbCopper.SetFloat("_Smoothness", 0.6f);
        Material icBlack = CreateMaterial("Assets/Materials/BlackPlastic.mat", new Color(0.1f, 0.1f, 0.1f));
        
        Material irOff = CreateMaterial("Assets/Materials/IROff.mat", new Color(0.2f, 0.0f, 0.0f));
        Material irOn = CreateMaterial("Assets/Materials/IRON.mat", new Color(0.0f, 1.0f, 0.0f));
        irOn.EnableKeyword("_EMISSION");
        irOn.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        irOn.SetColor("_EmissionColor", new Color(0.0f, 1.0f, 0.0f) * 2.0f);

        // 2. Build PCB Prefab
        Debug.Log("Building PCB Prefab...");
        GameObject pcbRoot = new GameObject("InspectionPCB");
        PCBController pcbController = pcbRoot.AddComponent<PCBController>();
        BoxCollider pcbCollider = pcbRoot.AddComponent<BoxCollider>();
        pcbCollider.size = new Vector3(0.1f, 0.005f, 0.07f);
        Rigidbody pcbRb = pcbRoot.AddComponent<Rigidbody>();
        pcbRb.isKinematic = true;
        pcbRb.useGravity = false;
        pcbRoot.tag = "Untagged";

        // PCB Board
        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = "PCB_Board";
        board.transform.parent = pcbRoot.transform;
        board.transform.localPosition = Vector3.zero;
        board.transform.localScale = new Vector3(0.1f, 0.0016f, 0.07f);
        board.GetComponent<Renderer>().sharedMaterial = pcbGreen;
        Object.DestroyImmediate(board.GetComponent<Collider>());

        // Main IC
        GameObject icMain = GameObject.CreatePrimitive(PrimitiveType.Cube);
        icMain.name = "IC_Main";
        icMain.transform.parent = pcbRoot.transform;
        icMain.transform.localPosition = new Vector3(0.01f, 0.0015f, 0.0f);
        icMain.transform.localScale = new Vector3(0.03f, 0.002f, 0.03f);
        icMain.GetComponent<Renderer>().sharedMaterial = icBlack;
        Object.DestroyImmediate(icMain.GetComponent<Collider>());

        // Secondary IC
        GameObject icSec = GameObject.CreatePrimitive(PrimitiveType.Cube);
        icSec.name = "IC_Secondary";
        icSec.transform.parent = pcbRoot.transform;
        icSec.transform.localPosition = new Vector3(-0.02f, 0.0015f, -0.015f);
        icSec.transform.localScale = new Vector3(0.015f, 0.0015f, 0.02f);
        icSec.GetComponent<Renderer>().sharedMaterial = icBlack;
        Object.DestroyImmediate(icSec.GetComponent<Collider>());

        // Connector
        GameObject connector = GameObject.CreatePrimitive(PrimitiveType.Cube);
        connector.name = "Connectors";
        connector.transform.parent = pcbRoot.transform;
        connector.transform.localPosition = new Vector3(-0.045f, 0.002f, 0.0f);
        connector.transform.localScale = new Vector3(0.005f, 0.004f, 0.05f);
        connector.GetComponent<Renderer>().sharedMaterial = icBlack;
        Object.DestroyImmediate(connector.GetComponent<Collider>());

        // Some Solder Pads / Resistors
        for (int i = 0; i < 5; i++)
        {
            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.name = "SolderPads_" + i;
            pad.transform.parent = pcbRoot.transform;
            pad.transform.localPosition = new Vector3(0.03f, 0.0009f, -0.02f + (i * 0.01f));
            pad.transform.localScale = new Vector3(0.005f, 0.0002f, 0.005f);
            pad.GetComponent<Renderer>().sharedMaterial = pcbCopper;
            Object.DestroyImmediate(pad.GetComponent<Collider>());
        }

        // Save Prefab
        string prefabPath = "Assets/Prefabs/PCB/InspectionPCB.prefab";
        PrefabUtility.SaveAsPrefabAsset(pcbRoot, prefabPath);
        Object.DestroyImmediate(pcbRoot);
        Debug.Log("PCB Prefab saved.");

        // 3. Open MainScene
        Debug.Log("Modifying MainScene...");
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");

        // 4. Set up Conveyor
        GameObject conveyor = GameObject.Find("ConveyorSystem");
        if (conveyor != null)
        {
            ConveyorController convController = conveyor.GetComponent<ConveyorController>();
            if (convController == null)
            {
                convController = conveyor.AddComponent<ConveyorController>();
            }

            // Find the belt to assign material scrolling robustly
            Transform belt = null;
            string[] possibleNames = { "ConveyorBelt", "Belt", "BeltSurface", "ConveyorSurface", "BeltMesh" };
            
            foreach (string n in possibleNames)
            {
                belt = conveyor.transform.Find(n);
                if (belt != null) break;
            }

            if (belt == null)
            {
                // Fallback: search recursively if not an immediate child
                foreach (Transform child in conveyor.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name.Contains("Belt"))
                    {
                        belt = child;
                        break;
                    }
                }
            }

            if (belt != null)
            {
                convController.beltRenderer = belt.GetComponent<Renderer>();
                convController.beltMaterialIndex = 0;
            }
            else
            {
                Debug.LogWarning("Belt not found in ConveyorSystem! Make sure the belt GameObject is named ConveyorBelt or Belt.");
            }
        }
        else
        {
            Debug.LogError("ConveyorSystem not found in MainScene!");
        }

        // 5. Set up PCB Spawner
        GameObject pcbSpawnPoint = GameObject.Find("PCBSpawnPoint");
        if (pcbSpawnPoint == null)
        {
            pcbSpawnPoint = new GameObject("PCBSpawnPoint");
        }
        // Position at input of conveyor (adjusting Z based on standard 8m length, zero center)
        pcbSpawnPoint.transform.position = new Vector3(0, 1.0f + 0.0025f, -3.8f);
        pcbSpawnPoint.transform.rotation = Quaternion.identity;
        
        PCBSpawner spawner = pcbSpawnPoint.GetComponent<PCBSpawner>();
        if (spawner == null)
        {
            spawner = pcbSpawnPoint.AddComponent<PCBSpawner>();
        }
        spawner.pcbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        // 6. Set up Inspection Zone
        GameObject visionCamera = GameObject.Find("VisionCamera");
        if (visionCamera != null)
        {
            GameObject inspectionZone = GameObject.Find("InspectionZone");
            if (inspectionZone == null)
            {
                inspectionZone = new GameObject("InspectionZone");
                inspectionZone.transform.parent = visionCamera.transform.parent; // Attach to InspectionStation
            }
            // Position directly under the camera, on the conveyor
            Vector3 camPos = visionCamera.transform.position;
            inspectionZone.transform.position = new Vector3(camPos.x, 1.05f, camPos.z);
            
            BoxCollider zoneCollider = inspectionZone.GetComponent<BoxCollider>();
            if (zoneCollider == null)
            {
                zoneCollider = inspectionZone.AddComponent<BoxCollider>();
            }
            zoneCollider.isTrigger = true;
            zoneCollider.size = new Vector3(0.5f, 0.5f, 0.5f);

            if (inspectionZone.GetComponent<InspectionZoneController>() == null)
            {
                inspectionZone.AddComponent<InspectionZoneController>();
            }
        }
        else
        {
            Debug.LogWarning("VisionCamera not found!");
        }

        // 7. Set up IR Sensor
        GameObject irSensor = GameObject.Find("IRSensor");
        if (irSensor != null)
        {
            // Add trigger collider for detection
            BoxCollider irCollider = irSensor.GetComponent<BoxCollider>();
            if (irCollider == null)
            {
                irCollider = irSensor.AddComponent<BoxCollider>();
            }
            irCollider.isTrigger = true;
            // Stretch the trigger across the conveyor
            irCollider.size = new Vector3(1.0f, 0.5f, 0.1f);
            irCollider.center = new Vector3(0, -0.2f, 0); // shift down from sensor top

            // Create Indicator visual
            Transform indicator = irSensor.transform.Find("IRIndicator");
            if (indicator == null)
            {
                GameObject indObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                indObj.name = "IRIndicator";
                indObj.transform.parent = irSensor.transform;
                indObj.transform.localPosition = new Vector3(0, 0.06f, 0); // On top of sensor
                indObj.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
                Object.DestroyImmediate(indObj.GetComponent<Collider>());
                indicator = indObj.transform;
            }

            IRSensorController irLogic = irSensor.GetComponent<IRSensorController>();
            if (irLogic == null)
            {
                irLogic = irSensor.AddComponent<IRSensorController>();
            }
            irLogic.indicatorRenderer = indicator.GetComponent<Renderer>();
            irLogic.offMaterial = irOff;
            irLogic.onMaterial = irOn;
        }
        else
        {
            Debug.LogWarning("IRSensor not found!");
        }

        // 8. Save Scene
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Phase 2 Build Complete!");
    }

    private static Material CreateMaterial(string path, Color color)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.color = color;
        }
        return mat;
    }
}
