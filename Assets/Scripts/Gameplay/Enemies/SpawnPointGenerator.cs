using System.Linq;
using Gameplay.Enemies;
using UnityEngine;

public static class SpawnPointGenerator
{
    private static readonly EnemyType[] _SFlyingUnitTypes = { EnemyType.Swarm, EnemyType.GlassCannon };
    
    public static Vector3[] GenerateSpawnPoints(int numberOfEnemies, float spawnRadius, Transform centralPoint, EnemyType[] enemyTypes)
    {
        var spawnPoints = new Vector3[numberOfEnemies];
        var angleStep = 360f / numberOfEnemies;

        for (var i = 0; i < numberOfEnemies; i++)
        {
            var angle = i * angleStep + Random.Range(-angleStep / 2, angleStep / 2);
            var x = Mathf.Sin(angle * Mathf.Deg2Rad) * spawnRadius;
            var z = Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius;

            var isFlying = IsFlyingUnit(enemyTypes[i]);
            var y = isFlying ? Random.Range(6.5f, 8.5f) : GetValidSpawnHeight(enemyTypes[i]);
            spawnPoints[i] = centralPoint.position + new Vector3(x, y, z);
        }

        return spawnPoints;
    }

    private static float GetValidSpawnHeight(EnemyType type)
    {
        if (type == EnemyType.Tank) return 3.2f;
        return 2.3f;
    }

    private static bool IsFlyingUnit(EnemyType type) =>
        _SFlyingUnitTypes.Contains(type);
}
