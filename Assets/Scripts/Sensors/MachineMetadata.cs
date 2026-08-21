using UnityEngine;

public class MachineMetadata : MonoBehaviour
{
    [Tooltip("The human-readable name of this machine component.")]
    public string displayName;

    [Tooltip("Description of what this component does.")]
    [TextArea(2, 5)]
    public string description;
}
