using System;

namespace Inspection
{
    public enum InspectionStatus
    {
        WAITING,
        CAPTURING,
        INSPECTING,
        COMPLETE
    }

    [Serializable]
    public struct InspectionData
    {
        public string InspectionID;
        public string PCB_ID;
        public float InspectionTimestamp;
        public InspectionStatus Status;
        public float Confidence;
        public string DefectType;
    }
}
