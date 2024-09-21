using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gameplay.Enemies;
using UnityEngine;

namespace NRTools.Analytics
{
    public class GameAnalytics
    {
        private static readonly StringBuilder _SCsvBuilder = new();
        private static DateTime _startTime;
        private static string _filePath;
        private static readonly List<WaveAnalytics> _allWaveAnalytics = new(); 

        public static void InitializeAnalytics()
        {
#if UNITY_EDITOR
            _startTime = DateTime.Now;
            _filePath = Path.Combine(Application.persistentDataPath,
                $"GameAnalytics_{_startTime:yyyy-MM-dd_HH-mm-ss}.csv");
#endif
    }

    public static void LogWaveData(WaveAnalytics waveAnalytics)
    {
#if UNITY_EDITOR
            waveAnalytics.UpdatePlayTime();
            _allWaveAnalytics.Add(waveAnalytics);
            Debug.Log($"Logged Wave {waveAnalytics.WaveNumber}");
#endif
    }

    public static void FinalizeAnalytics()
    {
#if UNITY_EDITOR
            _SCsvBuilder.AppendLine(GenerateCsvData());
            // File.WriteAllText(_filePath, _SCsvBuilder.ToString());
            Debug.Log($"Game analytics saved to {_filePath}");
            ResetAnalytics();
#endif
    }

    public static void ResetAnalytics()
    {
#if UNITY_EDITOR
        _SCsvBuilder.Clear();
        _allWaveAnalytics.Clear();
        #endif
    }

    // Generate the full CSV data, properly formatted
    private static string GenerateCsvData()
    {
#if UNITY_EDITOR
        StringBuilder csvData = new StringBuilder();

        // 1. Wave Data Section
        csvData.AppendLine("Wave Data");
        csvData.AppendLine("WaveNumber,SpawnRadius,NumberOfEnemies,PlayTimeSeconds,WaveStartTime");

        foreach (var wave in _allWaveAnalytics)
        {
            csvData.AppendLine($"{wave.WaveNumber},{wave.SpawnRadius},{wave.NumberOfEnemies}," +
                               $"{wave.PlayTimeSeconds},{wave.WaveStartTime}");
        }

        // 2. Player Balance Data Section
        csvData.AppendLine("\nPlayer Balance Data");
        csvData.AppendLine(
            "WaveNumber,CurrentDamage,BaseDamage,CurrentHealth,BaseHealth,CurrentAttackRange,BaseAttackRange,PlayerPerformance");

        foreach (var wave in _allWaveAnalytics)
        {
            csvData.AppendLine(
                $"{wave.WaveNumber},{wave.PlayerBalance.currentDamage},{wave.PlayerBalance.baseDamage}," +
                $"{wave.PlayerBalance.currentHealth},{wave.PlayerBalance.baseHealth}," +
                $"{wave.PlayerBalance.currentAttackRange},{wave.PlayerBalance.baseAttackRange}, {wave.playerPerformance}");
        }

        // 3. Enemy Balance Data by Type Section
        var enemyDataByType = new Dictionary<EnemyType, List<string>>();

        foreach (var wave in _allWaveAnalytics)
        {
            foreach (var enemyBalance in wave.enemyBalanceData)
            {
                if (!enemyDataByType.ContainsKey(enemyBalance.enemyType))
                {
                    enemyDataByType[enemyBalance.enemyType] = new List<string>
                    {
                        $"EnemyType,WaveNumber,CurrentDamage,BaseDamage,CurrentHealth,BaseHealth,CurrentAttackRange,CurrentAttackCooldown,Count,Elements"
                    };
                }

                enemyDataByType[enemyBalance.enemyType].Add(
                    $"{enemyBalance.enemyType},{wave.WaveNumber},{enemyBalance.currentDamage},{enemyBalance.baseDamage}," +
                    $"{enemyBalance.currentHealth},{enemyBalance.baseHealth},{enemyBalance.currentAttackRange}," +
                    $"{enemyBalance.currentAttackCoolDown},{enemyBalance.count},{string.Join(";", enemyBalance.Elements)}"
                );
            }
        }

        // Append enemy data to CSV
        foreach (var enemyType in enemyDataByType.Keys)
        {
            csvData.AppendLine($"\n{enemyType} Wave Balance Data");
            csvData.AppendLine(string.Join("\n", enemyDataByType[enemyType]));
        }

        return csvData.ToString();
        #endif
        return "";
    }
}

}