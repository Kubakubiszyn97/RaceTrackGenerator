using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MeshGeneration
{
    [CustomEditor(typeof(MeshGenerator))]
    public class MeshGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            MeshGenerator generator = (MeshGenerator)target;

            if (GUILayout.Button("Preview path points"))
            {
                generator.PreviewTrack();
            }

            if (GUILayout.Button("Generate Track Mesh"))
            {
                generator.GenerateTrack();
            }
        }
    }
}
