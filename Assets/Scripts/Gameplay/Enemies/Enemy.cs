using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float knockBackForce;
    
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
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
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
    }

    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        if (_currentHealth - damage <= 0)
        {
            Die();
        }
        else
        {
            KnockBack(hitDirection);
            _currentHealth -= damage;
        }
    }
    
    private void Die()
    {
        // play death animation
        gameObject.SetActive(false);
    }
    
    private void KnockBack(Vector3 hitDirection)
    {
        // color red
        _rigidbody.AddForce(hitDirection * knockBackForce, ForceMode.Impulse);
    }
}
