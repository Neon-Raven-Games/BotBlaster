using System.Collections;
using Gameplay.Enemies.EnemyBehaviors;
using Gameplay.Enemies.EnemyBehaviors.Base;
using NRTools.GpuSkinning;
using UnityEngine;

namespace Gameplay.Enemies.EnemyTypes
{
    public class GlassCannon : Enemy
    {
        [SerializeField] private Transform barrelTransform;
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
        private BaseEnemyBehavior _currentBehavior;
        private GlassCannonAnimator anim;
        public KamakzeGlassCannon kamakazeBehavior;
        public SlowAdvanceGlassCannon slowAdvance;

        protected override void Awake()
        {
            base.Awake();
            anim = meshAnimator as GlassCannonAnimator;
            kamakazeBehavior = new KamakzeGlassCannon(this);
            
            slowAdvance = new SlowAdvanceGlassCannon(this, meshAnimator as GlassCannonAnimator);
            slowAdvance.barrelTransform = barrelTransform;
            
            _currentBehavior = slowAdvance;

            // todo, find common assignment and abstract to a base
            slowAdvance.strafeSpeed = strafeSpeed;
            slowAdvance.strafeDistance = strafeDistance;
        }

        public override void FinishIntro()
        {
            base.FinishIntro();
            anim.PlayIdle();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            slowAdvance.strafeSpeed = strafeSpeed;
            slowAdvance.strafeDistance = strafeDistance;
            _currentBehavior.OnEnable();
            if (anim) anim.PlayIdle();
        }

        protected override void Attack()
        {
        }

        protected override void Move()
        {
            _currentBehavior.Move();
            return;
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