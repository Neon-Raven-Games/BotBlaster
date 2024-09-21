using System.Collections;
using Gameplay.Enemies.EnemyBehaviors.Base;
using NRTools.GpuSkinning;
using UnityEngine;
using Util;

namespace Gameplay.Enemies.EnemyBehaviors.Grunt
{
    public class GruntBehavior : BaseEnemyBehavior
    {
        private bool _isRetreating;
        private bool _inAttackRange;
        private readonly float _retreatThreshold = 0.3f; // Retreat when health is below 30%
        private readonly float _attackCooldown = 2f; // Time between shots
        private readonly float _attackRange = 15f;
        private Vector2 randomAttackRange = new(4, 12);
        private Quaternion _targetRotation;
        private Transform barrelTransform;
        private float attackDuration = 1f;
        private float currentAttackTime;
        private bool _attacking;

        private GruntBotAnimator _meshAnimator;
        public GruntBehavior(Enemy enemy, Transform barrel, GpuMeshAnimator meshAnimator) : base(enemy)
        {
            _meshAnimator = meshAnimator as GruntBotAnimator;
            barrelTransform = barrel;
        }

        public override void Attack()
        {
            _attacking = true;
            currentAttackTime = Random.Range(randomAttackRange.x, randomAttackRange.y);
            _targetRotation = Quaternion.LookRotation(player.position - enemy.transform.position);
            enemy.StartCoroutine(AttackRoutine());
        }

        public override void Move()
        {
            if (_attacking) return;
            var distanceToPlayer = Vector3.Distance(player.position, enemy.transform.position);
            if (_isRetreating || enemy.currentHealth <= enemy.baseHealth * _retreatThreshold)
            {
                Retreat();
                return;
            }

            if (distanceToPlayer <= _attackRange)
            {
                _meshAnimator.PlayIdle();
                currentAttackTime -= Time.deltaTime;
                if (currentAttackTime <= 0)
                {
                    StartAttack();
                }
            }
            else
            {
                Patrol();
            }
        }
        private float _zigzagPhase;
        private void Patrol()
        {
            var playerPosition = player.position;
            var enemyPosition = enemy.transform.position;

            var zigzagSpeed = 0.5f; 
            var zigzagAmplitude = 5f;
            _zigzagPhase += Time.deltaTime * zigzagSpeed; 
            var zigzagX = Mathf.Sin(_zigzagPhase) * zigzagAmplitude;

            var targetPosition = new Vector3(
                playerPosition.x + zigzagX, enemyPosition.y,
                Mathf.Min(enemyPosition.z + enemy.currentSpeed * Time.deltaTime / 2,
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

        private void StartAttack()
        {
            DrawWeapon();
            Attack();
        }

        private void DrawWeapon()
        {
            Debug.Log("Grunt is drawing weapon.");
        }

        private IEnumerator AttackRoutine()
        {
            var projectile = ElementPool.GetElement(enemy.element, barrelTransform.position);
            var proj = projectile.GetComponent<Projectile>();
            proj.damage = enemy.currentDamage;
            proj.effectiveDamage = enemy.currentDamage;
            projectile.transform.LookAt(player);

            var t = 0f;

            while (t < attackDuration)
            {
                enemy.RotateToFlatPlayer(5f);
                projectile.transform.position = barrelTransform.position;
                t += Time.deltaTime;
                yield return null;
            }

            _meshAnimator.PlayAttackAnimation();
            projectile.gameObject.SetActive(true);
            currentAttackTime = Random.Range(randomAttackRange.x, randomAttackRange.y);
            _attacking = false;


            while (t < 1f)
            {
                enemy.transform.rotation =
                    Quaternion.Slerp(enemy.transform.rotation, _targetRotation, Time.deltaTime * 3f);
                t += Time.deltaTime;
                yield return null;
            }
        }


        private void Retreat()
        {
            // Move the grunt away from the player when health is low
            Vector3 retreatDirection = (enemy.transform.position - player.position).normalized;
            enemy.transform.position += retreatDirection * enemy.currentSpeed * Time.deltaTime;
            _isRetreating = true;
            Debug.Log("Grunt is retreating!");
        }

        public override void OnEnable()
        {
            currentAttackTime = Random.Range(randomAttackRange.x, randomAttackRange.y);
            _attacking = false;
            _isRetreating = false;
        }

        public override void OnDisable()
        {
        }
    }
}