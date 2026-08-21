using UnityEngine;

namespace Inspection
{
    public class InspectionLightController : MonoBehaviour
    {
        [Header("Lighting")]
        [Tooltip("The main light component used for inspection illumination.")]
        public Light mainLight;
        
        [Tooltip("Any emissive renderer to make the LEDs glow visibly.")]
        public Renderer emissiveRenderer;
        public Material offMaterial;
        public Material onMaterial;

        [Header("Settings")]
        [Tooltip("Intensity of the light when ON.")]
        public float onIntensity = 5.0f;
        
        [Tooltip("Color of the inspection light.")]
        public Color inspectionColor = Color.white;

        private void Start()
        {
            if (mainLight != null)
            {
                mainLight.color = inspectionColor;
            }
            TurnOff();
        }

        public void TurnOn()
        {
            if (mainLight != null)
            {
                mainLight.intensity = onIntensity;
                mainLight.enabled = true;
            }

            if (emissiveRenderer != null && onMaterial != null)
            {
                emissiveRenderer.material = onMaterial;
            }
        }

        public void TurnOff()
        {
            if (mainLight != null)
            {
                mainLight.enabled = false;
            }

            if (emissiveRenderer != null && offMaterial != null)
            {
                emissiveRenderer.material = offMaterial;
            }
        }
    }
}
