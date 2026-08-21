using UnityEngine;
using UnityEngine.InputSystem;
using Integration;
using TMPro;
using AI;

namespace UI
{
    public class DebugPanelController : MonoBehaviour
    {
        [Header("References")]
        public GameObject debugPanel;
        public TextMeshProUGUI statusText;

        private bool isPanelActive = false;

        private void Start()
        {
            if (debugPanel != null) debugPanel.SetActive(false);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                isPanelActive = !isPanelActive;
                if (debugPanel != null) debugPanel.SetActive(isPanelActive);
            }

            if (isPanelActive && statusText != null)
            {
                UpdateDebugInfo();
            }
        }

        private void UpdateDebugInfo()
        {
            string apiMode = "UNKNOWN";
            var yolo = FindFirstObjectByType<YOLOv8InspectionSimulator>();
            if (yolo != null)
            {
                apiMode = yolo.demonstrationMode.ToString();
            }

            statusText.text = $"DEBUG PANEL\n\n" +
                              $"AI MODE: {apiMode}\n" +
                              $"BACKEND: {(SpringBootApiClient.Instance != null ? "CONFIGURED" : "MISSING")}\n" +
                              $"PCBs ACTIVE: {Object.FindObjectsByType<PCBController>(FindObjectsSortMode.None).Length}";
        }
    }
}
