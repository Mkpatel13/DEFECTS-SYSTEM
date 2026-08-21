using UnityEngine;
using System.Collections;

namespace Inspection
{
    public class InspectionCameraController : MonoBehaviour
    {
        public enum CameraStatus { IDLE, READY, INSPECTING }

        [Header("Status Indicator")]
        public Renderer statusIndicatorRenderer;
        public Material idleMaterial;
        public Material readyMaterial;
        public Material inspectingMaterial;

        [Header("Visual Effects")]
        [Tooltip("The parent object of the scanning line or projection effect.")]
        public GameObject scanningEffectParent;
        
        [Tooltip("Speed of the scanning line over the PCB.")]
        public float scanSpeed = 2.0f;
        
        [Tooltip("How far the scanning line moves along the Z axis relative to its start.")]
        public float scanDistance = 0.2f;

        private CameraStatus currentStatus = CameraStatus.IDLE;
        private Coroutine scanningCoroutine;
        private Vector3 initialScanLocalPosition;

        private void Start()
        {
            if (scanningEffectParent != null)
            {
                initialScanLocalPosition = scanningEffectParent.transform.localPosition;
                scanningEffectParent.SetActive(false);
            }
            SetStatus(CameraStatus.IDLE);
        }

        public void SetStatus(CameraStatus status)
        {
            currentStatus = status;
            UpdateIndicator();
        }

        private void UpdateIndicator()
        {
            if (statusIndicatorRenderer != null)
            {
                switch (currentStatus)
                {
                    case CameraStatus.IDLE:
                        statusIndicatorRenderer.material = idleMaterial;
                        break;
                    case CameraStatus.READY:
                        statusIndicatorRenderer.material = readyMaterial;
                        break;
                    case CameraStatus.INSPECTING:
                        statusIndicatorRenderer.material = inspectingMaterial;
                        break;
                }
            }
        }

        public void StartScanningAnimation(float duration)
        {
            if (scanningCoroutine != null)
            {
                StopCoroutine(scanningCoroutine);
            }
            scanningCoroutine = StartCoroutine(ScanRoutine(duration));
        }

        private IEnumerator ScanRoutine(float duration)
        {
            SetStatus(CameraStatus.INSPECTING);

            if (scanningEffectParent != null)
            {
                scanningEffectParent.SetActive(true);
                scanningEffectParent.transform.localPosition = initialScanLocalPosition;
                
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    float offset = Mathf.PingPong(elapsed * scanSpeed, scanDistance);
                    // Assuming scanning along local Z axis
                    scanningEffectParent.transform.localPosition = initialScanLocalPosition + new Vector3(0, 0, offset);
                    
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                scanningEffectParent.SetActive(false);
            }
            else
            {
                yield return new WaitForSeconds(duration);
            }

            SetStatus(CameraStatus.READY);
            scanningCoroutine = null;
        }
    }
}
