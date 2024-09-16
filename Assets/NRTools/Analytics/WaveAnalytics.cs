using System;
using System.Collections.Generic;
using Gameplay.Enemies;

namespace NRTools.Analytics
{
    // todo, we need to find a good spot for this, and make it easy to populate

    // after we start the wave, we can populate the old props
    // then we need to get the enemy balance objects and player balance objects.
    // we can maybe take the spawn method and add enemies to it if they aren't already added
    public class WaveAnalytics
    {
        public int WaveNumber { get; set; }
        public float SpawnRadius { get; set; }
        public int NumberOfEnemies { get; set; }
        public ElementFlag[] Elements { get; set; }
        public double PlayTimeSeconds { get; set; }
        public BalanceObject PlayerBalance { get; set; }
        public readonly List<EnemyBalanceObject> EnemyBalanceData = new();

        public DateTime WaveStartTime { get; private set; }

        public WaveAnalytics(int waveNumber, float spawnRadius, int numberOfEnemies, ElementFlag[] elements,
            BalanceObject playerBalance)
        {
            WaveNumber = waveNumber;
            SpawnRadius = spawnRadius;
            NumberOfEnemies = numberOfEnemies;
            Elements = elements;
            PlayerBalance = playerBalance;
            WaveStartTime = DateTime.Now;
        }

        public void UpdatePlayTime()
        {
            PlayTimeSeconds = (DateTime.Now - WaveStartTime).TotalSeconds;
        }

        public string ToCsvString()
        {
            // Wave Data
            string waveData = "Self Wave Data\n" +
                              "WaveNumber,SpawnRadius,NumberOfEnemies,Elements,PlayTimeSeconds,WaveStartTime\n" +
                              $"{WaveNumber},{SpawnRadius},{NumberOfEnemies},{string.Join(";", Elements)},{PlayTimeSeconds},{WaveStartTime}\n";

            // Player Balance Data
            string playerBalanceData = "Player Balance Data\n" +
                                       "WaveNumber,CurrentDamage,BaseDamage,CurrentHealth,BaseHealth,CurrentAttackRange,BaseAttackRange\n" +
                                       $"{WaveNumber},{PlayerBalance.currentDamage},{PlayerBalance.baseDamage},{PlayerBalance.currentHealth},{PlayerBalance.baseHealth},{PlayerBalance.currentAttackRange},{PlayerBalance.baseAttackRange}\n";

            // Enemy Balance Data by EnemyType
            var enemyDataByType = new Dictionary<EnemyType, List<string>>();
            foreach (var enemyBalance in EnemyBalanceData)
            {
                if (!enemyDataByType.ContainsKey(enemyBalance.enemyType))
                {
                    enemyDataByType[enemyBalance.enemyType] = new List<string>
                    {
                        $"EnemyType,WaveNumber,CurrentDamage,BaseDamage,CurrentHealth,BaseHealth,CurrentAttackRange,CurrentAttackCooldown,Count,Elements"
                    };
                }

                // Append enemy data for this wave
                enemyDataByType[enemyBalance.enemyType].Add(
                    $"{enemyBalance.enemyType},{WaveNumber},{enemyBalance.currentDamage},{enemyBalance.baseDamage},{enemyBalance.currentHealth},{enemyBalance.baseHealth},{enemyBalance.currentAttackRange},{enemyBalance.currentAttackCoolDown},{enemyBalance.count},{string.Join(";", enemyBalance.Elements)}"
                );
            }

            // Combine enemy data into CSV string
            string enemyBalanceData = "";
            foreach (var enemyType in enemyDataByType.Keys)
            {
                enemyBalanceData += $"\n{enemyType} Wave Balance Data\n" + string.Join("\n", enemyDataByType[enemyType]) + "\n";
            }

            // Combine everything into a single CSV string
            return waveData + "\n" + playerBalanceData + "\n" + enemyBalanceData;
        }
    }
}