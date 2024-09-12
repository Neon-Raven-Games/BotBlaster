using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NRTools.GpuSkinning.Util;
using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class GpuMeshAnimator : MonoBehaviour
    {
        private static readonly int _SFrameIndex = Shader.PropertyToID("_FrameIndex");
        private static readonly int _SInterpolationFactor = Shader.PropertyToID("_InterpolationFactor");
        private static readonly int _SFrameCount = Shader.PropertyToID("_FrameCount");
        private static readonly int _SVertCount = Shader.PropertyToID("_VertexCount");

        private static readonly int _SVertices = Shader.PropertyToID("vertices");
        private static readonly int _SDeltas = Shader.PropertyToID("deltas");
        private static readonly int _SBoneDq = Shader.PropertyToID("bone_dq");
        private static readonly int _SBoneMatrices = Shader.PropertyToID("bone_matrices");

        public string path;
        [SerializeField] private Mesh mesh;
        [SerializeField] private float animationSpeed = 1.0f;
        [SerializeField] internal DualQuaternionAnimationData animationData;
        [SerializeField] private Renderer renderer;

        private MaterialPropertyBlock _propertyBlock;

        private int _numFrames;
        private Matrix4x4[] _boneMatrix;

        private ComputeBuffer _vertexIDBuffer;
        private ComputeBuffer _deltaBuffer;
        private ComputeBuffer _boneMatrixBuffer;
        private ComputeBuffer _dualQuaternionBuffer;

        private Transform[] _bones;
        private float _currentFrame;

        private void Start()
        {
            DeserializeDualQuaternionSkinning();
            if (animationData == null || animationData.frameDeltas.Count == 0 ||
                animationData.verticesInfo.Count == 0 || mesh == null)
            {
                Debug.LogError("Missing variables to pass to the GpuMeshAnimator");
                return;
            }

            _numFrames = animationData.frameDeltas.Count;
            Debug.Log(animationData.frameDeltas.Count);
            var morphDeltasList = new List<MorphDelta>();
            foreach (var VARIABLE in animationData.frameDeltas)
            {
                foreach (var detla in VARIABLE)
                    morphDeltasList.Add(detla);
            }

            var morphDeltas = morphDeltasList.ToArray();

            _deltaBuffer = new ComputeBuffer(morphDeltas.Length, 48, ComputeBufferType.Structured);
            _deltaBuffer.SetData(morphDeltas);

            _vertexIDBuffer = new ComputeBuffer(animationData.verticesInfo.Count, 88);
            _vertexIDBuffer.SetData(animationData.verticesInfo.ToArray());

            _dualQuaternionBuffer = new ComputeBuffer(animationData.dualQuaternions.Count, sizeof(float) * 8);
            _dualQuaternionBuffer.SetData(animationData.dualQuaternions);

            renderer.sharedMaterial.SetBuffer(_SBoneMatrices, _boneMatrixBuffer);
            renderer.sharedMaterial.SetBuffer(_SBoneDq, _dualQuaternionBuffer);

            _propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetBuffer(_SVertices, _vertexIDBuffer);
            _propertyBlock.SetBuffer(_SDeltas, _deltaBuffer);
            _propertyBlock.SetInt(_SVertCount, animationData.verticesInfo.Count);
            _propertyBlock.SetInt(_SFrameCount, _numFrames);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        private void LogBoneMatrices(int frame)
        {
            Debug.Log($"Logging Bone Matrices for Frame: {frame}");

            var numBones = _boneMatrix.Length / _numFrames;
            for (int i = 0; i < numBones; i++)
            {
                Matrix4x4 boneMatrix = _boneMatrix[frame * numBones + i];
                Debug.Log($"Bone {i} Matrix:\n {boneMatrix}");
            }
        }

        private int _shaderFrameIndex = 0;

        private void Update()
        {
            _currentFrame += Time.deltaTime * animationSpeed;
            if (_currentFrame >= _numFrames) _currentFrame -= _numFrames;

            var frame0 = Mathf.FloorToInt(_currentFrame);
            var frame1 = (frame0 + 1) % _numFrames;
            var t = _currentFrame - frame0;

            renderer.GetPropertyBlock(_propertyBlock);
            if (_shaderFrameIndex != frame0)
            {
                _shaderFrameIndex = frame0;
                _propertyBlock.SetInt(_SFrameIndex, frame0);
            }

            Debug.Log("Shader should be: " +
                      (animationData.verticesInfo.Count * frame0) / animationData.verticesInfo.Count);
            _propertyBlock.SetInt(_SVertCount, animationData.verticesInfo.Count);
            _propertyBlock.SetInt(_SFrameCount, _numFrames);
            _propertyBlock.SetFloat(_SInterpolationFactor, t);
            renderer.SetPropertyBlock(_propertyBlock);
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
            if (_boneMatrixBuffer != null) _boneMatrixBuffer.Release();
            if (_dualQuaternionBuffer != null) _dualQuaternionBuffer.Release();
        }

        private void DeserializeDualQuaternionSkinning()
        {
            animationData = DeserializeAnimationData(path);
            Debug.Log("delta frame count: " + animationData.frameDeltas.Count);
            _boneMatrix = new Matrix4x4[animationData.boneMatricesPerFrame.Count * animationData.verticesInfo.Count];
            var numBones = 9;
            for (var i = 0; i < animationData.boneMatricesPerFrame.Count; i++)
            {
                var flattenedArray = animationData.boneMatricesPerFrame[i];
                for (var j = 0; j < numBones; j++)
                {
                    var flat = flattenedArray[j].ToMatrix4x4();
                    _boneMatrix[i * numBones + j] = flattenedArray[j].ToMatrix4x4();
                }
            }

            _boneMatrixBuffer = new ComputeBuffer(_boneMatrix.Length, sizeof(float) * 16);
            _boneMatrixBuffer.SetData(_boneMatrix);
        }
    }
}