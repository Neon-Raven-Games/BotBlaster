using System;
using System.Collections.Generic;
using Gameplay.Enemies;
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

    protected int currentHealth;
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

    private static void ShowDamageNumber(int damage, ElementFlag damageElementType, bool isWeak, bool isStrong, Vector3 position)
    {
        var status = StatusEffectiveness.Normal;
        if (isWeak) status = StatusEffectiveness.Weak;
        if (isStrong) status = StatusEffectiveness.Strong;
        DamageNumberPool.SetElementDamageNumber(damageElementType, position, status, damage);
    }

    public int ApplyDamage(int damage, ElementFlag hitElement, Vector3 position, int elementLevel = 1)
    {
        // our character has element, and this is passing in the hit element.
        // we want to check if our element is strong or weak against the hit element
        damage = ApplyElementalDamage(damage, element, hitElement, elementLevel, ElementFlag.Rock, position);
        damage = ApplyElementalDamage(damage, element, hitElement, elementLevel, ElementFlag.Water, position);
        damage = ApplyElementalDamage(damage, element, hitElement, elementLevel, ElementFlag.Fire, position);
        damage = ApplyElementalDamage(damage, element, hitElement, elementLevel, ElementFlag.Wind, position);
        damage = ApplyElementalDamage(damage, element, hitElement, elementLevel, ElementFlag.Electricity, position);
        
        // remove elements without status effects
        hitElement &= ~ElementFlag.Rock;
        hitElement &= ~ElementFlag.Wind;

        // Apply other debuff stacks (Fire, Wind, Electric)
        ApplyDebuff(hitElement);

        return damage;
    }

    private int ApplyElementalDamage(int damage, ElementFlag characterElement, ElementFlag hitElement, int elementLevel,
        ElementFlag targetElement, Vector3 position)
    {
        if (characterElement == ElementFlag.None || (characterElement & targetElement) == 0)
        {
            if (showDamageNumbers)
                ShowDamageNumber(damage, targetElement, false, false, position);
            return damage;
        }

        var multiplier = ElementDecorator.BASE_MULTIPLIER * elementLevel;
        var isWeak = false;
        var isStrong = false;

        // Check if the hit element is strong or weak against the target element
        if ((hitElement & WeaknessesFor(targetElement)) != 0)
        {
            multiplier *= ElementDecorator.WEAKNESS_MULTIPLIER;
            isWeak = true;
        }

        if ((hitElement & StrengthsFor(targetElement)) != 0)
        {
            multiplier *= ElementDecorator.STRENGTH_MULTIPLIER;
            isStrong = true;
        }

        var finalDamage = damage + (int) (damage * multiplier * GetStackMultiplier(targetElement));
        if (showDamageNumbers)
            ShowDamageNumber(finalDamage, targetElement, isWeak, isStrong, position);

        return finalDamage;
    }

    private float GetStackMultiplier(ElementFlag debuffElement)
    {
        return stacks.ContainsKey(debuffElement) ? stacks[debuffElement] : 1f;
    }

    protected ElementFlag WeaknessesFor(ElementFlag current)
    {
        return current switch
        {
            ElementFlag.Rock => (ElementFlag) Weakness.Rock,
            ElementFlag.Water => (ElementFlag) Weakness.Water,
            ElementFlag.Fire => (ElementFlag) Weakness.Fire,
            ElementFlag.Wind => (ElementFlag) Weakness.Wind,
            ElementFlag.Electricity => (ElementFlag) Weakness.Electricity,
            _ => ElementFlag.None
        };
    }

    protected ElementFlag StrengthsFor(ElementFlag current)
    {
        return current switch
        {
            ElementFlag.Rock => (ElementFlag) Strength.Rock,
            ElementFlag.Water => (ElementFlag) Strength.Water,
            ElementFlag.Fire => (ElementFlag) Strength.Fire,
            ElementFlag.Wind => (ElementFlag) Strength.Wind,
            ElementFlag.Electricity => (ElementFlag) Strength.Electricity,
            _ => ElementFlag.None
        };
    }

    protected float GetMovementCurrentSpeed()
    {
        if ((debuffs & ElementFlag.Electricity) != 0)
        {
            float debuffFactor = 1.0f - 0.1f * stacks[ElementFlag.Electricity];
            return currentSpeed * Mathf.Max(debuffFactor, 0f);
        }

        return currentSpeed;
    }

    protected virtual void Update()
    {
        if (_lastTick < Time.time && stacks[ElementFlag.Fire] > 0)
        {
            _lastTick = Time.time + ElementDecorator.DEBUFF_TICK;
            var fireDamage = ApplyElementalDamage(baseDamage, element, ElementFlag.Fire, stacks[ElementFlag.Fire],
                ElementFlag.Fire, transform.position);
            currentHealth -= fireDamage;
            
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
            // Skip the None flag
            if (flag == ElementFlag.None) continue;

            // Check if the current element has the flag and if it's a valid debuff
            if ((element & flag) != 0 && stacks.ContainsKey(flag))
            {
                if (stacks[flag] < ElementDecorator.MAX_STACKS)
                {
                    stacks[flag]++;
                    TimerManager.AddTimer(ElementDecorator.DEBUFF_DURATION, () => RemoveDebuff(flag));
                }
            }
        }
    }
}