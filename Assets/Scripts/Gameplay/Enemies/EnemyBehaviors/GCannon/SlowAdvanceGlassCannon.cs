using System.Collections;
using System.Collections.Generic;
using Gameplay.Enemies.EnemyBehaviors.Base;
using NRTools.GpuSkinning;
using UnityEngine;
using Util;

namespace Gameplay.Enemies.EnemyBehaviors
{
    
    public class SlowAdvanceGlassCannon : BaseEnemyBehavior
    {
        private readonly List<float> _heightLevels = new() {1.4f, 6f, 10.9f};
        public Transform barrelTransform;
        public float strafeSpeed;
        public float strafeDistance;

        private float _strafeAngle;
        private readonly GlassCannonAnimator _animator;
        private float _targetY;

        // todo, base on speed when animation transitions finished
        private const float _DASH_ANIMATION_PLAY_TIME = 1.2f;
        private float _dashAnimationPlayTimeCounter;
        private float _randomAttackTimer;
        private readonly Vector2 _randomAttackTimeRange = new(2f, 5f);
        private bool _attacking;
        private float _originalStrafeSpeed;
        private float _originalForwardSpeed;
        private bool _isSlowedDown;
        private float _forwardSpeed = 60f;
        private bool _left;
        private bool _dashing;

        public SlowAdvanceGlassCannon(Enemy enemy, GlassCannonAnimator animator) : base(enemy)
        {
            this._animator = animator;
        }

        public override void Attack()
        {
            if (_attacking || !enemy.gameObject.activeInHierarchy) return;
            _attacking = true;
            _animator.PlayIdle();
            enemy.StartCoroutine(ChargeAttack());
        }

        public override void Move()
        {
            if (_attacking)
            {
                if (!_isSlowedDown)
                {
                    _originalStrafeSpeed = strafeSpeed;
                    _originalForwardSpeed = _forwardSpeed;

                    strafeSpeed *= 0.3f;
                    _forwardSpeed *= 0.3f;
                    _isSlowedDown = true;
                }
            }
            else
            {
                if (_isSlowedDown)
                {
                    strafeSpeed = _originalStrafeSpeed;
                    _forwardSpeed = _originalForwardSpeed;
                    _forwardSpeed = Mathf.Lerp(_forwardSpeed, _originalForwardSpeed, Time.deltaTime);
                    if (_forwardSpeed >= _originalForwardSpeed)
                    {
                        _forwardSpeed = _originalForwardSpeed;
                        _isSlowedDown = false;
                    }
                }
            }

            var leftBound = -12f;
            var rightBound = 50f;
            var padding = Random.Range(0, 5f);

            _strafeAngle += strafeSpeed * Time.deltaTime;
            var offset = new Vector3(Mathf.Sin(_strafeAngle), 0, 0) * strafeDistance * 20;
            var strafePosition = enemy.transform.position + offset;
            var clampedX = Mathf.Clamp(strafePosition.x, leftBound + padding, rightBound - padding);

            var distanceToEdge = Mathf.Min(Mathf.Abs(strafePosition.x - leftBound),
                Mathf.Abs(strafePosition.x - rightBound));
            var normalizedEdgeDistance = Mathf.InverseLerp(.5f, 0, distanceToEdge);
            var dynamicStrafeSpeed = Mathf.Lerp(0.4f * strafeSpeed, strafeSpeed, normalizedEdgeDistance);

            var forwardMovement = enemy.transform.forward * _forwardSpeed * Time.deltaTime;
            var newPosition = new Vector3(clampedX, enemy.transform.position.y, enemy.transform.position.z) +
                              forwardMovement;
            
            if (Mathf.Abs(enemy.transform.position.y - _targetY) > 0.1f)
            {
                Debug.Log(_targetY);
                newPosition.y = Mathf.Lerp(enemy.transform.position.y, _targetY, Time.deltaTime * 60f);
                Debug.Log(newPosition.y);
            }

            _randomAttackTimer -= Time.deltaTime;
            if (_randomAttackTimer <= 0)
            {
                _randomAttackTimer = Random.Range(_randomAttackTimeRange.x, _randomAttackTimeRange.y);
                Attack();
                return;
            }

            if (newPosition.x < enemy.transform.position.x && !_left)
            {
                _left = true;
                _animator.PlayDashAnimation(_left);
                if (!_dashing)
                {
                    _dashing = true;
                    _dashAnimationPlayTimeCounter = _DASH_ANIMATION_PLAY_TIME;
                }
            }
            else if (newPosition.x > enemy.transform.position.x && _left)
            {
                _left = false;
                _animator.PlayDashAnimation(_left);
                if (!_dashing)
                {
                    _dashing = true;
                    _dashAnimationPlayTimeCounter = _DASH_ANIMATION_PLAY_TIME;
                }
            }

            if (_dashing)
            {
                _dashAnimationPlayTimeCounter -= Time.deltaTime;
                if (_dashAnimationPlayTimeCounter <= 0)
                {
                    _dashing = false;
                    _animator.PlayIdle();
                    _dashAnimationPlayTimeCounter = 0;
                }
            }

            enemy.transform.position =
                Vector3.Lerp(enemy.transform.position, newPosition, Time.deltaTime * dynamicStrafeSpeed);

            enemy.RotateToPlayer(50);

            UpdateSoundIntensity();
        }

        private void UpdateSoundIntensity()
        {
            var distanceToPlayer = Vector3.Distance(enemy.transform.position, player.position);
            var normalizedDistance = Mathf.InverseLerp(0f, 20f, distanceToPlayer);
            var soundIntensity = Mathf.Lerp(1f, 0f, normalizedDistance);

            // enemySound.SetIntensity(soundIntensity);
            // low intensity:
            // main sound low freq/pitch
            // bass drones growling
            
            // high intensity:
            // main sound high freq/pitch
            // bass drones fade out
        }

        private IEnumerator ChargeAttack()
        {
            _attacking = true;
            enemy.RotateToPlayer(50);
            var projectile = ElementPool.GetElement(enemy.element, barrelTransform.position);
            var proj = projectile.GetComponent<Projectile>();
            proj.damage = enemy.currentDamage;
            proj.effectiveDamage = enemy.currentDamage;
            projectile.transform.LookAt(player);

            // Charge for the specified time
            var t = 0f;
            while (t < 1f)
            {
                enemy.RotateToPlayer(50);
                t += Time.deltaTime;

                projectile.transform.position = barrelTransform.position;
                projectile.transform.LookAt(player);

                yield return null;
            }

            // Activate the projectile if the enemy is still alive and active
            if (enemy.currentHealth > 0 && enemy.gameObject.activeInHierarchy)
                projectile.gameObject.SetActive(true);

            // Set a random cooldown before the next attack
            _randomAttackTimer = Random.Range(_randomAttackTimeRange.x, _randomAttackTimeRange.y); // Random delay
            _attacking = false; // End attack

            // Reset speeds
            strafeSpeed = _originalStrafeSpeed;
            _forwardSpeed = _originalForwardSpeed;
        }


        public override void OnEnable()
        {
            _randomAttackTimer = Random.Range(_randomAttackTimeRange.x, _randomAttackTimeRange.y);
            _targetY = _heightLevels[Random.Range(0, _heightLevels.Count)];
            _strafeAngle = 0f;
        }

        public override void OnDisable()
        {
        }
    }
}