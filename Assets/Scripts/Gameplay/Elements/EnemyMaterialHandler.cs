﻿using System;
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
        private static readonly int _SMainTex = Shader.PropertyToID("_MainTex");

        public static void ApplyElement(this Enemy enemy, ElementFlag elementFlag)
        {
            // Check if the texture exists for this enemy type and element flag
            if (!EnemyMaterialHandler.ContainsTexture(enemy.enemyType, elementFlag)) return;

            // Get the corresponding texture for this element
            var texture = EnemyMaterialHandler.GetEnemyMaterial(enemy.enemyType, elementFlag);
            var renderer = enemy.GetComponent<Renderer>() ?? enemy.GetComponentInChildren<Renderer>();

            if (renderer != null)
            {
                var propertyBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propertyBlock);  

                propertyBlock.SetTexture(_SMainTex, texture);  
                renderer.SetPropertyBlock(propertyBlock);  

            }
        }
        public static Texture GetSwarmElementTexture(this ElementFlag elementFlag)
        {
            return EnemyMaterialHandler.GetEnemyMaterial(EnemyType.Swarm, elementFlag);
        }
    }
}