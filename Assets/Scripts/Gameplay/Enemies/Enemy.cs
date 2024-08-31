using System;
using System.Collections;
using Gameplay.Enemies;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyType enemyType;
    public Vector2 knockBackForce;
    [SerializeField] private float knockBackTime = 0.5f;
    private bool _knockingBack;

    [Header("Dynamic Populated Fields from EnemyData, Debug Purposes")]
    public ElementFlag element;
    public int baseHealth;
    public int baseDamage;
    public float baseSpeed;
    public float baseAttackRange;
    public float baseAttackCoolDown;
    public int minWaveSpawn;

    private int _currentHealth;
    private int _currentDamage;
    private float _currentSpeed;
    private float _currentAttackRange;
    private float _currentAttackCoolDown;
    
    private Rigidbody _rigidbody;
    private Transform _player;
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _player = Camera.main.transform;
    }

    private void OnEnable()
    {
        _knockingBack = false;
        _currentHealth = baseHealth;
        _currentDamage = baseDamage;
        _currentSpeed = baseSpeed;
        _currentAttackRange = baseAttackRange;
        _currentAttackCoolDown = baseAttackCoolDown;
    }

    public void Initialize(EnemyData enemyData)
    {
        element = enemyData.elementFlag;
        baseHealth = enemyData.baseHealth;
        baseDamage = enemyData.baseDamage;
        baseSpeed = enemyData.baseSpeed;
        baseAttackRange = enemyData.baseAttackRange;
        baseAttackCoolDown = enemyData.baseAttackCooldown;
        minWaveSpawn = enemyData.minWaveSpawn;
        
        _currentHealth = baseHealth;
        _currentDamage = baseDamage;
        _currentSpeed = baseSpeed;
        _currentAttackRange = baseAttackRange;
        _currentAttackCoolDown = baseAttackCoolDown;
        enemyType = enemyData.enemyType;
    }

    private void Update()
    {
        var playerDistance = Vector3.Distance(transform.position, _player.position);
        if (playerDistance <= _currentAttackRange)
        {
            Attack();
        }
        else
        {
            Move();
        }
    }

    private void Attack()
    {
    }

    private void Move()
    {
        if (_knockingBack) return;
        var directionToPlayer = _player.position - transform.position;
        
        if (enemyType == EnemyType.Grunt) directionToPlayer.y = 0; 
        
        if (directionToPlayer != Vector3.zero) transform.rotation = Quaternion.LookRotation(directionToPlayer);
        _rigidbody.velocity = directionToPlayer.normalized * (_currentSpeed * Time.deltaTime);
    }

    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        if (_currentHealth - damage <= 0)
        {
            _knockingBack = true;
            Die();
        }
        else
        {
            if (!_knockingBack)
            {
                _knockingBack = true;
                KnockBack(hitDirection);
            }
            _currentHealth -= damage;
        }
    }
 
    private IEnumerator KnockBackTimer()
    {
        knockBackTime = 0.5f;
        while (knockBackTime > 0)
        {
            knockBackTime -= Time.deltaTime;
            yield return null;
        }
        _knockingBack = false;
    }
    
    private void KnockBack(Vector3 hitDirection)
    {
        hitDirection.y = 1;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.AddForce(hitDirection.normalized * knockBackForce, ForceMode.Impulse);
        StartCoroutine(KnockBackTimer());
    }
    
    private void Die()
    {
        gameObject.SetActive(false);
        GameBalancer.KillEnemy();
    }
   
}
