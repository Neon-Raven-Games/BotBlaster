using UnityEngine;

public static class GameBalancer
{

    public static int KillEnemy(Wave wave)
    {
        wave.numberOfEnemies--;
        return wave.numberOfEnemies;
    }
    // we can use RNG needs to create more fun probability based waves when introducing elements
    public static Wave GenerateWave(int waveNumber, EnemyType[] availableEnemyTypes, Transform centralPoint)
    {
        var numberOfEnemies = Mathf.CeilToInt(waveNumber * 1.5f);
        var spawnRadius = Mathf.Min(10f + waveNumber, 50f);
        var selectedEnemyTypes = new EnemyType[numberOfEnemies];
        
        for (int i = 0; i < numberOfEnemies; i++)
        {
            selectedEnemyTypes[i] = availableEnemyTypes[Random.Range(0, availableEnemyTypes.Length)];
        }

        var spawnPoints = SpawnPointGenerator.GenerateSpawnPoints(numberOfEnemies, spawnRadius, centralPoint, selectedEnemyTypes);
        var spawnInterval = Mathf.Max(1.5f - waveNumber * 0.01f, 0.3f);
        return new Wave(numberOfEnemies, selectedEnemyTypes, spawnInterval, spawnPoints);
    }
}