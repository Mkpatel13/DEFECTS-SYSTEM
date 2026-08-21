using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace Integration
{
    public class OperatorStationSetup : EditorWindow
    {
        [InitializeOnLoadMethod]
        public static void AutoRun()
        {
            if (SessionState.GetBool("OperatorStationSetupDone", false)) return;
            SessionState.SetBool("OperatorStationSetupDone", true);
            EditorApplication.delayCall += SetupOperatorStation;
        }

        [MenuItem("Tools/Setup Operator Station UI")]
        public static void SetupOperatorStation()
        {
            GameObject station = GameObject.Find("OperatorStation");
            if (station == null)
            {
                Debug.LogWarning("OperatorStation not found in scene. Cannot auto-setup UI.");
                return;
            }

            // 1. Find Existing Hierarchy (Do NOT create duplicates or rename)
            Transform monitor = FindChildRecursive(station.transform, "Monitor");
            if (monitor == null)
            {
                Debug.LogWarning("OperatorStation Monitor not found. Please ensure it exists.");
                return;
            }

            Transform screenTransform = FindChildRecursive(monitor, "Screen");
            if (screenTransform == null)
            {
                Debug.LogWarning("OperatorStation Screen not found. Please ensure it exists.");
                return;
            }

            // 2. Fix Pink/Magenta Material on the physical screen and desk/monitor
            FixMaterial(station.transform, "Desk", new Color(0.2f, 0.2f, 0.2f), 0.8f, 0.2f);
            FixMaterial(station.transform, "Monitor", new Color(0.05f, 0.05f, 0.05f), 0.1f, 0.3f);
            FixMaterial(screenTransform, new Color(0.02f, 0.02f, 0.02f), 0f, 0.8f);

            // Ensure SpringBootApiClient is in scene
            if (UnityEngine.Object.FindFirstObjectByType<SpringBootApiClient>() == null)
            {
                GameObject apiObj = new GameObject("SpringBootApiClient");
                apiObj.AddComponent<SpringBootApiClient>();
            }

            // 3. Setup UI Canvas
            Transform canvasTransform = screenTransform.Find("MonitorCanvas");
            GameObject canvasObj;
            if (canvasTransform != null)
            {
                canvasObj = canvasTransform.gameObject;
            }
            else
            {
                canvasObj = new GameObject("MonitorCanvas");
                canvasObj.transform.SetParent(screenTransform, false);
                
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.dynamicPixelsPerUnit = 100;

                canvasObj.AddComponent<GraphicRaycaster>();
            }

            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(800, 600);
            canvasRect.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
            canvasRect.localPosition = new Vector3(0, 0, -0.011f);
            canvasRect.localRotation = Quaternion.Euler(0, 0, 0);

            // Main Panel
            Transform panelTransform = canvasObj.transform.Find("MainPanel");
            GameObject panelObj;
            if (panelTransform != null)
            {
                panelObj = panelTransform.gameObject;
            }
            else
            {
                panelObj = new GameObject("MainPanel");
                panelObj.transform.SetParent(canvasObj.transform, false);
                Image panelImage = panelObj.AddComponent<Image>();
                panelImage.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);
                RectTransform panelRect = panelObj.GetComponent<RectTransform>();
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.sizeDelta = Vector2.zero;
                panelRect.anchoredPosition = Vector2.zero;
            }

            // UI Elements
            TextMeshProUGUI titleText = CreateText(panelObj.transform, "TitleText", "PCB INSPECTION MONITOR", 36, new Vector2(0, 250), new Vector2(800, 50));
            titleText.alignment = TextAlignmentOptions.Center;

            TextMeshProUGUI statusText = CreateText(panelObj.transform, "StatusText", "SYSTEM: ONLINE", 24, new Vector2(-200, 180), new Vector2(300, 40));
            statusText.color = Color.cyan;

            CreateText(panelObj.transform, "StatsLabel", "TOTAL INSPECTED", 24, new Vector2(-250, 100), new Vector2(200, 40));
            TextMeshProUGUI totalText = CreateText(panelObj.transform, "TotalValue", "0", 24, new Vector2(-100, 100), new Vector2(100, 40));
            
            CreateText(panelObj.transform, "GoodLabel", "GOOD", 24, new Vector2(-250, 50), new Vector2(200, 40));
            TextMeshProUGUI goodText = CreateText(panelObj.transform, "GoodValue", "0", 24, new Vector2(-100, 50), new Vector2(100, 40));
            goodText.color = Color.green;

            CreateText(panelObj.transform, "DefectLabel", "DEFECTIVE", 24, new Vector2(-250, 0), new Vector2(200, 40));
            TextMeshProUGUI defectText = CreateText(panelObj.transform, "DefectValue", "0", 24, new Vector2(-100, 0), new Vector2(100, 40));
            defectText.color = Color.red;

            CreateText(panelObj.transform, "LatestTitle", "LATEST INSPECTION", 28, new Vector2(200, 180), new Vector2(300, 40));
            
            CreateText(panelObj.transform, "IDLabel", "PCB ID:", 20, new Vector2(150, 120), new Vector2(100, 40));
            TextMeshProUGUI idText = CreateText(panelObj.transform, "IDValue", "-", 20, new Vector2(250, 120), new Vector2(200, 40));
            
            CreateText(panelObj.transform, "ResultLabel", "RESULT:", 20, new Vector2(150, 80), new Vector2(100, 40));
            TextMeshProUGUI resText = CreateText(panelObj.transform, "ResultValue", "-", 20, new Vector2(250, 80), new Vector2(200, 40));
            
            CreateText(panelObj.transform, "DefectTypeLabel", "DEFECT:", 20, new Vector2(150, 40), new Vector2(100, 40));
            TextMeshProUGUI typeText = CreateText(panelObj.transform, "DefectTypeValue", "-", 20, new Vector2(250, 40), new Vector2(200, 40));
            
            CreateText(panelObj.transform, "ConfLabel", "CONFIDENCE:", 20, new Vector2(150, 0), new Vector2(150, 40));
            TextMeshProUGUI confText = CreateText(panelObj.transform, "ConfValue", "-", 20, new Vector2(280, 0), new Vector2(150, 40));
            
            CreateText(panelObj.transform, "TimeLabel", "TIME:", 20, new Vector2(150, -40), new Vector2(100, 40));
            TextMeshProUGUI timeText = CreateText(panelObj.transform, "TimeValue", "-", 20, new Vector2(250, -40), new Vector2(200, 40));

            // Setup Controller and UI component
            OperatorStationUI ui = station.GetComponent<OperatorStationUI>();
            if (ui == null) ui = station.AddComponent<OperatorStationUI>();
            
            ui.mainPanel = panelObj;
            ui.statusText = statusText;
            ui.totalInspectedText = totalText;
            ui.goodCountText = goodText;
            ui.defectiveCountText = defectText;
            
            ui.lastPcbIdText = idText;
            ui.lastResultText = resText;
            ui.lastDefectText = typeText;
            ui.lastConfidenceText = confText;
            ui.lastTimeText = timeText;

            OperatorStationController controller = station.GetComponent<OperatorStationController>();
            if (controller == null) controller = station.AddComponent<OperatorStationController>();
            
            // Interaction Prompt Canvas
            GameObject promptObj = GameObject.Find("InteractionPromptCanvas");
            if (promptObj == null)
            {
                promptObj = new GameObject("InteractionPromptCanvas");
                Canvas pCanvas = promptObj.AddComponent<Canvas>();
                pCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler pScaler = promptObj.AddComponent<CanvasScaler>();
                pScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                
                TextMeshProUGUI pText = CreateText(promptObj.transform, "PromptText", "[E] VIEW INSPECTION RESULTS", 24, new Vector2(0, -100), new Vector2(400, 50));
                pText.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                Transform pTextTransform = promptObj.transform.Find("PromptText");
                if (pTextTransform != null)
                {
                    TextMeshProUGUI pt = pTextTransform.GetComponent<TextMeshProUGUI>();
                    if (pt != null) pt.text = "[E] VIEW INSPECTION RESULTS";
                }
            }
            
            controller.promptCanvas = promptObj;

            Debug.Log("Successfully setup Operator Station UI!");
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                Transform found = FindChildRecursive(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static void FixMaterial(Transform parent, string childName, Color color, float metallic, float smoothness)
        {
            Transform child = FindChildRecursive(parent, childName);
            if (child != null) FixMaterial(child, color, metallic, smoothness);
        }

        private static void FixMaterial(Transform target, Color color, float metallic, float smoothness)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer == null) return;
            
            if (renderer.sharedMaterial == null || 
                renderer.sharedMaterial.shader.name.Contains("InternalError") ||
                renderer.sharedMaterial.shader.name == "Standard")
            {
                Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
                if (urpLit != null)
                {
                    Material mat = new Material(urpLit);
                    mat.color = color;
                    mat.SetFloat("_Metallic", metallic);
                    mat.SetFloat("_Smoothness", smoothness);
                    renderer.material = mat;
                }
            }
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, int fontSize, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            Transform existing = parent.Find(name);
            GameObject textObj;
            if (existing != null)
            {
                textObj = existing.gameObject;
            }
            else
            {
                textObj = new GameObject(name);
                textObj.transform.SetParent(parent, false);
            }
            
            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = textObj.AddComponent<TextMeshProUGUI>();
            
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            
            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            
            return tmp;
        }
    }
}
