using System;
using System.Collections.Generic;
using Gameplay.Enemies;
using Gameplay.Enemies.EnemyTypes;
using Gameplay.Util;
using UnityEngine;

public class Actor : MonoBehaviour
{
    public ElementFlag element;

    public int baseHealth;
    public int baseDamage;
    public float baseSpeed;
    public float baseAttackRange;
    public float baseAttackCoolDown;

    public int currentHealth;
    protected int currentDamage;
    protected float currentSpeed;
    protected float currentAttackRange;
    protected float currentAttackCoolDown;

    private float _lastTick;

    private Dictionary<ElementFlag, int> stacks = new();
    public ElementFlag debuffs;
    [SerializeField] private bool showDamageNumbers = true;

    protected virtual void Awake()
    {
        foreach (ElementFlag element in Enum.GetValues(typeof(ElementFlag)))
        {
            if (element == ElementFlag.None) continue;
            stacks.Add(element, 0);
        }
    }

    public void Initialize(ActorData actorData)
    {
        baseDamage = actorData.baseDamage;
        baseHealth = actorData.baseHealth;
        baseSpeed = actorData.baseSpeed;
        baseAttackRange = actorData.baseAttackRange;
        baseAttackCoolDown = actorData.baseAttackCooldown;

        currentDamage = actorData.baseDamage;
        currentHealth = actorData.baseHealth;
        currentSpeed = actorData.baseSpeed;
        currentAttackRange = actorData.baseAttackRange;
        currentAttackCoolDown = actorData.baseAttackCooldown;
    }

    private static void ShowDamageNumber(int damage, ElementFlag damageElementType, bool isWeak, bool isStrong,
        Vector3 position)
    {
        var status = StatusEffectiveness.Normal;
        if (isWeak) status = StatusEffectiveness.Weak;
        if (isStrong) status = StatusEffectiveness.Strong;
        DamageNumberPool.SetElementDamageNumber(damageElementType, position, status, damage);
    }

    
    public bool IsWeakAgainst(ElementFlag hitElement)
    {
        return (WeaknessesFor(element) & hitElement) != 0;
    }
    
    public int ApplyDamage(int damage, ElementFlag hitElement, Vector3 position, int elementLevel = 1, int stackLevel = 1)
    {
        stackEffectivenessLevel = stackLevel;
        damage = ApplyElementalDamage(damage, element, hitElement, elementLevel, ElementFlag.Rock);
        damage = ApplyElementalDamage(damage, element, hitElement, elementLevel, ElementFlag.Water);
        damage = ApplyElementalDamage(damage, element, hitElement, elementLevel, ElementFlag.Fire);
        damage = ApplyElementalDamage(damage, element, hitElement, elementLevel, ElementFlag.Wind);
        damage = ApplyElementalDamage(damage, element, hitElement, elementLevel, ElementFlag.Electricity);

        var weakness = WeaknessesFor(hitElement) & element;
        var strength = StrengthsFor(hitElement) & element;
        
        if (showDamageNumbers) ShowDamageNumber(damage, hitElement, weakness != 0, strength != 0, position);
        
        // remove elements without status effects
        hitElement &= ~ElementFlag.Rock;
        hitElement &= ~ElementFlag.Wind;

        if (this is DevController dev)
        {
            currentHealth -= damage;
            dev.HapticFeedback();
            if (currentHealth <= 0)
            {
                if (weakness != 0) Die(StatusEffectiveness.Weak);
                else if (strength != 0) Die(StatusEffectiveness.Strong);
                else Die(StatusEffectiveness.Normal);
            }
        }

        ApplyDebuff(hitElement);
        return damage;
    }

    private int ApplyElementalDamage(int damage, ElementFlag attackerElement, ElementFlag defenderElement, int elementLevel, ElementFlag targetElement)
    {
        if (attackerElement == ElementFlag.None || (attackerElement & targetElement) == 0)
        {
            return damage;
        }

        var multiplier = ElementDecorator.BASE_MULTIPLIER * elementLevel;

        if ((defenderElement & WeaknessesFor(attackerElement)) != 0)
            multiplier *= ElementDecorator.WEAKNESS_MULTIPLIER;
        else if ((defenderElement & StrengthsFor(attackerElement)) != 0)
            multiplier *= ElementDecorator.STRENGTH_MULTIPLIER;

        var finalDamage = damage + (int)(damage * multiplier * GetStackMultiplier(ElementFlag.Water));

        return finalDamage;
    }

