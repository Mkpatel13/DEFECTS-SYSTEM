using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using AI;

namespace Integration
{
    public class FastApiClient : MonoBehaviour
    {
        public static FastApiClient Instance { get; private set; }

        [Header("API Configuration")]
        public string apiUrl = "http://localhost:8000/predict";
        public float timeoutSeconds = 10f;

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

        public void Predict(Texture2D pcbImage, string pcbId, Action<DetectionResult> onSuccess, Action<string> onError)
        {
            StartCoroutine(PredictCoroutine(pcbImage, pcbId, onSuccess, onError));
        }

        private IEnumerator PredictCoroutine(Texture2D image, string pcbId, Action<DetectionResult> onSuccess, Action<string> onError)
        {
            byte[] imageBytes = image.EncodeToJPG(90);
            
            WWWForm form = new WWWForm();
            form.AddBinaryData("file", imageBytes, $"{pcbId}.jpg", "image/jpeg");

            using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, form))
            {
                request.timeout = (int)timeoutSeconds;
                
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || 
                    request.result == UnityWebRequest.Result.ProtocolError ||
                    request.result == UnityWebRequest.Result.DataProcessingError)
                {
                    string errorDetail = $"AI SERVICE REQUEST FAILED\nURL: {apiUrl}\nHTTP Status: {request.responseCode}\nError: {request.error}\nResponse: {(request.downloadHandler != null ? request.downloadHandler.text : "None")}";
                    Debug.LogWarning(errorDetail);
                    
                    if (request.result == UnityWebRequest.Result.ConnectionError)
                    {
                        onError?.Invoke("AI SERVICE OFFLINE");
                    }
                    else
                    {
                        onError?.Invoke($"AI SERVICE ERROR {request.responseCode}");
                    }
                }
                else
                {
                    try
                    {
                        string jsonResult = request.downloadHandler.text;
                        AiPredictionResponse response = JsonUtility.FromJson<AiPredictionResponse>(jsonResult);

                        DetectionResult result = new DetectionResult
                        {
                            PCB_ID = pcbId,
                            Inspection_ID = System.Guid.NewGuid().ToString(),
                            DefectDetected = response.isDefective,
                            DefectType = response.defectType,
                            Confidence = response.confidence,
                            InspectionTimestamp = Time.time
                            // BoundingBox is not provided by the real FastAPI
                        };
                        
                        onSuccess?.Invoke(result);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to parse FastAPI response: {e.Message}");
                        onError?.Invoke("AI SERVICE ERROR");
                    }
                }
            }
        }

        [Serializable]
        private class AiPredictionResponse
        {
            public string defectType;
            public float confidence;
            public bool isDefective;
            public string detectedImagePath;
        }
    }
}
