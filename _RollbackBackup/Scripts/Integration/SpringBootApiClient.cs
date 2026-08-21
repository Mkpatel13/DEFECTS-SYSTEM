using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using AI;

namespace Integration
{
    public class SpringBootApiClient : MonoBehaviour
    {
        public static SpringBootApiClient Instance { get; private set; }

        [Header("API Configuration")]
        public string baseUrl = "http://localhost:8081/api";
        public float timeoutSeconds = 5f;
        public long defaultProductId = 1;

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

        public void SaveResult(DetectionResult result, Action onSuccess = null, Action<string> onError = null)
        {
            StartCoroutine(SaveResultCoroutine(result, onSuccess, onError));
        }

        private IEnumerator SaveResultCoroutine(DetectionResult result, Action onSuccess, Action<string> onError)
        {
            InspectionResultDto dto = new InspectionResultDto
            {
                productId = defaultProductId,
                pcbId = result.PCB_ID,
                isDefective = result.DefectDetected,
                defectType = result.DefectType,
                confidence = result.Confidence,
                imagePath = $"unity_captured_{result.PCB_ID}.jpg"
            };

            string json = JsonUtility.ToJson(dto);
            string url = $"{baseUrl}/inspections/save-result";

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = (int)timeoutSeconds;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || 
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    string errorDetail = $"BACKEND REQUEST FAILED\nURL: {url}\nHTTP Status: {request.responseCode}\nError: {request.error}\nResponse: {(request.downloadHandler != null ? request.downloadHandler.text : "None")}";
                    Debug.LogWarning(errorDetail);
                    
                    if (request.result == UnityWebRequest.Result.ConnectionError)
                    {
                        onError?.Invoke("BACKEND OFFLINE");
                    }
                    else
                    {
                        onError?.Invoke($"BACKEND ERROR {request.responseCode}");
                    }
                }
                else
                {
                    onSuccess?.Invoke();
                }
            }
        }

        [Serializable]
        private class InspectionResultDto
        {
            public long productId;
            public string pcbId;
            public bool isDefective;
            public string defectType;
            public float confidence;
            public string imagePath;
        }

        public void GetDashboardStats(Action<DashboardStatsDto> onSuccess, Action<string> onError)
        {
            StartCoroutine(GetRequestCoroutine($"{baseUrl}/inspections/stats", onSuccess, onError));
        }

        public void GetLatestInspections(Action<InspectionListDto> onSuccess, Action<string> onError)
        {
            StartCoroutine(GetRequestCoroutine($"{baseUrl}/inspections", 
                (string json) => {
                    // Unity's JsonUtility cannot deserialize root arrays directly.
                    // Wrap the array in a dummy object.
                    string wrappedJson = "{\"inspections\":" + json + "}";
                    try {
                        InspectionListDto list = JsonUtility.FromJson<InspectionListDto>(wrappedJson);
                        onSuccess?.Invoke(list);
                    } catch (Exception e) {
                        Debug.LogError($"Parse Error: {e.Message}\nJSON: {wrappedJson}");
                        onError?.Invoke("JSON_PARSE_ERROR");
                    }
                }, 
                onError));
        }

        private IEnumerator GetRequestCoroutine<T>(string url, Action<T> onSuccess, Action<string> onError)
        {
            yield return GetRequestCoroutine(url, 
                (string json) => {
                    try {
                        T result = JsonUtility.FromJson<T>(json);
                        onSuccess?.Invoke(result);
                    } catch (Exception e) {
                        Debug.LogError($"Parse Error: {e.Message}\nJSON: {json}");
                        onError?.Invoke("JSON_PARSE_ERROR");
                    }
                }, 
                onError);
        }

        private IEnumerator GetRequestCoroutine(string url, Action<string> onSuccess, Action<string> onError)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)timeoutSeconds;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || 
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning($"BACKEND GET FAILED\nURL: {url}\nError: {request.error}");
                    onError?.Invoke(request.result == UnityWebRequest.Result.ConnectionError ? "BACKEND OFFLINE" : $"HTTP ERROR {request.responseCode}");
                }
                else
                {
                    onSuccess?.Invoke(request.downloadHandler.text);
                }
            }
        }

        [Serializable]
        public class DashboardStatsDto
        {
            public long totalInspections;
            public long defectiveCount;
            public double defectRate;
            // Note: C# dictionaries are not natively serialized by JsonUtility, 
            // but the prompt only asks for total/good/defective, which are covered here.
        }

        [Serializable]
        public class InspectionEntityDto
        {
            public long id;
            public string pcbId;
            public bool isDefective;
            public string defectType;
            public float confidence;
            public string imagePath;
            public string inspectedAt;
        }

        [Serializable]
        public class InspectionListDto
        {
            public InspectionEntityDto[] inspections;
        }
    }
}