    private float GetStackMultiplier(ElementFlag debuffElement)
    {
        return stacks.ContainsKey(debuffElement) ? stacks[debuffElement] + stackEffectivenessLevel * ElementDecorator.BASE_MULTIPLIER: 1f;
    }

    // target element is weak against
    protected ElementFlag WeaknessesFor(ElementFlag targetElement)
    {
        switch (targetElement)
        {
            case ElementFlag.Fire: return ElementFlag.Water | ElementFlag.Rock;
            case ElementFlag.Water: return ElementFlag.Electricity;
            case ElementFlag.Rock: return ElementFlag.Water;
            case ElementFlag.Wind: return ElementFlag.Electricity;
            case ElementFlag.Electricity: return ElementFlag.Rock;
            default: return ElementFlag.None;
        }
    }
    
    // target element is strong against
    protected ElementFlag StrengthsFor(ElementFlag targetElement)
    {
        switch (targetElement)
        {
            case ElementFlag.Fire: return ElementFlag.Electricity;
            case ElementFlag.Water: return ElementFlag.Fire | ElementFlag.Rock;
            case ElementFlag.Rock: return ElementFlag.Wind | ElementFlag.Fire;
            case ElementFlag.Wind: return ElementFlag.Fire;
            case ElementFlag.Electricity: return ElementFlag.Water;
            default: return ElementFlag.None;
        }
    }

    protected float GetMovementCurrentSpeed()
    {
        if ((debuffs & ElementFlag.Electricity) != 0)
        {
            float debuffFactor = (1.0f - 0.1f * stacks[ElementFlag.Electricity]) - (stackEffectivenessLevel * ElementDecorator.BASE_MULTIPLIER);
            return currentSpeed * Mathf.Max(debuffFactor, 0f);
        }

        return currentSpeed;
    }

    private int stackEffectivenessLevel;
    // todo, we need to make dev controller adhere the same as enemies or vice versa
    // the swarm needs to account for the fire debuff on each unit
    protected virtual void Update()
    {
        if (WaveController.paused) return;
        if (_lastTick < Time.time && stacks[ElementFlag.Fire] > 0)
        {
            if (this is Swarm) return;
            _lastTick = Time.time + ElementDecorator.DEBUFF_TICK;
            var fireDamage = ApplyDamage(baseDamage + (int) (stackEffectivenessLevel * ElementDecorator.BASE_MULTIPLIER), ElementFlag.Fire, transform.position, stacks[ElementFlag.Fire]);
            
            if (this is not DevController) currentHealth -= fireDamage;
            if (currentHealth <= 0)
            {
                var weak = WeaknessesFor(ElementFlag.Fire);
                var strong = StrengthsFor(ElementFlag.Fire);
                if ((weak & element) != 0) Die(StatusEffectiveness.Weak);
                else if ((strong & element) != 0) Die(StatusEffectiveness.Strong);
                else Die(StatusEffectiveness.Normal);
            }
        }
    }

    protected virtual void Die(StatusEffectiveness statusEffectiveness)
    {
    }

    private void RemoveDebuff(ElementFlag element)
    {
        if (stacks.ContainsKey(element) && stacks[element] > 0)
        {
            stacks[element]--;
            if (stacks[element] == 0)
            {
                debuffs &= ~element;
            }
        }
    }

    private void ApplyDebuff(ElementFlag element)
    {
        foreach (ElementFlag flag in Enum.GetValues(typeof(ElementFlag)))
        {
            if (flag == ElementFlag.None) continue;

            if ((element & flag) != 0 && stacks.ContainsKey(flag))
            {
                if (stacks[flag] < ElementDecorator.MAX_STACKS)
                {
                    stacks[flag]++;
                    // todo, refactor to actor level debuff
                    TimerManager.AddTimer(ElementDecorator.DEBUFF_DURATION, () => RemoveDebuff(flag));
                }
            }
        }
    }
}