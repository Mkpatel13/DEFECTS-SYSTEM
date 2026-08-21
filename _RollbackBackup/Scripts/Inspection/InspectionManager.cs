using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using AI;

namespace Inspection
{
    public class InspectionManager : MonoBehaviour
    {
        public static InspectionManager Instance { get; private set; }

        [Header("Timing Configuration")]
        [Tooltip("Time it takes to position the PCB under the camera after detection.")]
        public float positioningTime = 0.5f;
        [Tooltip("Time before the camera captures the image after positioning.")]
        public float imageCaptureDuration = 1.0f;
        [Tooltip("Time spent showing AI Processing.")]
        public float aiProcessingDuration = 1.5f;
        [Tooltip("How long the inspection result stays visible.")]
        public float resultDisplayDuration = 1.5f;

        [HideInInspector]
        public bool isOccupied = false;

        [Header("Component References")]
        public InspectionCameraController cameraController;
        public InspectionLightController lightController;
        public Transform inspectionPoint;

        [Header("Events")]
        public UnityEvent<PCBController> OnPCBDetected;
        public UnityEvent<PCBController> OnInspectionStarted;
        public UnityEvent<PCBController> OnImageCaptured;
        public UnityEvent<DetectionResult> OnInspectionCompleted;
        public UnityEvent<PCBController> OnPCBReleased;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (cameraController != null)
                cameraController.SetStatus(InspectionCameraController.CameraStatus.READY);
                
            if (inspectionPoint == null)
            {
                ConveyorController conv = UnityEngine.Object.FindFirstObjectByType<ConveyorController>();
                if (conv != null && conv.waypointsRoot != null)
                {
                    inspectionPoint = conv.waypointsRoot.Find("PCB_InspectionCenter");
                }
            }
        }

        public void HandlePCBDetected(PCBController pcb)
        {
            if (pcb == null || pcb.currentState == PCBController.PCBState.Inspecting) return;

            OnPCBDetected?.Invoke(pcb);
            StartCoroutine(InspectionSequence(pcb));
        }

        private IEnumerator InspectionSequence(PCBController pcb)
        {
            pcb.currentState = PCBController.PCBState.Inspecting;

            OnInspectionStarted?.Invoke(pcb);

            if (lightController != null) lightController.TurnOn();

            yield return new WaitForSeconds(imageCaptureDuration);

            if (cameraController != null)
            {
                cameraController.StartScanningAnimation(resultDisplayDuration);
            }

            OnImageCaptured?.Invoke(pcb);

            bool aiFinished = false;
            
            UnityAction<DetectionResult> completionHandler = (result) => 
            {
                aiFinished = true;
                this.finalResult = result;
                OnInspectionCompleted?.Invoke(result);
            };

            var aiSim = UnityEngine.Object.FindFirstObjectByType<AI.YOLOv8InspectionSimulator>();
            if (aiSim != null) aiSim.OnDetectionComplete.AddListener(completionHandler);

            while (aiSim != null && !aiFinished)
            {
                yield return null;
            }

            if (aiSim != null) aiSim.OnDetectionComplete.RemoveListener(completionHandler);
            
            // Artificial delay to make process understandable
            yield return new WaitForSeconds(aiProcessingDuration);

            if (aiFinished)
            {
                pcb.hasBeenInspected = true;
                pcb.isDefective = finalResult.DefectDetected;
                
                if (pcb.isDefective)
                {
                    DrawDefectVisualizer(pcb, finalResult);
                }
                else
                {
                    DrawGoodVisualizer(pcb);
                }
            }

            yield return new WaitForSeconds(resultDisplayDuration);

            if (lightController != null) lightController.TurnOff();
            
            Transform vis = pcb.transform.Find("DefectVisualizer");
            if (vis != null) Destroy(vis.gameObject);

            OnPCBReleased?.Invoke(pcb);
        }

        private DetectionResult finalResult;

        private void DrawDefectVisualizer(PCBController pcb, DetectionResult result)
        {
            GameObject visObj = new GameObject("DefectVisualizer");
            visObj.transform.SetParent(pcb.transform, false);
            visObj.transform.localPosition = new Vector3(0, 0.05f, 0); 

            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.SetParent(visObj.transform, false);
            box.transform.localScale = new Vector3(0.1f, 0.02f, 0.1f);
            
            box.transform.localPosition = new Vector3(
                UnityEngine.Random.Range(-0.1f, 0.1f), 
                0, 
                UnityEngine.Random.Range(-0.1f, 0.1f)
            );

            Renderer r = box.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = Color.red; 
                Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
                if (urpLit != null) r.material.shader = urpLit;
            }
            Destroy(box.GetComponent<Collider>());
            
            // Add red alert light
            Light alertLight = visObj.AddComponent<Light>();
            alertLight.type = LightType.Point;
            alertLight.color = Color.red;
            alertLight.range = 0.5f;
            alertLight.intensity = 2f;
        }

        private void DrawGoodVisualizer(PCBController pcb)
        {
            GameObject visObj = new GameObject("DefectVisualizer");
            visObj.transform.SetParent(pcb.transform, false);
            visObj.transform.localPosition = new Vector3(0, 0.05f, 0); 
            
            // Green Pass Light
            Light passLight = visObj.AddComponent<Light>();
            passLight.type = LightType.Point;
            passLight.color = Color.green;
            passLight.range = 0.5f;
            passLight.intensity = 2f;
        }
    }
}
