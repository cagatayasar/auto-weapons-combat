﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoWeapons {

public class CWRevolver : CW
{
    public new WInfoRevolver weaponInfo => base.weaponInfo as WInfoRevolver;


    int firstShotDamageMultiplier;
    bool isFirstMoment = true;

    bool reloadSoundPlayed;
    bool afterReloadSoundPlayed;

    int bullets;
    float reloadTimer;

    float projectileSpeed;

    public Action<POneTarget> onReleaseProjectile;
    public Action<POneTarget> onDestroyProjectile;
    public Action<POneTarget, float> onUpdateProjectile;
    public Action<int, float> onUpdateBulletFillAmount;

    public CWRevolver(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, ref rnd)
    {
        UpdateLevelBasedStats();
        projectileSpeed = weaponInfo.projectileSpeed * CombatMain.combatAreaScale;
        bullets = weaponInfo.bullets;
        _30DegreesRotationDuration = weaponInfo._30DegreesRotationDuration;
        if (weapon.attachment == AttachmentType.FasterReload) {
            weaponInfo.reloadTimePerBullet /= CombatMain.attachmentAttributes.fasterReload_Multiplier;
        }
        ApplyExistingPermanentEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        onAnimatorSetFloat?.Invoke("speed", "revolver_anim_attack", 1f / (actionTimePeriod * weaponInfo.animNonidlePortionMin));
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
            UpdateReloading(deltaTime);
            if (!isRotating && reloadState == ReloadState.Shoot)
                ActIfReady();

            onUpdateHealthBar?.Invoke(deltaTime);
            UpdateProjectiles(deltaTime);
        }
    }

    public override void UpdateTarget()
    {
        targetEnemy = CombatFunctions.TargetEnemyRanged(this, range, targetEnemy, targetRowsList, timePassed, oneLessRangeForXSeconds);
    }

    public override void UpdateAnimator()
    {
        if (prevEffectSpeedMultiplier == effectSpeedMultiplier) return;
        prevEffectSpeedMultiplier = effectSpeedMultiplier;

        float actionSpeed = effectSpeedMultiplier / actionTimePeriod;
        float actionSpeedClamped = actionSpeed;
        float animationNonidleMultiplier;
        if (actionSpeed < weaponInfo.animNonidleSpeedMin) {
            animationNonidleMultiplier = weaponInfo.animNonidlePortionMin;
            actionSpeedClamped = weaponInfo.animNonidleSpeedMin;
        }
        else if (actionSpeed > weaponInfo.animNonidleSpeedMax) {
            animationNonidleMultiplier = weaponInfo.animNonidlePortionMax;
        }
        else {
            float actionSpeedForNonidleNormalized = (actionSpeed - weaponInfo.animNonidleSpeedMin) / (weaponInfo.animNonidleSpeedMax - weaponInfo.animNonidleSpeedMin);
            float actionSpeedForNonidleMapped = actionSpeedForNonidleNormalized * (weaponInfo.animNonidlePortionMax - weaponInfo.animNonidlePortionMin);
            animationNonidleMultiplier = weaponInfo.animNonidlePortionMin + actionSpeedForNonidleMapped;
        }

        onAnimatorSetFloat?.Invoke("speed", "revolver_anim_attack", actionSpeedClamped / animationNonidleMultiplier);
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

    public void ReloadOne() {
        bullets++;
        onUpdateBulletFillAmount?.Invoke(bullets-1, 1f);
    }

    public override void UpdateReloading(float deltaTime) {
        if (reloadState == ReloadState.Shoot) {
            if (bullets == 0) {
                reloadState = ReloadState.Reload;
                reloadTimer = 0f;
            }
        }
        else if (reloadState == ReloadState.Reload) {
            reloadTimer += deltaTime;
            float t = reloadTimer / weaponInfo.reloadTimePerBullet;
            onUpdateBulletFillAmount?.Invoke(bullets, (t - 1f + weaponInfo.reloadAnimationUIPortion) / weaponInfo.reloadAnimationUIPortion);
            if (!reloadSoundPlayed && t - 1f + weaponInfo.reloadAnimationUIPortion > 0f) {
                reloadSoundPlayed = true;
                onSfxTrigger?.Invoke("reloadSound");
            }

            if (reloadTimer > weaponInfo.reloadTimePerBullet) {
                reloadSoundPlayed = false;
                reloadTimer = 0f;
                bullets++;
                if (bullets == weaponInfo.bullets) {
                    reloadState = ReloadState.WaitAfterReload;
                    reloadTimer = 0f;
                }
            }
        }
        else if (reloadState == ReloadState.WaitAfterReload) {
            reloadTimer += deltaTime;
            if (!afterReloadSoundPlayed && reloadTimer >= weaponInfo.playAfterReloadSoundAfter) {
                afterReloadSoundPlayed = true;
                onSfxTrigger?.Invoke("wheelSound");
            }
            if (reloadTimer > weaponInfo.waitLengthAfterReload) {
                afterReloadSoundPlayed = false;
                reloadState = ReloadState.Shoot;
            }
        }
    }

    public override void ActIfReady()
    {
        if (actionTimePassed >= actionTimePeriod && targetEnemy != null) {
            actionTimePassed = 0f;
            ReleaseProjectile();
            bullets--;
            onSfxTrigger?.Invoke("shotSound");
            onAnimatorSetTrigger?.Invoke("attack");
            onUpdateBulletFillAmount?.Invoke(bullets, 0f);
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
        float finalDmgMultiplier = 1f;
        if (isFirstMoment) {
            finalDmgMultiplier = 2f;
            isFirstMoment = false;
        }

        if (CombatMain.isRandomized) {
            return CombatFunctions.GetCombatAction(rnd, effects, this, allyCWs, allyRowsList, damageMin, damageMax, finalDmgMultiplierParameter: finalDmgMultiplier);
        } else {
            return CombatFunctions.GetCombatAction(rnd, effects, this, allyCWs, allyRowsList, damageFixed, damageFixed, finalDmgMultiplierParameter: finalDmgMultiplier);
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
