using UnityEngine;
using DG.Tweening;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private GameObject foreGround;
    public float maxValue;
    public float currentValue;

    private void Start()
    {
        FillMax();
    }

    public void FillMax()
    {
        currentValue = maxValue;
        foreGround.transform.DOScaleX(1, 0.5f).SetEase(Ease.OutBounce); 
    }

    public void FillEmpty()
    {
        currentValue = 0;
        foreGround.transform.DOScaleX(0, 0.5f).SetEase(Ease.InOutQuad);
    }

    public void ReduceValue(float value)
    {
        currentValue -= value;
        var newScale = Mathf.Clamp(currentValue / maxValue, 0, 1);
        foreGround.transform.DOScaleX(newScale, 0.5f).SetEase(Ease.OutBounce);
    }

    public void IncreaseValue(float value)
    {
        currentValue += value;
        var newScale = Mathf.Clamp(currentValue / maxValue, 0, 1);
        foreGround.transform.DOScaleX(newScale, 0.5f).SetEase(Ease.OutCubic);
    }

    public void SetValue(float value)
    {
        currentValue = Mathf.Clamp(value, 0, maxValue);
        var newScale = currentValue / maxValue;
        foreGround.transform.DOScaleX(newScale, 0.5f).SetEase(Ease.InOutQuad);
    }

    public void SetMaxValue(float max)
    {
        maxValue = max;
    }
}
