using System;
using System.Collections;
using Gameplay.Enemies;
using UnityEngine;

public class Enemy : Actor
{
    public EnemyType enemyType;
    public Vector2 knockBackForce;
    [SerializeField] private float knockBackTime = 0.5f;
    private bool _knockingBack;
    public int minWaveSpawn;
    
    private Rigidbody _rigidbody;
    private Transform _player;
    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody>();
        _player = Camera.main.transform;
    }

    private void OnEnable()
    {
        _knockingBack = false;
        currentHealth = baseHealth;
        currentDamage = baseDamage;
        currentSpeed = baseSpeed;
        currentAttackRange = baseAttackRange;
        currentAttackCoolDown = baseAttackCoolDown;
    }

    public void Initialize(EnemyData enemyData)
    {
        base.Initialize(enemyData);
        element = enemyData.elementFlag;
        minWaveSpawn = enemyData.minWaveSpawn;
        enemyType = enemyData.enemyType;
    }

    private float _lastAttackTime;
    
    private bool CanAttack()
    {
        var timeSinceLastAttack = Time.time - _lastAttackTime;

        // Apply attack speed boost if Wind element is active
        var adjustedAttackCooldown = currentAttackCoolDown;
        if ((element & ElementFlag.Wind) != 0)
        {
            adjustedAttackCooldown /= ElementDecorator.STRENGTH_MULTIPLIER;
        }

        return timeSinceLastAttack >= adjustedAttackCooldown;
    }
    
    protected override void Update()
    {
        base.Update();
        var playerDistance = Vector3.Distance(transform.position, _player.position);
        if (playerDistance <= currentAttackRange && CanAttack())
        {
            _lastAttackTime = Time.time;
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
        
        if (enemyType == EnemyType.Grunt || enemyType == EnemyType.Tank) directionToPlayer.y = 0; 
        
        if (directionToPlayer != Vector3.zero) transform.rotation = Quaternion.LookRotation(directionToPlayer);
        _rigidbody.velocity = directionToPlayer.normalized * (GetMovementCurrentSpeed() * Time.deltaTime);
    }

    public void TakeDamage(int damage, Vector3 hitDirection, ElementFlag elementFlag)
    {
        
        damage = ApplyDamage(damage, elementFlag);
        if (currentHealth - damage <= 0)
        {
            _knockingBack = true;
            var weak = WeaknessesFor(ElementFlag.Fire);
            var strong = StrengthsFor(ElementFlag.Fire);
            
            if ((weak & element) != 0) Die(StatusEffectiveness.Weak);
            else if ((strong & element) != 0) Die(StatusEffectiveness.Strong);
            else Die(StatusEffectiveness.Normal);
        }
        else
        {
            if (!_knockingBack)
            {
                _knockingBack = true;
                KnockBack(hitDirection);
            }
            currentHealth -= damage;
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
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        hitDirection.y = 1;
        _rigidbody.AddForce(hitDirection.normalized * knockBackForce, ForceMode.Impulse);
        StartCoroutine(KnockBackTimer());
    }
    
    protected override void Die(StatusEffectiveness status)
    {
        gameObject.SetActive(false);
        GameBalancer.KillEnemy(status);
    }
}
