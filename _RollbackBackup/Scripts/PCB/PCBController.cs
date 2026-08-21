using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCBController : MonoBehaviour
{
    public enum PCBState
    {
        Waiting,
        WaitingAtBuffer,
        WaitingForDecision,
        MovingToInspection,
        Inspecting,
        MovingToOutput,
        Rejecting,
        OnRejectConveyor,
        EnteringChute,
        FallingToBin,
        Rejected
    }

    [Header("Identification")]
    public string pcbId = "PCB-000";

    [Header("State")]
    public PCBState currentState = PCBState.MovingToInspection;
    
    [Header("Inspection Results")]
    public bool isDefective = false;
    public bool hasBeenInspected = false;

    [Header("Inspection Settings")]
    [Tooltip("Duration to pause at the inspection zone in seconds.")]
    public float inspectionPauseDuration = 2.0f;

    [Header("Routing Settings")]
    [Tooltip("Small offset above the belt surface.")]
    public float pcbSurfaceHeightOffset = 0.02f;

    private bool externallyControlled = false;
    
    // Waypoint tracking
    private Transform[] pathWaypoints = new Transform[5];
    private int currentWaypointIndex = 0;
    
    // Reject path
    private Transform rejectStart;
    private Transform rejectEnd;
    private Transform rejectChuteFall;

    private void Start()
    {
        CalculateHeightOffset();

        ConveyorController conveyor = UnityEngine.Object.FindFirstObjectByType<ConveyorController>();
        if (conveyor != null && conveyor.waypointsRoot != null)
        {
            Transform root = conveyor.waypointsRoot;
            pathWaypoints[0] = root.Find("PCB_Input");
            pathWaypoints[1] = root.Find("PCB_InspectionApproach");
            pathWaypoints[2] = root.Find("PCB_InspectionCenter");
            pathWaypoints[3] = root.Find("PCB_DecisionPoint");
            pathWaypoints[4] = root.Find("PCB_GoodOutput");
            
            rejectStart = root.Find("PCB_RejectStart");
            rejectEnd = root.Find("PCB_RejectEnd");
            rejectChuteFall = root.Find("PCB_RejectChuteFall");

            if (pathWaypoints[0] != null)
            {
                transform.position = pathWaypoints[0].position + Vector3.up * pcbSurfaceHeightOffset;
                transform.rotation = pathWaypoints[0].rotation;
                currentWaypointIndex = 1;
            }
        }
    }

    private void CalculateHeightOffset()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            float minY = float.MaxValue;
            foreach (var r in renderers)
            {
                if (r.bounds.min.y < minY) minY = r.bounds.min.y;
            }
            float rootY = transform.position.y;
            float dropDown = rootY - minY;
            pcbSurfaceHeightOffset = dropDown + 0.005f;
        }
    }

    private void Update()
    {
        if (externallyControlled) return;

        if (currentState == PCBState.WaitingAtBuffer)
        {
            var mgr = UnityEngine.Object.FindFirstObjectByType<Inspection.InspectionManager>();
            if (mgr != null && !mgr.isOccupied)
            {
                mgr.isOccupied = true;
                currentState = PCBState.MovingToInspection;
            }
        }
        else if (currentState == PCBState.WaitingForDecision)
        {
            if (hasBeenInspected)
            {
                currentState = PCBState.MovingToOutput;
                currentWaypointIndex++;
                var mgr = UnityEngine.Object.FindFirstObjectByType<Inspection.InspectionManager>();
                if (mgr != null) mgr.isOccupied = false;
            }
        }
        else if (currentState == PCBState.MovingToInspection || currentState == PCBState.MovingToOutput || currentState == PCBState.Inspecting)
        {
            MoveAlongWaypoints();
        }
        else if (currentState == PCBState.OnRejectConveyor)
        {
            MoveTowards(rejectEnd, PCBState.EnteringChute, 1.2f); // Slightly faster on reject belt
        }
        else if (currentState == PCBState.EnteringChute)
        {
            MoveTowards(rejectChuteFall, PCBState.FallingToBin, 1.2f);
        }
    }

    private void MoveAlongWaypoints()
    {
        if (currentWaypointIndex >= pathWaypoints.Length)
        {
            Destroy(gameObject);
            return;
        }

        Transform targetWP = pathWaypoints[currentWaypointIndex];
        
        // Handle routing divergence at DecisionPoint
        if (currentWaypointIndex == 4 && hasBeenInspected && isDefective && rejectStart != null)
        {
            StartCoroutine(PushToRejectConveyor());
            return;
        }

        if (targetWP == null) return; // safety
        MoveTowards(targetWP, PCBState.MovingToOutput, 1.0f);
    }

    private void MoveTowards(Transform targetWP, PCBState nextState, float speedMultiplier)
    {
        float speed = 0.4f * speedMultiplier;
        if (ConveyorController.Instance != null)
        {
            speed = ConveyorController.Instance.conveyorSpeed * speedMultiplier;
        }

        Vector3 targetPos = targetWP.position + Vector3.up * pcbSurfaceHeightOffset;
        Vector3 direction = (targetPos - transform.position).normalized;
        float distanceThisFrame = speed * Time.deltaTime;
        float distanceToTarget = Vector3.Distance(transform.position, targetPos);

        if (distanceToTarget <= distanceThisFrame)
        {
            transform.position = targetPos;
            
            if (currentState == PCBState.MovingToInspection || currentState == PCBState.MovingToOutput || currentState == PCBState.Inspecting)
            {
                if (currentWaypointIndex == 1) // At Buffer
                {
                    currentWaypointIndex++;
                    var mgr = UnityEngine.Object.FindFirstObjectByType<Inspection.InspectionManager>();
                    if (mgr != null && mgr.isOccupied)
                    {
                        currentState = PCBState.WaitingAtBuffer;
                    }
                    else if (mgr != null)
                    {
                        mgr.isOccupied = true;
                    }
                }
                else if (currentWaypointIndex == 3) // At Decision
                {
                    if (!hasBeenInspected)
                    {
                        currentState = PCBState.WaitingForDecision;
                    }
                    else
                    {
                        currentWaypointIndex++;
                        var mgr = UnityEngine.Object.FindFirstObjectByType<Inspection.InspectionManager>();
                        if (mgr != null) mgr.isOccupied = false;
                    }
                }
                else
                {
                    currentWaypointIndex++;
                }
            }
            else if (currentState == PCBState.OnRejectConveyor)
            {
                currentState = PCBState.EnteringChute;
            }
            else if (currentState == PCBState.EnteringChute)
            {
                StartFalling();
            }
        }
        else
        {
            transform.position += direction * distanceThisFrame;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetWP.rotation, 5f * Time.deltaTime);
        }
    }

    private IEnumerator PushToRejectConveyor()
    {
        currentState = PCBState.Rejecting;
        
        RejectActuatorController actuator = UnityEngine.Object.FindFirstObjectByType<RejectActuatorController>();
        if (actuator != null) 
        {
            actuator.ActivateActuator();
        }

        Vector3 startPos = transform.position;
        Vector3 targetPos = rejectStart.position + Vector3.up * pcbSurfaceHeightOffset;
        
        // Sync with actuator push duration (approx 0.2s)
        float pushDuration = (actuator != null) ? actuator.extendDuration : 0.2f;
        float elapsed = 0f;

        while (elapsed < pushDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / pushDuration);
            yield return null;
        }
        transform.position = targetPos;
        
        currentState = PCBState.OnRejectConveyor;
    }

    private void StartFalling()
    {
        currentState = PCBState.FallingToBin;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        
        // Add a slight tumble
        rb.AddTorque(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 0.5f, ForceMode.Impulse);
        
        Destroy(gameObject, 4.0f); // clean up after falling
    }

    public void SetExternalControl(bool isControlled)
    {
        externallyControlled = isControlled;
    }
}
