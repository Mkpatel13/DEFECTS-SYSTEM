using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace EditorScripts
{
    public class Phase7Builder : EditorWindow
    {
        [MenuItem("Tools/Build Phase 7 End-to-End Simulation")]
        public static void BuildPhase7()
        {
            BuildDetailedPCBPrefab();
            BuildRejectStation();
            BuildGoodOutputArea();
            BuildServerRack();
            BuildMachineFocusCanvases();
            BuildDebugPanel();
            
            Debug.Log("Phase 7 Builder: Successfully built end-to-end environment components!");
        }

        private static void BuildDetailedPCBPrefab()
        {
            // Do not duplicate if it exists in scene or project. We will just create a new prefab in Assets/Resources/
            // Actually, we'll just build it as a hidden template in the scene and assign it to PCBSpawner.
            GameObject existingTemplate = GameObject.Find("PCBPrefab_Template");
            if (existingTemplate != null)
            {
                Debug.Log("PCB Template already exists.");
                return;
            }

            GameObject pcbRoot = new GameObject("PCBPrefab_Template");
            pcbRoot.SetActive(false); // Hide template
            
            // Substrate
            GameObject baseBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseBoard.transform.SetParent(pcbRoot.transform);
            baseBoard.transform.localScale = new Vector3(0.4f, 0.02f, 0.4f);
            baseBoard.transform.localPosition = Vector3.zero;
            ApplyMaterial(baseBoard, new Color(0.0f, 0.3f, 0.1f), 0.2f, 0.6f); // Dark Green

            // Big Chip (CPU/FPGA)
            GameObject chip1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chip1.transform.SetParent(pcbRoot.transform);
            chip1.transform.localScale = new Vector3(0.12f, 0.03f, 0.12f);
            chip1.transform.localPosition = new Vector3(-0.05f, 0.015f, 0.05f);
            ApplyMaterial(chip1, new Color(0.1f, 0.1f, 0.1f), 0.1f, 0.8f); // Matte Black

            // Small Chip (Memory)
            GameObject chip2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chip2.transform.SetParent(pcbRoot.transform);
            chip2.transform.localScale = new Vector3(0.08f, 0.025f, 0.12f);
            chip2.transform.localPosition = new Vector3(0.1f, 0.015f, -0.05f);
            ApplyMaterial(chip2, new Color(0.15f, 0.15f, 0.15f), 0.1f, 0.8f); // Dark Gray

            // Connector
            GameObject conn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            conn.transform.SetParent(pcbRoot.transform);
            conn.transform.localScale = new Vector3(0.3f, 0.04f, 0.05f);
            conn.transform.localPosition = new Vector3(0f, 0.02f, 0.17f);
            ApplyMaterial(conn, new Color(0.8f, 0.8f, 0.8f), 0.8f, 0.5f); // Silver/Metallic

            // Setup Controller
            PCBController pcbController = pcbRoot.AddComponent<PCBController>();
            
            // Update Spawner
            PCBSpawner spawner = UnityEngine.Object.FindFirstObjectByType<PCBSpawner>();
            if (spawner != null)
            {
                spawner.pcbPrefab = pcbRoot;
                spawner.safePcbMaterial = baseBoard.GetComponent<Renderer>().sharedMaterial; // fallback
            }
        }

        private static void BuildRejectStation()
        {
            GameObject existing = GameObject.Find("RejectStation");
            if (existing != null) return;

            GameObject rejectStation = new GameObject("RejectStation");
            
            // Assuming Inspection Point is at Z=0. We'll place Reject Station at Z=4.5 (decisionPointDistance)
            rejectStation.transform.position = new Vector3(0, 0, 4.5f);

            // Reject Bin
            GameObject bin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bin.name = "RejectBin";
            bin.transform.SetParent(rejectStation.transform);
            bin.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
            // Place it to the right of the conveyor
            bin.transform.localPosition = new Vector3(1.5f, -0.25f, 0); 
            ApplyMaterial(bin, new Color(0.8f, 0.2f, 0.1f), 0.3f, 0.3f); // Dull Red
            
            // Hollow out the bin visually by adding black cube inside
            GameObject binInner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            binInner.transform.SetParent(bin.transform);
            binInner.transform.localScale = new Vector3(0.9f, 1.1f, 0.9f);
            binInner.transform.localPosition = new Vector3(0, 0.1f, 0);
            ApplyMaterial(binInner, new Color(0.05f, 0.05f, 0.05f), 0f, 0f); // Black hole

            // Pusher Mechanism visual
            GameObject pusherBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pusherBase.name = "PusherArm";
            pusherBase.transform.SetParent(rejectStation.transform);
            pusherBase.transform.localScale = new Vector3(0.5f, 0.2f, 0.2f);
            pusherBase.transform.localPosition = new Vector3(-0.8f, 0.1f, 0);
            ApplyMaterial(pusherBase, new Color(0.6f, 0.6f, 0.6f), 0.7f, 0.5f); // Steel
            
            // Label
            CreateWorldText(rejectStation.transform, "Label", "REJECT BIN", new Vector3(1.5f, 0.1f, 0), Color.white, 5);
        }

        private static void BuildGoodOutputArea()
        {
            GameObject existing = GameObject.Find("GoodOutputStation");
            if (existing != null) return;

            GameObject outputStation = new GameObject("GoodOutputStation");
            outputStation.transform.position = new Vector3(0, 0, 7.5f); // Further down the belt

            GameObject arch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arch.name = "PassArch";
            arch.transform.SetParent(outputStation.transform);
            arch.transform.localScale = new Vector3(1.5f, 0.8f, 0.2f);
            arch.transform.localPosition = new Vector3(0, 0.4f, 0);
            ApplyMaterial(arch, new Color(0.2f, 0.2f, 0.2f), 0.5f, 0.5f);
            
            // Hollow it out
            GameObject archInner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            archInner.transform.SetParent(arch.transform);
            archInner.transform.localScale = new Vector3(0.8f, 1.1f, 1.1f);
            archInner.transform.localPosition = new Vector3(0, -0.1f, 0);
            ApplyMaterial(archInner, new Color(0.05f, 0.05f, 0.05f), 0f, 0f);

            GameObject greenLight = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            greenLight.transform.SetParent(outputStation.transform);
            greenLight.transform.localScale = new Vector3(0.1f, 0.05f, 0.1f);
            greenLight.transform.localPosition = new Vector3(0, 0.85f, 0);
            ApplyMaterial(greenLight, Color.green, 0.1f, 0.9f);
            
            CreateWorldText(outputStation.transform, "Label", "PASS / GOOD", new Vector3(0, 1.0f, 0), Color.green, 5);
        }

        private static void BuildServerRack()
        {
            GameObject existing = GameObject.Find("ServerRack");
            if (existing != null) return;

            GameObject serverRack = new GameObject("ServerRack");
            serverRack.transform.position = new Vector3(-3f, 0, 2f);

            GameObject rack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rack.transform.SetParent(serverRack.transform);
            rack.transform.localScale = new Vector3(0.8f, 2.0f, 0.8f);
            rack.transform.localPosition = new Vector3(0, 1.0f, 0);
            ApplyMaterial(rack, new Color(0.1f, 0.1f, 0.1f), 0.5f, 0.5f);

            // Server lights
            for(int i=0; i<4; i++)
            {
                GameObject sl = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sl.transform.SetParent(serverRack.transform);
                sl.transform.localScale = new Vector3(0.6f, 0.05f, 0.05f);
                sl.transform.localPosition = new Vector3(0, 0.5f + (i*0.4f), 0.4f);
                ApplyMaterial(sl, Color.cyan, 0.1f, 0.9f);
            }

            CreateWorldText(serverRack.transform, "Label", "YOLOv8 AI\nFastAPI\nSpring Boot\nMySQL", new Vector3(0, 2.2f, 0), Color.cyan, 3);
        }

        private static void BuildMachineFocusCanvases()
        {
            // Camera popup
            GameObject cam = GameObject.Find("InspectionVisionCamera");
            if (cam != null && cam.GetComponent<UI.MachineFocusController>() == null)
            {
                UI.MachineFocusController fc = cam.AddComponent<UI.MachineFocusController>();
                fc.focusCanvas = CreatePopupCanvas(cam.transform, "FocusPopup", "VISION INSPECTION CAMERA", "STATUS: PROCESSING", new Vector3(0, 0.5f, 0));
            }
        }

        private static void BuildDebugPanel()
        {
            GameObject existing = GameObject.Find("DebugPanelCanvas");
            if (existing != null) return;

            GameObject canvasObj = new GameObject("DebugPanelCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            GameObject panel = new GameObject("DebugPanel");
            panel.transform.SetParent(canvasObj.transform, false);
            Image img = panel.AddComponent<Image>();
            img.color = new Color(0, 0.2f, 0, 0.8f); // Dark green debug
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(400, 0);
            rect.anchoredPosition = new Vector2(200, 0);

            GameObject textObj = new GameObject("DebugText");
            textObj.transform.SetParent(panel.transform, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "DEBUG...";
            tmp.fontSize = 20;
            tmp.color = Color.green;
            RectTransform txtRect = textObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = new Vector2(20, 20);
            txtRect.offsetMax = new Vector2(-20, -20);

            UI.DebugPanelController controller = canvasObj.AddComponent<UI.DebugPanelController>();
            controller.debugPanel = panel;
            controller.statusText = tmp;
        }

        private static void ApplyMaterial(GameObject obj, Color color, float metallic, float smoothness)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer == null) return;
            
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit != null)
            {
                Material mat = new Material(urpLit);
                mat.color = color;
                mat.SetFloat("_Metallic", metallic);
                mat.SetFloat("_Smoothness", smoothness);
                renderer.sharedMaterial = mat;
            }
        }

        private static GameObject CreatePopupCanvas(Transform parent, string name, string title, string subtitle, Vector3 offset)
        {
            GameObject canvasObj = new GameObject(name);
            canvasObj.transform.SetParent(parent, false);
            canvasObj.transform.localPosition = offset;
            
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            
            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(300, 100);
            canvasRect.localScale = new Vector3(0.005f, 0.005f, 0.005f);

            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(canvasObj.transform, false);
            Image img = panel.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            RectTransform pRect = panel.GetComponent<RectTransform>();
            pRect.anchorMin = Vector2.zero;
            pRect.anchorMax = Vector2.one;
            pRect.sizeDelta = Vector2.zero;

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(panel.transform, false);
            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = title + "\n" + subtitle;
            tmp.fontSize = 18;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform tRect = txtObj.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;

            return canvasObj;
        }

        private static void CreateWorldText(Transform parent, string name, string text, Vector3 localPos, Color color, float size)
        {
            GameObject txtObj = new GameObject(name);
            txtObj.transform.SetParent(parent, false);
            txtObj.transform.localPosition = localPos;

            TextMeshPro tmp = txtObj.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
        }
    }
}
