using System.Collections.Generic;
using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class AnimationData : ScriptableObject
    {
        public List<int> vertexIndices;        // List of vertex indices (keys of dictionary)
        public List<Vector3> vertexPositions;  // Corresponding vertex positions (values of dictionary)
        public List<FrameDelta> frameDeltas;

        // Function to convert the serialized lists back to a dictionary for use
        public Dictionary<int, Vector3> GetVertexLookup()
        {
            var dict = new Dictionary<int, Vector3>();
            for (int i = 0; i < vertexIndices.Count; i++)
            {
                dict[vertexIndices[i]] = vertexPositions[i];
            }
            return dict;
        }
    }
    [System.Serializable]
    public class FrameDelta
    {
        public List<Vector3> deltaVertices;  // This will be serialized properly by Unity

        public FrameDelta(int count)
        {
            deltaVertices = new List<Vector3>(count);
        }
    }
}