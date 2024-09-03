using System.Collections;
using UnityEngine;

namespace Gameplay.Enemies.EnemyTypes
{
    public class Grunt : Enemy
    {
        [SerializeField] private float dashingChance;
        [SerializeField] private float dashSpeed;
        [SerializeField] private float attackDuration;
        [SerializeField] private float dashDuration;
        [SerializeField] private float rotationSpeed = 16f;
        private bool _attacking;
        private bool _dashing;

        protected override void Attack()
        {
            if (_attacking || _dashing) return;
            _attacking = true;
            StartCoroutine(AttackRoutine());
        }

        private IEnumerator AttackRoutine()
        {
            var attackPosition = player.position;
            var t = 0f;
            while (t < attackDuration)
            {
                RotateToFlatPlayer(rotationSpeed);
                t += Time.deltaTime;
                yield return null;
            }
            
            lastAttackTime = Time.time;
            playerComponent.ApplyDamage(currentDamage, element, attackPosition);
            _attacking = false;
        }

        private IEnumerator DashRoutine()
        {
            var dashAngle = Random.Range(0, 2);
            if (dashAngle == 0) dashAngle = -90;
            else dashAngle = 90;
            
            var dashDirection = Quaternion.Euler(0, dashAngle, 0) * transform.forward;
            dashDirection.y = 0;
            
            var t = 0f;
            // dashing animation
            while (t < dashDuration)
            {
                RotateToFlatPlayer(rotationSpeed);
                t += Time.deltaTime * dashSpeed;
                transform.position += dashDirection * Time.deltaTime * dashSpeed;
                yield return null;
            }

            _dashing = false;
        }

        protected override void Move()
        {
            if (_attacking || _dashing) return;

            var playerDirection = player.position - transform.position;
            if (playerDirection.magnitude > currentAttackRange)
            {
                if (Random.value < dashingChance)
                {
                    _dashing = true;
                    StartCoroutine(DashRoutine());
                }
                else transform.position += transform.forward * currentSpeed * Time.deltaTime;
            }

            RotateToFlatPlayer(rotationSpeed);
        }
    }
}