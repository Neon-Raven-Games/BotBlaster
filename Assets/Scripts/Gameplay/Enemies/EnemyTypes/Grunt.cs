using System;
using System.Collections;
using Gameplay.Enemies.EnemyBehaviors.Base;
using Gameplay.Enemies.EnemyBehaviors.Grunt;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Enemies.EnemyTypes
{
    public class Grunt : Enemy
    {
        [SerializeField] private float attackDuration;
        [SerializeField] private Transform barrelTransform;
        
        private BaseEnemyBehavior _currentBehavior;
        private GruntBehavior _gruntBehavior;
        
        protected override void Awake()
        {
            base.Awake();
            _gruntBehavior = new GruntBehavior(this, barrelTransform, meshAnimator);
            _currentBehavior = _gruntBehavior;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _currentBehavior?.OnEnable();
        }

        private void OnDisable()
        {
        }

        protected override void Attack()
        {
        }

        protected override void Move()
        {
            _currentBehavior.Move();
        }
    }
}