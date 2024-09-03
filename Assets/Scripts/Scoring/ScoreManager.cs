using System.Linq;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private const int _SCORE_PER_ENEMY = 100;
    private static ScoreManager _instance;
    private int _score;
    [SerializeField] private TextMeshPro textMeshPro;
    [SerializeField] private TextMeshPro highScoreText;
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
        if (statusEffectiveness == StatusEffectiveness.Strong) return 2f;
        return 1;
    }

    public static void FinalizeScore()
    {
        var highScores = PlayerPrefs.GetString("HighScores", "");
        if (highScores == "")
        {
            PlayerPrefs.SetString("HighScores", _instance._score.ToString() + ";");
            return;
        }
        
        var scores = highScores.Split(';');
        var newHighScores = "";
        var scoreValues = new int[scores.Length + 1];
        var added = false;
        foreach (var score in scores)
        {
            scoreValues[scores.ToList().IndexOf(score)] = int.Parse(score);
        }
        scoreValues[^1] = _instance._score;

        var sorted = scoreValues.ToList();
        sorted.Sort();
        sorted = sorted.GetRange(0, Mathf.Min(5, sorted.Count));
        var scoreStr = "";
        var i = 0;
        foreach (var score in sorted)
        {
            _instance.highScoreText.text += i + ": " + score + "\n";
            scoreStr += score + ";";
        }
        PlayerPrefs.SetString("HighScores", scoreStr);
    }
}
