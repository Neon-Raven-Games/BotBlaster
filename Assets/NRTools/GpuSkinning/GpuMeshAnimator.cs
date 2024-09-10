using Unity.Mathematics;
using UnityEngine;

namespace NRTools.GpuSkinning
{


    public class GpuMeshAnimator : MonoBehaviour
    {
        private static readonly int _SFrameIndex = Shader.PropertyToID("_FrameIndex");
        private static readonly int _SInterpolationFactor = Shader.PropertyToID("_InterpolationFactor");
        private static readonly int _SFrameCount = Shader.PropertyToID("_FrameCount");

        private static readonly int _SVertexIDs = Shader.PropertyToID("_VertexIDs");
        private static readonly int _SDeltas = Shader.PropertyToID("_Deltas");

        [SerializeField] private Mesh mesh;
        [SerializeField] private float animationSpeed = 1.0f;
        [SerializeField] private AnimationData animationData;
        [SerializeField] private Renderer renderer;

        private MaterialPropertyBlock _propertyBlock;

        private int _numFrames;

        private ComputeBuffer _vertexIDBuffer;
        private ComputeBuffer _deltaBuffer;

        private Vector3[] _baseVertices;
        private Vector3[][] _deltaFrames;

        private int[] _vertexIDs;
        private float _currentFrame;

        private void LoadDeltaData()
        {
            _deltaFrames = new Vector3[animationData.frameDeltas.Count][];
            for (var i = 0; i < animationData.frameDeltas.Count; i++)
                _deltaFrames[i] = animationData.frameDeltas[i].deltaVertices.ToArray();
        }

        private void Start()
        {
            if (animationData == null || animationData.frameDeltas.Count == 0 ||
                animationData.vertexIndices.Count == 0 || mesh == null)
            {
                Debug.LogError("Missing variables to pass to the GpuMeshAnimator");
                return;
            }

            _numFrames = animationData.frameDeltas.Count;
            _baseVertices = mesh.vertices;
            _vertexIDs = animationData.vertexIndices.ToArray();

            LoadDeltaData();

            int totalDeltas = _vertexIDs.Length * _numFrames;
            Vector3[] allDeltas = new Vector3[totalDeltas];

            for (int i = 0; i < _numFrames; i++)
            {
                for (int j = 0; j < _vertexIDs.Length; j++)
                {
                    allDeltas[i * _vertexIDs.Length + j] = _deltaFrames[i][j];
                }
            }

            _deltaBuffer = new ComputeBuffer(totalDeltas, sizeof(float) * 3);
            _deltaBuffer.SetData(allDeltas);
            _vertexIDBuffer = new ComputeBuffer(_baseVertices.Length, sizeof(int));
            _vertexIDBuffer.SetData(_vertexIDs);

            renderer.sharedMaterial.SetBuffer(_SVertexIDs, _vertexIDBuffer);
            renderer.sharedMaterial.SetBuffer(_SDeltas, _deltaBuffer);

            _propertyBlock = new MaterialPropertyBlock();

            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetInt(_SFrameCount, _baseVertices.Length);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        private void Update()
        {
            _currentFrame += Time.deltaTime * animationSpeed;
            if (_currentFrame >= _numFrames) _currentFrame = 0;

            var frame0 = Mathf.FloorToInt(_currentFrame);
            var t = _currentFrame - frame0;

            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetInt(_SFrameIndex, frame0);
            _propertyBlock.SetFloat(_SInterpolationFactor, t);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        private void OnDestroy()
        {
            if (_vertexIDBuffer != null) _vertexIDBuffer.Release();
            if (_deltaBuffer != null) _deltaBuffer.Release();
        }
    }
}