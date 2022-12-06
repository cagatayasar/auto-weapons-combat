using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CWSniperRifle : CombatWeapon
{
    public StatsSniperRifle stats;
    int damageFixed;
    int damageMin;
    int damageMax;

    bool didFirstShot;
    float firstShotTimer;

    float shotAndCockSlowLength = 1.55f;
    float shotAndCockMedLength = 1.22f;
    bool reloadSoundPlayed;
    bool afterReloadSoundPlayed;

    float reloadTimer;
    int bullets;

    float projectileSpeed;

    public event Action<POneTarget> onReleaseProjectile;
    public event Action<POneTarget> onDestroyProjectile;
    public event Action<POneTarget, float> onUpdateProjectile;
    public event Action<int, float> onUpdateBulletFillAmount;

    public CWSniperRifle(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, CombatMode combatMode, ref Unity.Mathematics.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, combatMode, ref rnd)
    {
        stats = DataManager.inst.weaponsPackage.sniperRifle;
        statsGeneral = stats.statsGeneral;
        UpdateLevelBasedStats();
        projectileSpeed = stats.projectileSpeed * DataManager.inst.globalAttributes.combatAreaScale;
        bullets = stats.bullets;
        _30DegreesRotationDuration = stats._30DegreesRotationDuration;
        if (weapon.attachment == AttachmentType.FasterReload) {
            stats.reloadTimePerBullet /= DataManager.inst.attachmentAttributes.fasterReload_Multiplier;
        }
        ApplyExistingPermanentStatusEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        OnAnimatorSetFloat("speed", "sniper_anim_attack", 1f / (actionTimePeriod * stats.animationNonidleMultiplierMin));
    }

    public override void UpdateLevelBasedStats()
    {
        base.UpdateLevelBasedStats();
        if (weapon.combatLevel == 1) {
            damageFixed = statsGeneral.damage1Fixed;
            damageMin = statsGeneral.damage1Min;
            damageMax = statsGeneral.damage1Max;
            range = stats.range1;
        } else if (weapon.combatLevel == 2) {
            damageFixed = statsGeneral.damage2Fixed;
            damageMin = statsGeneral.damage2Min;
            damageMax = statsGeneral.damage2Max;
            range = stats.range2;
        }
    }

    public override void Update(float deltaTime)
    {
        seActionSpeedMultiplier = CombatFunctions.HandleStatusEffects(this, deltaTime);
        if (!isDead)
        {
            timePassed += deltaTime;
            actionTimePassed += deltaTime * seActionSpeedMultiplier;
            if (!didFirstShot) {
                firstShotTimer += deltaTime * seActionSpeedMultiplier;
                if (firstShotTimer > stats.doFirstShotAfter) didFirstShot = true;
            }

            UpdateTarget();
            UpdateAnimator();
            if (isRotating)
                UpdateRotating(deltaTime);
            RotateIfNeeded(targetEnemy);
            UpdateReloading(deltaTime);
            if (!isRotating && reloadState == ReloadState.Shoot && didFirstShot)
                ActIfReady();

            OnUpdateHealthBar(deltaTime);
            UpdateProjectiles(deltaTime);
        }
    }

    public override void UpdateTarget()
    {
        int rangeToAddOrSubstract = CombatFunctions.GetRangeToAddOrSubstract(this, range, timePassed, oneLessRangeForXSeconds);
        targetEnemy = CombatFunctions.TargetEnemyRangedFurthest(this, range + rangeToAddOrSubstract, targetEnemy, targetRowsList);
    }

    public override void UpdateAnimator()
    {
        if (prevSeActionSpeedMultiplier == seActionSpeedMultiplier) return;
        prevSeActionSpeedMultiplier = seActionSpeedMultiplier;

        float actionSpeed = seActionSpeedMultiplier / actionTimePeriod;
        float actionSpeedClamped = actionSpeed;
        float animationNonidleMultiplier;
        if (actionSpeed < stats.actionSpeedForNonidleMin) {
            animationNonidleMultiplier = stats.animationNonidleMultiplierMin;
            actionSpeedClamped = stats.actionSpeedForNonidleMin;
        }
        else if (actionSpeed > stats.actionSpeedForNonidleMax) {
            animationNonidleMultiplier = stats.animationNonidleMultiplierMax;
        }
        else {
            float actionSpeedForNonidleNormalized = (actionSpeed - stats.actionSpeedForNonidleMin) / (stats.actionSpeedForNonidleMax - stats.actionSpeedForNonidleMin);
            float actionSpeedForNonidleMapped = actionSpeedForNonidleNormalized * (stats.animationNonidleMultiplierMax - stats.animationNonidleMultiplierMin);
            animationNonidleMultiplier = stats.animationNonidleMultiplierMin + actionSpeedForNonidleMapped;
        }

        OnAnimatorSetFloat("speed", "sniper_anim_attack", actionSpeedClamped / animationNonidleMultiplier);
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

    public void ReloadOne()
    {
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
            float t = reloadTimer / stats.reloadTimePerBullet;
            onUpdateBulletFillAmount?.Invoke(bullets, (t - 1f + stats.reloadAnimationUIPortion) / stats.reloadAnimationUIPortion);
            if (!reloadSoundPlayed && t - 1f + stats.reloadAnimationUIPortion > 0f) {
                reloadSoundPlayed = true;
                OnSfxTrigger("reloadSound");
            }

            if (reloadTimer > stats.reloadTimePerBullet) {
                reloadSoundPlayed = false;
                reloadTimer = 0f;
                bullets++;
                if (bullets == stats.bullets) {
                    reloadState = ReloadState.WaitAfterReload;
                    reloadTimer = 0f;
                }
            }
        }
        else if (reloadState == ReloadState.WaitAfterReload) {
            reloadTimer += deltaTime;
            if (!afterReloadSoundPlayed && reloadTimer >= stats.playAfterReloadSoundAfter) {
                afterReloadSoundPlayed = true;
                OnSfxTrigger("cockSound");
            }
            if (reloadTimer > stats.waitLengthAfterReload) {
                afterReloadSoundPlayed = false;
                reloadState = ReloadState.Shoot;
            }
        }
    }

    public override void ActIfReady()
    {
        if (actionTimePassed >= actionTimePeriod && targetEnemy != null)
        {
            actionTimePassed = 0f;
            ReleaseProjectile();
            bullets--;
            if (combatMode == CombatMode.Object) {
                if (bullets == 0)
                    OnSfxTrigger("shotSound");
                else if (actionTimePeriod / seActionSpeedMultiplier > shotAndCockSlowLength) {
                    OnSfxTrigger("shotAndCockSoundSlow");
                } else if (actionTimePeriod / seActionSpeedMultiplier > shotAndCockMedLength) {
                    OnSfxTrigger("shotAndCockSoundMed");
                } else {
                    OnSfxTrigger("shotAndCockSoundFast");
                }
                OnAnimatorSetTrigger("attack");
                onUpdateBulletFillAmount?.Invoke(bullets, 0f);
            }
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
        if (DataManager.inst.isRandomized) {
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
}
