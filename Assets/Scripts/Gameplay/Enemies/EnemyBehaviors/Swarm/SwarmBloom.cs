using Gameplay.Enemies.EnemyBehaviors.Base;
using Gameplay.Enemies.EnemyTypes;
using UnityEngine;

namespace Gameplay.Enemies.EnemyBehaviors.Grunt
{
    public class SwarmBloom : BaseEnemyBehavior
    {
        private bool _initialDiveBombTriggered; // Track if the first dive bomb delay has been applied
        private readonly float diveBombDelay = 25f;
        private float _orbitAngle;
        private float _lastDiveBombTime;
        private float _circleRadius = 11f; 
        private const float _CIRCLE_RADIUS_MIN_DIST = 5f; 
        private const float _ORBIT_SPEED = 0.4f;
        private const float _CLOSING_SPEED_MULTIPLIER = 0.5f;
        private const float _DIVE_BOMB_COOLDOWN = 3f; // Cooldown between dive bombs
        float playAreaWidth = 40f;
        float playAreaDepth = 30f;

        private Swarm swarm;

        public SwarmBloom(Enemy enemy) : base(enemy)
        {
            swarm = enemy as Swarm;
        }

        public override void Attack()
        {
        }

        public override void Move()
        {
            _circleRadius = 15;
            _orbitAngle += _ORBIT_SPEED * Time.deltaTime;
            var bloomX = Mathf.Sin(_orbitAngle) * _circleRadius;
            var bloomZ = Mathf.Cos(_orbitAngle) * _circleRadius;

            bloomZ -= 16;
            var targetPosition = new Vector3(
                Mathf.Clamp(player.position.x + bloomX, -playAreaWidth / 2, playAreaWidth / 2), // Keep within horizontal bounds
                20, 
                Mathf.Clamp(player.position.z + bloomZ, -playAreaDepth / 2, playAreaDepth / 2) // Keep within depth bounds
            );

            
            if (_circleRadius > _CIRCLE_RADIUS_MIN_DIST)
                _circleRadius = Mathf.Max(_CIRCLE_RADIUS_MIN_DIST, _circleRadius - _CLOSING_SPEED_MULTIPLIER * Time.deltaTime);
            enemy.transform.position = Vector3.Lerp(enemy.transform.position, targetPosition, Time.deltaTime * enemy.currentSpeed);
            if (!_initialDiveBombTriggered)
            {
                if (Time.time - _lastDiveBombTime >= diveBombDelay)
                {
                    swarm.TriggerDiveBomb();
                    _lastDiveBombTime = Time.time;
                    _initialDiveBombTriggered = true; 
                }
            }
            else
            {
                if (Time.time - _lastDiveBombTime >= _DIVE_BOMB_COOLDOWN)
                {
                    swarm.TriggerDiveBomb();
                    _lastDiveBombTime = Time.time; 
                }
            }
        }


        public override void OnEnable()
        {
            _lastDiveBombTime = Time.time;
            _initialDiveBombTriggered = false;
        }


        public override void OnDisable()
        {
            _orbitAngle = 0;
            _initialDiveBombTriggered = false;
        }
    }
}