using System;
using System.Collections.Generic;
using Gameplay.Elements;
using UnityEngine;

namespace Gameplay.Enemies.EnemyTypes
{


    public class SwarmUnit : MonoBehaviour
    {
        [SerializeField] private GameObject deathParticles;
        [SerializeField] private Animator animator;
        [SerializeField] private TextureAnimator textureAnimator;
        [SerializeField] private List<ElementMaterial> elementMaterials;
        private Dictionary<ElementFlag, ElementMaterial> _elementMaterials;
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
        private static readonly int _SHit = Animator.StringToHash("Hit");
        private static readonly int _SAttack = Animator.StringToHash("Attack");
        private static readonly int _SHitNum = Animator.StringToHash("HitNum");

        private void Awake()
        {
            deathParticles.transform.parent = null;
            _elementMaterials = new();
            foreach (var mat in elementMaterials)
                _elementMaterials.Add(mat.elementFlag, mat);
        }

        public void Initialize(Actor playerComponent, int currentDamage, int currentHealth, ElementFlag elementFlag)
        {
            deathParticles.SetActive(false);
            _playerComponent = playerComponent;
            _currentDamage = currentDamage;
            _currentHealth = currentHealth;
            element = elementFlag;
            if (element == ElementFlag.None) return;
            textureAnimator.SwitchElement(elementFlag, _elementMaterials[elementFlag].characterMaterial);
        }

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
            animator.SetTrigger(_SAttack);
            _isDiveBombing = true;
            _diveBombTarget = playerPosition;
        }

        private void SetHitAnimation()
        {
            var hitAnimation = UnityEngine.Random.Range(0, 3);
            animator.SetInteger(_SHitNum, hitAnimation);
            animator.SetTrigger(_SHit);
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
                    _swarmComponent.SwarmUnitDead();
                    gameObject.SetActive(false);
                }
                else SetHitAnimation();
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
            else SetHitAnimation();
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

            if (Vector3.Distance(transform.position, _diveBombTarget) < 0.5f)
                OnDiveBombImpact();
        }

        private void OnDiveBombImpact()
        {
            _playerComponent.ApplyDamage(_currentDamage, element, transform.position);
            gameObject.SetActive(false);
        }
    }
}