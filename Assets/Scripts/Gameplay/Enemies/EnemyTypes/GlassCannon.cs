using System.Collections;
using UnityEngine;
using Util;

namespace Gameplay.Enemies.EnemyTypes
{
    public class GlassCannon : Enemy
    {
        [SerializeField] private Transform barrelTransform;
        [SerializeField] private float chargeTime;
        [SerializeField] private Vector2 ySpawnHeightRange;
        [SerializeField] private float rotationSpeed;

        [Header("Strafe Settings")] [SerializeField]
        private float strafeSpeed;

        [SerializeField] private float strafeDistance;

        [Header("Swoop Settings")] [SerializeField]
        private float targetHeight;

        [SerializeField] private float swoopSpeed;
        [SerializeField] private float swoopBufferDistance = 2f;

        private bool _isCharging;
        private float _strafeAngle;

        protected override void OnEnable()
        {
            base.OnEnable();
            _isCharging = false;
            _strafeAngle = 0f;

            var pos = transform.position;
            pos.y = Random.Range(ySpawnHeightRange.x, ySpawnHeightRange.y);
            transform.position = pos;
        }

        protected override void Attack()
        {
            _isCharging = true;
            StartCoroutine(ChargeAttack());
        }

        private IEnumerator ChargeAttack()
        {
            var playerPosition = player.position;
            transform.LookAt(player);
            var projectile = ElementPool.GetElement(element, barrelTransform.position);
            projectile.GetComponent<Projectile>().damage = currentDamage;
            projectile.transform.LookAt(player);

            // play animation/particle.sound
            var t = 0f;
            while (t < chargeTime)
            {
                t += Time.deltaTime;
                transform.LookAt(playerPosition);
                projectile.transform.position = barrelTransform.position;
                projectile.transform.Translate(Vector3.forward * t);
                yield return null;
            }

            projectile.gameObject.SetActive(true);
            _isCharging = false;
            lastAttackTime = Time.time;
        }

        protected override void Move()
        {
            if (_isCharging) return;

            var playerDistance = Vector3.Distance(transform.position, player.position);

            if (playerDistance > currentAttackRange + swoopBufferDistance)
            {
                var targetPosition = player.position;
                targetPosition.y = Mathf.Lerp(transform.position.y, targetHeight, Time.deltaTime * swoopSpeed);

                transform.position =
                    Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);
            }
            else
            {
                _strafeAngle += strafeSpeed * Time.deltaTime;
                var offset = new Vector3(Mathf.Sin(_strafeAngle), 0, Mathf.Cos(_strafeAngle)) * strafeDistance;
                var strafePosition = player.position + offset;

                transform.position = Vector3.Lerp(transform.position, strafePosition, Time.deltaTime * currentSpeed);
            }

            RotateToPlayer(rotationSpeed);
        }
    }
}