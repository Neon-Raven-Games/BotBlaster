using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootToPlay : MonoBehaviour
{
    [SerializeField] private WaveController waveController;
    [SerializeField] private GameObject UI;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            waveController.StartWaves();
            UI.SetActive(false);
        }
    }
}
