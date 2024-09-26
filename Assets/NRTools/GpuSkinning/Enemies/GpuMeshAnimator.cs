using System;
using Gameplay.Enemies;
using NRTools.Animator.NRNodes;
using NRTools.AtlasHelper;
using NRTools.CustomAnimator;
using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class GpuMeshAnimator : MonoBehaviour
    {
        private static readonly int _SInterpolationFactor = Shader.PropertyToID("_InterpolationFactor");
        private static readonly int _SVertCount = Shader.PropertyToID("_VertexCount");
        private static readonly int _SLocalScale = Shader.PropertyToID("_LocalScale");
        private static readonly int _SFrameOffset = Shader.PropertyToID("_FrameOffset");
        private static readonly int _SUVOffset = Shader.PropertyToID("_UVOffset");
        public TextureType textureType;

        [ShowIf("textureType", TextureType.Bots)]
        public EnemyType enemyType;

        [SerializeField] private ElementFlag element;
        [SerializeField] private Mesh mesh;
        [SerializeField] private float animationSpeed = 1.0f;
        [SerializeField] private Renderer renderer;

        private AnimationData _animationData;
        private MaterialPropertyBlock _propertyBlock;
        internal int _numFrames;
        internal float _currentFrame;
        private int _shaderFrameIndex;
        private bool _initialized;
        private AtlasIndex _atlasIndex;
        private static readonly int _SNextAnimationOffset = Shader.PropertyToID("_NextAnimationOffset");
        private static readonly int _SBlendFactor = Shader.PropertyToID("_BlendFactor");

        public float AnimationDuration => _numFrames / 24f;

        protected virtual void TransitionToNextAnimation()
        {
            if (_nextAnimationData != null && _nextAnimationData != _animationData)
            {
                TransitionToAnimation(_nextAnimationData);
                _nextAnimationData = null; // Reset after transitioning
            }
        }

        protected virtual AnimationData GetNextAnimation()
        {
            return null;
        }

        public virtual void PlayOneShotHitAnimation()
        {
        }

        public virtual void PlayAttackAnimation()
        {
        }

        private void Start()
        {
            if (!AnimationManager.IsLoaded) AnimationManager.OnLoaded += OnAnimationManagerLoaded;
            else OnAnimationManagerLoaded();
        }

        private void OnAnimationManagerLoaded()
        {
            if (_initialized) return;
            AnimationManager.OnLoaded -= OnAnimationManagerLoaded;
            _animationData = InitialAnimation();
            if (_animationData == null || _animationData.frameCount == 0 || mesh == null)
            {
                Debug.LogError("Failed to load animation data or mesh.");
                return;
            }

            _propertyBlock = new MaterialPropertyBlock();
            _atlasIndex = GetComponent<AtlasIndex>();
            _numFrames = _animationData.frameCount;
            renderer.GetPropertyBlock(_propertyBlock);

            _propertyBlock.SetInt(_SVertCount, _animationData.vertexCount);
            _propertyBlock.SetInt(_SFrameOffset, _animationData.vertexOffset);

            if (element == ElementFlag.None)
            {
                // todo, when we have uv mappings for none, we need to have a uv rect for the plain blaster/enemies
                var uvRect = _atlasIndex.GetRect(ElementFlag.Electricity, out var page);
                _propertyBlock.SetVector(_SUVOffset, new Vector4(
                    uvRect.x, uvRect.y, uvRect.width, uvRect.height));
            }
            else
            {
                var uvRect = _atlasIndex.GetRect(element, out var page);
                _propertyBlock.SetVector(_SUVOffset, new Vector4(
                    uvRect.x, uvRect.y, uvRect.width, uvRect.height));
            }

            _propertyBlock.SetFloat(_SLocalScale, Math.Abs(transform.localScale.x));

            renderer.SetPropertyBlock(_propertyBlock);
            _initialized = true;
        }

        public void UpdateElement(ElementFlag elementFlag)
        {
            element = elementFlag;
            if (element != ElementFlag.None)
            {
                if (!_initialized && AnimationManager.IsLoaded) OnAnimationManagerLoaded();

                _atlasIndex = GetComponent<AtlasIndex>();

                if (_atlasIndex)
                {
                    _propertyBlock = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(_propertyBlock);
                    _atlasIndex.textureType = textureType;
                    if (textureType == TextureType.Bots) _atlasIndex.enemyType = enemyType;
                    var uvRect = _atlasIndex.GetRect(element, out var page);
                    _propertyBlock.SetVector(_SUVOffset, new Vector4(
                        uvRect.x, uvRect.y, uvRect.width, uvRect.height));
                    renderer.SetPropertyBlock(_propertyBlock);
                }
            }
        }

        public void SetUninitialized() => _initialized = false;

#if UNITY_EDITOR
        [ExecuteAlways]
        private void OnDrawGizmos()
        {
            if (Application.isPlaying) return;
            if (element != ElementFlag.None)
            {
                _atlasIndex = GetComponent<AtlasIndex>();

                if (_atlasIndex)
                {
                    _propertyBlock = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(_propertyBlock);
                    _atlasIndex.textureType = textureType;
                    if (textureType == TextureType.Bots) _atlasIndex.enemyType = enemyType;
                    var uvRect = _atlasIndex.GetRect(element, out var page);
                    _propertyBlock.SetVector(_SUVOffset, new Vector4(
                        uvRect.x, uvRect.y, uvRect.width, uvRect.height));
                    renderer.SetPropertyBlock(_propertyBlock);
                }
            }
        }
#endif

        private bool _blending;
        private AnimationData _nextAnimationData;
        private float _blendProgress;
        private float _blendDuration = 0.5f;

        protected void TransitionToAnimation(AnimationData nextAnimation)
        {
            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetInt(_SNextAnimationOffset, nextAnimation.vertexOffset);
            _propertyBlock.SetFloat(_SBlendFactor, 0.01f);
            renderer.SetPropertyBlock(_propertyBlock);

            _nextAnimationData = nextAnimation;
            _blendProgress = 0f;
            _blending = true;
        }

        protected void SetAnimation(AnimationData data)
        {
            _animationData = data;
            _currentFrame = 0;
            _numFrames = _animationData.frameCount;
            _shaderFrameIndex = 0;
            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetInt(_SFrameOffset, _animationData.vertexOffset);
            _propertyBlock.SetFloat(_SBlendFactor, 0f);
            _propertyBlock.SetFloat(_SInterpolationFactor, 0f);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        private AnimationTransitionController _transitionController;

        public void EditorUpdate(float seconds)
        {
            if (!_initialized) return;
            
            _transitionController.EditorUpdate(seconds);
            return;
            _currentFrame = seconds * 24f;
            var frame0 = Mathf.FloorToInt(_currentFrame);
            var t = _currentFrame - frame0;

            renderer.GetPropertyBlock(_propertyBlock);

            if (!_blending && _currentFrame >= _numFrames - 2 &&
                HandleTransitionlessAnimation(ref frame0, ref t)) return;

            UpdateShaderFrame(frame0);
            _propertyBlock.SetFloat(_SInterpolationFactor, t);

            if (_blending) HandleBlending();
            else _propertyBlock.SetFloat(_SBlendFactor, 0f);

            renderer.SetPropertyBlock(_propertyBlock);
        }

        // we need to load in the transition controller with the proper graph
        public virtual void Update()
        {
            if (!_initialized) return;
            EditorUpdate(Time.deltaTime);
            return;
            
            _currentFrame += Time.deltaTime * animationSpeed;
            var frame0 = Mathf.FloorToInt(_currentFrame);
            var t = _currentFrame - frame0;

            renderer.GetPropertyBlock(_propertyBlock);

            if (!_blending && _currentFrame >= _numFrames - 1 &&
                HandleTransitionlessAnimation(ref frame0, ref t)) return;

            UpdateShaderFrame(frame0);
            _propertyBlock.SetFloat(_SInterpolationFactor, t);

            if (_blending) HandleBlending();
            else _propertyBlock.SetFloat(_SBlendFactor, 0f);

            renderer.SetPropertyBlock(_propertyBlock);
        }

        private void UpdateShaderFrame(int frame0)
        {
            if (_shaderFrameIndex != frame0)
            {
                _shaderFrameIndex = frame0;
                _propertyBlock.SetInt(_SFrameOffset,
                    _animationData.vertexOffset + _animationData.vertexCount * _shaderFrameIndex);
            }
        }

        private bool HandleTransitionlessAnimation(ref int frame0, ref float t)
        {
            if (_animationData.loop) frame0 = WrapFrame(out t);
            else
            {
                HoldLastFrame();
                return true;
            }

            return false;
        }

        private int WrapFrame(out float t)
        {
            _currentFrame %= _numFrames;
            var frame0 = Mathf.FloorToInt(_currentFrame);
            t = _currentFrame - frame0;
            _shaderFrameIndex = frame0;
            return frame0;
        }

        public void SetTransitionController()
        {
            if (!_transitionController)
            {
                _transitionController ??= gameObject.GetComponent<AnimationTransitionController>();
                _transitionController ??= gameObject.AddComponent<AnimationTransitionController>();
                _transitionController.Start();
                _transitionController.renderer = renderer;
                _transitionController.PlayAnimation(InitialAnimation());
            }
        }
        
        public void TransitionTo(AnimationTransitionData blend)
        {
            if (blend == null) return;
            // _transitionController.Transition(blend);
        }

        private void HoldLastFrame()
        {
            _propertyBlock.SetInt(_SFrameOffset,
                _animationData.vertexOffset + _animationData.vertexCount * (_numFrames - 2));
            renderer.SetPropertyBlock(_propertyBlock);
        }

        private void HandleBlending()
        {
            _blendProgress += Time.deltaTime / _blendDuration;
            _blendProgress = Mathf.Clamp01(_blendProgress);

            if (_blendProgress >= 1f) HandleBlendingFinished();
            else UpdateBlendProgress();
        }

        private void UpdateBlendProgress()
        {
            _propertyBlock.SetFloat(_SBlendFactor, _blendProgress);
            _propertyBlock.SetInt(_SNextAnimationOffset, _nextAnimationData.vertexOffset);
        }

        private void HandleBlendingFinished()
        {
            _blendProgress = 1f;
            _blending = false;
            _animationData = _nextAnimationData;

            if (_animationData != null)
            {
                _numFrames = _animationData.frameCount;
                _shaderFrameIndex = 0;
                _propertyBlock.SetInt(_SFrameOffset, _animationData.vertexOffset);
            }
            else _animationData = InitialAnimation();

            _propertyBlock.SetFloat(_SInterpolationFactor, 0f);
            _propertyBlock.SetFloat(_SBlendFactor, 0f);
            _currentFrame = 0;
        }

        protected virtual AnimationData InitialAnimation()
        {
            return null;
        }

        public void PlayAnimation(AnimatorNode node)
        {
            // _transitionController.PlayAnimation(node.data); 
            _transitionController.PreviewSequence(node.GUID); 
        }
        public void PlayAnimation(string animator, string animName)
        {
            _animationData = AnimationManager.GetAnimationData(animator, animName);
            _currentFrame = 0;
            _numFrames = _animationData.frameCount;
            _transitionController.PlayAnimation(_animationData);
        }
    }
}