using Gameplay.Enemies;
using UI;
using UnityEngine;
using UnityEngine.UI;

public enum UpgradeType
{
    StatusEffect,
    Damage,
    Utility
}

public enum UtilityUpgrade
{
    Health,
    HealthRegen,
    Damage
}

public class UpgradeHoverUI : MonoBehaviour
{
    [SerializeField] internal ElementFlag elementFlag;
    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] internal UtilityUpgrade utilityUpgrade;
    private Image _iconImage;
    private UpgradeSelectionManager _upgradeSelectionManager;


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            if (upgradeType == UpgradeType.StatusEffect || upgradeType == UpgradeType.Damage)
                _upgradeSelectionManager.UpgradeSelected(upgradeType, elementFlag);
            else _upgradeSelectionManager.UpgradeSelected(utilityUpgrade);
        }
    }

    private void Start()
    {
        _upgradeSelectionManager = GetComponentInParent<UpgradeSelectionManager>();
        _iconImage = GetComponent<Image>();
    }
}