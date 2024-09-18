using System;
using Cysharp.Threading.Tasks;
using Gameplay.Util;
using NRTools.Analytics;
using UI;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features.Extensions.PerformanceSettings;

public class WaveController : MonoBehaviour
{
    [SerializeField] public GameObject ui;
    [SerializeField] float menuSpawnDelay;

    private static int totalKillsInWave;
    private static float waveStartTime;
    private static float totalKillRate = 1;
    private static int wavesCompleted = 0;

    public EnemySpawner enemySpawner;
    private bool _waveSpawning;
    private static WaveController _instance;
    public static bool paused;
    public UpgradeSelectionManager upgradeSelectionManager;
    public static int waveEnemies;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _instance = this;
    }

    public void StartWaves()
    {
#if UNITY_EDITOR
        GameAnalytics.InitializeAnalytics();
#endif
        XrPerformanceSettingsFeature.SetPerformanceLevelHint(PerformanceDomain.Cpu, PerformanceLevelHint.Boost);
        enemySpawner.currentWave = 1;
        paused = false;
        enemySpawner.paused = false;
        waveEnemies = 0;
        WaveRoutine().Forget();
        XrPerformanceSettingsFeature.SetPerformanceLevelHint(PerformanceDomain.Cpu, PerformanceLevelHint.SustainedHigh);
    }

    private void StopWaves()
    {
        paused = true;
        _waveSpawning = false;
        enemySpawner.currentWave = 1;
#if UNITY_EDITOR
        GameAnalytics.LogWaveData(enemySpawner._waveAnalytics);
        GameAnalytics.FinalizeAnalytics();
#endif
        waveEnemies = 0;
        EnemyPool.SleepAll();
    }

    private async UniTaskVoid WaveRoutine()
    {
        totalKillsInWave = 0;
        waveStartTime = Time.time;
        enemySpawner.StartNextWave();
        _waveSpawning = true;

        while (_waveSpawning)
        {
            if (!paused && enemySpawner.WaveCompleted())
            {
                waveEnemies = 0;
                await PauseForPlayerUpgrades();

        totalKillsInWave = 0;
        waveStartTime = Time.time;
                EndWave();
                enemySpawner.StartNextWave();
            }

            await UniTask.Yield();
        }

        TimerManager.ClearTimers();
        EnemyPool.SleepAll();
        waveEnemies = 0;
        enemySpawner.paused = true;
    }

    public static void EndWave()
    {
        float waveDuration = Time.time - waveStartTime; // Duration of the wave in seconds
        float killsPerSecond = totalKillsInWave / waveDuration; // Calculate kills per second
        float killsPerMinute = killsPerSecond * 60f; // Convert to kills per minute

        // Update the average kill rate across all waves
        totalKillRate = ((totalKillRate * wavesCompleted) + killsPerMinute) / (wavesCompleted + 1);
        wavesCompleted++;

    }

    private async UniTask PauseForPlayerUpgrades()
    {
        await UniTask.WaitForSeconds(menuSpawnDelay);
        upgradeSelectionManager.gameObject.SetActive(true);
        await upgradeSelectionManager.ShowUpgradeSelection();
    }

    public static void EndGame()
    {
        _instance.StopWaves();
        ScoreManager.FinalizeScore();
    }

    public static float GetKillRate()
    {
        float waveDuration = Time.time - waveStartTime;
        waveDuration = Mathf.Max(waveDuration, 1f); 
        return totalKillsInWave / waveDuration * 60f;
    }


    public static float AverageKillRate()
    {
        return totalKillRate; // Return the average kill rate across all waves
    }

    public static bool IsWaveSpawning() => _instance._waveSpawning;

    public void Ready()
    {
        XrPerformanceSettingsFeature.SetPerformanceLevelHint(PerformanceDomain.Cpu, PerformanceLevelHint.SustainedHigh);
        XrPerformanceSettingsFeature.SetPerformanceLevelHint(PerformanceDomain.Gpu, PerformanceLevelHint.SustainedHigh);
        ui.SetActive(true);
    }
}