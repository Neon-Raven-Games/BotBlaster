using System;
using System.Linq;
using Gameplay.Util;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs; // Array of enemy prefabs matching the EnemyType enum
    public Transform centralPoint; // Center point for 360 spawning

    private int currentWave = 0;
    private Wave currentWaveData;
    public void StartNextWave()
    {
        currentWaveData = GameBalancer.GenerateWave(currentWave + 1, Enum.GetValues(typeof(EnemyType)).Cast<EnemyType>().ToArray(), centralPoint);
        SpawnWave(currentWaveData);
        currentWave++;
    }
    
    public void WaveCompleted()
    {
        if (currentWaveData.numberOfEnemies > 0)
        {
            Debug.Log("All waves completed!");
            return;
        }
        
        StartNextWave();
    }

    // todo, create a way to cache this and prevent closure/allocation
    // this needs to be managed much better!
    private void SpawnWave(Wave wave)
    {
        for (var i = 0; i < wave.numberOfEnemies; i++)
        {
            var i1 = i;
            TimerManager.AddTimer(wave.spawnInterval * i, () =>
            {
                SpawnEnemy(wave.enemyTypes[i1 % wave.enemyTypes.Length],
                        wave.spawnPositions[i1 % wave.spawnPositions.Length]);
            });
        }
    }

    private static void SpawnEnemy(EnemyType type, Vector3 position)
    {
        var enemy = EnemyPool.GetEnemy(type);
        enemy.transform.position = position;
        enemy.gameObject.SetActive(true);
    }
}