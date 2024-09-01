using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Enemies;
using TMPro;
using UI.DamageNumbers;
using UnityEngine;

[Serializable]
public class DamageNumberIndex
{
    public ElementFlag elementFlag;
    public TextMeshPro text;
}

public class DamageNumber : MonoBehaviour
{
    private Transform _mainCamera;
    private TextMeshPro text;
    public void SetScoreText(int score, int multiplier)
    {
        ScoreTextTween.TweenScoreText(text, score, multiplier);
    }

    public void SetElementText(ElementFlag elementFlag, StatusEffectiveness statusEffectiveness, int number)
    {
        ElementTextTween.TweenElementText(text, elementFlag, statusEffectiveness, number);
    }

    private void Awake()
    {
        _mainCamera = Camera.main.transform;
        text = GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        var forward = _mainCamera.position - transform.position;
        if (forward != Vector3.zero) transform.rotation = Quaternion.LookRotation(-forward, Vector3.up);
    }
}