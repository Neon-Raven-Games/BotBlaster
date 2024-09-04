using System.Collections;
using System.Collections.Generic;
using Gameplay.Enemies;
using Gameplay.Enemies.EnemyTypes;
using UnityEngine;

namespace Util
{
    public class StreamProjectile : MonoBehaviour
    {
        public int damage;
        [SerializeField] private ElementFlag element;
        [SerializeField] private List<ParticleSystem> zAxisParticles; // Particle systems along the Z-axis
        [SerializeField] private GameObject projectileImpact; // Impact effect prefab
        [SerializeField] private float endDistance = 10f; // Maximum distance the laser can extend
        [SerializeField] private float jetSpeed = 5f; // Speed at which the laser extends
        [SerializeField] private float fireRate = 0.25f;
        private float _currentHitTime;
        private Coroutine _laserCoroutine;
        private float _currentDistance;
        private readonly RaycastHit[] _hitInfo = new RaycastHit[1];

        private void OnEnable()
        {
            _currentDistance = 0f; // Start at zero distance
            _laserCoroutine = StartCoroutine(FireLaserStream());
        }

        private void OnDisable()
        {
            if (_laserCoroutine != null)
            {
                StopCoroutine(_laserCoroutine);
            }
        }

        private IEnumerator FireLaserStream()
        {
            while (true)
            {
                var startPosition = transform.position;
                var direction = transform.forward;

                // can we only register collisions that are not on the player layer?
                var playerLayer = LayerMask.NameToLayer("Player");
                var playerLayerMask = 1 << playerLayer;
                var layerMask = ~playerLayerMask;
                
                var hitDetected = Physics.RaycastNonAlloc(startPosition, direction, _hitInfo, endDistance, layerMask) > 0;
                var targetDistance = hitDetected ? _hitInfo[0].distance : endDistance;

                _currentDistance = Mathf.MoveTowards(_currentDistance, targetDistance, jetSpeed * Time.deltaTime);
                SetParticleSize(_currentDistance);

                if (hitDetected)
                {
                    HandleImpact(_hitInfo[0]);
                }
                else
                {
                    projectileImpact.SetActive(false);
                }

                yield return null; 
            }
        }

        private void SetParticleSize(float size)
        {
            foreach (var system in zAxisParticles)
            {
                var mainModule = system.main;
                mainModule.startSizeZ = size * .18f;
            }
        }
        

        private void HandleImpact(RaycastHit hitPoint)
        {
            if (projectileImpact != null)
            {
                projectileImpact.transform.position = hitPoint.point;
                projectileImpact.SetActive(true);
                if (hitPoint.collider.CompareTag("Enemy") && Time.time > _currentHitTime + fireRate)
                {
                    var swarmUnit = hitPoint.collider.GetComponent<SwarmUnit>();
                    if (swarmUnit)
                    {
                        swarmUnit.ApplySwarmDamage(damage, element);
                    }
                    else
                    {
                        hitPoint.collider.GetComponent<Enemy>().TakeDamage(damage, Vector3.zero, element);
                    }
                    _currentHitTime = Time.time;
                }
            }
        }
    }
}
