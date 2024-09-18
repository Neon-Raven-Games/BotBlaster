using Gameplay.Enemies;
using UnityEngine;


[System.Serializable]
public class Wave
{
    public int waveNumber;
    public int numberOfEnemies;
    public float spawnInterval;

    public Wave(int numberEnemies, float spawnInterval, int waveNumber)
    {
        numberOfEnemies = numberEnemies;
        this.spawnInterval = spawnInterval;
        this.waveNumber = waveNumber;
    }
}

