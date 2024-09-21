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

        [SerializeField] private GameObject explodePrefab;
        private GameObject _explodeObject;

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
            _explodeObject = Instantiate(explodePrefab);
            _explodeObject.SetActive(false);
            
            anim = meshAnimator as GlassCannonAnimator;
            
            kamakazeBehavior = new KamakzeGlassCannon(this);

            slowAdvance = new SlowAdvanceGlassCannon(this, meshAnimator as GlassCannonAnimator);
            slowAdvance.barrelTransform = barrelTransform;
            slowAdvance.strafeSpeed = strafeSpeed;
            slowAdvance.strafeDistance = strafeDistance;
            
            _currentBehavior = slowAdvance;
            // _currentBehavior = kamakazeBehavior;
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
        }

        public void Explode()
        {
            _explodeObject.transform.position = transform.position;
            _explodeObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}