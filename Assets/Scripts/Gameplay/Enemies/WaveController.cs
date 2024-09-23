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

    private static int _totalKillsInWave;
    private static float _waveStartTime;
    private static float _totalKillRate = 1;
    private static int _wavesCompleted;

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
        if (enemySpawner._waveAnalytics != null) GameAnalytics.LogWaveData(enemySpawner._waveAnalytics);
        GameAnalytics.FinalizeAnalytics();
#endif
        waveEnemies = 0;
        EnemyPool.SleepAll();
    }

    private async UniTaskVoid WaveRoutine()
    {
        _totalKillsInWave = 0;
        _waveStartTime = Time.time;
        enemySpawner.StartNextWave();
        _waveSpawning = true;

        while (_waveSpawning)
        {
            if (!paused && EnemySpawner.WaveCompleted())
            {
                Debug.Log("Wave completed!");
                waveEnemies = 0;

                await UniTask.Delay(750);

                _totalKillsInWave = 0;
                _waveStartTime = Time.time;
                EndWave();
                enemySpawner.StartNextWave();
            }

            await UniTask.Yield();
        }

        TimerManager.ClearTimers();
        EnemyPool.SleepAll();
        waveEnemies = 0;
    }

    public static void EndWave()
    {
        var waveDuration = Time.time - _waveStartTime;
        var killsPerSecond = _totalKillsInWave / waveDuration;
        var killsPerMinute = killsPerSecond * 60f;

        _totalKillRate = ((_totalKillRate * _wavesCompleted) + killsPerMinute) / (_wavesCompleted + 1);
        _wavesCompleted++;
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
        var waveDuration = Time.time - _waveStartTime;
        waveDuration = Mathf.Max(waveDuration, 1f);
        return _totalKillsInWave / waveDuration * 60f;
    }


    public static float AverageKillRate()
    {
        return _totalKillRate;
    }

    public static bool IsWaveSpawning() => _instance._waveSpawning;

    public void Ready()
    {
        XrPerformanceSettingsFeature.SetPerformanceLevelHint(PerformanceDomain.Cpu, PerformanceLevelHint.SustainedHigh);
        XrPerformanceSettingsFeature.SetPerformanceLevelHint(PerformanceDomain.Gpu, PerformanceLevelHint.SustainedHigh);
        ui.SetActive(true);
    }
}