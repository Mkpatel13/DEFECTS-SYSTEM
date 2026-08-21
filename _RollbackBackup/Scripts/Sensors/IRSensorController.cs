using UnityEngine;

public class IRSensorController : MonoBehaviour
{
    [Header("Indicator Settings")]
    [Tooltip("The renderer for the IR sensor indicator light.")]
    public Renderer indicatorRenderer;
    
    [Tooltip("Material for when no PCB is detected.")]
    public Material offMaterial;
    
    [Tooltip("Material for when a PCB is detected.")]
    public Material onMaterial;

    private int detectionCount = 0;

    private void Start()
    {
        UpdateIndicator();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PCBController>() != null)
        {
            detectionCount++;
            UpdateIndicator();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PCBController>() != null)
        {
            detectionCount--;
            if (detectionCount < 0) detectionCount = 0;
            UpdateIndicator();
        }
    }

    private void UpdateIndicator()
    {
        if (indicatorRenderer != null)
        {
            indicatorRenderer.material = (detectionCount > 0) ? onMaterial : offMaterial;
        }
    }
}
