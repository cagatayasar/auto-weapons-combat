using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoWeapons {

public class CWSword : CW, ICWCancelTransition
{
    public new WInfoSword weaponInfo => base.weaponInfo as WInfoSword;

    MeleeTargetType targetType = MeleeTargetType.Null;


    // animation variables
    float animationUpwardAttackClipLength;
    float animationFrontAttackClipLength;

    float attackTriggerTime;
    float damageUpwardTriggerTime;
    float damageFrontTriggerTime;

    public CWSword(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, ref rnd)
    {
        UpdateLevelBasedStats();
        ApplyExistingPermanentEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        onAnimatorSetFloat?.Invoke("attackSpeedUpward", "sword_anim_upwardattack", 1f / (actionTimePeriod * weaponInfo.animNonidlePortionMin));
        onAnimatorSetFloat?.Invoke("attackSpeedFront", "sword_anim_frontattack", 1f / (actionTimePeriod * weaponInfo.animNonidlePortionMin));
    }

    public override void UpdateLevelBasedStats()
    {
        base.UpdateLevelBasedStats();
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (!isDead)
        {
            timePassed += deltaTime;
            actionTimePassed += deltaTime * effectSpeedMultiplier;

            UpdateTarget();
            UpdateAnimator();
            if (meleeState == MeleeState.Canceling)
                UpdateTransition(deltaTime);
            ActIfReady();

            onUpdateHealthBar?.Invoke(deltaTime);
        }
    }

    public override void UpdateTarget()
    {
        prevTargetEnemy = targetEnemy;
        targetEnemy = CombatFunctions.TargetEnemyMelee(this, targetEnemy, targetRowsList, false);
    }

    public override void UpdateAnimator()
    {
        if (prevEffectSpeedMultiplier == effectSpeedMultiplier) return;
        prevEffectSpeedMultiplier = effectSpeedMultiplier;

        float actionSpeed = effectSpeedMultiplier / actionTimePeriod;
        float animationAttackPortion;
        if (actionSpeed < weaponInfo.actionSpeedForAnimationMin)
            animationAttackPortion = weaponInfo.animNonidlePortionMin;
        else if (actionSpeed > weaponInfo.actionSpeedForAnimationMax)
            animationAttackPortion = weaponInfo.animNonidlePortionMax;
        else {
            float actionSpeedForNonidleNormalized = (actionSpeed - weaponInfo.actionSpeedForAnimationMin) / (weaponInfo.actionSpeedForAnimationMax - weaponInfo.actionSpeedForAnimationMin);
            float actionSpeedForNonidleMapped = actionSpeedForNonidleNormalized * (weaponInfo.animNonidlePortionMax - weaponInfo.animNonidlePortionMin);
            animationAttackPortion = weaponInfo.animNonidlePortionMin + actionSpeedForNonidleMapped;
        }
        attackTriggerTime = actionTimePeriod * (1f - animationAttackPortion);
        damageUpwardTriggerTime = attackTriggerTime + (actionTimePeriod - attackTriggerTime) * weaponInfo.animationUpwardDamageEnemyPortion;
        damageFrontTriggerTime = attackTriggerTime + (actionTimePeriod - attackTriggerTime) * weaponInfo.animationFrontDamageEnemyPortion;

        onAnimatorSetFloat?.Invoke("attackSpeedUpward", "sword_anim_upwardattack", 1f / ((actionTimePeriod / effectSpeedMultiplier) * animationAttackPortion));
        onAnimatorSetFloat?.Invoke("attackSpeedFront", "sword_anim_frontattack", 1f / ((actionTimePeriod / effectSpeedMultiplier) * animationAttackPortion));
    }

    public override void ActIfReady()
    {
        if (meleeState == MeleeState.Attacking
            && (targetEnemy == null || targetEnemy != prevTargetEnemy
                || (targetType == MeleeTargetType.Upward && targetEnemy.verticalPosition != verticalPosition + 1)
                || (targetType == MeleeTargetType.Front  && targetEnemy.verticalPosition != verticalPosition)))
        {
            meleeState = MeleeState.Canceling;
            CancelTransition();
            return;
        }

        if (meleeState == MeleeState.Idle && actionTimePassed >= attackTriggerTime && targetEnemy != null)
        {
            actionTimePassed = attackTriggerTime;
            meleeState = MeleeState.Attacking;
            if (targetEnemy.verticalPosition > verticalPosition) {
                targetType = MeleeTargetType.Upward;
                onAnimatorSetTrigger?.Invoke("attackUpward");
            } else {
                targetType = MeleeTargetType.Front;
                onAnimatorSetTrigger?.Invoke("attackFront");
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
        if (CombatMain.isRandomized) {
            return CombatFunctions.GetCombatAction(rnd, effects, this, allyCWs, allyRowsList, damageMin, damageMax);
        } else {
            return CombatFunctions.GetCombatAction(rnd, effects, this, allyCWs, allyRowsList, damageFixed, damageFixed);
        }
    }

    public override void ReceiveAction(CombatAction action)
    {
        base.ReceiveAction(action);
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
}
