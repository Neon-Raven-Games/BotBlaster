using UnityEngine;

public class ShootToPlay : MonoBehaviour
{
    [SerializeField] private WaveController waveController;
    [SerializeField] private GameObject UI;
    private bool _gameStarted;
    public void StartWaves()
    {
        
        waveController.StartWaves();
        UI.SetActive(false);
    }

    private void OnDisable()
    {
        _gameStarted = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_gameStarted) return;
        _gameStarted = true;
        if (collision.gameObject.CompareTag("Projectile")) StartWaves();
    }
}