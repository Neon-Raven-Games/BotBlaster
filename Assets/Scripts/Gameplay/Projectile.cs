using System;
using UnityEngine;

namespace Gameplay
{
    public class Projectile : MonoBehaviour
    {
        public ElementFlag elementFlag;
        public float speed;
        public float damage;

        public GameObject impact;

        private void OnEnable()
        {
            impact.SetActive(false);
        }

        private void Update()
        {
            transform.position += transform.forward * (speed * Time.deltaTime);
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                Debug.Log("Hit enemy!");
                
                // collision.gameObject.GetComponent<Enemy>().TakeDamage(damage);
                impact.transform.parent = null;
                impact.SetActive(true);
                gameObject.SetActive(false);
                collision.gameObject.SetActive(false);
            }
            
        }
    }
}