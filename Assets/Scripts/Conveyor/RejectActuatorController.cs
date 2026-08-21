using UnityEngine;
using System.Collections;

public class RejectActuatorController : MonoBehaviour
{
    public Transform pusherRod;
    public Vector3 startLocalPos;
    public Vector3 extendedLocalPos;
    public float extendDuration = 0.20f;
    public float holdDuration = 0.15f;
    public float retractDuration = 0.25f;
    
    private bool isAnimating = false;

    public void ActivateActuator()
    {
        if (!isAnimating && pusherRod != null)
        {
            StartCoroutine(AnimateActuator());
        }
    }

    private IEnumerator AnimateActuator()
    {
        isAnimating = true;
        float elapsed = 0f;
        
        // Extend
        while (elapsed < extendDuration)
        {
            elapsed += Time.deltaTime;
            pusherRod.localPosition = Vector3.Lerp(startLocalPos, extendedLocalPos, elapsed / extendDuration);
            yield return null;
        }
        pusherRod.localPosition = extendedLocalPos;
        
        // Short pause
        yield return new WaitForSeconds(holdDuration);
        
        // Retract
        elapsed = 0f;
        while (elapsed < retractDuration)
        {
            elapsed += Time.deltaTime;
            pusherRod.localPosition = Vector3.Lerp(extendedLocalPos, startLocalPos, elapsed / retractDuration);
            yield return null;
        }
        pusherRod.localPosition = startLocalPos;
        isAnimating = false;
    }
}
