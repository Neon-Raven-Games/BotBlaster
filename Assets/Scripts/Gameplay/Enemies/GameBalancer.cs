using System;
using Gameplay.Enemies;
using RNGNeeds;
using UnityEngine;
using Random = UnityEngine.Random;

public static class GameBalancer
{
    // player performance, adaptive difficulty scale
    private const float _HEALTH_PERFORMANCE_WEIGHT = 0.4f;
    private const float _KILL_RATE_PERFORMANCE_WEIGHT = 0.3f;
    private const float _DAMAGE_TAKEN_PERFORMANCE_WEIGHT = 0.3f;

    // spawning
    private static float _spawnRateModifier = 2.5f;
    private static float _difficultyRamp = 1.05f;
    private static float _randomFactorRange = 0.9f;
    private const float baseSpawnRateModifier = 2.5f;

    // enemy spawn probabilities
    internal static readonly ProbabilityList<ElementFlag> _SElementProbabilityList = new();
    internal static readonly ProbabilityList<EnemyType> _SEnemyProbability = new();
    public static float playerPerformance = 1;

    private const float _POST_BOSS_HEALTH_BOOST = 1.01f;
    private const float _POST_BOSS_DAMAGE_BOOST = 1.01f;
    public static int BossesDefeated { get; private set; } = 0;

