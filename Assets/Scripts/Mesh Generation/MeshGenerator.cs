using System.Collections;
using UnityEngine;
using PathCreation;
using System.Collections.Generic;
using UnityEditor;
using System;

namespace MeshGeneration
{
    [ExecuteInEditMode, RequireComponent(typeof(PathCreator))]
    
    public class MeshGenerator : MonoBehaviour
    {
        [Space(5), Header("PathSettings")]
        [SerializeField] int pointsPerSegment;
        [SerializeField] float startAngle;
        [SerializeField] float endAngle;
        [SerializeField] float trackRadius;
        [SerializeField] float numberOfSegments;
        [SerializeField] string objectName;
        [SerializeField] Material defaultTrackMaterial;

        [Space(5), Header("Debug")]
        [SerializeField] bool showPoints;
        [SerializeField, Range(0, 1)] float pointsGizmosSize = .1f;

        PathCreator pathCreator;
        List<Vector3> verticies = new();
        List<PathPoint> pathPoints = new ();

        private void Start()
        {
            this.transform.position = Vector3.zero;
            pathCreator = GetComponent<PathCreator>();
        }

        public void GenerateTrack()
        {
            var createdMesh = CreateNewObject(objectName);
            GetPathPoints();
            CrateVerticies();
            CreateTrackMesh(createdMesh);
            AddCollider(createdMesh.gameObject);
        }

        public void PreviewTrack()
        {
            GetPathPoints();
            CrateVerticies();
        }

        MeshFilter CreateNewObject(string gameObjectName)
        {
            if (!pathCreator)
                pathCreator = GetComponent<PathCreator>();

            var gameObject = new GameObject();
            gameObject.name = gameObjectName;

            var filter = gameObject.AddComponent<MeshFilter>();
            
            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.material = defaultTrackMaterial;
            
            return filter;
        }

        void CreateTrackMesh(MeshFilter meshFilter)
        {
            Mesh mesh = new Mesh();
            mesh.name = "TrackMesh";

            var verts = verticies.ToArray();
            mesh.vertices = verts;

            var squaresPerSegment = pointsPerSegment - 1;
            var triangles = new List<int>();

            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                for (int j = 0; j < squaresPerSegment; j++)
                {
                    var firstSegmentVertex = i * pointsPerSegment + j;
                    //lowerSquare
                    triangles.Add(firstSegmentVertex);
                    triangles.Add(firstSegmentVertex + pointsPerSegment);
                    triangles.Add(firstSegmentVertex + 1);

                    //upperSquare
                    triangles.Add(firstSegmentVertex + pointsPerSegment);
                    triangles.Add(firstSegmentVertex + pointsPerSegment + 1);
                    triangles.Add(firstSegmentVertex + 1);
                }
            }
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            var uvs = CalculateUVs();
            mesh.uv = uvs;
            meshFilter.mesh = mesh;
        }

        void CrateVerticies()
        {
            verticies = new List<Vector3>();
            foreach (var point in pathPoints) CreateVerticiesForPoint(point);
        }

        void CreateVerticiesForPoint(PathPoint point)
        {
            var step = (endAngle - startAngle) / (pointsPerSegment - 1);
            for (int i = 0; i < pointsPerSegment; i++)
            {
                var angle = startAngle + step * i;
                var radAngle = angle * Mathf.Deg2Rad;
                var newPoint = new Vector3(trackRadius * Mathf.Cos(radAngle), trackRadius * Mathf.Sin(radAngle));
                var transformedPoint = TransformPoint(point, newPoint);
                verticies.Add(transformedPoint);
            }
        }

        Vector3 TransformPoint(PathPoint point, Vector3 pointToTransform)
        {
            return point.position + point.rotation * Vector3.Scale(transform.lossyScale, pointToTransform);
        }

        private void GetPathPoints()
        {
            pathPoints.Clear();
            verticies.Clear();
            var delta = 1f / (numberOfSegments - 1);

            for (int i = 0; i < numberOfSegments; i++)
            {
                var currentDist = Mathf.Clamp01(i * delta);
                CreatePathPointAtDisctance(currentDist);
            }
        }

        private void CreatePathPointAtDisctance(float currentDist)
        {
            var position = pathCreator.path.GetPointAtTime(currentDist, EndOfPathInstruction.Stop);
            var normal = pathCreator.path.GetNormal(currentDist, EndOfPathInstruction.Stop);
            var rotation = pathCreator.path.GetRotation(currentDist, EndOfPathInstruction.Stop);

            var pathPoint = new PathPoint(position, rotation, normal);
            pathPoints.Add(pathPoint);
        }

        private Vector2[] CalculateUVs()
        {
            var uvs = new Vector2[verticies.Count];
            int index = 0;
            for (int i = 0; i < pathPoints.Count; i++)
            {
                var vCord = (float)i / (pathPoints.Count - 1);
                for (int j = 0; j < pointsPerSegment; j++)
                {
                    var uCord = (float)j / (pointsPerSegment - 1);
                    uvs[index++] = new Vector2(uCord, vCord);
                }
            }
            return uvs;
        }

        private void AddCollider(GameObject mesh) 
        {
            mesh.AddComponent<MeshCollider>();
        }

        #region Validation & Gizmos
        private void OnDrawGizmosSelected()
        {
            if (!showPoints || verticies is null) return;
            for (int i = 0; i < verticies.Count; i++)
            {
                var point = verticies[i];
                Gizmos.DrawWireSphere(point, pointsGizmosSize);
                Handles.Label(point, verticies.IndexOf(point).ToString());
            }
        }

        private void Update()
        {
            if (transform.hasChanged && this.transform.position != Vector3.zero)
                OnPositionChanged();
        }

        private void OnValidate()
        {
            if (transform.position != Vector3.zero)
                OnPositionChanged();
        }

        private void OnPositionChanged()
        {
            Debug.LogError("For this tool to work properly, this objects need to be at position (0,0,0)");
            transform.position = Vector3.zero;
        }
        #endregion
    }
}

