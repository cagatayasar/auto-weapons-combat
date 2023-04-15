using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CWScimitar : CW, ICWCancelTransition, ICWStackWeapon
{
    public new WInfoScimitar weaponInfo => base.weaponInfo as WInfoScimitar;
    public int damageFixed;
    public int damageMin;
    public int damageMax;

    public int damagePerStack;

    public float attackTriggerTime;
    public float damageTriggerTime;

    public event Action onUpdateStackUI;

    public CWScimitar(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, ref rnd)
    {
        UpdateLevelBasedStats();
        _30DegreesRotationDuration = weaponInfo._30DegreesRotationDuration;
        ApplyExistingPermanentStatusEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        OnAnimatorSetFloat("attackSpeed", "scimitar_anim_attack", 1f / (actionTimePeriod * weaponInfo.animationNonidlePortionMin));
    }

    public override void UpdateLevelBasedStats()
    {
        base.UpdateLevelBasedStats();
        if (weapon.combatLevel == 1) {
            damageFixed = base.weaponInfo.damageFixed1;
            damageMin = base.weaponInfo.damageMin1;
            damageMax = base.weaponInfo.damageMax1;
            maxStacks = weaponInfo.maxStackAmount1;
            damagePerStack = weaponInfo.damagePerStack1;
        } else if (weapon.combatLevel == 2) {
            damageFixed = base.weaponInfo.damageFixed2;
            damageMin = base.weaponInfo.damageMin2;
            damageMax = base.weaponInfo.damageMax2;
            maxStacks = weaponInfo.maxStackAmount2;
            damagePerStack = weaponInfo.damagePerStack2;
        }
    }

    public override void Update(float deltaTime)
    {
        seActionSpeedMultiplier = CombatFunctions.HandleStatusEffects(this, deltaTime);
        if (!isDead)
        {
            timePassed += deltaTime;
            actionTimePassed += deltaTime * seActionSpeedMultiplier;

            UpdateTarget();
            UpdateAnimator();
            if (meleeState == MeleeState.Canceling)
                UpdateTransition(deltaTime);
            if (isRotating)
                UpdateRotating(deltaTime);
            RotateIfNeeded(targetEnemy);
            ActIfReady();

            OnUpdateHealthBar(deltaTime);
        }
    }

    public override void UpdateTarget()
    {
        prevTargetEnemy = targetEnemy;
        targetEnemy = CombatFunctions.TargetEnemyMelee(this, targetEnemy, targetRowsList);
    }

    public void IncrementStack()
    {
        if (stacks < maxStacks) {
            stacks++;
            onUpdateStackUI?.Invoke();
        }
    }

    public override void UpdateAnimator()
    {
        if (prevSeActionSpeedMultiplier == seActionSpeedMultiplier) return;
        prevSeActionSpeedMultiplier = seActionSpeedMultiplier;

        float actionSpeed = seActionSpeedMultiplier / actionTimePeriod;
        float animationAttackPortion;
        if (actionSpeed < weaponInfo.actionSpeedForNonidleMin)
            animationAttackPortion = weaponInfo.animationNonidlePortionMin;
        else if (actionSpeed > weaponInfo.actionSpeedForNonidleMax)
            animationAttackPortion = weaponInfo.animationNonidlePortionMax;
        else {
            float actionSpeedForNonidleNormalized = (actionSpeed - weaponInfo.actionSpeedForNonidleMin) / (weaponInfo.actionSpeedForNonidleMax - weaponInfo.actionSpeedForNonidleMin);
            float actionSpeedForNonidleMapped = actionSpeedForNonidleNormalized * (weaponInfo.animationNonidlePortionMax - weaponInfo.animationNonidlePortionMin);
            animationAttackPortion = weaponInfo.animationNonidlePortionMin + actionSpeedForNonidleMapped;
        }
        attackTriggerTime = actionTimePeriod * (1f - animationAttackPortion);
        damageTriggerTime = attackTriggerTime + (actionTimePeriod - attackTriggerTime) * weaponInfo.animationDamageEnemyPortion;

        OnAnimatorSetFloat("attackSpeed", "scimitar_anim_attack", 1f / ((actionTimePeriod / seActionSpeedMultiplier) * animationAttackPortion));
    }

    public override void ActIfReady()
    {
        if (meleeState == MeleeState.Attacking
            && (targetEnemy == null || targetEnemy != prevTargetEnemy))
        {
            meleeState = MeleeState.Canceling;
            CancelTransition();
            return;
        }

        if (!isRotating && meleeState == MeleeState.Idle && actionTimePassed >= attackTriggerTime && targetEnemy != null)
        {
            actionTimePassed = attackTriggerTime;
            meleeState = MeleeState.Attacking;
            OnAnimatorSetTrigger("attack");
        }
        else if (meleeState == MeleeState.Attacking && actionTimePassed >= damageTriggerTime)
        {
            actionTimePassed = damageTriggerTime;
            meleeState = MeleeState.Returning;
            CombatFunctions.ApplyActionToTarget(targetEnemy, this, GetCombatAction());
            IncrementStack();
        }
        else if (meleeState == MeleeState.Returning && actionTimePassed >= actionTimePeriod)
        {
            meleeState = MeleeState.Idle;
            actionTimePassed = 0f;
        }
    }

    public void OnCancelTransitionEnd()
    {
        meleeState = MeleeState.Idle;
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
            return CombatFunctions.GetCombatAction(rnd, statusEffects, this, allyCWs, allyRowsList,
                damageMin + damagePerStack * stacks, damageMax + damagePerStack * stacks);
        } else {
            return CombatFunctions.GetCombatAction(rnd, statusEffects, this, allyCWs, allyRowsList,
                damageFixed + damagePerStack * stacks, damageFixed + damagePerStack * stacks);
        }
    }

    public override void ReceiveAction(CombatAction action)
    {
        base.ReceiveAction(action);
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
