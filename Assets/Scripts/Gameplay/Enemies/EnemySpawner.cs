using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay.Enemies;
using Gameplay.Util;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform centralPoint; // Center point for 360 spawning
    [SerializeField] private float spawnRadius = 15f;
    public int currentWave = 0;
    internal Wave currentWaveData;

    public void Awake()
    {
        GameBalancer.spawner = this;
    }
    public void StartNextWave()
    {
        currentWaveData = GameBalancer.GenerateWave(currentWave + 1, spawnRadius, Enum.GetValues(typeof(EnemyType)).Cast<EnemyType>().ToArray(), centralPoint);
        SpawnWave(currentWaveData).Forget();
        currentWave++;
    }
    
    public bool WaveCompleted() =>
        currentWaveData.numberOfEnemies <= 0;

    // todo, create a way to cache this and prevent closure/allocation
    // this needs to be managed much better!
    private async UniTaskVoid SpawnWave(Wave wave)
    {
        for (var i = 0; i < wave.numberOfEnemies; i++)
        {
            var i1 = i;
            var enemyType = wave.enemyTypes[i1 % wave.enemyTypes.Length];
            var spawnPosition = wave.spawnPositions[i1 % wave.spawnPositions.Length];
            TimerManager.AddTimer(wave.spawnInterval * i, () =>
            {
                SpawnEnemy(enemyType, spawnPosition);
            });
            await UniTask.Yield();
        }
    }

    private static void SpawnEnemy(EnemyType type, Vector3 position)
    {
        var enemy = EnemyPool.GetEnemy(type);
        enemy.element = ElementFlag.Water;
        enemy.transform.position = position;
        enemy.gameObject.SetActive(true);
    }
}