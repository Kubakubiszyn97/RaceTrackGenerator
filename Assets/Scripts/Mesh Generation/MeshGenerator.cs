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
        [SerializeField, Range(0, 15)] int pointsPerSegment = 10;
        [SerializeField, Range(1, 20)] int verticiesPerStep = 10;
        [SerializeField, Range(.01f, .5f)] float totalSegmentTransitionDistance = .1f;
        [SerializeField] List<TrackSegment> segments;

        [Space(5), Header("Settings")]
        [SerializeField] string objectName;
        [SerializeField] Material defaultTrackMaterial;

        [Space(5), Header("Debug")]
        [SerializeField] bool showPoints;
        [SerializeField, Range(0, 1)] float pointsGizmosSize = .1f;

        PathCreator pathCreator;
        List<Vector3> verticies = new();
        List<PathPoint> pathPoints = new();
        Dictionary<SegmentType, ITrackSegmentGenerator> generatorLookup;

        public PathCreator PathCreator => pathCreator;
        public int SegmentStep => pointsPerSegment;
        public int VerticiesPerStep => verticiesPerStep;

        private void Start()
        {
            this.transform.position = Vector3.zero;
            pathCreator = GetComponent<PathCreator>();
        }

        public void GenerateTrack()
        {
            var createdMesh = CreateNewObject(objectName);
            GeneratePath();
            CreateTrackMesh(createdMesh);
            AddCollider(createdMesh.gameObject);
        }

        public void PreviewTrack()
        {
            GeneratePath();
        }

        void GeneratePath()
        {
            InitializeSegments();
            pathPoints.Clear();
            verticies.Clear();
            if (generatorLookup == null)
                    generatorLookup = new();
            
            foreach (var segment in segments)
            {
                ITrackSegmentGenerator generator = GetSegmentGenerator(segment.segmentType);
                generator.Generate(segment, pathPoints, verticies);
            }
        }

        ITrackSegmentGenerator GetSegmentGenerator(SegmentType type)
        {
            if (generatorLookup.TryGetValue(type, out ITrackSegmentGenerator generator))
            {
                return generator;
            }

            generator = SegmentGeneratorBase.Factory.CrateGenerator(type, this);
            if (generator == null)
            {
                Debug.LogError($"SegmentGenerator for {type} does not exist");
            }
            generatorLookup[type] = generator;
            return generator;
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

            var squaresPerSegment = verticiesPerStep - 1;
            var triangles = new List<int>();

            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                for (int j = 0; j < squaresPerSegment; j++)
                {
                    var firstSegmentVertex = i * verticiesPerStep + j;
                    //lowerSquare
                    triangles.Add(firstSegmentVertex);
                    triangles.Add(firstSegmentVertex + VerticiesPerStep);
                    triangles.Add(firstSegmentVertex + 1);

                    //upperSquare
                    triangles.Add(firstSegmentVertex + VerticiesPerStep);
                    triangles.Add(firstSegmentVertex + verticiesPerStep + 1);
                    triangles.Add(firstSegmentVertex + 1);
                }
            }
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            var uvs = CalculateUVs();
            mesh.uv = uvs;
            meshFilter.mesh = mesh;
        }

        private Vector2[] CalculateUVs()
        {
            var uvs = new Vector2[verticies.Count];
            int index = 0;
            for (int i = 0; i < pathPoints.Count; i++)
            {
                var vCord = (float)i / (pathPoints.Count - 1);
                for (int j = 0; j < verticiesPerStep; j++)
                {
                    var uCord = (float)j / (verticiesPerStep - 1);
                    uvs[index++] = new Vector2(uCord, vCord);
                }
            }
            return uvs;
        }

        private void InitializeSegments()
        {
            if (segments.Count == 0) return;
            float singleSegmentTransitionDistance = totalSegmentTransitionDistance / (segments.Count - 1);
            float segmentLength = (1 - totalSegmentTransitionDistance) / segments.Count;

            for (int i = 0; i < segments.Count; i++)
            {
                var segmentStart = i == 0 ? 0 : i * segmentLength + singleSegmentTransitionDistance;
                segments[i].segmentStart = segmentStart;
                segments[i].segmentEnd = segmentStart + segmentLength;
            }
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

