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
    [SerializeField] private DevController player;
    private const int MAX_ENEMY_COUNT = 20;
    public Transform centralPoint;
    public int currentWave;
    public bool paused;

    [SerializeField] private int bossWaveInterval = 20;

    internal Wave currentWaveData;
#if UNITY_EDITOR
    internal WaveAnalytics _waveAnalytics;
#endif
    [SerializeField] private float spawnRadius = 10f;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        DrawWireCircle(centralPoint.position, spawnRadius, 50);
    }
#endif

    private static void DrawWireCircle(Vector3 position, float radius, int segments)
    {
        var angle = 0f;
        var angleStep = 360f / segments;
        var prevPoint = position + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), 0, Mathf.Sin(Mathf.Deg2Rad * angle)) *
            radius;

        for (var i = 0; i <= segments; i++)
        {
            angle += angleStep;
            var nextPoint = position +
                            new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), 0, Mathf.Sin(Mathf.Deg2Rad * angle)) * radius;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }


    public void StartNextWave()
    {
        GameBalancer.InitializeElementProbability(currentWave);

        GameBalancer.CalculatePlayerPerformance(player);
        currentWaveData = currentWave % bossWaveInterval != 0 && currentWave != 0
            ? GameBalancer.GenerateWave(currentWave)
            : GameBalancer.GenerateBossWave(currentWave / bossWaveInterval);

#if UNITY_EDITOR
        if (_waveAnalytics != null)
        {
            _waveAnalytics.UpdatePlayTime();
            GameAnalytics.LogWaveData(_waveAnalytics);
        }

        _waveAnalytics = new WaveAnalytics(
            currentWave,
            spawnRadius,
            currentWaveData.numberOfEnemies,
            new BalanceObject(FindObjectOfType<DevController>()));
#endif
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

    public async UniTaskVoid SpawnWave()
    {
        for (int i = 0; i < currentWaveData.numberOfEnemies; i++)
        {
            while (EnemyPool.CurrentEnemyCount >= MAX_ENEMY_COUNT)
            {
                await UniTask.Delay(50);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(currentWaveData.spawnInterval));

            if (!WaveController.IsWaveSpawning()) return;

            var enemyType = GameBalancer._SEnemyProbability.PickValue();
            var element = GameBalancer._SElementProbabilityList.PickValue();
            SpawnEnemy(enemyType, currentWave, element);

            // we should update the intensity here when approached on the todo
            // UpdateAudioIntensity(i);

            await UniTask.Yield();
        }
    }


    private void SpawnEnemy(EnemyType type, int waveNumber, ElementFlag element)
    {
        var enemy = EnemyPool.GetEnemy(type);
        enemy.element = element;
        if (type == EnemyType.Swarm && enemy is Swarm swarm)
            swarm.swarmCount = Mathf.CeilToInt(1 * ElementDecorator.STRENGTH_MULTIPLIER * waveNumber);

        enemy.element = element;
        enemy.ApplyBalance(waveNumber);
        IntroController.StartIntro(enemy);

#if UNITY_EDITOR
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
#endif
    }
}