using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
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
    }
    
    private static float GetMultiplier(StatusEffectiveness statusEffectiveness)
    {
        if (statusEffectiveness == StatusEffectiveness.Normal) return 1.5f;
        if (statusEffectiveness == StatusEffectiveness.Strong) return 2f;
        return 1;
    }
    
    private void Start()
    {
        
    }

    private void Update()
    {
        
    }
}
