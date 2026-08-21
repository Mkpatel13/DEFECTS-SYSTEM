using UnityEngine;
using UnityEngine.InputSystem;

namespace Integration
{
    public class DashboardLauncher : MonoBehaviour
    {
        [Header("Dashboard Settings")]
        [Tooltip("The URL to open when the player presses the dashboard key.")]
        public string dashboardUrl = "http://localhost:3000";

        private void Update()
        {
            // DISABLED: The external browser launch behavior is no longer correct.
            // OperatorStationController now handles displaying the dashboard internally.
            /*
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log($"Opening Dashboard: {dashboardUrl}");
                Application.OpenURL(dashboardUrl);
            }
            */
        }
    }
}
