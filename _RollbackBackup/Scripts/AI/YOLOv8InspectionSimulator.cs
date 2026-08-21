using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Inspection;

namespace AI
{
    public class YOLOv8InspectionSimulator : MonoBehaviour
    {
        public enum DemoMode
        {
            REAL_API,
            Automatic,
            AlwaysGood,
            AlwaysDefective
        }

        [Header("Simulation Settings")]
        [Tooltip("How the simulation determines the result.")]
        public DemoMode demonstrationMode = DemoMode.REAL_API;
        
        [Tooltip("Simulated processing time in seconds.")]
        public float simulatedProcessingDelay = 0.5f;

        [Header("Events")]
        public UnityEvent<DetectionResult> OnDetectionComplete;

        private int inspectionCounter = 0;

        private void OnEnable()
        {
            if (InspectionManager.Instance != null)
            {
                InspectionManager.Instance.OnImageCaptured.AddListener(HandleImageCaptured);
            }
            else
            {
                // In case InspectionManager hasn't initialized yet
                StartCoroutine(WaitForManager());
            }
        }

        private IEnumerator WaitForManager()
        {
            while (InspectionManager.Instance == null)
            {
                yield return null;
            }
            InspectionManager.Instance.OnImageCaptured.AddListener(HandleImageCaptured);
        }

        private void OnDisable()
        {
            if (InspectionManager.Instance != null)
            {
                InspectionManager.Instance.OnImageCaptured.RemoveListener(HandleImageCaptured);
            }
        }

        private void HandleImageCaptured(PCBController pcb)
        {
            if (demonstrationMode == DemoMode.REAL_API && Integration.FastApiClient.Instance != null)
            {
                StartCoroutine(ProcessRealAPI(pcb));
            }
            else
            {
                StartCoroutine(ProcessImageSim(pcb));
            }
        }

        private IEnumerator ProcessRealAPI(PCBController pcb)
        {
            Texture2D tex = CaptureCameraToTexture();
            
            bool isDone = false;
            DetectionResult finalResult = default;
            
            Integration.FastApiClient.Instance.Predict(tex, pcb.pcbId, 
                onSuccess: (result) => { finalResult = result; isDone = true; },
                onError: (err) => { 
                    Debug.LogError(err); 
                    // Fallback to demo if API fails
                    finalResult = GenerateDemoResult(pcb); 
                    finalResult.ErrorMessage = err; 
                    isDone = true; 
                }
            );

            while (!isDone) yield return null;
            
            Destroy(tex);

            if (Integration.SpringBootApiClient.Instance != null)
            {
                Integration.SpringBootApiClient.Instance.SaveResult(finalResult, 
                    onSuccess: () => Debug.Log("Result saved to backend."),
                    onError: (err) => {
                        Debug.LogError(err);
                        finalResult.ErrorMessage = string.IsNullOrEmpty(finalResult.ErrorMessage) ? err : finalResult.ErrorMessage + " & " + err;
                        OnDetectionComplete?.Invoke(finalResult);
                    }
                );
            }

            OnDetectionComplete?.Invoke(finalResult);
        }

        private Texture2D CaptureCameraToTexture()
        {
            Camera cam = GameObject.Find("InspectionVisionCamera")?.GetComponent<Camera>();
            if (cam == null || cam.targetTexture == null) return new Texture2D(512, 512, TextureFormat.RGB24, false);
            
            RenderTexture activeRT = RenderTexture.active;
            RenderTexture.active = cam.targetTexture;
            
            cam.Render();
            
            Texture2D tex = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
            tex.Apply();
            
            RenderTexture.active = activeRT;
            return tex;
        }

        private IEnumerator ProcessImageSim(PCBController pcb)
        {
            // Simulate AI processing delay
            yield return new WaitForSeconds(simulatedProcessingDelay);

            DetectionResult result = GenerateDemoResult(pcb);
            
            if (Integration.SpringBootApiClient.Instance != null)
            {
                Integration.SpringBootApiClient.Instance.SaveResult(result,
                    onSuccess: null,
                    onError: (err) => {
                        result.ErrorMessage = err;
                        OnDetectionComplete?.Invoke(result); // Re-invoke if error happens later
                    }
                );
            }

            OnDetectionComplete?.Invoke(result);
        }

        private DetectionResult GenerateDemoResult(PCBController pcb)
        {
            inspectionCounter++;
            bool isDefective = DetermineIfDefective();
            
            DetectionResult result = new DetectionResult
            {
                PCB_ID = pcb.pcbId,
                Inspection_ID = System.Guid.NewGuid().ToString(),
                DefectDetected = isDefective,
                InspectionTimestamp = Time.time
            };

            if (isDefective)
            {
                // Pick a random defect for demonstration
                int defectIndex = Random.Range(0, DefectDatabase.DefectClasses.Count);
                result.DefectType = DefectDatabase.DefectClasses[defectIndex];
                
                // Realistic confidence for defect (e.g., 91.5% to 98.9%)
                result.Confidence = Random.Range(0.915f, 0.989f);
                
                // Get reasonable bounding box
                result.BoundingBox = DefectDatabase.GetReasonableBoundingBox(result.DefectType);
            }
            else
            {
                result.DefectType = "None";
                // Realistic confidence for GOOD (e.g., 97.8% to 99.6%)
                result.Confidence = Random.Range(0.978f, 0.996f);
                result.BoundingBox = new Rect(0, 0, 0, 0);
            }

            return result;
        }

        private bool DetermineIfDefective()
        {
            switch (demonstrationMode)
            {
                case DemoMode.AlwaysGood:
                    return false;
                case DemoMode.AlwaysDefective:
                    return true;
                case DemoMode.Automatic:
                default:
                    // Alternate between Good and Defective for reliable demonstration
                    return inspectionCounter % 2 == 0;
            }
        }
    }
}
