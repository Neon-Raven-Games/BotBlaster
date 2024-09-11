using System.IO;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class GpuMeshAnimator : MonoBehaviour
    {
        private static readonly int _SFrameIndex = Shader.PropertyToID("_FrameIndex");
        private static readonly int _SInterpolationFactor = Shader.PropertyToID("_InterpolationFactor");
        private static readonly int _SVertexCount = Shader.PropertyToID("_VertexCount");
        private static readonly int _SFrameCount = Shader.PropertyToID("_FrameCount");

        private static readonly int _SVertexIDs = Shader.PropertyToID("_VertexIDs");
        private static readonly int _SDeltas = Shader.PropertyToID("_Deltas");
        private static readonly int _SBoneDQ = Shader.PropertyToID("_BoneDQ");

        private static readonly int _SBoneCount = Shader.PropertyToID("_BoneCount");
        private static readonly int _SBoneIndices = Shader.PropertyToID("_BoneIndices");
        private static readonly int _SBoneWeights = Shader.PropertyToID("_BoneWeights");
        private static readonly int _SBoneMatrices = Shader.PropertyToID("_BoneMatrices");

        public string path;
        [SerializeField] private Mesh mesh;
        [SerializeField] private float animationSpeed = 1.0f;
        [SerializeField] private AnimationData animationData;
        [SerializeField] private Renderer renderer;

        // [SerializeField] private ComputeShader dualQuaternionShader;

        private MaterialPropertyBlock _propertyBlock;

        private int _numFrames;

        private ComputeBuffer _vertexIDBuffer;
        private ComputeBuffer _deltaBuffer;

        private ComputeBuffer _boneIndexBuffer;
        private ComputeBuffer _boneWeightBuffer;
        private ComputeBuffer _boneMatrixBuffer;

        private ComputeBuffer _dualQuaternionBuffer;
        private int _dualQuaternionKernelHandle;

        private Transform[] _bones;
        private Vector3[] _baseVertices;
        private Vector3[][] _deltaFrames;

        private int[] _vertexIDs;
        private float _currentFrame;

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

            LoadBoneData();

            renderer.sharedMaterial.SetBuffer(_SVertexIDs, _vertexIDBuffer);
            renderer.sharedMaterial.SetBuffer(_SDeltas, _deltaBuffer);
            renderer.sharedMaterial.SetBuffer(_SBoneIndices, _boneIndexBuffer);
            renderer.sharedMaterial.SetBuffer(_SBoneWeights, _boneWeightBuffer);
            renderer.sharedMaterial.SetBuffer(_SBoneMatrices, _boneMatrixBuffer);

            _propertyBlock = new MaterialPropertyBlock();

            var quat = DeserializeAnimationData(path);
            _dualQuaternionBuffer = new ComputeBuffer(quat.dualQuaternions.Count, sizeof(float) * 8);
            _dualQuaternionBuffer.SetData(quat.dualQuaternions);
            
            renderer.sharedMaterial.SetBuffer(_SBoneDQ, _dualQuaternionBuffer);
            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetInt(_SVertexCount, _baseVertices.Length);
            _propertyBlock.SetInt(_SFrameCount, _numFrames);
            _propertyBlock.SetInt(_SBoneCount, animationData.boneMatricesPerFrame.Count / _numFrames);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        private void Update()
        {
            _currentFrame += Time.deltaTime * animationSpeed;
            if (_currentFrame >= _numFrames) _currentFrame = 0;

            var frame0 = Mathf.FloorToInt(_currentFrame);
            var frame1 = (frame0 + 1) % _numFrames;
            var t = _currentFrame - frame0;


            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetInt(_SFrameIndex, frame0);
            _propertyBlock.SetFloat(_SInterpolationFactor, t);
            renderer.SetPropertyBlock(_propertyBlock);

            // int boneCount = animationData.boneMatricesPerFrame.Count / _numFrames;
            // SampleBoneMatricesForFrame(frame0, frame1, boneCount, t);
        }

        private static DualQuaternionAnimationData DeserializeAnimationData(string path)
        {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<DualQuaternionAnimationData>(json);
        }

        private void OnDestroy()
        {
            if (_vertexIDBuffer != null) _vertexIDBuffer.Release();
            if (_deltaBuffer != null) _deltaBuffer.Release();
            if (_boneIndexBuffer != null) _boneIndexBuffer.Release();
            if (_boneWeightBuffer != null) _boneWeightBuffer.Release();
            if (_boneMatrixBuffer != null) _boneMatrixBuffer.Release();
        }

        private void LoadDeltaData()
        {
            _deltaFrames = new Vector3[animationData.frameDeltas.Count][];
            for (var i = 0; i < animationData.frameDeltas.Count; i++)
                _deltaFrames[i] = animationData.frameDeltas[i].deltaVertices.ToArray();
        }

        private void LoadBoneData()
        {
            var vertexCount = mesh.vertexCount;

            var boneIndices = new int4[vertexCount];
            var boneWeights = new float4[vertexCount];

            for (var i = 0; i < vertexCount; i++)
            {
                var skinData = animationData.frameDeltas[0].deltaSkinData[i];
                boneIndices[i] = skinData.boneIndices;
                boneWeights[i] = skinData.boneWeights;
            }

            for (int i = 0; i < vertexCount; i++)
            {
                float weightSum = boneWeights[i].x + boneWeights[i].y + boneWeights[i].z + boneWeights[i].z;
                if (Mathf.Abs(weightSum - 1.0f) > 0.01f)
                {
                    Debug.LogWarning($"Bone weights for vertex {i} are not normalized: {weightSum}");
                }
            }

            _boneIndexBuffer = new ComputeBuffer(vertexCount, sizeof(int) * 4);
            _boneIndexBuffer.SetData(boneIndices);
            _boneWeightBuffer = new ComputeBuffer(vertexCount, sizeof(float) * 4);
            _boneWeightBuffer.SetData(boneWeights);

            _boneMatrixBuffer = new ComputeBuffer(animationData.boneMatricesPerFrame.Count, sizeof(float) * 16);
            _boneMatrixBuffer.SetData(animationData.boneMatricesPerFrame);
        }
    }
}