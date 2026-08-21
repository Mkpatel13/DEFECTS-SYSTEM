using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public static class DefectDatabase
    {
        public static readonly List<string> DefectClasses = new List<string>
        {
            "Missing Component",
            "Component Misalignment",
            "Solder Bridge",
            "Insufficient Solder",
            "Excess Solder",
            "Tombstoning",
            "Damaged Component"
        };

        /// <summary>
        /// Returns a reasonable bounding box for a given defect type to ensure it looks realistic on the PCB.
        /// Values are normalized [0, 1].
        /// </summary>
        public static Rect GetReasonableBoundingBox(string defectType)
        {
            switch (defectType)
            {
                case "Missing Component":
                    return new Rect(0.2f, 0.3f, 0.1f, 0.1f);
                case "Component Misalignment":
                    return new Rect(0.6f, 0.4f, 0.08f, 0.12f);
                case "Solder Bridge":
                    return new Rect(0.45f, 0.7f, 0.05f, 0.05f);
                case "Insufficient Solder":
                    return new Rect(0.8f, 0.2f, 0.06f, 0.06f);
                case "Excess Solder":
                    return new Rect(0.3f, 0.8f, 0.07f, 0.07f);
                case "Tombstoning":
                    return new Rect(0.7f, 0.6f, 0.05f, 0.1f);
                case "Damaged Component":
                    return new Rect(0.5f, 0.5f, 0.15f, 0.15f);
                default:
                    return new Rect(0.4f, 0.4f, 0.2f, 0.2f);
            }
        }
    }
}
