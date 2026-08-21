using UnityEngine;
using Inspection;

public class InspectionZoneController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PCBController pcb = other.GetComponent<PCBController>();
        if (pcb != null && InspectionManager.Instance != null)
        {
            InspectionManager.Instance.HandlePCBDetected(pcb);
        }
    }
}
