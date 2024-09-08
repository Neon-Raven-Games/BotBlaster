using System;
using Gameplay.Enemies;
using RNGNeeds;
using UnityEngine;

public static class GameBalancer
{
    private static readonly ProbabilityList<ElementFlag> _SElementProbabilityList = new();
    private static readonly ProbabilityList<EnemyType> _SEnemyProbability = new();

    private const float _POST_BOSS_HEALTH_BOOST = 1.1f;
    private const float _POST_BOSS_DAMAGE_BOOST = 1.05f;
    public static int BossesDefeated { get; private set; } = 0;


    public const float minSpawnRadius = 7f;
    public const float maxSpawnRadius = 20f;
    public const int bossWaveInterval = 10;

    private const float _HEALTH_SCALING_FACTOR = 1.1f;
    private const float _DAMAGE_SCALING_FACTOR = 1.05f;
    private const float _SPEED_SCALING_FACTOR = 1.02f;
    private const float _ATTACK_RANGE_SCALING_FACTOR = 1.02f;
    private const float _COOLDOWN_SCALING_FACTOR = 0.98f;

    // todo, we need to make this dynamic based on the wave number
    private const int _BOSS_MINIONS = 5;

    private const int _EARLY_WAVE_LIMIT = 5;
    private const int _MID_WAVE_LIMIT = 10;
    private const int _LATE_WAVE_LIMIT = 20;

    private const int _EARLY_WATER_PROBABILITY = 70;
    private const int _EARLY_FIRE_PROBABILITY = 10;
    private const int _EARLY_ROCK_PROBABILITY = 10;
    private const int _EARLY_ELECTRICITY_PROBABILITY = 10;
    private const int _EARLY_WIND_PROBABILITY = 10;

    private const int _MID_WATER_PROBABILITY = 40;
    private const int _MID_FIRE_PROBABILITY = 30;
    private const int _MID_EARTH_PROBABILITY = 30;
    private const int _MID_ELECTRICITY_PROBABILITY = 10;
    private const int _MID_WIND_PROBABILITY = 10;

    private const int _LATE_ELEMENT_PROBABILITY = 25;

    public static void InitializeElementProbability(int waveNumber)
    {
        _SElementProbabilityList.ClearList();
        _SEnemyProbability.ClearList();

        if (waveNumber < _EARLY_WAVE_LIMIT)
        {
            _SEnemyProbability.AddItem(EnemyType.Grunt, 70);
            _SEnemyProbability.AddItem(EnemyType.GlassCannon, 20);
            _SEnemyProbability.AddItem(EnemyType.Tank, 10);

            // In early waves, bias towards Water element
            _SElementProbabilityList.AddItem(ElementFlag.Water, _EARLY_WATER_PROBABILITY);
            _SElementProbabilityList.AddItem(ElementFlag.Fire, _EARLY_FIRE_PROBABILITY);
            _SElementProbabilityList.AddItem(ElementFlag.Rock, _EARLY_ROCK_PROBABILITY);
            _SElementProbabilityList.AddItem(ElementFlag.Wind, _EARLY_WIND_PROBABILITY);
            _SElementProbabilityList.AddItem(ElementFlag.Electricity, _EARLY_ELECTRICITY_PROBABILITY);
        }
        else if (waveNumber < _MID_WAVE_LIMIT)
        {
            Debug.Log("Probability list initialized for mid waves.");
            _SEnemyProbability.AddItem(EnemyType.Grunt, 50);
            _SEnemyProbability.AddItem(EnemyType.GlassCannon, 30);
            _SEnemyProbability.AddItem(EnemyType.Tank, 20);
            _SEnemyProbability.AddItem(EnemyType.Swarm, 5);

            _SElementProbabilityList.AddItem(ElementFlag.Water, _MID_WATER_PROBABILITY);
            _SElementProbabilityList.AddItem(ElementFlag.Fire, _MID_FIRE_PROBABILITY);
            _SElementProbabilityList.AddItem(ElementFlag.Rock, _MID_EARTH_PROBABILITY);
            _SElementProbabilityList.AddItem(ElementFlag.Wind, _MID_WIND_PROBABILITY);
            _SElementProbabilityList.AddItem(ElementFlag.Electricity, _MID_ELECTRICITY_PROBABILITY);
        }
        else if (waveNumber < _LATE_WAVE_LIMIT)
        {
            Debug.Log("Probability list initialized for late waves.");
            _SEnemyProbability.AddItem(EnemyType.Grunt, 40);
            _SEnemyProbability.AddItem(EnemyType.GlassCannon, 30);
            _SEnemyProbability.AddItem(EnemyType.Tank, 20);
            _SEnemyProbability.AddItem(EnemyType.Swarm, 15);

            _SElementProbabilityList.AddItem(ElementFlag.Water, _LATE_ELEMENT_PROBABILITY);
            _SElementProbabilityList.AddItem(ElementFlag.Fire, _LATE_ELEMENT_PROBABILITY);
            _SElementProbabilityList.AddItem(ElementFlag.Rock, _LATE_ELEMENT_PROBABILITY);
            _SElementProbabilityList.AddItem(ElementFlag.Wind, _LATE_ELEMENT_PROBABILITY);
            _SElementProbabilityList.AddItem(ElementFlag.Electricity, _LATE_ELEMENT_PROBABILITY);
        }
        else
        {
            Debug.Log("Probability list initialized for else.");
            foreach (ElementFlag element in Enum.GetValues(typeof(ElementFlag)))
            {
                if (element == ElementFlag.None) continue;
                _SElementProbabilityList.AddItem(element, _LATE_ELEMENT_PROBABILITY);
            }

            _SEnemyProbability.AddItem(EnemyType.GlassCannon, 30);
            _SEnemyProbability.AddItem(EnemyType.Tank, 30);
            _SEnemyProbability.AddItem(EnemyType.Grunt, 35);
            _SEnemyProbability.AddItem(EnemyType.Swarm, 25);
        }
    }

