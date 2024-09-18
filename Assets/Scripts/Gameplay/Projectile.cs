using Gameplay.Enemies;
using UnityEngine;

namespace Gameplay
{
    public class Projectile : MonoBehaviour
    {
        private bool _isPlayerProjectile;

        public bool isPlayerProjectile
        {
            get => _isPlayerProjectile;
            set
            {
                _isPlayerProjectile = value;
                gameObject.layer = LayerMask.NameToLayer(value ? 
                    "PlayerProjectile" : "RobotProjectile");
            }
        }
        public ElementFlag elementFlag;
        public float speed;
        public int damage;
        public GameObject impact;
        public int effectiveDamage;
        public HandCannon cannon;

        private void Awake()
        {
            if (impact) impact.transform.parent = null;
        }

        private void OnEnable()
        {
            if (impact) impact.SetActive(false);
            speed = 20;
        }

        private void OnDisable()
        {
            isPlayerProjectile = false;
            effectiveDamage = 0;
            cannon = null;
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
                if (enemy)
                {
                    if (enemy.IsWeakAgainst(elementFlag)) damage = effectiveDamage;
                    enemy.TakeDamage(damage, transform.forward, elementFlag);
                }
            }
            else if (collision.gameObject.CompareTag("Player") && !isPlayerProjectile)
            {
                collision.gameObject.GetComponent<Actor>().ApplyDamage(damage, elementFlag, collision.GetContact(0).point);
            }
            else if (collision.gameObject.CompareTag("PowerUp") && cannon)
            {
                cannon.InitializeElementChange();
                cannon.blasterElement = collision.gameObject.GetComponent<PowerUp>().GetElement();
                cannon.FinalizeElementChange();
            }
            else if (collision.gameObject.CompareTag("Player") && isPlayerProjectile) return;
            
            impact.transform.position = collision.GetContact(0).point;
            impact.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}