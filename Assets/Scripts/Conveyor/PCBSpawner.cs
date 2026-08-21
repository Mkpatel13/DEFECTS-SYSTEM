using System.Collections.Generic;
using UnityEngine;

public class PCBSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [Tooltip("The PCB Prefab to spawn.")]
    public GameObject pcbPrefab;
    [Tooltip("Time between spawns in seconds.")]
    public float spawnInterval = 5.0f;
    [Tooltip("Maximum number of PCBs allowed at once.")]
    public int maxActivePCBs = 4;
    [Tooltip("Scale multiplier for the spawned PCB.")]
    public float pcbScaleMultiplier = 1.4f;

    private float timer = 0f;
    private List<GameObject> activePCBs = new List<GameObject>();
    private int pcbCounter = 1;

    [Header("Material Fallback")]
    [Tooltip("URP compatible material to use if the prefab has a missing/pink material.")]
    public Material safePcbMaterial;

    private void Update()
    {
        // Clean up any destroyed PCBs from the list
        activePCBs.RemoveAll(item => item == null);

        timer += Time.deltaTime;
        
        if (timer >= spawnInterval)
        {
            timer = 0f;
            if (activePCBs.Count < maxActivePCBs && pcbPrefab != null)
            {
                SpawnPCB();
            }
        }
    }

    private void SpawnPCB()
    {
        // Spawner is moved to PCB_Input by Phase8Setup, but we can double check
        Vector3 spawnPos = transform.position;
        Quaternion spawnRot = transform.rotation;

        ConveyorController conveyor = Object.FindFirstObjectByType<ConveyorController>();
        if (conveyor != null && conveyor.waypointsRoot != null)
        {
            Transform pcbInput = conveyor.waypointsRoot.Find("PCB_Input");
            if (pcbInput != null)
            {
                spawnPos = pcbInput.position;
                spawnRot = pcbInput.rotation;
            }
        }

        GameObject newPCB = Instantiate(pcbPrefab, spawnPos, spawnRot);
        newPCB.transform.localScale *= pcbScaleMultiplier;
        
        // 1. Assign ID
        PCBController controller = newPCB.GetComponent<PCBController>();
        if (controller != null)
        {
            controller.pcbId = $"PCB-{pcbCounter:D3}";
            pcbCounter++;
        }

        // 2. Material Safety Check (No Pink/Magenta)
        Renderer[] renderers = newPCB.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            if (r.sharedMaterial == null || r.sharedMaterial.shader.name == "Hidden/InternalErrorShader" || r.sharedMaterial.shader.name == "Standard")
            {
                if (safePcbMaterial != null)
                {
                    r.material = safePcbMaterial;
                }
                else
                {
                    Debug.LogWarning($"PCBSpawner: PCB '{newPCB.name}' has invalid material on '{r.name}' but no safePcbMaterial is assigned!");
                }
            }
        }

        activePCBs.Add(newPCB);
    }
}
