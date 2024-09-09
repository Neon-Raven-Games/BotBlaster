using System;
using Gameplay.Enemies;
using NRTools.AtlasHelper;
using UnityEngine;

[Serializable]
public class AtlasData : ScriptableObject
{
    public TextureType textureType;

    [ShowIf("textureType", TextureType.Bots)]
    public EnemyType enemyType;

    [ShowIf("textureType", TextureType.Bots, TextureType.Blaster, TextureType.BlasterCombined)]
    public ElementFlag elementFlag;

    [ShowIf("textureType", TextureType.Environment)]
    public string sceneName;

    public Rect UVRect;
    public int AtlasPage;
    
    [HideInInspector]
    public GameObject prefab;
}

namespace NRTools.AtlasHelper
{
    public class NRAtlasManager : MonoBehaviour
    {
        [SerializeField] private Material material;
        private static NRAtlasManager _instance;
        private static readonly int _SUVOffset = Shader.PropertyToID("_UVOffset");
        private static MaterialPropertyBlock _materialPropertyBlock;
        [SerializeField] private int _textureWidth;
        [SerializeField] private int _textureHeight;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static void SetUVAndAtlasPage(Rect uvRect, int atlasPage, Renderer renderer)
        {
            if (_materialPropertyBlock == null)
                _materialPropertyBlock = new MaterialPropertyBlock();

            _materialPropertyBlock.Clear();
            _materialPropertyBlock.SetVector(_SUVOffset, new Vector4(
                uvRect.x , uvRect.y, uvRect.width, uvRect.height));

            renderer.SetPropertyBlock(_materialPropertyBlock);
        }
    }
}