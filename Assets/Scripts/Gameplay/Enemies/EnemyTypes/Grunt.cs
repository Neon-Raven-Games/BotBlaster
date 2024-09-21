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
        [SerializeField] private float dashingChance;
        [SerializeField] private float dashSpeed;
        [SerializeField] private float attackDuration;
        [SerializeField] private float dashDuration;
        [SerializeField] private float rotationSpeed = 16f;
        [SerializeField] private Transform barrelTransform;
        private bool _attacking;
        private bool _dashing;
        
        private BaseEnemyBehavior _currentBehavior;
        private GruntBehavior _gruntBehavior;
        
        protected override void Awake()
        {
            base.Awake();
            _gruntBehavior = new GruntBehavior(this, barrelTransform, meshAnimator);
            _currentBehavior = _gruntBehavior;
        }

        private void OnDisable()
        {
            _dashing = false;
            _attacking = false;
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