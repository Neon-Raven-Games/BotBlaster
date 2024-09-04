using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay.Enemies;
using Gameplay.Util;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform centralPoint; 
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
    
    // Muzzle parenting
    // UI Select not generated at runtime, fixed
    
    // AI Animations:
    // Expression based on wave level (number animations? wave based division)
    // Hurt/Hit
    // Death
    // Falling (right before death on flying enemies0
    // Shooting/Divebombing - Attack (Divebomb for swarm units, powerup/shoot for glass cannon, melee for tank/grunt)
    // Lerp animation in (lerp color? High white intensity, then fade to normal color)

    public bool WaveCompleted() =>
        currentWaveData.numberOfEnemies <= 0;

    // todo, create a way to cache this and prevent closure/allocation
    private static async UniTaskVoid SpawnWave(Wave wave)
    {
        // this should fix the spawn issues
        var enemies = wave.numberOfEnemies;
        for (var i = 0; i < enemies; i++)
        {
            var enemyType = wave.enemyTypes[i % wave.enemyTypes.Length];
            var spawnPosition = wave.spawnPositions[i % wave.spawnPositions.Length];
            TimerManager.AddTimer(wave.spawnInterval * i, () =>
            {
                SpawnEnemy(enemyType, spawnPosition, wave.waveNumber);
            });
            await UniTask.Yield();
        }
    }

    private static void SpawnEnemy(EnemyType type, Vector3 position, int waveNumber)
    {
        var enemy = EnemyPool.GetEnemy(type);
        enemy.element = ElementFlag.Water;
        enemy.transform.position = position;
        enemy.ApplyBalance(waveNumber);
        enemy.gameObject.SetActive(true);
    }
}