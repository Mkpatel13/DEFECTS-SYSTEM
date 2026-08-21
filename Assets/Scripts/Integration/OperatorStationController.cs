using UnityEngine;
using UnityEngine.InputSystem;

namespace Integration
{
    [RequireComponent(typeof(OperatorStationUI))]
    public class OperatorStationController : MonoBehaviour
    {
        [Header("Interaction Settings")]
        public float interactionDistance = 4.0f;

        [Header("UI Prompt")]
        public GameObject promptCanvas; // Screen-space canvas for "[E] VIEW INSPECTION RESULTS"

        private Transform playerCamera;
        private OperatorStationUI stationUI;
        private bool isPlayerFocused = false;

        private void Start()
        {
            stationUI = GetComponent<OperatorStationUI>();
            if (promptCanvas != null) promptCanvas.SetActive(false);

            if (Camera.main != null)
            {
                playerCamera = Camera.main.transform;
            }
        }

        private void Update()
        {
            if (playerCamera == null)
            {
                if (Camera.main != null) playerCamera = Camera.main.transform;
                else return;
            }

            CheckPlayerFocus();
            HandleInput();
        }

        private void CheckPlayerFocus()
        {
            isPlayerFocused = false;

            float distance = Vector3.Distance(playerCamera.position, transform.position);
            
            // Focus check via Raycast to see if looking at the station
            if (distance <= interactionDistance)
            {
                Ray ray = new Ray(playerCamera.position, playerCamera.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
                {
                    if (hit.collider != null && hit.collider.transform.IsChildOf(transform))
                    {
                        isPlayerFocused = true;
                    }
                }
            }

            // Hide prompt if UI is already open, otherwise show if focused
            if (stationUI.IsScreenActive())
            {
                if (promptCanvas != null) promptCanvas.SetActive(false);
            }
            else
            {
                if (promptCanvas != null) promptCanvas.SetActive(isPlayerFocused);
            }
        }

        private void HandleInput()
        {
            if (Keyboard.current == null) return;

            // Toggle logic using the New Input System
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (stationUI.IsScreenActive())
                {
                    stationUI.SetScreenActive(false);
                }
                else if (isPlayerFocused)
                {
                    stationUI.SetScreenActive(true);
                }
            }
            
            // Close logic on ESC
            if (Keyboard.current.escapeKey.wasPressedThisFrame && stationUI.IsScreenActive())
            {
                stationUI.SetScreenActive(false);
            }
        }
    }
}