    private const float _HEALTH_SCALING_FACTOR = 1.05f;
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
        lock (_SEnemyProbability)
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
                _SElementProbabilityList.AddItem(ElementFlag.Rock, _EARLY_ROCK_PROBABILITY);
                _SElementProbabilityList.AddItem(ElementFlag.Wind, _EARLY_WIND_PROBABILITY);
                _SElementProbabilityList.AddItem(ElementFlag.Electricity, _EARLY_ELECTRICITY_PROBABILITY);
            }
            else if (waveNumber < _MID_WAVE_LIMIT)
            {
                _SEnemyProbability.AddItem(EnemyType.Grunt, 45);
                _SEnemyProbability.AddItem(EnemyType.GlassCannon, 35);
                _SEnemyProbability.AddItem(EnemyType.Tank, 15);
                _SEnemyProbability.AddItem(EnemyType.Swarm, 10);
                _SElementProbabilityList.AddItem(ElementFlag.Water, _MID_WATER_PROBABILITY);
                _SElementProbabilityList.AddItem(ElementFlag.Fire, _MID_FIRE_PROBABILITY);
                _SElementProbabilityList.AddItem(ElementFlag.Rock, _MID_EARTH_PROBABILITY);
                _SElementProbabilityList.AddItem(ElementFlag.Wind, _MID_WIND_PROBABILITY);
                _SElementProbabilityList.AddItem(ElementFlag.Electricity, _MID_ELECTRICITY_PROBABILITY);
            }
            else if (waveNumber < _LATE_WAVE_LIMIT)
            {
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
                foreach (ElementFlag element in Enum.GetValues(typeof(ElementFlag)))
                {
                    if (element == ElementFlag.None) continue;
                    _SElementProbabilityList.AddItem(element, _LATE_ELEMENT_PROBABILITY);
                }

                _SEnemyProbability.AddItem(EnemyType.GlassCannon, 35);
                _SEnemyProbability.AddItem(EnemyType.Tank, 20);
                _SEnemyProbability.AddItem(EnemyType.Grunt, 45);
                _SEnemyProbability.AddItem(EnemyType.Swarm, 20);
            }
        }
    }

    public static BalanceMultipliers GetBalanceMultipliers(int waveNumber)
    {
        var multipliers = new BalanceMultipliers
        {
            HealthMultiplier = Mathf.Pow(_HEALTH_SCALING_FACTOR, waveNumber),
            DamageMultiplier = Mathf.Pow(_DAMAGE_SCALING_FACTOR, waveNumber),
            SpeedMultiplier = Mathf.Pow(_SPEED_SCALING_FACTOR, waveNumber),
            AttackRangeMultiplier = Mathf.Pow(_ATTACK_RANGE_SCALING_FACTOR, waveNumber),
            AttackCooldownMultiplier = Mathf.Pow(_COOLDOWN_SCALING_FACTOR, waveNumber)
        };

        if (BossesDefeated > 0)
        {
            multipliers.HealthMultiplier *=
                Mathf.Pow(_POST_BOSS_HEALTH_BOOST, BossesDefeated);
            multipliers.DamageMultiplier *=
                Mathf.Pow(_POST_BOSS_DAMAGE_BOOST, BossesDefeated);
        }

        return multipliers;
    }

    public static void OnBossDefeated()
    {
        BossesDefeated++;
        Debug.Log($"Boss defeated! Total bosses defeated: {BossesDefeated}");
    }

    public static void KillEnemy(StatusEffectiveness statusEffectiveness, Enemy deadEnemy)
    {
        EnemyPool.HandleEnemyDeactivation(deadEnemy);
        ScoreManager.AddScore(statusEffectiveness);
        WaveController.waveEnemies--;
    }

    public static Wave GenerateBossWave(int tier)
    {
        int numberOfEnemies = _BOSS_MINIONS + 1; // 1 boss + minions
        float spawnInterval = 0.75f;

        return new Wave(numberOfEnemies, spawnInterval, tier);
    }

    public static void UpdateSpawnModifier(int waveNumber)
    {
        // todo, balance getting too hard?
        // Exponential scaling based on wave number for more aggressive difficulty increase
        // maybe we can modify this curve if gets too hard
        _spawnRateModifier = Mathf.Pow(_difficultyRamp, waveNumber) * baseSpawnRateModifier;

        var randomFactor = Random.Range(_randomFactorRange, 1.1f);
        _spawnRateModifier *= randomFactor;
        if (playerPerformance > 1.0f)
        {
            _spawnRateModifier *= playerPerformance;
        }
        else
        {
            _spawnRateModifier *= Mathf.Lerp(0.5f, 1.1f, playerPerformance);
        }

        _spawnRateModifier = Mathf.Clamp(_spawnRateModifier, 1.0f, 5.0f);
    }

    public static void CalculatePlayerPerformance(DevController player)
    {
        // Avoid division by zero for healthFactor
        var cappedHealth = Mathf.Max(player.cappedHealth, 1f); // Ensure cappedHealth is not zero
        var healthFactor = Mathf.Clamp(player.currentHealth / cappedHealth, 0.5f, 1.0f);

        // Kill rate and average kill rate handling
        var killRate = WaveController.GetKillRate();
        var averageKillRate = Mathf.Max(WaveController.AverageKillRate(), 1f); // Avoid division by zero
        var normalizedKillRate = Mathf.Clamp(killRate / averageKillRate, 0.5f, 1.5f);

        // Recent damage taken and capped health handling
        var recentDamageTaken = player.GetRecentDamageTaken();
        var normalizedDamage = Mathf.Clamp(1.0f - (recentDamageTaken / cappedHealth), 0.5f, 1.0f);

        // Calculate final performance score using weighted factors
        var performance = (healthFactor * _HEALTH_PERFORMANCE_WEIGHT) +
                          (normalizedKillRate * _KILL_RATE_PERFORMANCE_WEIGHT) +
                          (normalizedDamage * _DAMAGE_TAKEN_PERFORMANCE_WEIGHT);

        // Log for debugging
        Debug.Log("|Player Performance: " + performance);

        // A value between 0.5 (struggling) and 1.5 (excelling)
        playerPerformance = performance;
    }


    public static Wave GenerateWave(int waveNumber)
    {
        var numberOfEnemies = Mathf.CeilToInt(waveNumber * _spawnRateModifier);
        var spawnInterval = Mathf.Max(1.5f - waveNumber * 0.01f, 0.3f);

        UpdateSpawnModifier(waveNumber);
        Debug.Log("SpawnRate Modifier after wave generation: " + _spawnRateModifier);
        return new Wave(numberOfEnemies, spawnInterval, waveNumber);
    }

    public static void UpdateElementProbabilities(ElementFlag playerElement)
    {
        lock (_SEnemyProbability)
        {
            _SElementProbabilityList.ClearList();
            var weakElement = WeaknessesFor(playerElement);
            _SElementProbabilityList.AddItem(weakElement, 60);

            foreach (ElementFlag element in Enum.GetValues(typeof(ElementFlag)))
            {
                if (element == weakElement || element == ElementFlag.None || !IsSingleFlag(element)) continue;
                _SElementProbabilityList.AddItem(element, 10);
            }
        }
    }

    private static bool IsSingleFlag(ElementFlag element)
    {
        return element != 0 && (element & (element - 1)) == 0;
    }

    public static ElementFlag WeaknessesFor(ElementFlag targetElement)
    {
        switch (targetElement)
        {
            case ElementFlag.Fire: return ElementFlag.Electricity;
            case ElementFlag.Water: return ElementFlag.Fire;
            case ElementFlag.Rock: return ElementFlag.Fire;
            case ElementFlag.Wind: return ElementFlag.Fire;
            case ElementFlag.Electricity: return ElementFlag.Water;
            default: return ElementFlag.None;
        }
    }
}

public struct BalanceMultipliers
{
    public float HealthMultiplier;
    public float DamageMultiplier;
    public float SpeedMultiplier;
    public float AttackRangeMultiplier;
    public float AttackCooldownMultiplier;
}