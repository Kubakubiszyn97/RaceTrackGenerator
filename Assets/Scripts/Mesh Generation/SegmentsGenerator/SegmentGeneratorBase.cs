using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshGeneration
{
    public abstract class SegmentGeneratorBase : ITrackSegmentGenerator
    {
        protected MeshGenerator generator;
        public PathCreator Path => generator.PathCreator;

        public SegmentGeneratorBase(MeshGenerator generator)
        {
            this.generator = generator;
        }

        public virtual void Generate(TrackSegment segment, List<PathPoint> points, List<Vector3> verticies)
        {
            throw new System.NotImplementedException();
        }

        protected virtual List<PathPoint> GeneratePathPoints(TrackSegment segment)
        {
            var points = new List<PathPoint>();
            float nextPointOffset = (segment.segmentEnd - segment.segmentStart) / (generator.SegmentStep - 1);
            for (int i = 0; i < generator.SegmentStep; i++)
            {
                var point = CreatePointAtDistance(segment.segmentStart + i * nextPointOffset);
                points.Add(point);
            }
            return points; 
        }

        protected virtual List<Vector3> GenerateSegmentVerticies(TrackSegment segment, List<PathPoint> points)
        {
            throw new System.NotImplementedException();
        }

        protected Vector3 TransformPoint(PathPoint point, Vector3 pointToTransform)
        {
            return point.position + point.rotation * Vector3.Scale(generator.transform.lossyScale, pointToTransform);
        }

        private PathPoint CreatePointAtDistance(float normalizedDistance)
        {
            var clampedDist = Mathf.Clamp01(normalizedDistance);
            var position = Path.path.GetPointAtTime(clampedDist, EndOfPathInstruction.Stop);
            var normal = Path.path.GetNormal(clampedDist, EndOfPathInstruction.Stop);
            var rotation = Path.path.GetRotation(clampedDist, EndOfPathInstruction.Stop);

            return new PathPoint(position, rotation, normal);
        }

        public static class Factory
        {
            public static ITrackSegmentGenerator CrateGenerator(SegmentType type, MeshGenerator generator)
            {
                return type switch
                {
                    SegmentType.Round => new RoundSegmentGenerator(generator),
                    SegmentType.Flat => new FlatSegmentGenerator(generator),
                    _ => null
                };
            }
        }
    }
}

