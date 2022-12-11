﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CWRagingBow : CombatWeapon, ICWStackWeapon
{
    public new WInfoRagingBow weaponInfo => base.weaponInfo as WInfoRagingBow;
    int damageFixed;
    int damageMin;
    int damageMax;
    int actionTimePeriodBuffPercent;

    // animation variables
    float animationDrawClipLength;
    float animationReleaseClipLength;

    float drawTriggerTime;
    float waitForReleaseTriggerTime;
    float releaseTriggerTime;

    float projectileSpeed;

    public event Action onUpdateStackUI;
    public event Action<POneTarget> onReleaseProjectile;
    public event Action<POneTarget> onDestroyProjectile;
    public event Action<POneTarget, float> onUpdateProjectile;

    public CWRagingBow(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, CombatMode combatMode, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, combatMode, ref rnd)
    {
        UpdateLevelBasedStats();
        projectileSpeed = weaponInfo.projectileSpeed * CombatMain.combatAreaScale;
        _30DegreesRotationDuration = weaponInfo._30DegreesRotationDuration;
        ApplyExistingPermanentStatusEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        OnAnimatorSetFloat("drawSpeed", "ragingbow_anim_draw", 1f / (actionTimePeriod * weaponInfo.animationNonidlePortionMin * weaponInfo.animationAttackDrawPortion));
        OnAnimatorSetFloat("releaseSpeed", "ragingbow_anim_release", 1f / weaponInfo.animationAttackReleaseTime);
    }

    public override void UpdateLevelBasedStats()
    {
        base.UpdateLevelBasedStats();
        if (weapon.combatLevel == 1) {
            damageFixed = base.weaponInfo.damage1Fixed;
            damageMin = base.weaponInfo.damage1Min;
            damageMax = base.weaponInfo.damage1Max;
            actionTimePeriodBuffPercent = weaponInfo.speedBuffPercent1;
            range = weaponInfo.range1;
        } else if (weapon.combatLevel == 2) {
            damageFixed = base.weaponInfo.damage2Fixed;
            damageMin = base.weaponInfo.damage2Min;
            damageMax = base.weaponInfo.damage2Max;
            actionTimePeriodBuffPercent = weaponInfo.speedBuffPercent2;
            range = weaponInfo.range2;
        }
    }

    public override void Update(float deltaTime)
    {
        seActionSpeedMultiplier = CombatFunctions.HandleStatusEffects(this, deltaTime);
        if (!isDead)
        {
            timePassed += deltaTime;
            actionTimePassed += deltaTime * (seActionSpeedMultiplier + stacks * actionTimePeriodBuffPercent / 100f);

            UpdateTarget();
            UpdateAnimator();
            if (isRotating)
                UpdateRotating(deltaTime);
            RotateIfNeeded(targetEnemy);
            ActIfReady();

            OnUpdateHealthBar(deltaTime);
            UpdateProjectiles(deltaTime);
        }
    }

    public override void UpdateTarget()
    {
        targetEnemy = CombatFunctions.TargetEnemyRanged(this, range, targetEnemy, targetRowsList, timePassed, oneLessRangeForXSeconds);
    }

    public override void UpdateProjectiles(float deltaTime)
    {
        for (int i = 0; i < pOneTargets.Count; i++)
        {
            var projectile = pOneTargets[i];
            if (pOneTargets[i].target == null || pOneTargets[i].target.isDead)
            {
                onDestroyProjectile?.Invoke(projectile);
                pOneTargets.RemoveAt(i);
                i--;
                continue;
            }

            if (projectile.currentTravelTime + deltaTime >= projectile.totalTravelTime)
            {
                CombatFunctions.ApplyActionToTarget(projectile.target, this, projectile.combatAction);
                onDestroyProjectile?.Invoke(projectile);
                pOneTargets.RemoveAt(i);
                i--;
                continue;
            }

            onUpdateProjectile?.Invoke(projectile, deltaTime);
            projectile.currentTravelTime += deltaTime;
        }
    }

    public override void UpdateAnimator()
    {
        float actionSpeedMultiplier = (seActionSpeedMultiplier + stacks * actionTimePeriodBuffPercent / 100f);
        if (prevSeActionSpeedMultiplier == actionSpeedMultiplier) return;
        prevSeActionSpeedMultiplier = actionSpeedMultiplier;

        float actionSpeed = actionSpeedMultiplier / actionTimePeriod;
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
        drawTriggerTime = actionTimePeriod * (1f - animationAttackPortion);
        releaseTriggerTime = actionTimePeriod - weaponInfo.animationAttackReleaseTime;
        waitForReleaseTriggerTime = drawTriggerTime + (releaseTriggerTime - drawTriggerTime) * weaponInfo.animationAttackDrawPortion;

        OnAnimatorSetFloat("drawSpeed", "ragingbow_anim_draw", 1f /
            (((actionTimePeriod / actionSpeedMultiplier) * animationAttackPortion - weaponInfo.animationAttackReleaseTime) * weaponInfo.animationAttackDrawPortion));
    }

    public override void ActIfReady()
    {
        if (bowState == BowState.Idle && actionTimePassed >= drawTriggerTime)
        {
            actionTimePassed = drawTriggerTime;
            bowState = BowState.Drawing;
            OnAnimatorSetTrigger("draw");
        }
        else if (bowState == BowState.Drawing && actionTimePassed >= waitForReleaseTriggerTime)
        {
            actionTimePassed = waitForReleaseTriggerTime;
            bowState = BowState.WaitingForRelease;
        }
        else if (!isRotating && bowState == BowState.WaitingForRelease && actionTimePassed >= releaseTriggerTime && targetEnemy != null)
        {
            actionTimePassed = releaseTriggerTime;
            bowState = BowState.Releasing;
            OnAnimatorSetTrigger("release");
            ReleaseProjectile();
        }
        else if (bowState == BowState.Releasing && actionTimePassed >= actionTimePeriod)
        {
            IncrementStack();
            bowState = BowState.Idle;
            actionTimePassed = 0f;
        }
    }

    public void ReleaseProjectile()
    {
        Vec3 targetImaginaryPos = CombatFunctions.GetCHPosition(targetEnemy.coordX, targetEnemy.coordY, targetEnemy.isPlayer);
        Vec3 attackerImaginaryPos = CombatFunctions.GetCHPosition(coordX, coordY, this.isPlayer);
        float totalTravelTime = (targetImaginaryPos - attackerImaginaryPos).magnitude / projectileSpeed;
        var projectile = new POneTarget(targetEnemy, GetCombatAction(), projectileSpeed, totalTravelTime);
        pOneTargets.Add(projectile);
        onReleaseProjectile?.Invoke(projectile);
    }

    public override void PrepareCombatStart(int roundCount)
    {
        PrepareCombatStartHP();
        PrepareCombatStartOther(roundCount);
        PrepareCombatStartSpeed();
        PrepareCombatStartRange();
    }

    public override CombatAction GetCombatAction()
    {
        if (CombatMain.isRandomized) {
            return CombatFunctions.GetCombatAction(rnd, statusEffects, this, allyCombatWeapons, allyRowsList, damageMin, damageMax);
        } else {
            return CombatFunctions.GetCombatAction(rnd, statusEffects, this, allyCombatWeapons, allyRowsList, damageFixed, damageFixed);
        }
    }

    public override void ReceiveAction(CombatAction action)
    {
        base.ReceiveAction(action);
        if (isDead) {
            foreach (var projectile in pOneTargets) {
                onDestroyProjectile?.Invoke(projectile);
            }
        }
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}

    public void IncrementStack() {
        stacks++;
        onUpdateStackUI?.Invoke();
    }
}
