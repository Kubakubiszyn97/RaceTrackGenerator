using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshGeneration
{
    public class RoundSegmentGenerator : SegmentGeneratorBase
    {
        public RoundSegmentGenerator(MeshGenerator generator) : base(generator)
        {
        }

        public override void Generate(TrackSegment segment, List<PathPoint> points, List<Vector3> verticies)
        {
            var newPathPoints = GeneratePathPoints(segment);
            points.AddRange(newPathPoints);

            var newVerticies = GenerateSegmentVerticies(segment, newPathPoints);
            verticies.AddRange(newVerticies);
        }

        protected override List<Vector3> GenerateSegmentVerticies(TrackSegment segment, List<PathPoint> points)
        {
            var veritices = new List<Vector3>();
            foreach (var point in points)
            {
                var verticiesForPoint = CreateVerticiesForPathPoint(segment, point);
                veritices.AddRange(verticiesForPoint);
            }
            return veritices;
        }

        List<Vector3> CreateVerticiesForPathPoint(TrackSegment segment, PathPoint point)
        {
            var verticies = new List<Vector3>();
            var step = (segment.endAngle - segment.startAngle) / (generator.VerticiesPerStep - 1);
            for (int i = 0; i < generator.VerticiesPerStep; i++)
            {
                var angle = segment.startAngle + step * i;
                var radAngle = angle * Mathf.Deg2Rad;
                var newPoint = new Vector3(segment.size * Mathf.Cos(radAngle), segment.size * Mathf.Sin(radAngle));
                var transformedPoint = TransformPoint(point, newPoint);
                verticies.Add(transformedPoint);
            }
            return verticies;
        }
    }
}