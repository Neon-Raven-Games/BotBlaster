using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Enemies;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features.Extensions.PerformanceSettings;
using Object = UnityEngine.Object;

[Serializable]
public class ActorData
{
    public int baseHealth;
    public int baseDamage;
    public float baseSpeed;
    public float baseAttackRange;
    public float baseAttackCooldown;

    public void Initialize(Actor character) =>
        character.Initialize(this);
}

[Serializable]
public class EnemyData : ActorData
{
    public EnemyType enemyType;
    public ElementFlag elementFlag;
    public GameObject enemyPrefab;
    public int poolSize;
    public int minWaveSpawn;


    public void Initialize(Enemy enemy) =>
        enemy.Initialize(this);
}

public class EnemyCollection
{
    public EnemyType enemyType;
    public List<Enemy> enemies;
    public int currentIndex;

    public EnemyCollection(EnemyType enemyType, List<Enemy> enemies)
    {
        this.enemyType = enemyType;
        this.enemies = enemies;
        currentIndex = 0;
    }


    public Enemy GetEnemy()
    {
        var enemy = enemies[currentIndex];
        if (enemy.gameObject.activeInHierarchy)
        {
            enemy = enemies.Find(x => !x.gameObject.activeInHierarchy);
            if (enemy == null)
            {
                currentIndex = 0;
                enemy = Object.Instantiate(enemies[0], enemies[0].transform.position, Quaternion.identity,
                    enemies[0].transform.parent).GetComponent<Enemy>();
                enemy.gameObject.SetActive(false);
                enemies.Add(enemy);
            }
        }

        currentIndex = (currentIndex + 1) % enemies.Count;

        return enemy;
    }
}

public class EnemyPool : MonoBehaviour
{
    public static int CurrentEnemyCount { get; private set; } = 0;
    public List<EnemyData> enemyData;
    private static EnemyPool _instance;
    [SerializeField] private WaveController waveController;
    private static readonly ConcurrentDictionary<EnemyType, EnemyCollection> _SEnemyPool = new();

    public void TestSpawnGlassCannon()
    {
        var enemy = GetEnemy(EnemyType.GlassCannon);
        enemy.element = ElementFlag.Fire;
        enemy.ApplyBalance(1);
        IntroController.StartIntro(enemy);
    }

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        XrPerformanceSettingsFeature.SetPerformanceLevelHint(PerformanceDomain.Cpu, PerformanceLevelHint.Boost);
        XrPerformanceSettingsFeature.SetPerformanceLevelHint(PerformanceDomain.Gpu, PerformanceLevelHint.Boost);
        _instance = this;
        foreach (var data in enemyData)
            _SEnemyPool.TryAdd(data.enemyType, new EnemyCollection(data.enemyType,
                new List<Enemy>(data.poolSize + 1)));
        Initialize().Forget();
    }

    public static Enemy GetEnemy(EnemyType enemyType)
    {
        var enemyCollection = _SEnemyPool.TryGetValue(enemyType, out var collection)
            ? collection
            : null;
        if (enemyCollection != null)
        {
            CurrentEnemyCount++;
            return enemyCollection.GetEnemy();
        }

        return null;
    }

    public static void HandleEnemyDeactivation(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
        CurrentEnemyCount--;
    }

    private async UniTaskVoid Initialize()
    {
        foreach (var data in enemyData)
        {
            await UniTask.Yield();
            var enemyParent = new GameObject(data.enemyType.ToString());
            enemyParent.transform.SetParent(transform);
            for (var i = 0; i < data.poolSize; i++)
            {
                var enemy = Instantiate(data.enemyPrefab, transform.position, Quaternion.identity,
                    enemyParent.transform).GetComponent<Enemy>();
                data.Initialize(enemy);
                enemy.gameObject.SetActive(false);
                _SEnemyPool[data.enemyType].enemies.Add(enemy);
            }
        }

        _instance.waveController.Ready();
    }

    public static void SleepAll()
    {
        foreach (var enemyCollection in _SEnemyPool.Values)
        {
            foreach (var enemy in enemyCollection.enemies)
            {
                if (enemy.gameObject.activeInHierarchy)
                {
                    HandleEnemyDeactivation(enemy);
                }
            }
        }

        CurrentEnemyCount = 0;
        _instance.waveController.Ready();
    }
}