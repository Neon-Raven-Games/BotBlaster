using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class GpuMeshAnimator : MonoBehaviour
    {
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material material;
        [SerializeField] private float animationSpeed = 1.0f;
        [SerializeField] private bool useInterpolation = true;
        [SerializeField] private AnimationData animationData;
        private MaterialPropertyBlock _propertyBlock;
        private int _numFrames;
        private ComputeBuffer _vertexIDBuffer;
        private ComputeBuffer _deltaBuffer;
        private Vector3[] _interpolatedDeltas;
        private Vector3[] _baseVertices;
        private Vector3[][] _deltaFrames;
        private int[] _vertexIDs;
        private float _currentFrame;
        private static readonly int _FrameIndex = Shader.PropertyToID("_FrameIndex");
        private static readonly int _InterpolationFactor = Shader.PropertyToID("_InterpolationFactor");

        private static readonly int _SVertexIDs = Shader.PropertyToID("_VertexIDs");
        private static readonly int _SDeltas = Shader.PropertyToID("_Deltas");

        private void LoadDeltaData()
        {
            _baseVertices = animationData.vertexPositions.ToArray();
            _deltaFrames = new Vector3[animationData.frameDeltas.Count][];
            
            for (var i = 0; i < animationData.frameDeltas.Count; i++)
                _deltaFrames[i] = animationData.frameDeltas[i].deltaVertices.ToArray();
        }

        private void Start()
        {
            if (animationData == null || animationData.frameDeltas.Count == 0 || animationData.vertexIndices.Count == 0 || mesh == null || material == null)
            {
                Debug.LogError("Missing variables to pass to the GpuMeshAnimator");
                return;
            }
            _numFrames = animationData.frameDeltas.Count;
            _baseVertices = mesh.vertices;
            _interpolatedDeltas = new Vector3[_baseVertices.Length];

            _vertexIDs = animationData.vertexIndices.ToArray();

            _vertexIDBuffer = new ComputeBuffer(_baseVertices.Length, sizeof(int));
            _vertexIDBuffer.SetData(_vertexIDs);

            LoadDeltaData();

            _deltaBuffer = new ComputeBuffer(_baseVertices.Length, sizeof(float) * 3);
            UpdateDeltas(_deltaFrames[0]);
        }

        private void Update()
        {
            if (_numFrames <= 1)
            {
                UpdateDeltas(_deltaFrames[0]);
                return;
            }
            
            _currentFrame += Time.deltaTime * animationSpeed;
            if (_currentFrame >= _numFrames)
                _currentFrame = 0;

            if (useInterpolation)
            {
                var frame0 = Mathf.FloorToInt(_currentFrame);
                var frame1 = (frame0 + 1) % _numFrames;
                var t = _currentFrame - frame0;

                for (var i = 0; i < _baseVertices.Length; i++)
                    _interpolatedDeltas[i] = Vector3.Lerp(_deltaFrames[frame0][i], _deltaFrames[frame1][i], t);

                UpdateDeltas(_interpolatedDeltas);
            }
            else
            {
                var frameIndex = Mathf.FloorToInt(_currentFrame);
                UpdateDeltas(_deltaFrames[frameIndex]);
            }

            material.SetBuffer(_SVertexIDs, _vertexIDBuffer);
            material.SetBuffer(_SDeltas, _deltaBuffer);
        }

        private void UpdateDeltas(Vector3[] deltas) =>
            _deltaBuffer.SetData(deltas);

        private void OnDestroy()
        {
            if (_vertexIDBuffer != null) _vertexIDBuffer.Release();
            if (_deltaBuffer != null) _deltaBuffer.Release();
        }
    }
}