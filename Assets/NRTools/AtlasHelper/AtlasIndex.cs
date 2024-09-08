using System;
using System.Collections.Generic;
using Gameplay.Enemies;
using NRTools.AtlasHelper;
using UnityEngine;

[Serializable]
public class AtlasRuntimeData
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
}

namespace NRTools.AtlasHelper
{
    public class AtlasIndex : MonoBehaviour
    {
        public List<AtlasRuntimeData> AtlasData;
        private Dictionary<ElementFlag, AtlasRuntimeData> _elementRects = new();

        public void Awake()
        {
            _elementRects.Clear();
            foreach (var data in AtlasData)
            {
                if (data.elementFlag != ElementFlag.None)
                    _elementRects.Add(data.elementFlag, data);
            }
        }

        public Rect GetRect(ElementFlag element, out int page)
        {
            page = 0;
            if (_elementRects.TryGetValue(element, out var rect))
            {
                page = rect.AtlasPage;
                return rect.UVRect;
            }
            return Rect.zero;
        }
    }
}