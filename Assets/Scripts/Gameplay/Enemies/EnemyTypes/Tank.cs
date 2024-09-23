using Gameplay.Enemies.EnemyBehaviors.Base;
using Gameplay.Enemies.EnemyBehaviors.Tank;
using UnityEngine;

namespace Gameplay.Enemies.EnemyTypes
{
    public class Tank : Enemy
    {
        private bool _initialized;
        [SerializeField] private float attackDuration;
        [SerializeField] private Transform barrelTransform;
        private bool _attacking;
        private bool _dashing;
        private TankSnake _tankSnake;
        private BaseEnemyBehavior _currentBehavior;
        protected override void Awake()
        {
            base.Awake();
            // todo, we need to transition from one animation to the next now, we have been manually transitioning on glass cannon
            _tankSnake = new TankSnake(this, barrelTransform, meshAnimator);
            _currentBehavior = _tankSnake;
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            if (_initialized) _currentBehavior?.OnEnable();
            else _initialized = true;
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