using Gameplay.Enemies;
using NRTools.GpuSkinning;
using UnityEngine;

public class Enemy : Actor
{
    public EnemyType enemyType;
    [SerializeField] private EnemyHealthBar healthBar;
    [SerializeField] private GameObject deathParticleSystem;

    protected internal float lastAttackTime;
    protected internal Transform player;
    protected Actor playerComponent;
    protected GpuMeshAnimator meshAnimator;

    private Rigidbody rigidbody;
    private bool _dead;
    private bool _intro;

    protected virtual void OnEnable()
    {
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

        if (!meshAnimator) meshAnimator = GetComponent<GpuMeshAnimator>();
        if (!meshAnimator) return;
        meshAnimator.enemyType = enemyType;
        meshAnimator.UpdateElement(element);
    }

    public void ApplyBalance(int waveNumber)
    {
        _intro = true;
        var multipliers = GameBalancer.GetBalanceMultipliers(waveNumber);

        currentHealth = Mathf.CeilToInt(baseHealth * multipliers.HealthMultiplier);
        if (healthBar)
        {
            healthBar.SetMaxValue(currentHealth);
            healthBar.FillMax();
        }

        currentDamage = Mathf.CeilToInt(baseDamage * multipliers.DamageMultiplier);
        currentSpeed = baseSpeed * multipliers.SpeedMultiplier;
        currentAttackRange = baseAttackRange * multipliers.AttackRangeMultiplier;
        currentAttackCoolDown = baseAttackCoolDown * multipliers.AttackCooldownMultiplier;
    }

    protected override void Awake()
    {
        base.Awake();
        meshAnimator = GetComponent<GpuMeshAnimator>();
        rigidbody = GetComponent<Rigidbody>();
        player = Camera.main.transform;
        playerComponent = FindObjectOfType<DevController>();
    }

    protected internal void RotateToPlayer(float rotationSpeed)
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

        var adjustedAttackCooldown = currentAttackCoolDown;
        if ((element & ElementFlag.Wind) != 0)
        {
            adjustedAttackCooldown /= ElementDecorator.STRENGTH_MULTIPLIER;
        }

        return timeSinceLastAttack >= adjustedAttackCooldown;
    }

    public virtual void FinishIntro()
    {
        _intro = false;
    }

    protected override void Update()
    {
        base.Update();

        if (_intro)
        {
            if (playerComponent != null) RotateToPlayer(25);
            return;
        }
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
            healthBar.FillEmpty();
            var weak = WeaknessesFor(elementFlag);
            var strong = StrengthsFor(elementFlag);
            if ((weak & element) != 0) Die(StatusEffectiveness.Weak);
            else if ((strong & element) != 0) Die(StatusEffectiveness.Strong);
            else Die(StatusEffectiveness.Normal);
        }
        else
        {
            meshAnimator.PlayOneShotHitAnimation();
            healthBar.ReduceValue(damage);
            currentHealth -= damage;
        }
    }


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