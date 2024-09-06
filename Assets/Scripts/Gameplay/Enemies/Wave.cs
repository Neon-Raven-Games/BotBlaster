using Gameplay.Enemies;
using UnityEngine;


[System.Serializable]
public class Wave
{
    public int waveNumber;
    public int numberOfEnemies;
    public EnemyType[] enemyTypes;
    public float spawnInterval;
    public Vector3[] spawnPositions;

    public ElementFlag[] elementFlags;
    public Wave(int numberOfEnemies, EnemyType[] enemyTypes, float spawnInterval, Vector3[] spawnPositions,
        int waveNumber, ElementFlag[] elementFlags)
    {
        this.numberOfEnemies = numberOfEnemies;
        this.enemyTypes = enemyTypes;
        this.spawnInterval = spawnInterval;
        this.spawnPositions = spawnPositions;
        this.waveNumber = waveNumber;
        this.elementFlags = elementFlags;
    }
}

