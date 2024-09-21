using Gameplay.Enemies.EnemyBehaviors.Base;
using Gameplay.Enemies.EnemyBehaviors.Tank;
using UnityEngine;

namespace Gameplay.Enemies.EnemyTypes
{
    public class Tank : Enemy
    {
        [SerializeField] private float attackDuration;
        [SerializeField] private Transform barrelTransform;
        private bool _attacking;
        private bool _dashing;
        private TankSnake _tankSnake;
        private BaseEnemyBehavior _currentBehavior;
        protected override void Awake()
        {
            base.Awake();
            _tankSnake = new TankSnake(this, barrelTransform);
            _currentBehavior = _tankSnake;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _currentBehavior?.OnEnable();
        }

        protected override void Attack()
        {

        }

        private void OnDisable()
        {
            _dashing = false;
            _attacking = false;
        }
        
        protected override void Move()
        {
            _currentBehavior.Move();
        }
    }
}