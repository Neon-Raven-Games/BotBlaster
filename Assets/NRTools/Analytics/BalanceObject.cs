using System;
using System.Collections.Generic;
using Gameplay.Enemies;

namespace NRTools.Analytics
{
    public class BalanceObject
    {
        public int currentDamage;
        public int baseDamage;
        public int currentHealth;
        public int baseHealth;
        public float currentAttackRange;
        public float baseAttackRange;
        public float currentAttackCoolDown;
        public float baseAttackCoolDown;

        public BalanceObject(Actor actor)
        {
            currentDamage = actor.currentDamage;
            baseDamage = actor.baseDamage;
            currentHealth = actor.currentHealth;
            baseHealth = actor.baseHealth;
            currentAttackRange = actor.currentAttackRange;
            baseAttackRange = actor.baseAttackRange;
            currentAttackCoolDown = actor.currentAttackCoolDown;
            baseAttackCoolDown = actor.baseAttackCoolDown;
        }
    }

    public class EnemyBalanceObject : BalanceObject
    {
        public EnemyType enemyType;
        public List<ElementFlag> Elements = new();
        public int count;
        public EnemyBalanceObject(Enemy enemy) : base(enemy)
        {
            count = 1;
            enemyType = enemy.enemyType;
        }
    }
}