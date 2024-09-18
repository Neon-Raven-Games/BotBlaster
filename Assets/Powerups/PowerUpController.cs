using Gameplay.Enemies;
using UnityEngine;

namespace Powerups
{
    public class PowerUpController : MonoBehaviour
    {
        [SerializeField] private PowerUp powerUpPrefab; // Prefab for the power-up object
        [SerializeField] private float baseSpawnInterval = 30f; // Base interval for power-up spawns
        [SerializeField] private float spawnRadius = 10f; // Radius around player to spawn power-ups
        [SerializeField] private Transform playerTransform;

        private float spawnTimer;
        private float spawnInterval;
        private bool powerUpActive;
        private ElementFlag currentPlayerPowerUp;

        private void Start()
        {
            spawnInterval = baseSpawnInterval;
            spawnTimer = 0;
        }

        private void Update()
        {
            if (powerUpActive) return; // Don't spawn a new power-up while one is active

            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                SpawnPowerUp();
                spawnTimer = 0;
            }
        }

        // todo spawn pos.y needs to be better
        private void SpawnPowerUp()
        {
            var spawnPosition = playerTransform.position + Random.insideUnitSphere * spawnRadius;
            spawnPosition.y = 0;

            var newPowerUp = Instantiate(powerUpPrefab, spawnPosition, Quaternion.identity);

            lock (GameBalancer._SElementProbabilityList)
            {
                newPowerUp.element = GameBalancer._SElementProbabilityList.PickValue();
            }

            powerUpActive = true;

            newPowerUp.OnPowerUpCollected += OnPowerUpCollected;
        }

        private void OnPowerUpCollected(ElementFlag collectedElement, PowerUp powerUp)
        {
            powerUp.OnPowerUpCollected -= OnPowerUpCollected;
            powerUpActive = false;
            currentPlayerPowerUp = collectedElement;

            GameBalancer.UpdateElementProbabilities(currentPlayerPowerUp);
            AdjustPowerUpSpawnRate();
        }

        private void AdjustPowerUpSpawnRate()
        {
            var playerPerformance = GameBalancer.playerPerformance;
            spawnInterval = baseSpawnInterval / Mathf.Clamp(playerPerformance, 0.5f, 1.5f);
        }
    }
}