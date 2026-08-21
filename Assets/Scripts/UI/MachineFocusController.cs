using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    public class MachineFocusController : MonoBehaviour
    {
        [Header("Focus Settings")]
        public float interactionDistance = 4.0f;
        public string titleText = "MACHINE";
        public string subtitleText = "STATUS: ACTIVE";

        [Header("References")]
        public GameObject focusCanvas; // World-space canvas attached to the machine

        private Transform playerCamera;
        private bool isFocused = false;

        private void Start()
        {
            if (focusCanvas != null) focusCanvas.SetActive(false);

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

            CheckFocus();
        }

        private void CheckFocus()
        {
            isFocused = false;

            float distance = Vector3.Distance(playerCamera.position, transform.position);

            if (distance <= interactionDistance)
            {
                Ray ray = new Ray(playerCamera.position, playerCamera.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
                {
                    if (hit.collider != null && hit.collider.transform.IsChildOf(transform))
                    {
                        isFocused = true;
                    }
                }
            }

            if (focusCanvas != null)
            {
                focusCanvas.SetActive(isFocused);
                
                // Point popup toward camera so it's always readable (fixes mirrored text)
                if (isFocused)
                {
                    focusCanvas.transform.rotation = Quaternion.LookRotation(playerCamera.position - focusCanvas.transform.position);
                }
            }
        }
    }
}
