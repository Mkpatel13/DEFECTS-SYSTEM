using UnityEngine;
using Inspection;

public class StopperSensorController : MonoBehaviour
{
    private Renderer rend;
    private Material mat;
    private Color amberColor = new Color(1.0f, 0.75f, 0.0f);
    private Color greenColor = new Color(0.0f, 1.0f, 0.0f);

    private void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            mat = new Material(rend.sharedMaterial);
            rend.material = mat;
            mat.EnableKeyword("_EMISSION");
        }
    }

    private void Update()
    {
        if (mat == null) return;

        bool occupied = false;
        if (InspectionManager.Instance != null)
        {
            occupied = InspectionManager.Instance.isOccupied;
        }

        Color targetColor = occupied ? amberColor : greenColor;
        mat.SetColor("_BaseColor", targetColor);
        mat.SetColor("_EmissionColor", targetColor * 1.5f);
    }
}
