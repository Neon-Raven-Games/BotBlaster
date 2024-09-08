using System;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using Gameplay.Enemies;
using UnityEngine;

// Muzzle parenting
// UI Select not generated at runtime. make it hard coded and pentagram

// AI Animations:
// Expression based on wave level (number animations? wave based division)
// Hurt/Hit
// Death
// Falling (right before death on flying enemies0
// Shooting/Divebombing - Attack (Divebomb for swarm units, powerup/shoot for glass cannon, melee for tank/grunt)
// Lerp animation in (lerp color? High white intensity, then fade to normal color)

public class EnemySpawner : MonoBehaviour
{
    private const int MAX_ENEMY_COUNT = 20;
    public Transform centralPoint;
    public int currentWave;
    public bool paused;

    [SerializeField] private int bossWaveInterval = 5;

    internal Wave currentWaveData;

    public void Awake()
    {
        GameBalancer.spawner = this;
    }

    public void StartNextWave()
    {
        GameBalancer.InitializeElementProbability(currentWave);
        
        currentWaveData = currentWave % bossWaveInterval != 0 && currentWave != 0
            ? GameBalancer.GenerateWave(currentWave, GameBalancer.GetCurrentSpawnRadius(currentWave), centralPoint)
            : GameBalancer.GenerateBossWave(currentWave / bossWaveInterval, centralPoint);

        WaveController.waveEnemies = currentWaveData.numberOfEnemies;
        SpawnWave().Forget();
        if (currentWave % bossWaveInterval == 0 && currentWave > 0) OnBossKilled();
        currentWave++;
    }

    private void OnBossKilled()
    {
        GameBalancer.OnBossDefeated();
    }

    public bool WaveCompleted() =>
        EnemyPool.CurrentEnemyCount == 0 && WaveController.waveEnemies <= 0;

    private async UniTaskVoid SpawnWave()
    {
        var wave = currentWaveData;
        var enemies = wave.numberOfEnemies;
        if (paused) return;

        for (var i = 0; i < enemies; i++)
        {
            while (EnemyPool.CurrentEnemyCount >= MAX_ENEMY_COUNT)
            {
                Debug.Log("Max enemies, waiting");
                await UniTask.Yield();
            }

            await UniTask.Delay(TimeSpan.FromSeconds(wave.spawnInterval));

            if (paused) return;
            var enemyType = wave.enemyTypes[i % wave.enemyTypes.Length];
            var spawnPosition = wave.spawnPositions[i % wave.spawnPositions.Length];
            var element = wave.elementFlags[i % wave.elementFlags.Length];
            SpawnEnemy(enemyType, spawnPosition, wave.waveNumber, element);

            await UniTask.Yield();
        }
    }

    private static void SpawnEnemy(EnemyType type, Vector3 position, int waveNumber, ElementFlag element)
    {
        var enemy = EnemyPool.GetEnemy(type);
        enemy.element = element;
        enemy.transform.position = position;
        enemy.gameObject.SetActive(true);
        enemy.element = element;
        enemy.ApplyBalance(waveNumber);
    }
}

public static class GameAnalyticsHelper
{
    private static readonly StringBuilder _SCsvBuilder = new();
    private static DateTime _startTime;

    private static string _filePath;

    public static void InitializeAnalytics()
    {
#if UNITY_EDITOR
        _startTime = DateTime.Now;
        _SCsvBuilder.Clear();
        _SCsvBuilder.AppendLine("WaveNumber,SpawnRadius,NumberOfEnemies,Elements,PlayTimeSeconds");
        _filePath = Path.Combine(Application.persistentDataPath, $"GameAnalytics_{_startTime:yyyy-MM-dd_HH-mm-ss}.csv");
#endif
    }

    public static void LogWaveData(int waveNumber, float spawnRadius, int numberOfEnemies, ElementFlag[] elements)
    {
#if UNITY_EDITOR
        var elapsedTime = (DateTime.Now - _startTime).TotalSeconds;
        string elementString = string.Join(";", elements);
        _SCsvBuilder.AppendLine($"{waveNumber},{spawnRadius},{numberOfEnemies},{elementString},{elapsedTime}");

        Debug.Log(
            $"Logged Wave {waveNumber}: {spawnRadius} radius, {numberOfEnemies} enemies, Elements: {elementString}, Time: {elapsedTime}");
#endif
    }

    public static void FinalizeAnalytics()
    {
#if UNITY_EDITOR
        File.WriteAllText(_filePath, _SCsvBuilder.ToString());
        Debug.Log($"Game analytics saved to {_filePath}");
#endif
    }

    public static void ResetAnalytics()
    {
        _SCsvBuilder.Clear();
    }
}