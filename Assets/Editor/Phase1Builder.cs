using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public class Phase1Builder : Editor
{
    [MenuItem("FYP/Build Phase 1")]
    public static void BuildPhase1()
    {
        // 1. Create Directories
        string[] dirs = { "Assets/Materials", "Assets/Scenes", "Assets/Prefabs/Machine", "Assets/Prefabs/Conveyor", "Assets/Prefabs/Camera", "Assets/Prefabs/Lighting", "Assets/Prefabs/Sensors", "Assets/Prefabs/Operator", "Assets/Prefabs/Server", "Assets/Prefabs/Safety", "Assets/Prefabs/Environment" };
        foreach (string dir in dirs) {
            if (!AssetDatabase.IsValidFolder(dir)) {
                string parent = Path.GetDirectoryName(dir).Replace("\\", "/");
                string folder = Path.GetFileName(dir);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
        
        // 2. Create Materials
        Material industrialMetal = CreateURPMaterial("Assets/Materials/IndustrialMetal.mat", new Color(0.2f, 0.2f, 0.2f), 0.8f, 0.2f);
        Material darkMetal = CreateURPMaterial("Assets/Materials/DarkMetal.mat", new Color(0.1f, 0.1f, 0.1f), 0.9f, 0.1f);
        Material conveyorRubber = CreateURPMaterial("Assets/Materials/ConveyorRubber.mat", new Color(0.05f, 0.05f, 0.05f), 0.5f, 0.1f);
        Material pcbGreen = CreateURPMaterial("Assets/Materials/PCBGreen.mat", new Color(0.0f, 0.4f, 0.1f), 0.7f, 0.3f);
        Material copper = CreateURPMaterial("Assets/Materials/Copper.mat", new Color(0.72f, 0.45f, 0.2f), 0.8f, 1.0f);
        Material blackPlastic = CreateURPMaterial("Assets/Materials/BlackPlastic.mat", new Color(0.05f, 0.05f, 0.05f), 0.5f, 0.0f);
        Material safetyYellow = CreateURPMaterial("Assets/Materials/SafetyYellow.mat", new Color(0.9f, 0.8f, 0.0f), 0.6f, 0.0f);
        Material ledWhite = CreateURPMaterial("Assets/Materials/LEDWhite.mat", Color.white, 0.5f, 0.0f, true);
        Material glass = CreateURPMaterial("Assets/Materials/Glass.mat", new Color(0.8f, 0.9f, 1.0f, 0.3f), 0.9f, 0.0f);
        Material serverMetal = CreateURPMaterial("Assets/Materials/ServerMetal.mat", new Color(0.15f, 0.15f, 0.16f), 0.8f, 0.5f);
        Material floorIndustrial = CreateURPMaterial("Assets/Materials/FloorIndustrial.mat", new Color(0.3f, 0.3f, 0.3f), 0.6f, 0.1f);
        
        // 3. Create Scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // 4. Create Hierarchy Roots
        GameObject envRoot = new GameObject("Environment");
        GameObject machineRoot = new GameObject("InspectionMachine");
        GameObject conveyorRoot = new GameObject("ConveyorSystem");
        GameObject stationRoot = new GameObject("InspectionStation");
        GameObject cameraRoot = new GameObject("CameraSystem");
        GameObject lightRoot = new GameObject("LightingSystem");
        GameObject operatorRoot = new GameObject("OperatorStation");
        GameObject serverRoot = new GameObject("ServerRack");
        GameObject safetyRoot = new GameObject("SafetySystem");
        GameObject rejectRoot = new GameObject("RejectArea");
        
        // Lighting
        GameObject dirLight = new GameObject("Directional Light");
        Light light = dirLight.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(0.9f, 0.95f, 1f); // Neutral white
        dirLight.transform.rotation = Quaternion.Euler(50, -30, 0);
        dirLight.transform.parent = envRoot.transform;
        
        // --- Environment ---
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "IndustrialFloor";
        floor.transform.localScale = new Vector3(3, 1, 3);
        floor.GetComponent<Renderer>().sharedMaterial = floorIndustrial;
        floor.transform.parent = envRoot.transform;

        // Path Marking
        GameObject path = GameObject.CreatePrimitive(PrimitiveType.Plane);
        path.name = "PedestrianPath";
        path.transform.localScale = new Vector3(0.5f, 1, 2.5f);
        path.transform.position = new Vector3(3.0f, 0.01f, 0);
        path.GetComponent<Renderer>().sharedMaterial = safetyYellow;
        path.transform.parent = safetyRoot.transform;
        
        // --- Inspection Machine Base ---
        GameObject machineBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        machineBase.name = "MachineBase";
        machineBase.transform.localScale = new Vector3(6, 0.2f, 10);
        machineBase.transform.position = new Vector3(0, 0.1f, 0);
        machineBase.GetComponent<Renderer>().sharedMaterial = industrialMetal;
        machineBase.transform.parent = machineRoot.transform;

        // Frame
        GameObject frame1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame1.name = "FrameSupport";
        frame1.transform.localScale = new Vector3(6, 3, 0.2f);
        frame1.transform.position = new Vector3(0, 1.5f, -4.9f);
        frame1.GetComponent<Renderer>().sharedMaterial = darkMetal;
        frame1.transform.parent = machineRoot.transform;
        
        GameObject frame2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame2.name = "FrameSupport";
        frame2.transform.localScale = new Vector3(6, 3, 0.2f);
        frame2.transform.position = new Vector3(0, 1.5f, 4.9f);
        frame2.GetComponent<Renderer>().sharedMaterial = darkMetal;
        frame2.transform.parent = machineRoot.transform;
        
        // --- Conveyor System ---
        GameObject conveyorBelt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        conveyorBelt.name = "ConveyorBelt";
        conveyorBelt.transform.localScale = new Vector3(1, 0.1f, 8);
        conveyorBelt.transform.position = new Vector3(0, 1, 0);
        conveyorBelt.GetComponent<Renderer>().sharedMaterial = conveyorRubber;
        conveyorBelt.transform.parent = conveyorRoot.transform;
        
        GameObject railL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        railL.name = "SideRailL";
        railL.transform.localScale = new Vector3(0.1f, 0.2f, 8);
        railL.transform.position = new Vector3(-0.55f, 1.05f, 0);
        railL.GetComponent<Renderer>().sharedMaterial = industrialMetal;
        railL.transform.parent = conveyorRoot.transform;
        
        GameObject railR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        railR.name = "SideRailR";
        railR.transform.localScale = new Vector3(0.1f, 0.2f, 8);
        railR.transform.position = new Vector3(0.55f, 1.05f, 0);
        railR.GetComponent<Renderer>().sharedMaterial = industrialMetal;
        railR.transform.parent = conveyorRoot.transform;
        
        GameObject motor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        motor.name = "MotorHousing";
        motor.transform.localScale = new Vector3(0.6f, 0.6f, 0.8f);
        motor.transform.position = new Vector3(-0.8f, 0.5f, 3.5f);
        motor.GetComponent<Renderer>().sharedMaterial = darkMetal;
        motor.transform.parent = conveyorRoot.transform;

        // Labels
        CreateTextMesh("PCB INPUT", new Vector3(0, 1.3f, -3.8f), conveyorRoot.transform).transform.rotation = Quaternion.Euler(90, 0, 0);
        CreateTextMesh("PCB OUTPUT", new Vector3(0, 1.3f, 3.8f), conveyorRoot.transform).transform.rotation = Quaternion.Euler(90, 0, 0);
        
        // --- Inspection Station (Gantry) ---
        GameObject p1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        p1.name = "GantryLeg";
        p1.transform.localScale = new Vector3(0.2f, 1.5f, 0.2f);
        p1.transform.position = new Vector3(-1f, 1.75f, 0);
        p1.GetComponent<Renderer>().sharedMaterial = darkMetal;
        p1.transform.parent = stationRoot.transform;

        GameObject p2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        p2.name = "GantryLeg";
        p2.transform.localScale = new Vector3(0.2f, 1.5f, 0.2f);
        p2.transform.position = new Vector3(1f, 1.75f, 0);
        p2.GetComponent<Renderer>().sharedMaterial = darkMetal;
        p2.transform.parent = stationRoot.transform;

        GameObject cross = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cross.name = "GantryCrossbar";
        cross.transform.localScale = new Vector3(2.2f, 0.2f, 0.4f);
        cross.transform.position = new Vector3(0, 2.4f, 0);
        cross.GetComponent<Renderer>().sharedMaterial = industrialMetal;
        cross.transform.parent = stationRoot.transform;

        // --- Vision Camera ---
        GameObject camBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        camBody.name = "VisionCamera";
        camBody.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);
        camBody.transform.position = new Vector3(0, 2.1f, 0);
        camBody.GetComponent<Renderer>().sharedMaterial = blackPlastic;
        camBody.transform.parent = cameraRoot.transform;
        
        GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lens.name = "Lens";
        lens.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        lens.transform.position = new Vector3(0, 1.85f, 0);
        lens.GetComponent<Renderer>().sharedMaterial = glass;
        lens.transform.parent = camBody.transform;
        CreateTextMesh("VISION CAMERA\nIMAGE CAPTURE", new Vector3(0, 2.4f, -0.2f), camBody.transform);

        // --- LED Inspection Light ---
        GameObject led = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        led.name = "LEDInspectionLight";
        led.transform.localScale = new Vector3(0.4f, 0.02f, 0.4f);
        led.transform.position = new Vector3(0, 1.7f, 0);
        led.GetComponent<Renderer>().sharedMaterial = ledWhite;
        led.transform.parent = lightRoot.transform;
        
        GameObject ledHole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ledHole.transform.localScale = new Vector3(0.3f, 0.03f, 0.3f);
        ledHole.transform.position = new Vector3(0, 1.7f, 0);
        ledHole.GetComponent<Renderer>().sharedMaterial = blackPlastic;
        ledHole.transform.parent = led.transform; // Creates a ring effect

        // --- IRSensor ---
        GameObject irRoot = new GameObject("Sensors");
        GameObject irSensor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        irSensor.name = "IRSensor";
        irSensor.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        irSensor.transform.position = new Vector3(-0.6f, 1.2f, -1.5f);
        irSensor.GetComponent<Renderer>().sharedMaterial = blackPlastic;
        irSensor.transform.parent = irRoot.transform;
        CreateTextMesh("PCB PRESENCE SENSOR", new Vector3(-1.0f, 1.4f, -1.5f), irSensor.transform).transform.rotation = Quaternion.Euler(0, -90, 0);

        // --- PCB ---
        GameObject pcb = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pcb.name = "PCB_Product";
        pcb.transform.localScale = new Vector3(0.07f, 0.005f, 0.1f);
        pcb.transform.position = new Vector3(0, 1.055f, 0);
        pcb.GetComponent<Renderer>().sharedMaterial = pcbGreen;
        pcb.transform.parent = conveyorRoot.transform;

        // --- Server Rack ---
        GameObject rack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rack.name = "RackCabinet";
        rack.transform.localScale = new Vector3(0.8f, 2.2f, 1.0f);
        rack.transform.position = new Vector3(-2.5f, 1.1f, 0);
        rack.GetComponent<Renderer>().sharedMaterial = serverMetal;
        rack.transform.parent = serverRoot.transform;
        
        string[] servers = { "REACT DASHBOARD", "SPRING BOOT", "FASTAPI", "YOLOv8 AI ENGINE", "MYSQL", "MESSAGE / API LAYER" };
        for(int i = 0; i < servers.Length; i++) {
            GameObject srv = GameObject.CreatePrimitive(PrimitiveType.Cube);
            srv.name = "ServerUnit";
            srv.transform.localScale = new Vector3(0.75f, 0.2f, 0.9f);
            srv.transform.position = new Vector3(-2.5f, 0.3f + (i * 0.3f), 0);
            srv.GetComponent<Renderer>().sharedMaterial = blackPlastic;
            srv.transform.parent = serverRoot.transform;
            CreateTextMesh(servers[i], new Vector3(-2.0f, 0.3f + (i * 0.3f), 0), srv.transform).transform.rotation = Quaternion.Euler(0, 90, 0);
        }

        // --- Operator Station ---
        GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        desk.name = "Desk";
        desk.transform.localScale = new Vector3(1.5f, 0.9f, 0.8f);
        desk.transform.position = new Vector3(2.5f, 0.45f, 0);
        desk.GetComponent<Renderer>().sharedMaterial = industrialMetal;
        desk.transform.parent = operatorRoot.transform;
        
        GameObject monitor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        monitor.name = "Monitor";
        monitor.transform.localScale = new Vector3(0.6f, 0.4f, 0.05f);
        monitor.transform.position = new Vector3(2.5f, 1.1f, -0.1f);
        monitor.GetComponent<Renderer>().sharedMaterial = blackPlastic;
        monitor.transform.parent = operatorRoot.transform;
        
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
        screen.name = "Screen";
        screen.transform.localScale = new Vector3(0.58f, 0.38f, 1f);
        screen.transform.position = new Vector3(2.5f, 1.1f, -0.126f);
        screen.GetComponent<Renderer>().sharedMaterial = pcbGreen; // Cyanish/data placeholder
        screen.transform.rotation = Quaternion.Euler(0, 180, 0);
        screen.transform.parent = monitor.transform;
        
        CreateTextMesh("OPERATOR STATION", new Vector3(2.5f, 1.4f, 0), operatorRoot.transform).transform.rotation = Quaternion.Euler(0, -90, 0);

        // --- Reject Area ---
        GameObject bin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bin.name = "RejectBin";
        bin.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        bin.transform.position = new Vector3(1.2f, 0.4f, 2.5f);
        bin.GetComponent<Renderer>().sharedMaterial = safetyYellow;
        bin.transform.parent = rejectRoot.transform;
        CreateTextMesh("REJECT AREA", new Vector3(1.2f, 1.0f, 2.5f), rejectRoot.transform);

        // --- Player ---
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0, 1.7f, -6f);
        player.AddComponent<CharacterController>();
        player.AddComponent<PlayerController>();
        
        GameObject mainCam = new GameObject("MainCamera");
        mainCam.tag = "MainCamera";
        mainCam.transform.parent = player.transform;
        mainCam.transform.localPosition = new Vector3(0, 0.6f, 0);
        mainCam.AddComponent<Camera>();
        mainCam.AddComponent<AudioListener>();

        // 5. Save Prefabs (Optional depending on how strictly we want them saved, but user requested prefab organization)
        PrefabUtility.SaveAsPrefabAsset(machineRoot, "Assets/Prefabs/Machine/InspectionMachine.prefab");
        PrefabUtility.SaveAsPrefabAsset(conveyorRoot, "Assets/Prefabs/Conveyor/ConveyorSystem.prefab");
        PrefabUtility.SaveAsPrefabAsset(stationRoot, "Assets/Prefabs/Machine/InspectionStation.prefab");
        PrefabUtility.SaveAsPrefabAsset(operatorRoot, "Assets/Prefabs/Operator/OperatorStation.prefab");
        PrefabUtility.SaveAsPrefabAsset(serverRoot, "Assets/Prefabs/Server/ServerRack.prefab");

        // 6. Save Scene
        EditorSceneManager.SaveScene(newScene, "Assets/Scenes/MainScene.unity");
        Debug.Log("Phase 1 Build Complete!");
    }

    static Material CreateURPMaterial(string path, Color color, float metallic, float smoothness, bool isEmissive = false)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null)
            mat = new Material(Shader.Find("Standard")); // fallback

        mat.color = color;
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
        if (isEmissive)
        {
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            mat.SetColor("_EmissionColor", color * 2.0f);
        }
        
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    static GameObject CreateTextMesh(string text, Vector3 position, Transform parent)
    {
        GameObject go = new GameObject("Label_" + text.Replace(" ", "_").Replace("/", ""));
        go.transform.position = position;
        go.transform.parent = parent;
        TextMesh tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.characterSize = 0.1f;
        tm.fontSize = 64;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.white;
        return go;
    }
}
