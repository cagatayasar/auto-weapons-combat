using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CWSword : CombatWeapon, ICWCancelTransition
{
    MeleeTargetType targetType = MeleeTargetType.Null;

    StatsSword stats;
    int damageFixed;
    int damageMin;
    int damageMax;

    // animation variables
    float animationUpwardAttackClipLength;
    float animationFrontAttackClipLength;

    float attackTriggerTime;
    float damageUpwardTriggerTime;
    float damageFrontTriggerTime;

    public CWSword(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, CombatMode combatMode, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, combatMode, ref rnd)
    {
        stats = DataManager.inst.weaponsPackage.sword;
        statsGeneral = stats.statsGeneral;
        UpdateLevelBasedStats();
        ApplyExistingPermanentStatusEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        OnAnimatorSetFloat("attackSpeedUpward", "sword_anim_upwardattack", 1f / (actionTimePeriod * stats.animationNonidlePortionMin));
        OnAnimatorSetFloat("attackSpeedFront", "sword_anim_frontattack", 1f / (actionTimePeriod * stats.animationNonidlePortionMin));
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
            ActIfReady();

            OnUpdateHealthBar(deltaTime);
        }
    }

    public override void UpdateTarget()
    {
        prevTargetEnemy = targetEnemy;
        targetEnemy = CombatFunctions.TargetEnemyMelee(this, targetEnemy, targetRowsList, false);
    }

    public override void UpdateAnimator()
    {
        if (prevSeActionSpeedMultiplier == seActionSpeedMultiplier) return;
        prevSeActionSpeedMultiplier = seActionSpeedMultiplier;

        float actionSpeed = seActionSpeedMultiplier / actionTimePeriod;
        float animationAttackPortion;
        if (actionSpeed < stats.actionSpeedForAnimationMin)
            animationAttackPortion = stats.animationNonidlePortionMin;
        else if (actionSpeed > stats.actionSpeedForAnimationMax)
            animationAttackPortion = stats.animationNonidlePortionMax;
        else {
            float actionSpeedForNonidleNormalized = (actionSpeed - stats.actionSpeedForAnimationMin) / (stats.actionSpeedForAnimationMax - stats.actionSpeedForAnimationMin);
            float actionSpeedForNonidleMapped = actionSpeedForNonidleNormalized * (stats.animationNonidlePortionMax - stats.animationNonidlePortionMin);
            animationAttackPortion = stats.animationNonidlePortionMin + actionSpeedForNonidleMapped;
        }
        attackTriggerTime = actionTimePeriod * (1f - animationAttackPortion);
        damageUpwardTriggerTime = attackTriggerTime + (actionTimePeriod - attackTriggerTime) * stats.animationUpwardDamageEnemyPortion;
        damageFrontTriggerTime = attackTriggerTime + (actionTimePeriod - attackTriggerTime) * stats.animationFrontDamageEnemyPortion;

        OnAnimatorSetFloat("attackSpeedUpward", "sword_anim_upwardattack", 1f / ((actionTimePeriod / seActionSpeedMultiplier) * animationAttackPortion));
        OnAnimatorSetFloat("attackSpeedFront", "sword_anim_frontattack", 1f / ((actionTimePeriod / seActionSpeedMultiplier) * animationAttackPortion));
    }

    public override void ActIfReady()
    {
        if (meleeState == MeleeState.Attacking
            && (targetEnemy == null || targetEnemy != prevTargetEnemy
                || (targetType == MeleeTargetType.Upward && targetEnemy.positionFromBottom != positionFromBottom + 1)
                || (targetType == MeleeTargetType.Front  && targetEnemy.positionFromBottom != positionFromBottom)))
        {
            meleeState = MeleeState.Canceling;
            CancelTransition();
            return;
        }

        if (meleeState == MeleeState.Idle && actionTimePassed >= attackTriggerTime && targetEnemy != null)
        {
            actionTimePassed = attackTriggerTime;
            meleeState = MeleeState.Attacking;
            if (targetEnemy.positionFromBottom > positionFromBottom) {
                targetType = MeleeTargetType.Upward;
                OnAnimatorSetTrigger("attackUpward");
            } else {
                targetType = MeleeTargetType.Front;
                OnAnimatorSetTrigger("attackFront");
            }
        }
        else if (meleeState == MeleeState.Attacking && targetType == MeleeTargetType.Upward
            && actionTimePassed >= damageUpwardTriggerTime)
        {
            actionTimePassed = damageUpwardTriggerTime;
            meleeState = MeleeState.Returning;
            CombatFunctions.ApplyActionToTarget(targetEnemy, this, GetCombatAction());
        }
        else if (meleeState == MeleeState.Attacking && targetType == MeleeTargetType.Front
            && actionTimePassed >= damageFrontTriggerTime)
        {
            actionTimePassed = damageFrontTriggerTime;
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
