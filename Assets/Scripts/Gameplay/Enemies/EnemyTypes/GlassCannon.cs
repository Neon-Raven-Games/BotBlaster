using Gameplay.Enemies.EnemyBehaviors;
using Gameplay.Enemies.EnemyBehaviors.Base;
using NRTools.GpuSkinning;
using UnityEngine;

namespace Gameplay.Enemies.EnemyTypes
{
    public class GlassCannon : Enemy
    {
        [SerializeField] private Transform barrelTransform;
        
        // todo, this needs to be an explosion pvfx
        [SerializeField] private GameObject explodePrefab;
        
        private GameObject _explodeObject;

        [Header("Strafe Settings")] [SerializeField]
        private float strafeSpeed;
        [SerializeField] private float strafeDistance;
        
        private GlassCannonAnimator _anim;
        private BaseEnemyBehavior _currentBehavior;
        
        private KamakzeGlassCannon _kamakazeBehavior;
        private SlowAdvanceGlassCannon _slowAdvance;

        protected override void Awake()
        {
            base.Awake();
            _explodeObject = Instantiate(explodePrefab);
            _explodeObject.SetActive(false);

            _anim = meshAnimator as GlassCannonAnimator;

            _kamakazeBehavior = new KamakzeGlassCannon(this);

            _slowAdvance = new SlowAdvanceGlassCannon(this, meshAnimator as GlassCannonAnimator);
            _slowAdvance.barrelTransform = barrelTransform;
            _slowAdvance.strafeSpeed = strafeSpeed;
            _slowAdvance.strafeDistance = strafeDistance;

            _currentBehavior = _slowAdvance;
            // todo, pipeline to pick these!
            // _currentBehavior = kamakazeBehavior;
        }

        public override void FinishIntro()
        {
            base.FinishIntro();
            _anim.PlayIdle();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_slowAdvance != null)
            {
                _slowAdvance.strafeSpeed = strafeSpeed;
                _slowAdvance.strafeDistance = strafeDistance;
            }

            _currentBehavior?.OnEnable();
            if (_anim) _anim.PlayIdle();
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