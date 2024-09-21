using System;
using System.Collections.Generic;
using Gameplay.Enemies;

namespace NRTools.Analytics
{
    public class WaveAnalytics
    {
        public int WaveNumber { get; }
        public float SpawnRadius { get; }
        public int NumberOfEnemies { get; }
        public double PlayTimeSeconds { get; set; }
        public BalanceObject PlayerBalance { get; }
        public readonly List<EnemyBalanceObject> enemyBalanceData = new();
        public readonly float playerPerformance;

        public DateTime WaveStartTime { get; }

        public WaveAnalytics(int waveNumber, float spawnRadius, int numberOfEnemies, 
            BalanceObject playerBalance)
        {
            playerPerformance = GameBalancer.playerPerformance;
            WaveNumber = waveNumber;
            SpawnRadius = spawnRadius;
            NumberOfEnemies = numberOfEnemies;
            PlayerBalance = playerBalance;
            WaveStartTime = DateTime.Now;
        }

        public void UpdatePlayTime()
        {
            PlayTimeSeconds = (DateTime.Now - WaveStartTime).TotalSeconds;
        }
    }
}