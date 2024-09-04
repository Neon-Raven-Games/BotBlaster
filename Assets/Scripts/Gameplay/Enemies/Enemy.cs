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

    protected Rigidbody rigidbody;
    protected Transform player;
    protected Actor playerComponent;

    protected virtual void ApplyBalance(int waveNumber)
    {
            
    }
    protected override void Awake()
    {
        base.Awake();
        rigidbody = GetComponent<Rigidbody>();
        player = Camera.main.transform;
        playerComponent = FindObjectOfType<DevController>();
    }

    protected void RotateToPlayer(float rotationSpeed)
    {
        var playerDirection = player.position - transform.position;
        if (playerDirection == Vector3.zero) return;

        var targetRotation = Quaternion.LookRotation(playerDirection.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    protected void RotateToFlatPlayer(float rotationSpeed)
    {
        var playerDirection = player.position - transform.position;
        playerDirection.y = 0;
        if (playerDirection == Vector3.zero) return;
        
        var targetRotation = Quaternion.LookRotation(playerDirection.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    protected virtual void OnEnable()
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

    protected float lastAttackTime;

    private bool CanAttack()
    {
        var timeSinceLastAttack = Time.time - lastAttackTime;

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
        if (WaveController.paused) return;
        var playerDistance = Vector3.Distance(transform.position, player.position);
        if (playerDistance <= currentAttackRange && CanAttack())
        {
            lastAttackTime = Time.time;
            Attack();
        }
        else
        {
            Move();
        }
    }

    protected virtual void Attack()
    {
    }

    protected virtual void Move()
    {
        if (_knockingBack) return;
        var directionToPlayer = player.position - transform.position;

        if (enemyType == EnemyType.Grunt || enemyType == EnemyType.Tank) directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero) transform.rotation = Quaternion.LookRotation(directionToPlayer);
        rigidbody.velocity = directionToPlayer.normalized * (GetMovementCurrentSpeed() * Time.deltaTime);
    }

    public void TakeDamage(int damage, Vector3 hitDirection, ElementFlag elementFlag)
    {
        damage = ApplyDamage(damage, elementFlag, transform.position);
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

    // this needs to work for both flying and ground enemies
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
        // todo, update knock back update edition
        // StartCoroutine(KnockBackTimer());
    }

    protected override void Die(StatusEffectiveness status)
    {
        gameObject.SetActive(false);
        GameBalancer.KillEnemy(status);
    }
}