using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    public EnemySpawner enemySpawner;
    public float timeBetweenWaves = 6f;
    private bool _waveSpawning;

    public void StartWaves()
    {
        _waveSpawning = true;
        WaveRoutine().Forget();
    }
    
    public void StopWaves()
    {
        _waveSpawning = false;
        // cleanup here
    }

    // async wave routine for background task processing
    // offload the wave generation async to avoid blocking the main thread
    private async UniTaskVoid WaveRoutine()
    {
        enemySpawner.StartNextWave();
        while (_waveSpawning)
        {
            if (enemySpawner.WaveCompleted())
            {
                enemySpawner.StartNextWave();
                await UniTask.WaitForSeconds(timeBetweenWaves);
            }
            await UniTask.Yield();
        }
    }
}