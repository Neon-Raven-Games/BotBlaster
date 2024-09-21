using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMeshPro;
    [SerializeField] private TextMeshPro leaderboard;
    
    private const int _SCORE_PER_ENEMY = 100;
    private static ScoreManager _instance;
    private int _score;
    
    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }
        _instance = this;
    }
    
    public static void AddScore(StatusEffectiveness statusEffectiveness)
    {
        _instance._score += (int)(GetMultiplier(statusEffectiveness) * _SCORE_PER_ENEMY);
        _instance.textMeshPro.text = _instance._score.ToString("D6");
    }
    
    private static float GetMultiplier(StatusEffectiveness statusEffectiveness)
    {
        if (statusEffectiveness == StatusEffectiveness.Normal) return 1.5f;
        if (statusEffectiveness == StatusEffectiveness.Weak) return 2f;
        return 1;
    }

    public static void FinalizeScore()
    {
        var highScores = PlayerPrefs.GetString("HighScores", "");

        var scoreValues = new List<int>();

        if (!string.IsNullOrEmpty(highScores))
        {
            var scores = highScores.Split(';');
            foreach (var score in scores)
            {
                if (int.TryParse(score, out int parsedScore))
                {
                    scoreValues.Add(parsedScore);
                }
            }
        }

        scoreValues.Add(_instance._score);
        scoreValues.Sort((a, b) => b.CompareTo(a));
        if (scoreValues.Count > 5) scoreValues = scoreValues.GetRange(0, 5);

        var scoreStr = string.Join(";", scoreValues) + ";";
        PlayerPrefs.SetString("HighScores", scoreStr);
        
        _instance.leaderboard.text = "";
        for (int i = 0; i < scoreValues.Count; i++) 
            _instance.leaderboard.text += $"{i + 1}: {scoreValues[i]}\n";
    }
}
