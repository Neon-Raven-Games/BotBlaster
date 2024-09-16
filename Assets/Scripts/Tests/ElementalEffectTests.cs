using UnityEngine;
using UI;
using Gameplay.Enemies;
using NRTools;

namespace Tests
{
    public class ElementalEffectTests
    {
        private DevController _player;

        private readonly ActorData _actorData = new()
        {
            baseDamage = 15,
            baseHealth = 100,
            baseSpeed = 5,
            baseAttackRange = 5,
            baseAttackCooldown = 0.5f
        };

        [Setup]
        public void Setup()
        {
            _player = Object.FindObjectOfType<DevController>();
            _player.Initialize(_actorData);
        }

        [Teardown]
        public void Teardown()
        {
        }

        #region strong effectivness damage

        [Test]
        public AssertionResult AssertFireMoreDamageToElectricity()
        {
            _player.element = ElementFlag.Electricity;
            var pHealth = _player.currentHealth;
            _player.ApplyDamage(10, ElementFlag.Fire, Vector3.zero);
            var newHealth = _player.currentHealth;
            var diff = pHealth - newHealth;
            return NeonAssert.GreaterThan(diff, 10,
                $"Assert electricity takes normal damage from fire. 10 damage => {diff}");
        }

        [Test]
        public AssertionResult AssertWaterMoreDamageToFire()
        {
            _player.element = ElementFlag.Fire;
            var pHealth = _player.currentHealth;
            _player.ApplyDamage(10, ElementFlag.Water, Vector3.zero);
            var newHealth = _player.currentHealth;
            var diff = pHealth - newHealth;
            return NeonAssert.GreaterThan(diff, 10,
                $"Assert water deals more damage to fire. 10 damage => {diff}");
        }

        [Test]
        public AssertionResult AssertWaterMoreDamageToRock()
        {
            _player.element = ElementFlag.Rock;
            var pHealth = _player.currentHealth;
            _player.ApplyDamage(10, ElementFlag.Water, Vector3.zero);
            var newHealth = _player.currentHealth;
            var diff = pHealth - newHealth;
            return NeonAssert.GreaterThan(diff, 10,
                $"Assert water deals more damage to rock. 10 damage => {diff}");
        }

        [Test]
        public AssertionResult AssertElectricityMoreDamageToWater()
        {
            _player.element = ElementFlag.Water;
            var pHealth = _player.currentHealth;
            _player.ApplyDamage(10, ElementFlag.Electricity, Vector3.zero);
            var newHealth = _player.currentHealth;
            var diff = pHealth - newHealth;
            return NeonAssert.GreaterThan(diff, 10,
                $"Assert electricity deals more damage to water. 10 damage => {diff}");
        }

        [Test]
        public AssertionResult AssetWindMoreDamageToFire()
        {
            _player.element = ElementFlag.Fire;
            var pHealth = _player.currentHealth;
            _player.ApplyDamage(10, ElementFlag.Wind, Vector3.zero);
            var newHealth = _player.currentHealth;
            var diff = pHealth - newHealth;
            return NeonAssert.GreaterThan(diff, 10,
                $"Assert fire takes more damage from wind. 10 damage => {diff}");
        }

        #endregion

        #region neutral effectiveness
        
        [Test]
        public AssertionResult AssertWindNormalAgainstElectricity()
        {
            _player.element = ElementFlag.Wind;
            var pHealth = _player.currentHealth;
            _player.ApplyDamage(10, ElementFlag.Electricity, Vector3.zero);
            var newHealth = _player.currentHealth;
            var diff = pHealth - newHealth;
            return NeonAssert.AreEqual(diff, 10,
                $"Assert electricity deals normal damage to wind. 10 damage => {diff}");
        }
        
        [Test]
        public AssertionResult AssertWindNormalAgainstFire()
        {
            _player.element = ElementFlag.Wind;
            var pHealth = _player.currentHealth;
            _player.ApplyDamage(10, ElementFlag.Fire, Vector3.zero);
            var newHealth = _player.currentHealth;
            var diff = pHealth - newHealth;
            return NeonAssert.AreEqual(diff, 10,
                $"Assert wind takes normal damage from fire. 10 damage => {diff}");
        }
        
