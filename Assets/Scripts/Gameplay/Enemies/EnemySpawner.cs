using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay.Enemies;
using Gameplay.Enemies.EnemyTypes;
using NRTools.Analytics;
using UnityEngine;

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

    internal WaveAnalytics _waveAnalytics;

    [SerializeField] private float spawnRadius = 10f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
    
        DrawWireCircle(centralPoint.position, spawnRadius, 50);
    }

    private static void DrawWireCircle(Vector3 position, float radius, int segments)
    {
        var angle = 0f;
        var angleStep = 360f / segments;
        var prevPoint = position + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), 0, Mathf.Sin(Mathf.Deg2Rad * angle)) * radius;

        for (var i = 0; i <= segments; i++)
        {
            angle += angleStep;
            var nextPoint = position + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), 0, Mathf.Sin(Mathf.Deg2Rad * angle)) * radius;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }


    public void StartNextWave()
    {
        GameBalancer.InitializeElementProbability(currentWave);

        currentWaveData = currentWave % bossWaveInterval != 0 && currentWave != 0
            ? GameBalancer.GenerateWave(currentWave, GameBalancer.GetCurrentSpawnRadius(currentWave), centralPoint)
            : GameBalancer.GenerateBossWave(currentWave / bossWaveInterval, centralPoint);

        if (_waveAnalytics != null)
        {
            _waveAnalytics.UpdatePlayTime();
            GameAnalytics.LogWaveData(_waveAnalytics);
        }

        _waveAnalytics = new WaveAnalytics(currentWave,
            GameBalancer.GetCurrentSpawnRadius(currentWave),
            currentWaveData.numberOfEnemies, currentWaveData.elementFlags,
            new BalanceObject(FindObjectOfType<DevController>()));

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

        for (var i = 0; i < enemies; i++)
        {
            while (EnemyPool.CurrentEnemyCount >= MAX_ENEMY_COUNT)
            {
                Debug.Log("Max enemies, waiting");
                await UniTask.Yield();
            }

            await UniTask.Delay(TimeSpan.FromSeconds(wave.spawnInterval));

            if (paused || !WaveController.IsWaveSpawning())
                await UniTask.WaitUntil(() => !paused || !WaveController.IsWaveSpawning());
            if (!WaveController.IsWaveSpawning()) return;

            // todo, just generate this shit here, don't cache it
            var enemyType = wave.enemyTypes[i % wave.enemyTypes.Length];
            var spawnPosition = wave.spawnPositions[i % wave.spawnPositions.Length];
            var element = wave.elementFlags[i % wave.elementFlags.Length];
            SpawnEnemy(enemyType, spawnPosition, wave.waveNumber, element);

            await UniTask.Yield();
        }
    }

    private void SpawnEnemy(EnemyType type, Vector3 position, int waveNumber, ElementFlag element)
    {
        var enemy = EnemyPool.GetEnemy(type);
        enemy.element = element;
        enemy.transform.position = position;
        if (type == EnemyType.Swarm && enemy is Swarm swarm)
            swarm.swarmCount = Mathf.CeilToInt(1 * ElementDecorator.STRENGTH_MULTIPLIER * waveNumber);

        enemy.gameObject.SetActive(true);
        enemy.element = element;
        enemy.ApplyBalance(waveNumber);

        var enemyBalance = _waveAnalytics.EnemyBalanceData.FirstOrDefault(e => e.enemyType == type);

        if (enemyBalance == null)
        {
            enemyBalance = new EnemyBalanceObject(enemy) {count = 1, Elements = new List<ElementFlag> {element}};
            _waveAnalytics.EnemyBalanceData.Add(enemyBalance);
        }
        else
        {
            enemyBalance.count++;
            if (!enemyBalance.Elements.Contains(element)) enemyBalance.Elements.Add(element);
        }
    }
}