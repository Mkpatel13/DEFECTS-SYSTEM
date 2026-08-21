using UnityEngine;

public class ConveyorController : MonoBehaviour
{
    public static ConveyorController Instance { get; private set; }

    [Header("Conveyor Settings")]
    [Tooltip("Movement speed in meters per second.")]
    public float conveyorSpeed = 1.0f;

    [Header("Waypoints")]
    [Tooltip("Root object containing waypoints. Automatically found if null.")]
    public Transform waypointsRoot;

    [Header("Visuals")]
    [Tooltip("The renderer for the conveyor belt to animate the texture.")]
    public Renderer beltRenderer;
    [Tooltip("The material index of the belt.")]
    public int beltMaterialIndex = 0;

    private Material beltMaterial;

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
        if (beltRenderer != null && beltRenderer.materials.Length > beltMaterialIndex)
        {
            beltMaterial = beltRenderer.materials[beltMaterialIndex];
        }

        if (waypointsRoot == null)
        {
            waypointsRoot = transform.Find("Waypoints");
        }
    }

    private void Update()
    {
        if (beltMaterial != null)
        {
            // Scroll the texture based on speed and time
            Vector2 offset = Vector2.zero;
            
            // Check for URP BaseMap or Standard MainTex
            string textureName = beltMaterial.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex";
            
            if (beltMaterial.HasProperty(textureName))
            {
                offset = beltMaterial.GetTextureOffset(textureName);
                offset.y -= conveyorSpeed * Time.deltaTime; // Assuming Y is the length
                beltMaterial.SetTextureOffset(textureName, offset);
            }
        }
    }
}
