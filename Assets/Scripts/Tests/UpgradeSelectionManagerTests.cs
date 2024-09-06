using UnityEngine;
using UI;
using Gameplay.Enemies;
using NRTools;
using UnityEngine.Assertions;

public class UpgradeSelectionManagerTests
{
    private UpgradeSelectionManager _upgradeSelectionManager;
    private DevController _player;

    [Setup]
    public void Setup()
    {
        _player = Object.FindObjectOfType<DevController>();
        _upgradeSelectionManager = Object.FindObjectOfType<UpgradeSelectionManager>();
        _upgradeSelectionManager.gameObject.SetActive(true);
    }

    [Teardown]
    public void Teardown()
    {
        _upgradeSelectionManager.gameObject.SetActive(true);
    }

    [Test]
    public AssertionResult UpgradeSelected_Damage_Upgrade_Increments_ElementEffectiveness()
    {
        var initialFireEffectiveness = _player.elementEffectivenessUpgrades[ElementFlag.Fire];
        _upgradeSelectionManager.UpgradeSelected(UpgradeType.Damage, ElementFlag.Fire);
        var updatedFireEffectiveness = _player.elementEffectivenessUpgrades[ElementFlag.Fire];
        
        return NeonAssert.AreEqual(initialFireEffectiveness + 1, updatedFireEffectiveness, 
            "Assert player damage upgrade.");
    }

    [Test]
    public AssertionResult UpgradeSelected_StatusEffect_Upgrade_Increments_ElementStatus()
    {
        var initialWaterStatus = _player.elementStatusUpgrades[ElementFlag.Water];
        _upgradeSelectionManager.UpgradeSelected(UpgradeType.StatusEffect, ElementFlag.Water);
        
        var updatedWaterStatus = _player.elementStatusUpgrades[ElementFlag.Water];
        return NeonAssert.AreEqual(initialWaterStatus + DevController.elementStatusIncrement, updatedWaterStatus, 
            "Assert player water status upgrade.");
    }
    
    [Test]
    public AssertionResult UpgradeSelected_HealthUpgrade_Increases_PlayerBaseHealth()
    {
        var initialBaseHealth = _player.baseHealth;
        _upgradeSelectionManager.UpgradeSelected(UtilityUpgrade.Health);
        
        return NeonAssert.LessThan(_player.baseHealth, initialBaseHealth, "Assert player base health upgrade");
    }
    
    [Test]
    public AssertionResult UpgradeSelected_DamageUpgrade_Increases_PlayerBaseDamage()
    {
        var initialBaseDamage = _player.baseDamage;
        _upgradeSelectionManager.UpgradeSelected(UtilityUpgrade.Damage);
        
        return NeonAssert.GreaterThan(_player.baseDamage, initialBaseDamage);
    }
    
    [Test]
    public AssertionResult OnEnable_SetsCorrectRotationAndPosition()
    {
        var message = NeonAssert.AreNotEqual(_player.transform.rotation, _upgradeSelectionManager.transform.rotation, "Assert rotation not set to players.");
        if (!message.Passed) return message;
        return NeonAssert.AreNotEqual(_player.transform.position, _upgradeSelectionManager.transform.position, "Assert position not set to players.");
    }
    
    // todo, should we support async and IEnumerators? could be helpful for testing coroutines or timing functions
    // main drawback is that it would segregate our runner, but would be feasible
    // [RoutineTest]
    // public IEnumerator UpgradeSelected_HealthRegen_StartsHealthUpRoutine_When_HealthIsLow()
    
    // [AsyncTest]
    // public async Task UpgradeSelected_HealthRegen_StartsHealthUpRoutine_When_HealthIsLow()
}