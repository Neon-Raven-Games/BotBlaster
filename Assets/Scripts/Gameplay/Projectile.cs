using System;
using UnityEngine;

namespace Gameplay
{
    public class Projectile : MonoBehaviour
    {
        public ElementFlag elementFlag;
        public float speed;
        public int damage;

        public GameObject impact;

        private void Awake()
        {
            impact.transform.parent = null;
        }
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
                collision.gameObject.GetComponent<Enemy>().TakeDamage(damage, transform.forward);
                impact.SetActive(true);
                gameObject.SetActive(false);
            }
        }
    }
}