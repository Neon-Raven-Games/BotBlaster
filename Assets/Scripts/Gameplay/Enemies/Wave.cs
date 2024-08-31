using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EnemyType
{
    Grunt,
    Swarm,
}

[System.Serializable]
public class Wave
{
    public int numberOfEnemies;
    public EnemyType[] enemyTypes; // Array holding the types of enemies in this wave
    public float spawnInterval; // Time between spawns within the wave
    public Vector3[] spawnPositions; // Positions where enemies will spawn

    public Wave(int numberOfEnemies, EnemyType[] enemyTypes, float spawnInterval, Vector3[] spawnPositions)
    {
        this.numberOfEnemies = numberOfEnemies;
        this.enemyTypes = enemyTypes;
        this.spawnInterval = spawnInterval;
        this.spawnPositions = spawnPositions;
    }
}

