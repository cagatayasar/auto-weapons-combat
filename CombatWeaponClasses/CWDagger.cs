﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CWDagger : CombatWeapon, ICWCancelTransition
{
    StatsDagger stats;
    int damageFixed;
    int damageMin;
    int damageMax;

    float attackTriggerTime;
    float damageTriggerTime;

    public CWDagger(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, CombatMode combatMode, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, combatMode, ref rnd)
    {
        stats = DataManager.inst.weaponsPackage.dagger;
        statsGeneral = stats.statsGeneral;
        UpdateLevelBasedStats();
        _30DegreesRotationDuration = stats._30DegreesRotationDuration;
        ApplyExistingPermanentStatusEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        OnAnimatorSetFloat("attackSpeed", "dagger_anim_attack", 1f / (actionTimePeriod * stats.animationNonidlePortionMin));
    }

    public override void UpdateLevelBasedStats()
    {
        base.UpdateLevelBasedStats();
        if (weapon.combatLevel == 1) {
            damageFixed = statsGeneral.damage1Fixed;
            damageMin = statsGeneral.damage1Min;
            damageMax = statsGeneral.damage1Max;
        } else if (weapon.combatLevel == 2) {
            damageFixed = statsGeneral.damage2Fixed;
            damageMin = statsGeneral.damage2Min;
            damageMax = statsGeneral.damage2Max;
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

    public override void UpdateAnimator()
    {
        if (prevSeActionSpeedMultiplier == seActionSpeedMultiplier) return;
        prevSeActionSpeedMultiplier = seActionSpeedMultiplier;

        float actionSpeed = seActionSpeedMultiplier / actionTimePeriod;
        float animationAttackPortion;
        if (actionSpeed < stats.actionSpeedForNonidleMin)
            animationAttackPortion = stats.animationNonidlePortionMin;
        else if (actionSpeed > stats.actionSpeedForNonidleMax)
            animationAttackPortion = stats.animationNonidlePortionMax;
        else {
            float actionSpeedForNonidleNormalized = (actionSpeed - stats.actionSpeedForNonidleMin) / (stats.actionSpeedForNonidleMax - stats.actionSpeedForNonidleMin);
            float actionSpeedForNonidleMapped = actionSpeedForNonidleNormalized * (stats.animationNonidlePortionMax - stats.animationNonidlePortionMin);
            animationAttackPortion = stats.animationNonidlePortionMin + actionSpeedForNonidleMapped;
        }
        attackTriggerTime = actionTimePeriod * (1f - animationAttackPortion);
        damageTriggerTime = attackTriggerTime + (actionTimePeriod - attackTriggerTime) * stats.animationDamageEnemyPortion;

        OnAnimatorSetFloat("attackSpeed", "dagger_anim_attack", 1f / ((actionTimePeriod / seActionSpeedMultiplier) * animationAttackPortion));
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
