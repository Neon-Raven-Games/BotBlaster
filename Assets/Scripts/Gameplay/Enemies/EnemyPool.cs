using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Enemies;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class EnemyData
{
    public EnemyType enemyType;
    public ElementFlag elementFlag;
    public GameObject enemyPrefab;
    public int poolSize;
    public int minWaveSpawn;
    public int baseHealth;
    public int baseDamage;
    public float baseSpeed;
    public float baseAttackRange;
    public float baseAttackCooldown;
    
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
                enemy = Object.Instantiate(enemies[0], enemies[0].transform.position, Quaternion.identity, enemies[0].transform.parent).GetComponent<Enemy>();
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
    public List<EnemyData> enemyData;
    private static EnemyPool _instance;
    [SerializeField] private WaveController waveController;
    private static readonly ConcurrentDictionary<EnemyType, EnemyCollection> _SEnemyPool = new();
    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        foreach (var data in enemyData)
        {
            _SEnemyPool.TryAdd(data.enemyType, new EnemyCollection(data.enemyType, new List<Enemy>(data.poolSize + 1)));
        }
        Initialize().Forget();
    }
    
    public static Enemy GetEnemy(EnemyType enemyType)
    {
        var enemyCollection = _SEnemyPool.TryGetValue(enemyType, out var collection)
            ? collection
            : null;
        if (enemyCollection != null) return enemyCollection.GetEnemy();
        
        Debug.LogError("Enemy not found in pool.");
        return null;
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
                var enemy = Instantiate(data.enemyPrefab, transform.position, Quaternion.identity, enemyParent.transform).GetComponent<Enemy>();
                data.Initialize(enemy);
                enemy.gameObject.SetActive(false);
                _SEnemyPool[data.enemyType].enemies.Add(enemy);
            }
        }
        _instance.waveController.StartWaves();
    }
}