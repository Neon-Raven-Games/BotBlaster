using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Enemies;
using UnityEngine;

public class DamageNumberPool : MonoBehaviour
{
    public int elementsToPool;
    public DamageNumber damageNumberPrefab;
    private static DamageNumberPool _instance;
    private List<DamageNumber> _damageNumbers;

    public void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        DOTween.SetTweensCapacity(400, 50);
        DOTween.Init();
        _instance = this;
        _damageNumbers = new List<DamageNumber>(elementsToPool);
        SpawnProjectiles().Forget();
    }
    private static int index = 0;

    public static DamageNumber GetDamageNumber(Vector3 position)
    {
        var damageNumber = _instance._damageNumbers.Find(x => !x.gameObject.activeInHierarchy);
        if (damageNumber == null)
        {
                index = 0;
                damageNumber = Instantiate(_instance.damageNumberPrefab, _instance.transform);
                damageNumber.gameObject.SetActive(false);
                _instance._damageNumbers.Add(damageNumber);
        }
        damageNumber.transform.position = position;
        return damageNumber;
    }
    
    public static void SetElementDamageNumber(ElementFlag elementFlag, Vector3 position, StatusEffectiveness statusEffectiveness, int number)
    {
        var damageNumber = GetDamageNumber(position);
        damageNumber.SetElementText(elementFlag, statusEffectiveness, number);
    }
    
    public static void SetScoreDamageNumber(Vector3 position, int score, int multiplier)
    {
        var damageNumber = GetDamageNumber(position);
        damageNumber.SetScoreText(score, multiplier);
    }

    private async UniTaskVoid SpawnProjectiles()
    {
        for (var i = 0; i < elementsToPool; i++)
        {
            var obj = Instantiate(damageNumberPrefab, transform);
            await UniTask.Yield();
            obj.gameObject.SetActive(false);
            _damageNumbers.Add(obj);
        }
    }
}