        [Test]
        public AssertionResult AssertWindNormalAgainstWater()
        {
            _player.element = ElementFlag.Wind;
            var pHealth = _player.currentHealth;
            _player.ApplyDamage(10, ElementFlag.Water, Vector3.zero);
            var newHealth = _player.currentHealth;
            var diff = pHealth - newHealth;
            return NeonAssert.AreEqual(diff, 10,
                $"Assert wind takes normal damage from water. 10 damage => {diff}");
        }

        [Test]
        public AssertionResult AssertElectricityNormalAgainstRock()
        {
            _player.element = ElementFlag.Electricity;
            var pHealth = _player.currentHealth;
            _player.ApplyDamage(10, ElementFlag.Rock, Vector3.zero);
            var newHealth = _player.currentHealth;
            var diff = pHealth - newHealth;
            return NeonAssert.AreEqual(diff, 10,
                $"Assert Electricity takes normal damage from rock. 10 damage => {diff}");
            
        }

        [Test]
        public AssertionResult AssertFireNormalAgainstElectricity()
        {
            _player.element = ElementFlag.Fire;
            var pHealth = _player.currentHealth;
            _player.ApplyDamage(10, ElementFlag.Electricity, Vector3.zero);
            var newHealth = _player.currentHealth;
            var diff = pHealth - newHealth;
            return NeonAssert.AreEqual(diff, 10,
                $"Assert fire takes normal damage from electricity. 10 damage => {diff}");
        }
        
        #endregion

        #region weak effectiveness

        [Test]
        public AssertionResult AssertRockLessDamageFromWind()
        {
            _player.element = ElementFlag.Rock;
            var pHealth = _player.currentHealth;
            _player.ApplyDamage(10, ElementFlag.Wind, Vector3.zero);
            var newHealth = _player.currentHealth;
            var diff = pHealth - newHealth;
            return NeonAssert.LessThan(diff, 10,
                $"Assert rock takes less damage from wind. 10 damage => {diff}");
        }
        
        [Test]
        public AssertionResult AssertWaterLessDamageFromRock()
        {
            _player.element = ElementFlag.Water;
            var pHealth = _player.currentHealth;
            _player.ApplyDamage(10, ElementFlag.Rock, Vector3.zero);
            var newHealth = _player.currentHealth;
            var diff = pHealth - newHealth;
            return NeonAssert.LessThan(diff, 10,
                $"Assert water takes less damage from rock. 10 damage => {diff}");
        }

        [Test]
        public AssertionResult AssertRockLessDamageFromFire()
        {
            _player.element = ElementFlag.Rock;
            var pHealth = _player.currentHealth;
            _player.ApplyDamage(10, ElementFlag.Fire, Vector3.zero);
            var newHealth = _player.currentHealth;
            var diff = pHealth - newHealth;
            return NeonAssert.LessThan(diff, 10,
                $"Assert rock takes less damage from fire. 10 damage => {diff}");
        }

        [Test]
        public AssertionResult AssertWaterLessDamageFromFire()
        {
            _player.element = ElementFlag.Water;
            var pHealth = _player.currentHealth;
            _player.ApplyDamage(10, ElementFlag.Fire, Vector3.zero);
            var newHealth = _player.currentHealth;
            var diff = pHealth - newHealth;
            return
                NeonAssert.LessThan(diff, 10,
                    $"Assert water takes less damage from fire. 10 damage => {diff}");
        }


        [Test]
        public AssertionResult AssertElectricityLessDamageFromWater()
        {
            _player.element = ElementFlag.Electricity;
            var pHealth = _player.currentHealth;
            _player.ApplyDamage(10, ElementFlag.Water, Vector3.zero);
            var newHealth = _player.currentHealth;
            var diff = pHealth - newHealth;
            return NeonAssert.LessThan(diff, 10,
                $"Assert electricity takes less damage from water. 10 damage => {diff}");
        }

        #endregion
    }
}