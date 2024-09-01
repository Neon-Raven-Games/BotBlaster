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
    public List<DamageNumberIndex> damageNumbers;
    private readonly Dictionary<ElementFlag, TextMeshPro> _damageNumberDictionary = new();

    public void SetScoreText(int score, int multiplier)
    {
        if (_damageNumberDictionary.TryGetValue(ElementFlag.None, out var text))
            ScoreTextTween.TweenScoreText(text, score, multiplier);
    }
    public void SetElementText(ElementFlag elementFlag, StatusEffectiveness statusEffectiveness)
    {
        if (_damageNumberDictionary.TryGetValue(elementFlag, out var text))
            ElementTextTween.TweenElementText(text, elementFlag, statusEffectiveness);
    }
 
    private void Start()
    {
        foreach(var damageNumber in damageNumbers)
            _damageNumberDictionary.Add(damageNumber.elementFlag, damageNumber.text);
    }

    private void Awake()
    {
        _mainCamera = Camera.main.transform;
    }
    private void Update()
    {
        var forward = _mainCamera.position - transform.position;
        transform.rotation = Quaternion.LookRotation(-forward, Vector3.up);
    }

}
