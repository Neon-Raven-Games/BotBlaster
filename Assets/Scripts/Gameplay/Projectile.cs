using System;
using Gameplay.Enemies;
using UnityEngine;

namespace Gameplay
{
    public class Projectile : MonoBehaviour
    {
        public ElementFlag elementFlag;
        public float speed;
        public int damage;
        [SerializeField] private GameObject muzzleFlash;

        public GameObject impact;

        private void Awake()
        {
            impact.transform.parent = null;
            muzzleFlash.transform.parent = null;
        }

        private void OnEnable()
        {
            impact.SetActive(false);
            muzzleFlash.transform.position = transform.position;
            muzzleFlash.transform.rotation = transform.rotation;
            muzzleFlash.SetActive(true);
        }

        private void OnDisable()
        {
            if (muzzleFlash) muzzleFlash.SetActive(false);
        }

        private void Update()
        {
            transform.position += transform.forward * (speed * Time.deltaTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                var enemy = collision.gameObject.GetComponent<Enemy>();
                if (enemy) enemy.TakeDamage(damage, transform.forward, elementFlag);
            }
            else if (collision.gameObject.CompareTag("Player"))
            {
                Debug.Log("APlying player damage from collision");
                collision.gameObject.GetComponent<Actor>().ApplyDamage(damage, elementFlag, collision.GetContact(0).point );
            }
            
            impact.transform.position = collision.GetContact(0).point;
            impact.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}