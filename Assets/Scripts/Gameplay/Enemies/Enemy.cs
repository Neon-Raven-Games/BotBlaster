using System.Collections;
using Gameplay.Enemies;
using NRTools.AtlasHelper;
using UnityEngine;

public class Enemy : Actor
{
    public EnemyType enemyType;
    [SerializeField] private float knockBackTime = 0.5f;
    [SerializeField] private Renderer renderer;
    private bool _knockingBack;
    protected float lastAttackTime;
    
    protected Rigidbody rigidbody;
    protected Transform player;
    protected Actor playerComponent;
    private AtlasIndex _atlasIndex;

    protected virtual void OnEnable()
    {
        _knockingBack = false;
        currentHealth = baseHealth;
        currentDamage = baseDamage;
        currentSpeed = baseSpeed;
        currentAttackRange = baseAttackRange;
        currentAttackCoolDown = baseAttackCoolDown;
        _dead = false;
        
        if (deathParticleSystem)
        {
            deathParticleSystem.SetActive(false);
            deathParticleSystem.transform.parent = transform;
        }

        if(!_atlasIndex) _atlasIndex = GetComponent<AtlasIndex>();
        if (!_atlasIndex || !renderer) return;
        
        var rect = _atlasIndex.GetRect(element, out var page);
        NRAtlasManager.SetUVAndAtlasPage(rect, page, renderer);
    }

    public virtual void ApplyBalance(int waveNumber)
    {
        var multipliers = GameBalancer.GetBalanceMultipliers(waveNumber);

        currentHealth = Mathf.CeilToInt(baseHealth * multipliers.HealthMultiplier);
        currentDamage = Mathf.CeilToInt(baseDamage * multipliers.DamageMultiplier);
        currentSpeed = baseSpeed * multipliers.SpeedMultiplier;
        currentAttackRange = baseAttackRange * multipliers.AttackRangeMultiplier;
        currentAttackCoolDown = baseAttackCoolDown * multipliers.AttackCooldownMultiplier;
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


    public void Initialize(EnemyData enemyData)
    {
        base.Initialize(enemyData);
        element = enemyData.elementFlag;
        enemyType = enemyData.enemyType;
    }

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
            var weak = WeaknessesFor(elementFlag);
            var strong = StrengthsFor(elementFlag);

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

    private bool _dead;
    [SerializeField] private GameObject deathParticleSystem;

    protected override void Die(StatusEffectiveness status)
    {
        if (_dead) return;
        _dead = true;
        if (deathParticleSystem)
        {
            deathParticleSystem.transform.parent = null;
            deathParticleSystem.transform.position = transform.position;
            deathParticleSystem.SetActive(true);
        }
        GameBalancer.KillEnemy(status, this);
    }
}