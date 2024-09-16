using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Gameplay.Enemies;
using UnityEngine;

[Serializable]
public class ElementAnimatedTextures
{
    public ElementFlag elementFlag;
    public List<Sprite> textures;
}

public class TextureAnimator : MonoBehaviour
{
    [SerializeField] private List<Texture> textures;
    [SerializeField] private float frameRate = 0.1f;
    
    private MaterialPropertyBlock _propertyBlock;
   
    private int _currentFrame;
    private float _timeSinceLastFrame;
    private bool _playing;
    private static readonly int _SMainTex = Shader.PropertyToID("_MainTex");
    private Renderer _renderer;

    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
        _renderer = GetComponent<Renderer>();
        _playing = false;
        _renderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetTexture(_SMainTex, textures[0]);
        _playing = true;
    }

    private void OnEnable()
    {
        _playing = true;
    }

    private void OnDisable()
    {
        _playing = false;
    }

    public void SwitchElement(ElementFlag element, Material characterMaterial)
    {
        _playing = false;
        _renderer.GetPropertyBlock(_propertyBlock); 
        _propertyBlock.SetTexture(_SMainTex, textures[_currentFrame]);
        _renderer.SetPropertyBlock(_propertyBlock);
        _currentFrame = 0;
        _timeSinceLastFrame = 0;
        _playing = true;
    }

    private void Update()
    {
        if (!_playing) return;
        _timeSinceLastFrame += Time.deltaTime;
        if (_timeSinceLastFrame < frameRate) return;

        _timeSinceLastFrame = 0;
        _currentFrame = (_currentFrame + 1) % textures.Count;
        _renderer.GetPropertyBlock(_propertyBlock);  // Assuming the second material
        _propertyBlock.SetTexture(_SMainTex, textures[_currentFrame]);
        _renderer.SetPropertyBlock(_propertyBlock);
    }
}