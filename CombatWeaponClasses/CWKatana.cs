using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum KatanaState
{
    IdleFirst,
    AttackingFirstPhase1, // Attacking, has not damaged the enemy yet
    AttackingFirstPhase2, // Starts after damaging the enemy
    AttackingDownwardPhase1,
    AttackingDownwardPhase2,
    AttackingUpwardPhase1,
    AttackingUpwardPhase2,
    IdleAfterUpward,
    IdleAfterDownward,
    Null
}

public class CWKatana : CombatWeapon, ICWStackWeapon
{
    public KatanaState katanaState = KatanaState.IdleFirst;

    public StatsKatana stats;
    public int damageFixed;
    public int damageMin;
    public int damageMax;

    public float attackTimer;
    public float stackProgress;

    public event Action onUpdateStackUI;

    public CWKatana(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, CombatMode combatMode, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, combatMode, ref rnd)
    {
        stats = DataManager.inst.weaponsPackage.katana;
        statsGeneral = stats.statsGeneral;
        UpdateLevelBasedStats();
        _30DegreesRotationDuration = stats._30DegreesRotationDuration;
        ApplyExistingPermanentStatusEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        OnAnimatorSetFloat("attackSpeedFirst", "katana_anim_firstattack", 1f / stats.firstAttackLength);
        OnAnimatorSetFloat("attackSpeedUpward", "katana_anim_upwardattack", 1f / stats.upwardAttackLength);
        OnAnimatorSetFloat("attackSpeedDownward", "katana_anim_downwardattack", 1f / stats.downwardAttackLength);
    }

    public override void UpdateLevelBasedStats()
    {
        base.UpdateLevelBasedStats();
        if (weapon.combatLevel == 1) {
            damageFixed = statsGeneral.damage1Fixed;
            damageMin = statsGeneral.damage1Min;
            damageMax = statsGeneral.damage1Max;
            maxStacks = stats.maxStackAmount1;
        } else if (weapon.combatLevel == 2) {
            damageFixed = statsGeneral.damage2Fixed;
            damageMin = statsGeneral.damage2Min;
            damageMax = statsGeneral.damage2Max;
            maxStacks = stats.maxStackAmount2;
        }
    }

    public override void Update(float deltaTime)
    {
        seActionSpeedMultiplier = CombatFunctions.HandleStatusEffects(this, deltaTime);
        if (!isDead)
        {
            timePassed += deltaTime;
            actionTimePassed += deltaTime * seActionSpeedMultiplier;
            attackTimer += deltaTime * seActionSpeedMultiplier;

            UpdateTarget();
            UpdateStacks(deltaTime);
            if (isRotating)
                UpdateRotating(deltaTime);
            RotateIfNeeded(targetEnemy);
            ActIfReady();

            OnUpdateHealthBar(deltaTime);
        }
    }

    public override void UpdateTarget()
    {
        targetEnemy = CombatFunctions.TargetEnemyMelee(this, targetEnemy, targetRowsList);
    }

    public override void UpdateAnimator(){}

    public void IncrementStack()
    {
        if (stacks < maxStacks) {
            stacks++;
            UpdateStacks(0f);
        }
    }

    public void UpdateStacks(float deltaTime)
    {
        if (stacks >= maxStacks) return;
        stackProgress += deltaTime * seActionSpeedMultiplier / actionTimePeriod;
        if (stackProgress >= 1f) {
            stacks++;
            stackProgress -= 1f;
        }

        onUpdateStackUI?.Invoke();
    }

    public override void ActIfReady()
    {
        // todo2
        // if it is IdleFirst & target != null & if stack available:
        //     use the stack, remove it from stacks, attack the target enemy with attackFirst, start the attackTimer
        // else if it is AttackingFirst & attack timer >= firstAttackLength
        // else if it is AttackingFirst & attack timer >= firstAttackLength + waitLengthBetweenAttacks & ...
        // target != null & stack available:
        //     use the stack, remove it from stacks, attack the target enemy with attackDownward, start the attackTimer
        // else if AttackingDownward.....
        // else if AttackingUpward.....
        if (katanaState == KatanaState.IdleFirst && targetEnemy != null && stacks > 0)
        {
            attackTimer = 0f;
            katanaState = KatanaState.AttackingFirstPhase1;
            OnAnimatorSetTrigger("attackFirst");
        }
        else if (katanaState == KatanaState.AttackingFirstPhase1 && attackTimer >= stats.firstAttackLength * stats.firstAttackDamageEnemyPortion)
        {
            katanaState = KatanaState.AttackingFirstPhase2;
            AttackIfAble();
        }
        else if (katanaState == KatanaState.AttackingFirstPhase2 && attackTimer >= stats.firstAttackLength)
        {
            attackTimer = 0f;
            katanaState = KatanaState.IdleAfterUpward;
        }
        else if (katanaState == KatanaState.IdleAfterUpward && targetEnemy != null
            && stacks > 0 && attackTimer >= stats.waitTimeBetweenAttacks)
        {
            attackTimer = 0f;
            katanaState = KatanaState.AttackingDownwardPhase1;
            OnAnimatorSetTrigger("attackDownward");
        }
        else if (katanaState == KatanaState.AttackingDownwardPhase1 && attackTimer >= stats.downwardAttackLength * stats.downwardAttackDamageEnemyPortion) 
        {
            katanaState = KatanaState.AttackingDownwardPhase2;
            AttackIfAble();
        }
        else if (katanaState == KatanaState.AttackingDownwardPhase2 && attackTimer >= stats.downwardAttackLength)
        {
            attackTimer = 0f;
            katanaState = KatanaState.IdleAfterDownward;
        }
        else if (katanaState == KatanaState.IdleAfterDownward && targetEnemy != null
            && stacks > 0 && attackTimer >= stats.waitTimeBetweenAttacks)
        {
            attackTimer = 0f;
            katanaState = KatanaState.AttackingUpwardPhase1;
            OnAnimatorSetTrigger("attackUpward");
        }
        else if (katanaState == KatanaState.AttackingUpwardPhase1 && attackTimer >= stats.upwardAttackLength * stats.upwardAttackDamageEnemyPortion)
        {
            katanaState = KatanaState.AttackingUpwardPhase2;
            AttackIfAble();
        }
        else if (katanaState == KatanaState.AttackingUpwardPhase2 &&
                 attackTimer >= stats.upwardAttackLength)
        {
            attackTimer = 0f;
            katanaState = KatanaState.IdleAfterUpward;
        }
    }

    public void AttackIfAble()
    {
        if (targetEnemy != null && stacks > 0)
        {
            actionTimePassed = 0f;
            stacks--;
            CombatFunctions.ApplyActionToTarget(targetEnemy, this, GetCombatAction());
        }
    }

    public override void PrepareCombatStart(int roundCount)
    {
        PrepareCombatStartHP();
        PrepareCombatStartOther(roundCount);
        PrepareCombatStartSpeed();
    }

    public override CombatAction GetCombatAction()
    {
        if (DataManager.inst.isRandomized) {
            return CombatFunctions.GetCombatAction(rnd, statusEffects, this, allyCombatWeapons, allyRowsList, damageMin, damageMax);
        } else {
            return CombatFunctions.GetCombatAction(rnd, statusEffects, this, allyCombatWeapons, allyRowsList, damageFixed, damageFixed);
        }
    }

    public override void ReceiveAction(CombatAction action)
    {
        base.ReceiveAction(action);
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
