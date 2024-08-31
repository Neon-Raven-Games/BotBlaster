using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    public EnemySpawner enemySpawner;
    public float timeBetweenWaves = 10f;

    public void StartWaves()
    {
        WaveRoutine().Forget();
    }

    // async wave routine for background task processing
    // offload the wave generation async to avoid blocking the main thread
    private async UniTaskVoid WaveRoutine()
    {
        while (true)
        {
            enemySpawner.StartNextWave();
            await UniTask.WaitForSeconds(timeBetweenWaves);
        }
    }
}