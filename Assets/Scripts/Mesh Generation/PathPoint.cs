using UnityEngine;

namespace MeshGeneration
{
    public class PathPoint
    {
        #region Constructors
        public PathPoint(Vector3 position) : this(position, Quaternion.identity, Vector3.zero) { }

        public PathPoint(Vector3 position, Quaternion rotation) : this(position, rotation, Vector3.zero) { }

        public PathPoint(Vector3 position, Quaternion rotation, Vector3 normal)
        {
            this.position = position;
            this.rotation = rotation;
            this.normal = normal;
        } 
        #endregion

        public Vector3 position;
        public Quaternion rotation;
        public Vector3 normal;
    }
}