using System.Collections.Generic;
using Gameplay.Enemies.EnemyBehaviors.Base;
using Gameplay.Enemies.EnemyTypes;
using UnityEngine;

namespace Gameplay.Enemies.EnemyBehaviors
{
    public class KamakzeGlassCannon : BaseEnemyBehavior
    {
        private readonly float _initialSpeedBoost = 25f;
        private float _currentSpeed;
        private readonly float _speedDecreaseRate = 1f;
        private readonly GlassCannon _glassCannon;
        private readonly float _explosionRadius = 5f;
        private readonly float _healthReductionPercentage = 0.7f;
        public KamakzeGlassCannon(Enemy enemy) : base(enemy)
        {
            _glassCannon = enemy as GlassCannon;
        }

        public override void Attack()
        {
            
        }

        public override void Move()
        {
            _currentSpeed = Mathf.Lerp(_currentSpeed, enemy.currentSpeed, _speedDecreaseRate * Time.deltaTime);
            enemy.transform.position = 
                Vector3.MoveTowards(enemy.transform.position, enemy.player.position,
                    _currentSpeed * Time.deltaTime);
            
            enemy.RotateToPlayer(25);
            if (Vector3.Distance(enemy.transform.position, enemy.player.position) < 1f)
                ExplodeCannon();
        }

        private void ExplodeCannon()
        {
            _glassCannon.Explode();
        }

        public override void OnEnable()
        {
            _currentSpeed = enemy.currentSpeed + _initialSpeedBoost;
            enemy.currentHealth = (int) (_healthReductionPercentage * enemy.currentHealth);
        }

        public override void OnDisable()
        {
            
        }
    }
}