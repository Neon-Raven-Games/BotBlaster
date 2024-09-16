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

// we need to structure the data better.
// enemy type needs to share the same atlas rects, not instantiating all of these
// so we need a centralized controller that does not necessarily link the prefab,
// but allows us to get the data we need for the enemy type/element flag
namespace NRTools.AtlasHelper
{
    public class AtlasIndex : MonoBehaviour
    {
        public TextureType textureType;
        public EnemyType enemyType;
        public List<AtlasRuntimeData> AtlasData = new();

        public Rect GetRect(ElementFlag element, out int page)
        {
            page = 0;

            foreach (var data in AtlasData)
            {
                if (textureType == TextureType.Bots)
                {
                    if (enemyType == data.enemyType && data.elementFlag == element)
                    {
                        page = data.AtlasPage;
                        return data.UVRect;
                    }
                }

                if (textureType is TextureType.Blaster or TextureType.BlasterCombined)
                {
                    if (data.elementFlag == element)
                    {
                        page = data.AtlasPage;
                        return data.UVRect;
                    }
                }
            }

            if (textureType == TextureType.Bots)
            {
                var rect = AtlasMaster.GetRect(enemyType, element, out var pgNum);
                page = pgNum;
                return rect;
            }
            else
            {
                var rect = AtlasMaster.GetUVRect(textureType, element, out var pgNum);
                page = pgNum;
                return rect;
            }
        }
    }
}