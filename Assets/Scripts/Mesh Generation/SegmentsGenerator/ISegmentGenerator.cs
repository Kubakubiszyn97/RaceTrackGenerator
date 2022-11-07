using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshGeneration
{
    public interface ITrackSegmentGenerator
    {
        public void Generate(TrackSegment segment, List<PathPoint> points, List<Vector3> verticies);
    }
}

