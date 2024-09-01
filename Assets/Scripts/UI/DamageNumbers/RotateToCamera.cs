using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToCamera : MonoBehaviour
{
    private Transform mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main.transform;
    }
    private void Update()
    {
        var forward = mainCamera.position - transform.position;
        transform.rotation = Quaternion.LookRotation(-forward, Vector3.up);
    }
}
