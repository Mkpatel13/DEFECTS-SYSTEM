using System;
using UnityEngine;

namespace AI
{
    [Serializable]
    public struct DetectionResult
    {
        public string PCB_ID;
        public string Inspection_ID;
        public bool DefectDetected;
        public string DefectType;
        public float Confidence;
        
        /// <summary>
        /// Normalized coordinates [0, 1] for the bounding box.
        /// x, y are top-left corner, width and height are fractions of the image size.
        /// </summary>
        public Rect BoundingBox;
        
        public float InspectionTimestamp;
        public string ErrorMessage;
    }
}
