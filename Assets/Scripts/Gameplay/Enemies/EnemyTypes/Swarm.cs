using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay.Enemies.EnemyBehaviors.Base;
using Gameplay.Enemies.EnemyBehaviors.Grunt;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Enemies.EnemyTypes
{
    public class Swarm : Enemy
    {
        private int _currentSwarmUnitCount;
        [SerializeField] private GameObject swarmUnitPrefab;
        [SerializeField] internal int swarmCount = 5;
        [SerializeField] private float diveBombPercentage = 0.2f;
        [SerializeField] private float swarmRadius = 2f;
        [SerializeField] private float cohesionFactor = 1f;
        [SerializeField] private float separationFactor = 1.5f;
        [SerializeField] private float alignmentFactor = 1f;
        [SerializeField] private float randomFactor = 0.5f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float swayAmplitude = 0.5f;
        [SerializeField] private float swayFrequency = 1f;
        [SerializeField] private float noiseScale = 0.2f;

        private List<SwarmUnit> _swarmUnits = new();
        private float _lastDiveBombTime;
        private bool _initialized;
        private float _orbitAngle;
        private SwarmBloom _swarmBloom;
        private BaseEnemyBehavior _currentBehavior;

        protected override void Awake()
        {
            base.Awake();
            _swarmBloom = new SwarmBloom(this);
            _currentBehavior = _swarmBloom;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!_initialized)
            {
                _initialized = true;
                return;
            }

            if (_swarmUnits.Count == 0) InitializeSwarm(swarmCount);
            SetSwarmActive(swarmCount);
        }

        private void OnDisable()
        {
            SleepSwarm();
        }

        private void SleepSwarm()
        {
            foreach (var swarmUnit in _swarmUnits)
            {
                if (!swarmUnit) return;
                swarmUnit.gameObject.SetActive(false);
            }
        }

        protected override void Update()
        {
            base.Update();
            if (_currentSwarmUnitCount <= 0) base.Die(StatusEffectiveness.Normal);
        }

        private void SetSwarmActive(int count)
        {
            _currentSwarmUnitCount = count;
            if (_swarmUnits.Count < count)
                InitializeSwarm(count - _swarmUnits.Count + 1);

            for (var i = 0; i < count; i++)
            {
                var angle = i * Mathf.PI * 2f / count;
                var position = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * swarmRadius;
                position += transform.position;

                var swarmUnit = _swarmUnits[i];
                swarmUnit.transform.position = position;
                swarmUnit.speed = 15;
                swarmUnit.SetSwarmCenter(transform);
                swarmUnit.Initialize(playerComponent, currentDamage, currentHealth / count, element);
                swarmUnit.gameObject.SetActive(true);
            }

            MoveSwarmAsync().Forget();
        }

        private void InitializeSwarm(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var angle = i * Mathf.PI * 2f / count;
                var position = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * swarmRadius;
                position += transform.position;

                var swarmUnit = Instantiate(swarmUnitPrefab, position, Quaternion.identity).GetComponent<SwarmUnit>();
                swarmUnit.speed = currentSpeed;
                swarmUnit.SetSwarmCenter(transform);
                _swarmUnits.Add(swarmUnit);
                swarmUnit.gameObject.SetActive(false);
            }
        }

        private async UniTaskVoid MoveSwarmAsync()
        {
            while (gameObject && gameObject.activeInHierarchy)
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

            if (!this || !swarmUnit) return;

            var sway = Mathf.Sin(Time.time * swayFrequency + swarmUnit.transform.position.x) * swayAmplitude;

            var noiseX = Mathf.PerlinNoise(Time.time * noiseScale, swarmUnit.transform.position.y) * 2f - 1f;
            var noiseZ = Mathf.PerlinNoise(swarmUnit.transform.position.x, Time.time * noiseScale) * 2f - 1f;

            var swayMotion = new Vector3(noiseX, sway, noiseZ);
            var flockingDirection = (separation + alignment + cohesion + randomMovement + swayMotion).normalized;

            flockingDirection = Vector3.Lerp(swarmUnit.transform.forward, flockingDirection, 0.1f);
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

        internal void TriggerDiveBomb()
        {
            _lastDiveBombTime = Time.time;
            var diveBombCount = Mathf.CeilToInt(_currentSwarmUnitCount * diveBombPercentage);
            var activeUnits = _swarmUnits.Where(x => x.gameObject.activeInHierarchy).ToList();

            if (activeUnits == null || activeUnits.Count == 0)
            {
                base.Die(StatusEffectiveness.Strong);
                return;
            }

            for (var i = 0; i < diveBombCount && i < activeUnits.Count; i++)
            {
                activeUnits[i].isDiveBombing = true;
                activeUnits[i].DiveBomb(player.position);
            }
        }

        // todo, this is not killing enemies. Added one in the update loop
        protected override void Die(StatusEffectiveness status)
        {
            if (_currentSwarmUnitCount > 0) return;
            base.Die(status);
        }

        protected override void Move()
        {
            _currentBehavior.Move();
        }

        public void SwarmUnitDead()
        {
            _currentSwarmUnitCount--;
            if (_currentSwarmUnitCount <= 0)
                base.Die(StatusEffectiveness.Strong);
        }
    }
}