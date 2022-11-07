using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshGeneration
{
    [System.Serializable]
    public class TrackSegment
    {
        public TrackSegment(float segmentStart, float segmentEnd)
        {
            this.segmentStart = segmentStart;
            this.segmentEnd = segmentEnd;
        }
        public float segmentStart;
        public float segmentEnd;
        public SegmentType segmentType;
        public float size;
        [Range(0, 360)] public float startAngle;
        [Range(0, 360)] public float endAngle;
    }
}
