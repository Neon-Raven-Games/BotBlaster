using System;
using System.Collections.Generic;
using Gameplay.Enemies;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.Elements
{
    [Serializable]
    public class ElementMaterial
    {
        public ElementFlag elementFlag;
        public Material characterMaterial;
    }

    [Serializable]
    public class ElementTexture
    {
        public ElementFlag elementFlag;
        public Texture elementTexture;
    }

    [Serializable]
    public class EnemyTextureCollection
    {
        public EnemyElementTextures[] enemyElementMaterials;
    }

    [Serializable]
    public class EnemyElementTextures
    {
        public EnemyType enemyType;
        public ElementTexture[] elementMaterials;
    }

    public class EnemyMaterialHandler : MonoBehaviour
    {
        public EnemyElementTextures[] enemyElementTextures;
        private readonly Dictionary<EnemyType, Dictionary<ElementFlag, Texture>> _enemyMaterials = new();
        private static EnemyMaterialHandler _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            foreach (var mat in enemyElementTextures)
            {
                _enemyMaterials.Add(mat.enemyType, new Dictionary<ElementFlag, Texture>());
                foreach (var elementMaterial in mat.elementMaterials)
                {
                    _enemyMaterials[mat.enemyType].Add(elementMaterial.elementFlag, elementMaterial.elementTexture);
                }
            }
        }
        
        public static Texture GetEnemyMaterial(EnemyType enemyType, ElementFlag elementFlag)
        {
            return _instance._enemyMaterials[enemyType][elementFlag];
        }
        
        public static bool ContainsTexture(EnemyType enemyType, ElementFlag elementFlag)
        {
            if (_instance == null) return false;
            return _instance._enemyMaterials.ContainsKey(enemyType) && 
                   _instance._enemyMaterials[enemyType].ContainsKey(elementFlag);
        }
    }
    
    public static class ElementExtensions
    {
        
        public static void ApplyElement(this Enemy enemy, ElementFlag elementFlag)
        {
            if (!EnemyMaterialHandler.ContainsTexture(enemy.enemyType, elementFlag)) return;
            // todo, this is temp, waiting for art
            var rend = enemy.GetComponent<MeshRenderer>();
            if (!rend) rend = enemy.GetComponentInChildren<MeshRenderer>();
            rend.material.mainTexture = 
                EnemyMaterialHandler.GetEnemyMaterial(enemy.enemyType, elementFlag);
        }
        
        public static Texture GetElementTexture(this ElementFlag elementFlag)
        {
            return EnemyMaterialHandler.GetEnemyMaterial(EnemyType.Swarm, elementFlag);
        }
    }
}