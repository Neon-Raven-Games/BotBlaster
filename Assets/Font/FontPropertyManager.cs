using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FontPropertyManager : MonoBehaviour
{
    [SerializeField] private Color glowColor;
    private MaterialPropertyBlock _propertyBlock;
    private Renderer _renderer;
    private static readonly int _SGlowColor = Shader.PropertyToID("_GlowColor");
    private static readonly int _SFaceColor = Shader.PropertyToID("_FaceColor");

    private void OnDrawGizmos()
    {
        // if (!_renderer)
            Start();
    }

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(_SFaceColor, glowColor);
        _renderer.SetPropertyBlock(_propertyBlock);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
