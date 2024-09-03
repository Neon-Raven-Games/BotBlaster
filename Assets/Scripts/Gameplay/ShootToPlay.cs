using UnityEngine;

public class ShootToPlay : MonoBehaviour
{
    [SerializeField] private WaveController waveController;
    [SerializeField] private GameObject UI;

    public void StartWaves()
    {
            waveController.StartWaves();
            UI.SetActive(false);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile")) StartWaves();
    }
}
