using System.Collections.Generic;
using UnityEngine;

public class TextureAnimator : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private List<Texture> textures;
    [SerializeField] private float frameRate = 0.1f;
    private int _currentFrame;
    private float _timeSinceLastFrame;
    
    private void Start() =>
        material.mainTexture = textures[0];

    private void Update()
    {
        _timeSinceLastFrame += Time.deltaTime;
        if (_timeSinceLastFrame < frameRate) return;
        
        _timeSinceLastFrame = 0;
        _currentFrame = (_currentFrame + 1) % textures.Count;
        material.mainTexture = textures[_currentFrame];
    }
}
