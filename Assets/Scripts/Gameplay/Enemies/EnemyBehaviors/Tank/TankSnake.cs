using System.Collections;
using Gameplay.Enemies.EnemyBehaviors.Base;
using UnityEngine;
using Util;

namespace Gameplay.Enemies.EnemyBehaviors.Tank
{
    public class TankSnake : BaseEnemyBehavior
    {
        private readonly Transform _barrelTransform;
        private float _currentAttackTime;
        private bool _attacking;
        private float _zigzagPhase;
        
        // we can balance these if needed
        private readonly float attackDuration = 1f;
        private readonly Vector2 randomAttackRange = new(4, 12);

        public TankSnake(Enemy enemy, Transform barrel) : base(enemy)
        {
            _barrelTransform = barrel;
        }

        public override void Attack()
        {
            _attacking = true;
            enemy.StartCoroutine(AttackRoutine());
        }

        private Quaternion _targetRotation;
        public override void Move()
        {
            if (_attacking) return;
            _currentAttackTime -= Time.deltaTime;
            
            if (_currentAttackTime <= 0)
            {
                _currentAttackTime = Random.Range(randomAttackRange.x, randomAttackRange.y);
                _targetRotation = Quaternion.LookRotation(player.position - enemy.transform.position);
                Attack();
                return;
            }

            var playerPosition = player.position;
            var enemyPosition = enemy.transform.position;

            var zigzagSpeed = 0.5f; 
            var zigzagAmplitude = 10f;
            _zigzagPhase += Time.deltaTime * zigzagSpeed; // Keep updating the phase
            var zigzagX = Mathf.Sin(_zigzagPhase) * zigzagAmplitude;

            var targetPosition = new Vector3(
                playerPosition.x + zigzagX, enemyPosition.y,
                Mathf.Min(enemyPosition.z + enemy.currentSpeed * Time.deltaTime / 4,
                    playerPosition.z - 10f) 
            );

            var direction = targetPosition - enemyPosition;
            if (direction != Vector3.zero)
            {
                var targetRotation = Quaternion.LookRotation(direction);
                enemy.transform.rotation =
                    Quaternion.Slerp(enemy.transform.rotation, targetRotation, Time.deltaTime * 3f); // Smooth rotation
            }

            enemy.transform.position = targetPosition;
        }


        private IEnumerator AttackRoutine()
        {
            var projectile = ElementPool.GetElement(enemy.element, _barrelTransform.position);
            var proj = projectile.GetComponent<Projectile>();
            proj.damage = enemy.currentDamage;
            proj.effectiveDamage = enemy.currentDamage;
            projectile.transform.LookAt(player);

            var t = 0f;

            while (t < attackDuration)
            {
                enemy.RotateToFlatPlayer(5f);
                projectile.transform.position = _barrelTransform.position;
                t += Time.deltaTime;
                yield return null;
            }

            projectile.gameObject.SetActive(true);
            _currentAttackTime = Random.Range(randomAttackRange.x, randomAttackRange.y);
            _attacking = false;
            
            
            while (t < 1f)
            {
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, _targetRotation, Time.deltaTime * 3f);
                t += Time.deltaTime;
                yield return null;
            }
        }

        public override void OnEnable()
        {
        }

        public override void OnDisable()
        {
        }
    }
}