using System;
using NRTools.AtlasHelper;
using NRTools.GpuSkinning;
using UnityEngine;

namespace Gameplay.Enemies.EnemyTypes
{


    public class SwarmUnit : MonoBehaviour
    {
        [SerializeField] private GameObject deathParticles;
        [SerializeField] private EnemyHealthBar healthBar;
        public float speed;
        public ElementFlag element;
        private Transform _swarmCenter;
        private bool _isDiveBombing;
        private Vector3 _diveBombTarget;
        private float _diveBombSpeedMultiplier = 2.5f;
        private Actor _playerComponent;
        private int _currentDamage;
        private int _currentHealth;
        private Swarm _swarmComponent;
        public Vector3 flockingDirection { get; set; }
        public bool isDiveBombing { get; set; }
        private MeshFilter _meshFilter;
        private GpuMeshAnimator _gpuMeshAnimator;

        private void Awake()
        {
            _gpuMeshAnimator = GetComponent<GpuMeshAnimator>();
            deathParticles.transform.parent = null;
        }

        public void Initialize(Actor playerComponent, int currentDamage, int currentHealth, ElementFlag elementFlag)
        {
            deathParticles.SetActive(false);
            _playerComponent = playerComponent;
            _currentDamage = currentDamage;
            _currentHealth = currentHealth;
            element = elementFlag;
            if (element == ElementFlag.None) return;
            _gpuMeshAnimator.UpdateElement(element);
        }
        
        private AtlasIndex _atlasIndex;

        private void Update()
        {
            if (_isDiveBombing) PerformDiveBomb();
        }

        public void SetSwarmCenter(Transform flockCenter)
        {
            _swarmCenter = flockCenter;
            _swarmComponent = _swarmCenter.GetComponent<Swarm>();
        }

        public void DiveBomb(Vector3 playerPosition)
        {
            _gpuMeshAnimator.PlayAttackAnimation();
            _isDiveBombing = true;
            _diveBombTarget = playerPosition;
        }

        private void OnEnable()
        {
            healthBar.FillMax();
        }

        private void SetHitAnimation()
        {
            _gpuMeshAnimator.PlayOneShotHitAnimation();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Projectile"))
            {
                var collisionElement = collision.gameObject.GetComponent<Projectile>();
                var flag = collisionElement.elementFlag;
                var damage = collisionElement.damage;
                var dmg = _swarmComponent.ApplyDamage(damage, flag, transform.position);
                _currentHealth -= dmg;
                if (_currentHealth <= 0)
                {
                    healthBar.FillEmpty();
                    _swarmComponent.SwarmUnitDead();
                    gameObject.SetActive(false);
                }
                else
                {
                    healthBar.ReduceValue(dmg);
                    SetHitAnimation();
                }
            }
        }

        public void ApplySwarmDamage(int damage, ElementFlag flag)
        {
            _swarmComponent.ApplyDamage(damage, flag, transform.position);
            if (_currentHealth <= 0)
            {
                _swarmComponent.SwarmUnitDead();
                gameObject.SetActive(false);
            }
            else if(!_isDiveBombing) SetHitAnimation();
        }

        private void PerformDiveBomb()
        {
            var direction = (_diveBombTarget - transform.position).normalized;
            transform.position += direction * (speed * _diveBombSpeedMultiplier) * Time.deltaTime;
            var playerDirection = _diveBombTarget - transform.position;
            if (playerDirection != Vector3.zero)
            {
                var targetRotation = Quaternion.LookRotation(playerDirection.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
            }

            if (Vector3.Distance(transform.position, _diveBombTarget) < 0.5f) OnDiveBombImpact();
        }

        private void OnDiveBombImpact()
        {
            _playerComponent.ApplyDamage(_currentDamage, element, transform.position);
            gameObject.SetActive(false);
        }
    }
}