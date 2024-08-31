using System.Collections;
using System.Collections.Generic;
using Gameplay.Enemies;
using UnityEngine;


[System.Serializable]
public class Wave
{
    public int numberOfEnemies;
    public EnemyType[] enemyTypes;
    public float spawnInterval;
    public Vector3[] spawnPositions;

    public Wave(int numberOfEnemies, EnemyType[] enemyTypes, float spawnInterval, Vector3[] spawnPositions)
    {
        this.numberOfEnemies = numberOfEnemies;
        this.enemyTypes = enemyTypes;
        this.spawnInterval = spawnInterval;
        this.spawnPositions = spawnPositions;
    }
}

