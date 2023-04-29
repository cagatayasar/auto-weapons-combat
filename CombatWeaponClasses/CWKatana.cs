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

public class CWKatana : CW, ICWStackWeapon
{
    public new WInfoKatana weaponInfo => base.weaponInfo as WInfoKatana;

    public KatanaState katanaState = KatanaState.IdleFirst;

    public float attackTimer;
    public float stackProgress;

    public event Action onUpdateStackUI;

    public CWKatana(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, ref rnd)
    {
        UpdateLevelBasedStats();
        _30DegreesRotationDuration = weaponInfo._30DegreesRotationDuration;
        ApplyExistingPermanentStatusEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        OnAnimatorSetFloat("attackSpeedFirst", "katana_anim_firstattack", 1f / weaponInfo.firstAttackLength);
        OnAnimatorSetFloat("attackSpeedUpward", "katana_anim_upwardattack", 1f / weaponInfo.upwardAttackLength);
        OnAnimatorSetFloat("attackSpeedDownward", "katana_anim_downwardattack", 1f / weaponInfo.downwardAttackLength);
    }

    public override void UpdateLevelBasedStats()
    {
        base.UpdateLevelBasedStats();
        if (weapon.combatLevel == 1) {
            maxStacks = weaponInfo.maxStackAmount1;
        } else if (weapon.combatLevel == 2) {
            maxStacks = weaponInfo.maxStackAmount2;
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
        else if (katanaState == KatanaState.AttackingFirstPhase1 && attackTimer >= weaponInfo.firstAttackLength * weaponInfo.firstAttackDamageEnemyPortion)
        {
            katanaState = KatanaState.AttackingFirstPhase2;
            AttackIfAble();
        }
        else if (katanaState == KatanaState.AttackingFirstPhase2 && attackTimer >= weaponInfo.firstAttackLength)
        {
            attackTimer = 0f;
            katanaState = KatanaState.IdleAfterUpward;
        }
        else if (katanaState == KatanaState.IdleAfterUpward && targetEnemy != null
            && stacks > 0 && attackTimer >= weaponInfo.waitTimeBetweenAttacks)
        {
            attackTimer = 0f;
            katanaState = KatanaState.AttackingDownwardPhase1;
            OnAnimatorSetTrigger("attackDownward");
        }
        else if (katanaState == KatanaState.AttackingDownwardPhase1 && attackTimer >= weaponInfo.downwardAttackLength * weaponInfo.downwardAttackDamageEnemyPortion) 
        {
            katanaState = KatanaState.AttackingDownwardPhase2;
            AttackIfAble();
        }
        else if (katanaState == KatanaState.AttackingDownwardPhase2 && attackTimer >= weaponInfo.downwardAttackLength)
        {
            attackTimer = 0f;
            katanaState = KatanaState.IdleAfterDownward;
        }
        else if (katanaState == KatanaState.IdleAfterDownward && targetEnemy != null
            && stacks > 0 && attackTimer >= weaponInfo.waitTimeBetweenAttacks)
        {
            attackTimer = 0f;
            katanaState = KatanaState.AttackingUpwardPhase1;
            OnAnimatorSetTrigger("attackUpward");
        }
        else if (katanaState == KatanaState.AttackingUpwardPhase1 && attackTimer >= weaponInfo.upwardAttackLength * weaponInfo.upwardAttackDamageEnemyPortion)
        {
            katanaState = KatanaState.AttackingUpwardPhase2;
            AttackIfAble();
        }
        else if (katanaState == KatanaState.AttackingUpwardPhase2 &&
                 attackTimer >= weaponInfo.upwardAttackLength)
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
        if (CombatMain.isRandomized) {
            return CombatFunctions.GetCombatAction(rnd, statusEffects, this, allyCWs, allyRowsList, damageMin, damageMax);
        } else {
            return CombatFunctions.GetCombatAction(rnd, statusEffects, this, allyCWs, allyRowsList, damageFixed, damageFixed);
        }
    }

    public override void ReceiveAction(CombatAction action)
    {
        base.ReceiveAction(action);
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
