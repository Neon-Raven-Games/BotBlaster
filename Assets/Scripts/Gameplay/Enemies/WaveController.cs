using Cysharp.Threading.Tasks;
using Gameplay.Util;
using UI;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    [SerializeField] public GameObject ui;
    [SerializeField] float menuSpawnDelay;

    public EnemySpawner enemySpawner;
    private bool _waveSpawning;
    private static WaveController _instance;
    public static bool paused;
    public UpgradeSelectionManager upgradeSelectionManager;
    public static int waveEnemies;

    private void Awake()
    {
        Application.targetFrameRate = -1;
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
        GameAnalyticsHelper.InitializeAnalytics();
#endif
        enemySpawner.currentWave = 1;
        paused = false;
        enemySpawner.paused = false;
        waveEnemies = 0;
        WaveRoutine().Forget();
    }

    public void StopWaves()
    {
        paused = true;
        _waveSpawning = false;
        enemySpawner.currentWave = 1;
#if UNITY_EDITOR
        GameAnalyticsHelper.FinalizeAnalytics();
#endif
        waveEnemies = 0;
        EnemyPool.SleepAll();
    }

    private async UniTaskVoid WaveRoutine()
    {
        enemySpawner.StartNextWave();
        _waveSpawning = true;

        while (_waveSpawning)
        {
            if (!paused && enemySpawner.WaveCompleted())
            {
#if UNITY_EDITOR
                GameAnalyticsHelper.LogWaveData(enemySpawner.currentWave,
                    GameBalancer.GetCurrentSpawnRadius(enemySpawner.currentWave),
                    enemySpawner.currentWaveData.numberOfEnemies, enemySpawner.currentWaveData.elementFlags);
#endif
                await UniTask.Yield();
                await PauseForPlayerUpgrades();
                enemySpawner.StartNextWave();

                await UniTask.Yield();
            }

            await UniTask.Yield();
        }

        TimerManager.ClearTimers();
        EnemyPool.SleepAll();
        waveEnemies = 0;
        enemySpawner.paused = true;
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

    public void Ready()
    {
        ui.SetActive(true);
    }
}