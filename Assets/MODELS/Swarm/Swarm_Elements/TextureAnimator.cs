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
    [SerializeField] private Material material;
    [SerializeField] private List<Texture> textures;
    [SerializeField] private float frameRate = 0.1f;
    [SerializeField] private List<ElementAnimatedTextures> elementAnimatedTextures;
    private static Dictionary<ElementFlag, List<Texture>> _elementAnimatedTextures;
    
    // if we need to edit more properties, lets make 2 of these to avoid the conflict
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
        if (_elementAnimatedTextures == null)
        {
            _elementAnimatedTextures = new();
            lock (_elementAnimatedTextures)
            {
                foreach (var tex in elementAnimatedTextures)
                {
                    // can we convert the sprites to a texture?
                    var textures = new List<Texture>();
                    foreach (var sprite in tex.textures)
                    {
                        var texture = new Texture2D((int) sprite.rect.width, (int) sprite.rect.height);
                        var pixels = sprite.texture.GetPixels((int) sprite.rect.x, (int) sprite.rect.y,
                            (int) sprite.rect.width, (int) sprite.rect.height);
                        texture.SetPixels(pixels);
                        texture.Apply();
                        textures.Add(texture);
                    }

                    _elementAnimatedTextures.Add(tex.elementFlag, textures);
                }
            }
        }


        textures = _elementAnimatedTextures[ElementFlag.Fire];
        material.mainTexture = textures[0];
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
        textures = _elementAnimatedTextures[element];
       
        _renderer.GetPropertyBlock(_propertyBlock, 0);  // Assuming the second material
        _propertyBlock.SetTexture(_SMainTex, characterMaterial.mainTexture);
        _renderer.SetPropertyBlock(_propertyBlock, 1);
        
        _renderer.GetPropertyBlock(_propertyBlock, 1);  // Assuming the second material
        _propertyBlock.SetTexture(_SMainTex, textures[_currentFrame]);
        _renderer.SetPropertyBlock(_propertyBlock, 1);
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
        _renderer.GetPropertyBlock(_propertyBlock, 1);  // Assuming the second material
        _propertyBlock.SetTexture(_SMainTex, textures[_currentFrame]);
        _renderer.SetPropertyBlock(_propertyBlock, 1);
    }
}