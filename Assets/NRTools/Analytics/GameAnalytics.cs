using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace NRTools.Analytics
{
    public class GameAnalytics
    {
        private static readonly StringBuilder _SCsvBuilder = new();
        private static DateTime _startTime;
        private static string _filePath;

        public static void InitializeAnalytics()
        {
#if UNITY_EDITOR
            _startTime = DateTime.Now;
            _SCsvBuilder.Clear();

            // Add more headers for new fields (e.g., PlayerUpgrades, EnemyStats)
            _SCsvBuilder.AppendLine(
                "WaveNumber,SpawnRadius,NumberOfEnemies,Elements,PlayTimeSeconds,PlayerUpgrades,EnemyStats");

            _filePath = Path.Combine(Application.persistentDataPath,
                $"GameAnalytics_{_startTime:yyyy-MM-dd_HH-mm-ss}.csv");
#endif
        }

        public static void LogWaveData(WaveAnalytics waveAnalytics)
        {
#if UNITY_EDITOR
            // Log the current play time
            waveAnalytics.PlayTimeSeconds = (DateTime.Now - _startTime).TotalSeconds;

            // Add the data to the CSV builder
            _SCsvBuilder.AppendLine(waveAnalytics.ToCsvString());

            // Optional: Log to Unity's console for debugging
            // Debug.Log($"Logged Wave {waveAnalytics.WaveNumber}: {waveAnalytics.SpawnRadius} radius, " +
                      // $"{waveAnalytics.NumberOfEnemies} enemies, Player Balance: {waveAnalytics.PlayerBalance.currentDamage}/{waveAnalytics.PlayerBalance.currentHealth}, " +
                      // $"Time: {waveAnalytics.PlayTimeSeconds}");
#endif
        }

        public static void FinalizeAnalytics()
        {
#if UNITY_EDITOR
            File.WriteAllText(_filePath, _SCsvBuilder.ToString());
            Debug.Log($"Game analytics saved to {_filePath}");
#endif
        }

        public static void ResetAnalytics()
        {
            _SCsvBuilder.Clear();
        }
    }
}