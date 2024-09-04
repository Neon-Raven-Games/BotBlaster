using Gameplay.Enemies;
using UnityEngine;

public static class GameBalancer
{

    public static void KillEnemy(StatusEffectiveness statusEffectiveness)
    {
        spawner.currentWaveData.numberOfEnemies--;
        ScoreManager.AddScore(statusEffectiveness);
    }
    
    // we can use RNG needs to create more fun probability based waves when introducing elements
    public static Wave GenerateWave(int waveNumber, float spawnRadius, EnemyType[] availableEnemyTypes, Transform centralPoint)
    {
        var numberOfEnemies = Mathf.CeilToInt(waveNumber * 1.5f);
        var selectedEnemyTypes = new EnemyType[numberOfEnemies];
        
        for (var i = 0; i < numberOfEnemies; i++)
        {
            selectedEnemyTypes[i] = availableEnemyTypes[Random.Range(0, availableEnemyTypes.Length)];
        }

        var spawnPoints = SpawnPointGenerator.GenerateSpawnPoints(numberOfEnemies, spawnRadius, centralPoint, selectedEnemyTypes);
        var spawnInterval = Mathf.Max(1.5f - waveNumber * 0.01f, 0.3f);
        return new Wave(numberOfEnemies, selectedEnemyTypes, spawnInterval, spawnPoints, waveNumber);
    }

    public static EnemySpawner spawner { get; set; }
}