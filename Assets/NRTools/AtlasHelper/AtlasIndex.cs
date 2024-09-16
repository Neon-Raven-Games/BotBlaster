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
        public EnemyType enemyType;
        public List<AtlasRuntimeData> AtlasData;

        public Rect GetRect(ElementFlag element, out int page)
        {
            page = 0;
            
            foreach (var data in AtlasData)
            {
                if (enemyType == data.enemyType && data.elementFlag == element)
                {
                    page = data.AtlasPage;
                    return data.UVRect;
                }
            }
            var rect = AtlasMaster.GetRect(enemyType, element, out var pgNum);
            page = pgNum;
            return rect;
        }
    }
}