namespace NRTools.GpuSkinning
{
    using UnityEngine;

    public class DeltaShaderUpdater : MonoBehaviour
    {
        public Material material;
        public MeshVertexMapper vertexMapper;
        private Vector3[] deltas;
        private static readonly int _SVertexIDs = Shader.PropertyToID("_VertexIDs");
        private static readonly int _SDeltas = Shader.PropertyToID("_Deltas");

        private void Start()
        {
            deltas = new Vector3[vertexMapper.mesh.vertexCount]; 
        }

        private void Update()
        {
            vertexMapper.UpdateDeltas(deltas);
            material.SetBuffer(_SVertexIDs, vertexMapper.vertexIDBuffer);
            material.SetBuffer(_SDeltas, vertexMapper.deltaBuffer);
        }
    }

}