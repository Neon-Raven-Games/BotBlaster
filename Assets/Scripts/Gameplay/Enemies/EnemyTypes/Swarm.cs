using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Enemies.EnemyTypes
{
    public class Swarm : Enemy
    {
        [SerializeField] private GameObject swarmUnitPrefab;
        [SerializeField] private int swarmCount = 5;
        [SerializeField] private float circleRadius = 5f;
        [SerializeField] private float diveBombPercentage = 0.2f;
        [SerializeField] private float diveBombCooldown = 5f;
        [SerializeField] private float swarmRadius = 2f;
        [SerializeField] private float orbitSpeed = 2f;
        [SerializeField] private float closingSpeedMultiplier = 0.1f;
        [SerializeField] private float cohesionFactor = 1f;
        [SerializeField] private float separationFactor = 1.5f;
        [SerializeField] private float alignmentFactor = 1f;
        [SerializeField] private float randomFactor = 0.5f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float swayAmplitude = 0.5f;
        [SerializeField] private float swayFrequency = 1f;
        [SerializeField] private float noiseScale = 0.2f;
        [SerializeField] private float circleRadiusMinDist = 7f;

        private List<SwarmUnit> _swarmUnits = new();
        private float _lastDiveBombTime;
        private bool _initialized;
        private float _orbitAngle;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!_initialized)
            {
                _initialized = true;
                InitializeSwarm(20);
                return;
            }

            SetSwarmActive(swarmCount);
        }

        private void OnDisable()
        {
            SleepSwarm();
        }
        
        private void SleepSwarm()
        {
            foreach (var swarmUnit in _swarmUnits)
                swarmUnit.gameObject.SetActive(false);
        }

        private void SetSwarmActive(int count)
        {
            currentSwarmUnitCount = count;
            for (var i = 0; i < count; i++)
            {
                var angle = i * Mathf.PI * 2f / count;
                var position = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * swarmRadius;
                position += transform.position;

                var swarmUnit = _swarmUnits[i];
                swarmUnit.transform.position = position;
                swarmUnit.speed = currentSpeed;
                swarmUnit.SetSwarmCenter(transform);
                swarmUnit.Initialize(playerComponent, currentDamage, currentHealth / count, element);
                swarmUnit.gameObject.SetActive(true);
            }
            MoveSwarmAsync().Forget();
        }

        public void InitializeSwarm(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var angle = i * Mathf.PI * 2f / count;
                var position = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * swarmRadius;
                position += transform.position;

                var swarmUnit = Instantiate(swarmUnitPrefab, position, Quaternion.identity).GetComponent<SwarmUnit>();
                swarmUnit.speed = currentSpeed;
                swarmUnit.SetSwarmCenter(transform);
                swarmUnit.Initialize(playerComponent, currentDamage, currentHealth / count, element);

                _swarmUnits.Add(swarmUnit);
                swarmUnit.gameObject.SetActive(false);
            }
        }
        private async UniTaskVoid MoveSwarmAsync()
        {
            while (gameObject.activeInHierarchy)
            {
                var separationTasks = new List<UniTask>();
                foreach (var swarmUnit in _swarmUnits)
                {
                    if (swarmUnit.isDiveBombing) continue;
                    separationTasks.Add(CalculateFlockingAsync(swarmUnit));
                }

                await UniTask.WhenAll(separationTasks);

                foreach (var swarmUnit in _swarmUnits)
                {
                    if (swarmUnit.isDiveBombing) continue;
                    ApplyMovement(swarmUnit);
                }

                await UniTask.Yield();
            }
        }
        private async UniTask CalculateFlockingAsync(SwarmUnit swarmUnit)
        {
            var separation = Vector3.zero;
            var alignment = Vector3.zero;
            var cohesion = transform.position - swarmUnit.transform.position;

            foreach (var otherUnit in _swarmUnits)
            {
                if (otherUnit == swarmUnit) continue;

                var distance = Vector3.Distance(swarmUnit.transform.position, otherUnit.transform.position);
                if (distance < 5f)
                {
                    separation += (swarmUnit.transform.position - otherUnit.transform.position) / distance;
                }

                alignment += otherUnit.transform.forward;
            }

            await UniTask.Yield();

            alignment /= _swarmUnits.Count;
            alignment = alignment.normalized * alignmentFactor;

            separation = separation.normalized * separationFactor;
            cohesion = cohesion.normalized * cohesionFactor;

            var randomMovement = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * randomFactor;
            var sway = Mathf.Sin(Time.time * swayFrequency + swarmUnit.transform.position.x) * swayAmplitude;

            var noiseX = Mathf.PerlinNoise(Time.time * noiseScale, swarmUnit.transform.position.y) * 2f - 1f;
            var noiseZ = Mathf.PerlinNoise(swarmUnit.transform.position.x, Time.time * noiseScale) * 2f - 1f;

            var swayMotion = new Vector3(noiseX, sway, noiseZ);
            var flockingDirection = (separation + alignment + cohesion + randomMovement + swayMotion).normalized;

            // Smooth out movement by blending with previous direction
            flockingDirection = Vector3.Lerp(swarmUnit.transform.forward, flockingDirection, 0.1f);

            // Store the flocking direction to apply it later on the main thread
            swarmUnit.flockingDirection = flockingDirection;
        }
        private void ApplyMovement(SwarmUnit swarmUnit)
        {
            // Apply the final movement
            swarmUnit.transform.position += swarmUnit.flockingDirection * (swarmUnit.speed * Time.deltaTime);

            if (swarmUnit.flockingDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(swarmUnit.flockingDirection);
                swarmUnit.transform.rotation = Quaternion.Slerp(swarmUnit.transform.rotation, targetRotation,
                    Time.deltaTime * rotationSpeed);
            }
        }

        protected override void Attack()
        {
        }

        private void TriggerDiveBomb()
        {
            _lastDiveBombTime = Time.time;
            int diveBombCount = Mathf.CeilToInt(currentSwarmUnitCount * diveBombPercentage);

            var activeUnits = _swarmUnits.Where(x => x.gameObject.activeInHierarchy).ToList();
            if (activeUnits == null || activeUnits.Count == 0)
            {
                Die(StatusEffectiveness.Strong);
                return;
            }

            for (var i = 0; i < diveBombCount && i < activeUnits.Count; i++)
            {
                activeUnits[i].isDiveBombing = true;
                activeUnits[i].DiveBomb(player.position);
            }
        }

        protected override void Die(StatusEffectiveness status)
        {
            if (currentSwarmUnitCount > 0) return;
            base.Die(status);
            
        }

        protected override void Move()
        {
            _orbitAngle += orbitSpeed * Time.deltaTime;
            var offset = new Vector3(Mathf.Sin(_orbitAngle), 0, Mathf.Cos(_orbitAngle)) * circleRadius;
            var targetPosition = player.position + offset;
            targetPosition.y = transform.position.y;

            if (circleRadius > circleRadiusMinDist)
                circleRadius =
                    Mathf.Max(1f,
                        circleRadius - closingSpeedMultiplier * Time.deltaTime); // Adjust the circleRadius over time

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * currentSpeed);
            if (circleRadius <= circleRadiusMinDist && Time.time - _lastDiveBombTime >= diveBombCooldown)
                TriggerDiveBomb();
        }
        private int currentSwarmUnitCount;

        public void SwarmUnitDead()
        {
            currentSwarmUnitCount--;
            if (currentSwarmUnitCount <= 0) Die(StatusEffectiveness.Strong);
        }
    }
}