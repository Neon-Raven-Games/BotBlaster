using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class MeshVertexMapper : MonoBehaviour
    {
        public Mesh mesh;
        public ComputeBuffer vertexIDBuffer;
        public ComputeBuffer deltaBuffer;

        private Vector3[] baseVertices;
        private int[] vertexIDs;

        private void Start()
        {
            baseVertices = mesh.vertices;

            vertexIDs = new int[baseVertices.Length];
            for (int i = 0; i < baseVertices.Length; i++)
            {
                vertexIDs[i] = i; // Just a simple mapping of vertex index
            }

            vertexIDBuffer = new ComputeBuffer(baseVertices.Length, sizeof(int));
            vertexIDBuffer.SetData(vertexIDs);
        }

        public void UpdateDeltas(Vector3[] deltas)
        {
            if (deltaBuffer != null)
            {
                deltaBuffer.Release();
            }

            // Create a new compute buffer for the deltas
            deltaBuffer = new ComputeBuffer(deltas.Length, sizeof(float) * 3);
            deltaBuffer.SetData(deltas);
        }

        void OnDestroy()
        {
            if (vertexIDBuffer != null)
            {
                vertexIDBuffer.Release();
            }

            if (deltaBuffer != null)
            {
                deltaBuffer.Release();
            }
        }
    }
}