using System.Collections.Generic;
using Gameplay.Enemies;
using UnityEngine;

namespace NRTools.AtlasHelper
{
    public class AtlasMaster : MonoBehaviour
    {
        private static Dictionary<EnemyType, Dictionary<ElementFlag, AtlasRuntimeData>> EnemyAtlasLookup = new();
        public List<AtlasRuntimeData> atlasData;
        private static AtlasMaster _instance;

        #region editor
        public static void AssignInstance(AtlasMaster master)
        {
            _instance = master;
        }

        public static void AddData(AtlasRuntimeData data)
        {
            _instance.atlasData.Add(data);
        }

        public static bool CheckEnemy(EnemyType enemy, ElementFlag element)
        {
            if (EnemyAtlasLookup.TryGetValue(enemy, out var elementDict))
            {
                return elementDict.ContainsKey(element);
            }

            return false;
        }
        #endregion

        private void Awake()
        {
            if (EnemyAtlasLookup.Count != 0) return;
            foreach (var data in atlasData)
            {
                if (data.textureType == TextureType.Bots)
                {
                    if (EnemyAtlasLookup.ContainsKey(data.enemyType))
                    {
                        if (EnemyAtlasLookup[data.enemyType].ContainsKey(data.elementFlag))
                        {
                            EnemyAtlasLookup[data.enemyType][data.elementFlag] =  data;
                        }
                        else
                        {
                            EnemyAtlasLookup[data.enemyType].Add(data.elementFlag, data);
                        }
                        
                    }
                    else
                    {
                        EnemyAtlasLookup.Add(data.enemyType,
                            new Dictionary<ElementFlag, AtlasRuntimeData> {{data.elementFlag, data}});
                    }
                }
            }
        }
        
        public static Rect GetUVRect(TextureType type, ElementFlag element, out int page)
        {
            page = 0;
            if (EnemyAtlasLookup == null || EnemyAtlasLookup.Count == 0) _instance.Awake();
            if (_instance == null ||_instance.atlasData == null) 
                return Rect.zero;
            foreach (var data in _instance.atlasData)
            {
                if (data.textureType == type && data.elementFlag == element)
                {
                    page = data.AtlasPage;
                    return data.UVRect;
                }
            }
            return Rect.zero;
        }
        
        public static Rect GetRect(EnemyType enemyType, ElementFlag element, out int page)
        {
            page = 0;
            if (EnemyAtlasLookup == null || EnemyAtlasLookup.Count == 0) _instance.Awake();
            if (EnemyAtlasLookup.TryGetValue(enemyType, out var elementDict))
            {
                if (elementDict.TryGetValue(element, out var rect))
                {
                    page = rect.AtlasPage;
                    return rect.UVRect;
                }
            }

            return Rect.zero;
        }
    }
}