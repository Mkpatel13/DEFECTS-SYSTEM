using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Integration
{
    public class OperatorStationUI : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject mainPanel;
        public TextMeshProUGUI statusText;

        [Header("Stats Elements")]
        public TextMeshProUGUI totalInspectedText;
        public TextMeshProUGUI goodCountText;
        public TextMeshProUGUI defectiveCountText;

        [Header("Last Inspection Elements")]
        public TextMeshProUGUI lastPcbIdText;
        public TextMeshProUGUI lastResultText;
        public TextMeshProUGUI lastDefectText;
        public TextMeshProUGUI lastConfidenceText;
        public TextMeshProUGUI lastTimeText;

        [Header("Colors")]
        public Color goodColor = Color.green;
        public Color defectColor = Color.red;
        public Color offlineColor = Color.yellow;
        public Color onlineColor = Color.cyan;

        private Coroutine pollingCoroutine;
        private bool isScreenActive = false;

        private void Start()
        {
            if (mainPanel != null)
                mainPanel.SetActive(false);
        }

        public void SetScreenActive(bool active)
        {
            isScreenActive = active;
            if (mainPanel != null)
                mainPanel.SetActive(active);

            if (active)
            {
                statusText.text = "SYSTEM: FETCHING...";
                statusText.color = onlineColor;
                
                if (pollingCoroutine != null) StopCoroutine(pollingCoroutine);
                pollingCoroutine = StartCoroutine(PollDataCoroutine());
            }
            else
            {
                if (pollingCoroutine != null)
                {
                    StopCoroutine(pollingCoroutine);
                    pollingCoroutine = null;
                }
            }
        }

        public bool IsScreenActive()
        {
            return isScreenActive;
        }

        private IEnumerator PollDataCoroutine()
        {
            while (isScreenActive)
            {
                FetchData();
                yield return new WaitForSeconds(3f); // Controlled refresh interval
            }
        }

        private void FetchData()
        {
            if (SpringBootApiClient.Instance == null) return;

            // Fetch Stats
            SpringBootApiClient.Instance.GetDashboardStats(
                onSuccess: (stats) => {
                    statusText.text = "SYSTEM: ONLINE";
                    statusText.color = onlineColor;

                    totalInspectedText.text = stats.totalInspections.ToString();
                    long good = stats.totalInspections - stats.defectiveCount;
                    goodCountText.text = good.ToString();
                    defectiveCountText.text = stats.defectiveCount.ToString();
                },
                onError: (err) => {
                    statusText.text = err == "BACKEND OFFLINE" ? "BACKEND OFFLINE" : "SYSTEM: ERROR";
                    statusText.color = offlineColor;
                }
            );

            // Fetch Latest
            SpringBootApiClient.Instance.GetLatestInspections(
                onSuccess: (list) => {
                    if (list != null && list.inspections != null && list.inspections.Length > 0)
                    {
                        var latest = list.inspections[list.inspections.Length - 1]; // First if descending or last if chron
                        
                        lastPcbIdText.text = latest.pcbId;
                        if (latest.isDefective)
                        {
                            lastResultText.text = "DEFECT";
                            lastResultText.color = defectColor;
                            lastDefectText.text = latest.defectType;
                        }
                        else
                        {
                            lastResultText.text = "GOOD";
                            lastResultText.color = goodColor;
                            lastDefectText.text = "None";
                        }
                        
                        lastConfidenceText.text = $"{(latest.confidence * 100f):F1}%";
                        lastTimeText.text = latest.inspectedAt != null ? latest.inspectedAt.ToString() : "N/A";
                    }
                    else
                    {
                        lastPcbIdText.text = "NO INSPECTION RESULTS";
                        lastResultText.text = "-";
                        lastResultText.color = Color.white;
                        lastDefectText.text = "-";
                        lastConfidenceText.text = "-";
                        lastTimeText.text = "-";
                    }
                },
                onError: (err) => {
                    // Handled by stats error mostly
                }
            );
        }
    }
}
