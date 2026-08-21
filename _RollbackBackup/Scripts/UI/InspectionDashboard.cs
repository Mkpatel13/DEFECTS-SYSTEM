using UnityEngine;
using UnityEngine.UI;
using Inspection;
using AI;
using TMPro;

namespace UI
{
    public class InspectionDashboard : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject mainPanel;
        
        [Header("UI Elements")]
        public RawImage pcbImageDisplay;
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI pcbIdText;
        
        [Header("Defect Result Elements")]
        public RectTransform boundingBox;
        public TextMeshProUGUI defectLabelText;
        public TextMeshProUGUI confidenceText;
        
        [Header("Colors")]
        public Color processingColor = Color.yellow;
        public Color passColor = Color.green;
        public Color defectColor = Color.red;

        private void Start()
        {
            HideDashboard();
            
            if (InspectionManager.Instance != null)
            {
                InspectionManager.Instance.OnInspectionStarted.AddListener(HandleInspectionStarted);
                InspectionManager.Instance.OnImageCaptured.AddListener(HandleImageCaptured);
                InspectionManager.Instance.OnPCBReleased.AddListener(HandlePCBReleased);
            }

            YOLOv8InspectionSimulator aiSim = FindFirstObjectByType<YOLOv8InspectionSimulator>();
            if (aiSim != null)
            {
                aiSim.OnDetectionComplete.AddListener(HandleDetectionComplete);
            }
        }

        private void OnDestroy()
        {
            if (InspectionManager.Instance != null)
            {
                InspectionManager.Instance.OnInspectionStarted.RemoveListener(HandleInspectionStarted);
                InspectionManager.Instance.OnImageCaptured.RemoveListener(HandleImageCaptured);
                InspectionManager.Instance.OnPCBReleased.RemoveListener(HandlePCBReleased);
            }
            
            YOLOv8InspectionSimulator aiSim = FindFirstObjectByType<YOLOv8InspectionSimulator>();
            if (aiSim != null)
            {
                aiSim.OnDetectionComplete.RemoveListener(HandleDetectionComplete);
            }
        }

        private void HandleInspectionStarted(PCBController pcb)
        {
            mainPanel.SetActive(true);
            pcbIdText.text = $"ID: {pcb.pcbId}";
            statusText.text = "CAPTURING IMAGE...";
            statusText.color = processingColor;
            
            boundingBox.gameObject.SetActive(false);
            defectLabelText.gameObject.SetActive(false);
            confidenceText.gameObject.SetActive(false);
        }

        private void HandleImageCaptured(PCBController pcb)
        {
            if (!mainPanel.activeInHierarchy) return;
            statusText.text = "AI PROCESSING...";
            statusText.color = processingColor;
        }

        private void HandleDetectionComplete(DetectionResult result)
        {
            if (!mainPanel.activeInHierarchy) return;

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                statusText.text = result.ErrorMessage;
                statusText.color = defectColor;
                confidenceText.gameObject.SetActive(false);
                defectLabelText.gameObject.SetActive(false);
                boundingBox.gameObject.SetActive(false);
                return;
            }

            confidenceText.gameObject.SetActive(true);
            confidenceText.text = $"Confidence: {(result.Confidence * 100f):F1}%";

            if (result.DefectDetected)
            {
                statusText.text = "DEFECT DETECTED";
                statusText.color = defectColor;
                
                defectLabelText.gameObject.SetActive(true);
                defectLabelText.text = result.DefectType;
                defectLabelText.color = defectColor;

                // Draw bounding box
                boundingBox.gameObject.SetActive(true);
                
                // Assuming boundingBox parent is the RawImage representing the PCB
                RectTransform imageRect = pcbImageDisplay.rectTransform;
                
                float w = imageRect.rect.width * result.BoundingBox.width;
                float h = imageRect.rect.height * result.BoundingBox.height;
                
                // Normalized (0,0) is top-left in standard image coords, but Unity anchor is usually center or bottom-left.
                // Assuming anchor is top-left (0,1) for boundingBox.
                float x = imageRect.rect.width * result.BoundingBox.x;
                float y = -(imageRect.rect.height * result.BoundingBox.y);
                
                boundingBox.sizeDelta = new Vector2(w, h);
                boundingBox.anchoredPosition = new Vector2(x, y);
            }
            else
            {
                statusText.text = "INSPECTION PASSED";
                statusText.color = passColor;
                
                defectLabelText.gameObject.SetActive(false);
                boundingBox.gameObject.SetActive(false);
            }
        }

        private void HandlePCBReleased(PCBController pcb)
        {
            HideDashboard();
        }

        private void HideDashboard()
        {
            if (mainPanel != null)
                mainPanel.SetActive(false);
        }
    }
}
