using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    public class StreamProjectile : MonoBehaviour
    {
        [SerializeField] private List<ParticleSystem> zAxisParticles; // Particle systems along the Z-axis
        [SerializeField] private GameObject projectileImpact; // Impact effect prefab
        [SerializeField] private float endDistance = 10f; // Maximum distance the laser can extend
        [SerializeField] private float jetSpeed = 5f; // Speed at which the laser extends
        
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

                // Check for collision in front of the jet
                var hitDetected = Physics.RaycastNonAlloc(startPosition, direction, _hitInfo, endDistance) > 0;
                var targetDistance = hitDetected ? _hitInfo[0].distance : endDistance;

                // Calculate the new distance by incrementing based on jetSpeed
                _currentDistance = Mathf.MoveTowards(_currentDistance, targetDistance, jetSpeed * Time.deltaTime);
                
                // Update particle size and collider
                SetParticleSize(_currentDistance);

                // Handle impact effect
                if (hitDetected)
                {
                    HandleImpact(_hitInfo[0].point);
                }
                else if (projectileImpact != null)
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

        private void HandleImpact(Vector3 hitPoint)
        {
            if (projectileImpact != null)
            {
                projectileImpact.transform.position = hitPoint;
                projectileImpact.SetActive(true);
            }
        }
    }
}
