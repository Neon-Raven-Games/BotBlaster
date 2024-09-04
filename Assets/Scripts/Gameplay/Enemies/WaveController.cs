using Cysharp.Threading.Tasks;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    [SerializeField] public GameObject ui;
    public EnemySpawner enemySpawner;
    public float timeBetweenWaves = 6f;
    private bool _waveSpawning;
    private static WaveController _instance;
    public static bool paused;
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
        enemySpawner.currentWave = 0;
        paused = false;
        _waveSpawning = true;
        WaveRoutine().Forget();
    }
    
    public void StopWaves()
    {
        paused = true;
        _waveSpawning = false;
        enemySpawner.currentWave = 0;
    }

    // todo, sometimes the wave count gets off, seems to be in the midst of wave spawning. 
    // waited the full 6 seconds and still no spawn
    private async UniTaskVoid WaveRoutine()
    {
        enemySpawner.StartNextWave();
        
        while (_waveSpawning)
        {
            if (!paused && enemySpawner.WaveCompleted())
            {
                enemySpawner.StartNextWave();
                await UniTask.WaitForSeconds(timeBetweenWaves);
            }
            await UniTask.Yield();
        }
        
        EnemyPool.SleepAll();
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