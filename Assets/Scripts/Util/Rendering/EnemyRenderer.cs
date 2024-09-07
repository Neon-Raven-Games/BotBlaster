using System;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Enemies;

[Serializable]
public class EnemyToRender
{
    public EnemyType enemyType;
    public Mesh mesh;
    public Material material;
}

public class EnemyRenderer : MonoBehaviour
{
    public List<EnemyToRender> enemyRenderSettings;

    private Dictionary<EnemyType, Mesh> enemyMeshes = new();
    private Dictionary<EnemyType, Material> enemyMaterials = new();
    private Dictionary<EnemyType, List<Matrix4x4>> enemyMatrices = new();
    private static EnemyRenderer _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    public static void Initialize()
    {
        _instance.InitializeRenderSettings();
    }

    private void InitializeRenderSettings()
    {
        foreach (var setting in enemyRenderSettings)
        {
            if (enemyMeshes.ContainsKey(setting.enemyType)) continue;
            
            enemyMeshes[setting.enemyType] = setting.mesh;
            enemyMaterials[setting.enemyType] = setting.material;
            enemyMatrices[setting.enemyType] = new List<Matrix4x4>();
        }
    }

    // add transforms in here for dynamic updating
    public static void AddEnemyToRender(Enemy enemy)
    {
        if (!_instance.enemyMeshes.ContainsKey(enemy.enemyType)) return; 
        _instance.enemyMatrices[enemy.enemyType].Add(enemy.transform.localToWorldMatrix);
    }

    private void LateUpdate()
    {
        RenderEnemies();
    }

    private void RenderEnemies()
    {
        foreach (var enemyType in enemyMatrices.Keys)
        {
            var matrices = enemyMatrices[enemyType];
            if (matrices.Count == 0) continue;

            Graphics.DrawMeshInstanced(
                enemyMeshes[enemyType], 
                0, 
                enemyMaterials[enemyType], 
                matrices.ToArray(), 
                matrices.Count
            );

            matrices.Clear();
        }
    }
}
