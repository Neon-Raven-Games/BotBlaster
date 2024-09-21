using UnityEngine;

namespace Gameplay.Enemies.EnemyBehaviors.Base
{
    public abstract class BaseEnemyBehavior
    {
        public EnemyType enemyType => enemy.enemyType;
        public Transform player => enemy.player;
        protected Enemy enemy;
        
        public BaseEnemyBehavior(Enemy enemy)
        {
            this.enemy = enemy;
        }
        
        public abstract void Attack();
        public abstract void Move();
        public abstract void OnEnable();
        public abstract void OnDisable();
    }
}