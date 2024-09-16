using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class GpuSkinnedAnimator : MonoBehaviour
    {
        /*
        private static readonly int _SFrameIndex = Shader.PropertyToID("_FrameIndex");
        private static readonly int _SInterpolationFactor = Shader.PropertyToID("_InterpolationFactor");
        private static readonly int _SFrameCount = Shader.PropertyToID("_FrameCount");

        private static readonly int _SVertexIDs = Shader.PropertyToID("_VertexInfo");
        private static readonly int _SDeltas = Shader.PropertyToID("_Deltas");

        private static readonly int _SBoneDq = Shader.PropertyToID("_BoneDQ");
        private static readonly int _SMorphResultBuffer = Shader.PropertyToID("_Vertices");
        private static readonly int _SBlendResult = Shader.PropertyToID("_BlendResult");
        
        public string path;
        
        [SerializeField] [Range(0, 1)] private float bulgeCompensation = 0f;
        [SerializeField] private Mesh mesh;
        [SerializeField] private float animationSpeed = 1.0f;
        [SerializeField] private Renderer renderer;

        [FormerlySerializedAs("dualQuaternionShader")] [SerializeField]
        private ComputeShader blendBoneShader;

        [SerializeField] private ComputeShader applyMorphShader;

        [FormerlySerializedAs("boneBindShader")] [SerializeField]
        private ComputeShader computeBoneShader;

        private DualQuaternionAnimationData _animationData;
        private MaterialPropertyBlock _propertyBlock;

        private int _numFrames;

        // compute
        private ComputeBuffer _vertexBuffer;
        private ComputeBuffer _deltaBuffer;
        private ComputeBuffer _dualQuaternionBuffer;

        // blend
        private ComputeBuffer _poseMatricesBuffer;
        private ComputeBuffer _boneDirectionBuffer;

        private ComputeBuffer _morphResultBuffer;
        private ComputeBuffer _skinResultBuffer;


        private int _boneBindKernelHandle;
        private int _dualQuaternionKernelHandle;
        private int _applyMorphKernelHandle;

        private float _currentFrame;

        private const int _NUM_THREADS = 256; 

        private void DeserializeDualQuaternionSkinning()
        {
            _animationData = DeserializeAnimationData(path);
            _boneMatrix = new Matrix4x4[_animationData.boneMatricesPerFrame.Count * _animationData.verticesInfo.Count];
            // var numBones = _animationData.boneMatricesPerFrame.Values.First().Count / 16;
            var numBones = 9; // todo number is not populating right
            
            for (var i = 0; i < _animationData.boneMatricesPerFrame.Count; i++)
            {
                var flattenedArray = _animationData.boneMatricesPerFrame[i];
    
                for (var j = 0; j < numBones; j++)
                {
                    _boneMatrix[i * numBones + j] = flattenedArray[j].ToMatrix4x4();
                }
            }
        }

        private void Start()
        {
            DeserializeDualQuaternionSkinning();

            if (_animationData == null || _animationData.frameDeltas.Count == 0 ||
                _animationData.verticesInfo.Count == 0 || mesh == null || _animationData.dualQuaternions.Count == 0)
            {
                Debug.LogError("Missing variables to pass to the GpuMeshAnimator");
                return;
            }
            
            if (applyMorphShader == null || blendBoneShader == null || computeBoneShader == null)
            {
                Debug.LogError("One or more shaders are missing. Please assign the shaders in the inspector.");
                return;
            }
            _numFrames = _animationData.frameDeltas.Count;
            AssignKernels();

            var vertexInfoSize = PopulateVertexInfo();
            PopulateDeltas();
            SetSkinnedDualQuatCrossThread();
            SetPoseMatricesCrossThread();
            SetBoneDirectionsCrossThread();

            renderer.sharedMaterial.SetBuffer(_SVertexIDs, _vertexBuffer);
            renderer.sharedMaterial.SetBuffer(_SDeltas, _deltaBuffer);
            renderer.sharedMaterial.SetBuffer(_SBoneDq, _dualQuaternionBuffer);

            computeBoneShader.SetVector("bone_orientation", Vector3.up);
            computeBoneShader.SetBuffer(_boneBindKernelHandle, "bind_dual_quaternions", _boneDirectionBuffer);
            computeBoneShader.SetMatrix("self_matrix", transform.worldToLocalMatrix);

            blendBoneShader.SetInt("vertex_count", vertexInfoSize);
            blendBoneShader.SetBuffer(_dualQuaternionKernelHandle, "vertex_infos", _vertexBuffer);
            
            _propertyBlock = new MaterialPropertyBlock();

            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetInt(_SFrameCount, _numFrames);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        private int PopulateVertexInfo()
        {
            var vertexInfoSize = 88;
            _vertexBuffer = new ComputeBuffer(_animationData.verticesInfo.Count, vertexInfoSize, ComputeBufferType.Structured);
            _vertexBuffer.SetData(_animationData.verticesInfo.ToArray());
            return vertexInfoSize;
        }

        private void PopulateDeltas()
        {
            var morphDeltaSize = 48;
            var morphDeltasList = new List<MorphDelta>();
            foreach (var VARIABLE in _animationData.frameDeltas)
            {
                foreach (var detla in VARIABLE)
                    morphDeltasList.Add(detla);
            }
            
            var morphDeltas = morphDeltasList.ToArray();

            _deltaBuffer = new ComputeBuffer(morphDeltas.Length, morphDeltaSize, ComputeBufferType.Structured);
            _morphResultBuffer = new ComputeBuffer(_animationData.verticesInfo.Count, 88, ComputeBufferType.Structured);
            applyMorphShader.SetBuffer(_applyMorphKernelHandle, "source", _vertexBuffer);
            applyMorphShader.SetBuffer(_applyMorphKernelHandle, "delta", _deltaBuffer);
            applyMorphShader.SetBuffer(_applyMorphKernelHandle, "target", _morphResultBuffer); 
            renderer.sharedMaterial.SetBuffer(_SMorphResultBuffer, _morphResultBuffer);
            
            _skinResultBuffer = new ComputeBuffer(_animationData.verticesInfo.Count, morphDeltaSize, ComputeBufferType.Structured);
            blendBoneShader.SetBuffer(_dualQuaternionKernelHandle, "skinned_vertex_infos", _skinResultBuffer);
        }

        private void AssignKernels()
        {
            _dualQuaternionKernelHandle = blendBoneShader.FindKernel("cs_main");
            _boneBindKernelHandle = computeBoneShader.FindKernel("cs_main");
            _applyMorphKernelHandle = applyMorphShader.FindKernel("cs_main");
        }

        private void SetBoneDirectionsCrossThread()
        { 
            _boneDirectionBuffer = new ComputeBuffer(_animationData.boneDirections.Length, sizeof(float) * 4, ComputeBufferType.Structured);
            _boneDirectionBuffer.SetData(_animationData.boneDirections);
         
            computeBoneShader.SetBuffer(_boneBindKernelHandle, "bone_directions", _boneDirectionBuffer);
            blendBoneShader.SetBuffer(_dualQuaternionKernelHandle, "bone_directions", _boneDirectionBuffer);
        }

        // skinned_dual_quaternions
        // compute & blend
        // output
        private void SetSkinnedDualQuatCrossThread()
        {
            var dualQuaternionSize = sizeof(float) * 8;

            _dualQuaternionBuffer = new ComputeBuffer(_animationData.dualQuaternions.Count, dualQuaternionSize,
                ComputeBufferType.Structured);

            // outputs data to cross communicate between compute shaders
            blendBoneShader.SetBuffer(_dualQuaternionKernelHandle, "skinned_dual_quaternions", _dualQuaternionBuffer);
            computeBoneShader.SetBuffer(_boneBindKernelHandle, "skinned_dual_quaternions", _dualQuaternionBuffer);
        }

        // pose_matrices
        // compute & blend
        private void SetPoseMatricesCrossThread()
        {
            if (_boneMatrix.Length == 0) return;
            _poseMatricesBuffer = new ComputeBuffer(_boneMatrix.Length, sizeof(float) * 16);
            _poseMatricesBuffer.SetData(_boneMatrix);

            // we need to set the buffer to the poses on this frame
            blendBoneShader.SetBuffer(_dualQuaternionKernelHandle, "pose_matrices", _poseMatricesBuffer);
            computeBoneShader.SetBuffer(_boneBindKernelHandle, "pose_matrices", _poseMatricesBuffer);
        }
        private void Update()
        {
            _currentFrame += Time.deltaTime * animationSpeed;
            if (_currentFrame >= _numFrames) _currentFrame -= _numFrames;

            var frame0 = Mathf.FloorToInt(_currentFrame);
            var t = _currentFrame - frame0;

            renderer.sharedMaterial.SetBuffer(_SBlendResult, _skinResultBuffer);
            
            applyMorphShader.SetFloat("weight", t);
            applyMorphShader.SetInt("frame", frame0);
            
            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetInt(_SFrameIndex, frame0);
            _propertyBlock.SetFloat(_SInterpolationFactor, t);
            renderer.SetPropertyBlock(_propertyBlock);

            _deltaBuffer.SetData(_animationData.frameDeltas[frame0]);
            applyMorphShader.SetBuffer(_applyMorphKernelHandle, "delta", _deltaBuffer);
            
            var morphThreadGroups = Mathf.CeilToInt(_animationData.verticesInfo.Count / (float)_NUM_THREADS);
            applyMorphShader.Dispatch(_applyMorphKernelHandle, morphThreadGroups, 1, 1);
            
            var boneThreadGroups = Mathf.CeilToInt(9 / (float)_NUM_THREADS);
            // computeBoneShader.Dispatch(_boneBindKernelHandle, boneThreadGroups, 1, 1);

            var blendThreadGroups = Mathf.CeilToInt(_animationData.verticesInfo.Count / (float)_NUM_THREADS);
            blendBoneShader.SetFloat("compensation_coef", bulgeCompensation);
            // blendBoneShader.Dispatch(_dualQuaternionKernelHandle, blendThreadGroups, 1, 1);
        }

        private Matrix4x4[] _boneMatrix;

        private void OnDestroy()
        {
            ReleaseBuffer(ref _vertexBuffer);
            ReleaseBuffer(ref _deltaBuffer);
            ReleaseBuffer(ref _dualQuaternionBuffer);
            ReleaseBuffer(ref _boneDirectionBuffer);
            ReleaseBuffer(ref _poseMatricesBuffer);
            ReleaseBuffer(ref _morphResultBuffer);
            ReleaseBuffer(ref _skinResultBuffer);
        }

        private void ReleaseBuffer(ref ComputeBuffer buffer)
        {
            if (buffer == null) return;
            
            buffer.Release();
            buffer = null;
        }


        private static DualQuaternionAnimationData DeserializeAnimationData(string path)
        {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<DualQuaternionAnimationData>(json);
        }
        */
    }
}