    // Method to get balance multipliers based on the wave number
    public static BalanceMultipliers GetBalanceMultipliers(int waveNumber)
    {
        // Apply regular scaling based on wave number
        var multipliers = new BalanceMultipliers
        {
            HealthMultiplier = Mathf.Pow(_HEALTH_SCALING_FACTOR, waveNumber),
            DamageMultiplier = Mathf.Pow(_DAMAGE_SCALING_FACTOR, waveNumber),
            SpeedMultiplier = Mathf.Pow(_SPEED_SCALING_FACTOR, waveNumber),
            AttackRangeMultiplier = Mathf.Pow(_ATTACK_RANGE_SCALING_FACTOR, waveNumber),
            AttackCooldownMultiplier = Mathf.Pow(_COOLDOWN_SCALING_FACTOR, waveNumber) // Cooldown decreases over time
        };

        // Apply additional scaling if boss waves have been completed
        if (BossesDefeated > 0)
        {
            multipliers.HealthMultiplier *=
                Mathf.Pow(_POST_BOSS_HEALTH_BOOST, BossesDefeated); // Boost health after bosses
            multipliers.DamageMultiplier *=
                Mathf.Pow(_POST_BOSS_DAMAGE_BOOST, BossesDefeated); // Boost damage after bosses
        }

        return multipliers;
    }

    public static void OnBossDefeated()
    {
        BossesDefeated++;
        Debug.Log($"Boss defeated! Total bosses defeated: {BossesDefeated}");
    }

    // Central method to compute the spawn radius dynamically
    public static float GetCurrentSpawnRadius(int currentWave)
    {
        var wavesUntilBoss = bossWaveInterval - (currentWave % bossWaveInterval);
        var progressToBoss = wavesUntilBoss / (float) bossWaveInterval;

        // Calculate and return the current spawn radius using the progression toward the boss
        return Mathf.Lerp(minSpawnRadius, maxSpawnRadius, progressToBoss);
    }

    public static void KillEnemy(StatusEffectiveness statusEffectiveness, Enemy deadEnemy)
    {
        EnemyPool.HandleEnemyDeactivation(deadEnemy);
        ScoreManager.AddScore(statusEffectiveness);
        WaveController.waveEnemies--;
    }

    public static Wave GenerateBossWave(int tier, Transform centerPoint)
    {
        var numberOfEnemies = _BOSS_MINIONS + 1;
        var selectedEnemyTypes = new EnemyType[numberOfEnemies];
        var elementFlags = new ElementFlag[numberOfEnemies];

        // Boss enemy is the first in the array
        selectedEnemyTypes[0] = EnemyType.Tank;
        elementFlags[0] = _SElementProbabilityList.PickValue();

        for (var i = 1; i < numberOfEnemies; i++)
        {
            selectedEnemyTypes[i] = EnemyType.Swarm;
            elementFlags[i] = elementFlags[0];
        }

        var spawnPoints =
            SpawnPointGenerator.GenerateSpawnPoints(numberOfEnemies, 5f, centerPoint,
                selectedEnemyTypes); // 5f is a smaller radius for the boss wave
        var spawnInterval = 0.75f;

        return new Wave(numberOfEnemies, selectedEnemyTypes, spawnInterval, spawnPoints, tier, elementFlags);
    }


    public static Wave GenerateWave(int waveNumber, float spawnRadius, Transform centralPoint)
    {
        var numberOfEnemies = Mathf.CeilToInt(waveNumber * 2.5f);
        var selectedEnemyTypes = new EnemyType[numberOfEnemies];
        var elementFlags = new ElementFlag[numberOfEnemies];
        
        for (var i = 0; i < numberOfEnemies; i++)
        {
            selectedEnemyTypes[i] = _SEnemyProbability.PickValue();
            elementFlags[i] = _SElementProbabilityList.PickValue(); // Get elements based on probability list
        }

        var spawnPoints =
            SpawnPointGenerator.GenerateSpawnPoints(numberOfEnemies, spawnRadius, centralPoint, selectedEnemyTypes);
        var spawnInterval = Mathf.Max(1.5f - waveNumber * 0.01f, 0.3f);

        return new Wave(numberOfEnemies, selectedEnemyTypes, spawnInterval, spawnPoints, waveNumber, elementFlags);
    }

    public static EnemySpawner spawner { get; set; }
}
public struct BalanceMultipliers
{
    public float HealthMultiplier;
    public float DamageMultiplier;
    public float SpeedMultiplier;
    public float AttackRangeMultiplier;
    public float AttackCooldownMultiplier;
}