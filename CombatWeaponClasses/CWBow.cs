﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoWeapons {

public class CWBow : CW
{
    public new WInfoBow weaponInfo => base.weaponInfo as WInfoBow;

    // animation variables
    float animationDrawClipLength;
    float animationReleaseClipLength;

    float drawTriggerTime;
    float waitForReleaseTriggerTime;
    float releaseTriggerTime;

    float projectileSpeed;

    public Action<POneTarget> onReleaseProjectile;
    public Action<POneTarget> onDestroyProjectile;
    public Action<POneTarget, float> onUpdateProjectile;

    public CWBow(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, ref rnd)
    {
        UpdateLevelBasedStats();
        projectileSpeed = weaponInfo.projectileSpeed * CombatMain.combatAreaScale;
        _30DegreesRotationDuration = weaponInfo._30DegreesRotationDuration;
        ApplyExistingPermanentEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        onAnimatorSetFloat?.Invoke("drawSpeed", "bow_anim_draw", 1f / (actionTimePeriod * weaponInfo.animNonidlePortionMin * weaponInfo.animAttackDrawTime));
        onAnimatorSetFloat?.Invoke("releaseSpeed", "bow_anim_release", 1f / weaponInfo.animAttackReleaseTime);
    }

    public override void UpdateLevelBasedStats()
    {
        base.UpdateLevelBasedStats();
        if (weapon.combatLevel == 1) {
            range = weaponInfo.range1;
        } else if (weapon.combatLevel == 2) {
            range = weaponInfo.range2;
        }
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
            if (isRotating)
                UpdateRotating(deltaTime);
            RotateIfNeeded(targetEnemy);
            ActIfReady();

            onUpdateHealthBar?.Invoke(deltaTime);
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
                CombatFunctions.ApplyActionToTarget(projectile.target, this, projectile.CombatAction);
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
        if (prevEffectSpeedMultiplier == effectSpeedMultiplier) return;
        prevEffectSpeedMultiplier = effectSpeedMultiplier;

        float actionSpeed = effectSpeedMultiplier / actionTimePeriod;
        float animationAttackPortion;
        if (actionSpeed < weaponInfo.animNonidleSpeedMin)
            animationAttackPortion = weaponInfo.animNonidlePortionMin;
        else if (actionSpeed > weaponInfo.animNonidleSpeedMax)
            animationAttackPortion = weaponInfo.animNonidlePortionMax;
        else {
            float actionSpeedForNonidleNormalized = (actionSpeed - weaponInfo.animNonidleSpeedMin) / (weaponInfo.animNonidleSpeedMax - weaponInfo.animNonidleSpeedMin);
            float actionSpeedForNonidleMapped = actionSpeedForNonidleNormalized * (weaponInfo.animNonidlePortionMax - weaponInfo.animNonidlePortionMin);
            animationAttackPortion = weaponInfo.animNonidlePortionMin + actionSpeedForNonidleMapped;
        }
        drawTriggerTime = actionTimePeriod * (1f - animationAttackPortion);
        releaseTriggerTime = actionTimePeriod - weaponInfo.animAttackReleaseTime;
        waitForReleaseTriggerTime = drawTriggerTime + (releaseTriggerTime - drawTriggerTime) * weaponInfo.animAttackDrawTime;

        onAnimatorSetFloat?.Invoke("drawSpeed", "bow_anim_draw", 1f /
            (((actionTimePeriod / effectSpeedMultiplier) * animationAttackPortion - weaponInfo.animAttackReleaseTime) * weaponInfo.animAttackDrawTime));
    }

    public override void ActIfReady()
    {
        if (bowState == BowState.Idle && actionTimePassed >= drawTriggerTime)
        {
            actionTimePassed = drawTriggerTime;
            bowState = BowState.Drawing;
            onAnimatorSetTrigger?.Invoke("draw");
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
            onAnimatorSetTrigger?.Invoke("release");
            ReleaseProjectile();
        }
        else if (bowState == BowState.Releasing && actionTimePassed >= actionTimePeriod)
        {
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
            return CombatFunctions.GetCombatAction(rnd, effects, this, allyCWs, allyRowsList, damageMin, damageMax);
        } else {
            return CombatFunctions.GetCombatAction(rnd, effects, this, allyCWs, allyRowsList, damageFixed, damageFixed);
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
}
}
