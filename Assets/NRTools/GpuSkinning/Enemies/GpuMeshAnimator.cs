using System;
using System.Collections.Generic;
using System.IO;
using Gameplay.Enemies;
using Newtonsoft.Json;
using NRTools.AtlasHelper;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

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

        public void UpdateElement(ElementFlag elementFlag)
        {
            element = elementFlag;
            if (element != ElementFlag.None)
            {
                if (AnimationManager.IsLoaded) OnAnimationManagerLoaded();

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

#if UNITY_EDITOR
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

        protected void OverrideFrameNumber(int frame)
        {
            _currentFrame = frame;
        }

        private bool _blending;
        private AnimationData _nextAnimationData;
        private float _blendProgress;
        private float _blendDuration = 0.1f;

        protected void TransitionToAnimation(AnimationData nextAnimation)
        {
            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetInt(_SNextAnimationOffset, nextAnimation.vertexOffset);
            _propertyBlock.SetFloat(_SBlendFactor, 0.01f);
            renderer.SetPropertyBlock(_propertyBlock);
            _currentFrame = 0; 
            _blendProgress = 0f;
            _blending = true;
            _nextAnimationData = nextAnimation;
        }

        protected void SetAnimation(AnimationData data)
        {
            _animationData = data;
            _currentFrame = 0;
            _numFrames = _animationData.frameCount;
            _shaderFrameIndex = -1;
            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetInt(_SFrameOffset, _animationData.vertexOffset);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        private void OnAnimationManagerLoaded()
        {
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

        private void Start()
        {
            if (!AnimationManager.IsLoaded) AnimationManager.OnLoaded += OnAnimationManagerLoaded;
            else OnAnimationManagerLoaded();
        }

        protected virtual void Update()
        {
            if (!_initialized) return;

            // Update current frame
            _currentFrame += Time.deltaTime * animationSpeed;
            var frame0 = Mathf.FloorToInt(_currentFrame);
            var t = _currentFrame - frame0;

            if (_currentFrame >= _numFrames - 1)
            {
                if (_nextAnimationData != null && _nextAnimationData != _animationData)
                {
                    _animationData = _nextAnimationData;
                    _numFrames = _animationData.frameCount;
                    _currentFrame = 0;
                    renderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetInt(_SFrameOffset, _animationData.vertexOffset + _animationData.vertexCount * 2);
                    renderer.SetPropertyBlock(_propertyBlock); 
                }
                else if (_animationData.loop)
                {
                    _currentFrame %= _numFrames; // Loop back to the beginning
                    frame0 = Mathf.FloorToInt(_currentFrame);
                    t = _currentFrame - frame0;
                }
                else
                {
                    renderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetInt(_SFrameOffset, 
                        _animationData.vertexOffset + _animationData.vertexCount * (_numFrames - 2));
                    renderer.SetPropertyBlock(_propertyBlock);
                    return;
                }
            }


            renderer.GetPropertyBlock(_propertyBlock);

            // Update frame offset and interpolation factor
            if (_shaderFrameIndex != frame0)
            {
                _shaderFrameIndex = frame0;
                _propertyBlock.SetInt(_SFrameOffset,
                    _animationData.vertexOffset + _animationData.vertexCount * _shaderFrameIndex);
            }

            _propertyBlock.SetFloat(_SInterpolationFactor, t);

            if (_blending)
            {
                _blendProgress += Time.deltaTime / _blendDuration;
                
                if (_blendProgress >= 1f)
                {
                    _blendProgress = 1f;
                    _blending = false;
                    _animationData = _nextAnimationData;
                    if (_animationData != null)
                    {
                        _numFrames = _animationData.frameCount;
                        _propertyBlock.SetInt(_SFrameOffset, _animationData.vertexOffset);
                    }

                    _currentFrame = 0f;

                    _propertyBlock.SetFloat(_SInterpolationFactor, 0f);
                    _propertyBlock.SetFloat(_SBlendFactor, 0f);
                }
                else
                {
                    // Update blend factor
                    _propertyBlock.SetFloat(_SBlendFactor, _blendProgress);

                    // Set next animation frame offset
                    _propertyBlock.SetInt(_SNextAnimationOffset,
                        _nextAnimationData.vertexOffset + _nextAnimationData.vertexCount * _shaderFrameIndex);
                }
            }
            else
            {
                _propertyBlock.SetFloat(_SBlendFactor, 0f);
            }

            renderer.SetPropertyBlock(_propertyBlock);
        }


        protected virtual AnimationData InitialAnimation()
        {
            return null;
        }
    }
}