using System.Collections;
using UnityEngine;
using Util;

namespace Gameplay.Enemies.EnemyTypes
{
    public class Tank : Enemy
    {
        [SerializeField] private float dashingChance;
        [SerializeField] private float dashSpeed;
        [SerializeField] private float attackDuration;
        [SerializeField] private float chargeDuration;
        [SerializeField] private float dashDuration;
        [SerializeField] private float rotationSpeed = 16f;
        [SerializeField] private Transform barrelTransform;
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
            transform.LookAt(player);
            var projectile = ElementPool.GetElement(element, barrelTransform.position);
            var proj = projectile.GetComponent<Projectile>();
            proj.damage = currentDamage;
            proj.effectiveDamage = currentDamage;
            projectile.transform.LookAt(player);
            var attackPosition = player.position;
            var t = 0f;
            while (t < attackDuration)
            {
                RotateToFlatPlayer(rotationSpeed);
                projectile.transform.position = barrelTransform.position;
                projectile.transform.Translate(Vector3.forward * t);
                t += Time.deltaTime;
                yield return null;
            }
            projectile.gameObject.SetActive(true);
            lastAttackTime = Time.time;
            playerComponent.ApplyDamage(currentDamage, element, attackPosition);
            _attacking = false;
        }

        private IEnumerator DashRoutine()
        {
            var t = 0f;
            // charging animation
            while (t < chargeDuration)
            {
                RotateToFlatPlayer(rotationSpeed);
                t += Time.deltaTime;
                yield return null;
            }
            
            t = 0f;
            var dashAngle = Random.Range(-45f, 45f);
            var dashDirection = Quaternion.Euler(0, dashAngle, 0) * transform.forward;
            dashDirection.y = 0;
            
            // dashing animation
            while (t < dashDuration)
            {
                var targetRotation = Quaternion.LookRotation(dashDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                t += Time.deltaTime * dashSpeed;
                transform.position += dashDirection * Time.deltaTime * dashSpeed;
                yield return null;
            }

            _dashing = false;
        }

        private void OnDisable()
        {
            _dashing = false;
            _attacking = false;
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