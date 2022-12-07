using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CWGreatbow : CombatWeapon
{
    StatsGreatbow stats;
    int healthPercent;

    // animation variables
    float animationDrawClipLength;
    float animationReleaseClipLength;

    float drawTriggerTime;
    float waitForReleaseTriggerTime;
    float releaseTriggerTime;

    float projectileSpeed;

    public event Action<POneTarget> onReleaseProjectile;
    public event Action<POneTarget> onDestroyProjectile;
    public event Action<POneTarget, float> onUpdateProjectile;

    public CWGreatbow(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, CombatMode combatMode, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, combatMode, ref rnd)
    {
        stats = DataManager.inst.weaponsPackage.greatbow;
        statsGeneral = stats.statsGeneral;
        UpdateLevelBasedStats();
        projectileSpeed = stats.projectileSpeed * CombatMain.combatAreaScale;
        _30DegreesRotationDuration = stats._30DegreesRotationDuration;
        ApplyExistingPermanentStatusEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        OnAnimatorSetFloat("drawSpeed", "greatbow_anim_draw", 1f / (actionTimePeriod * stats.animationNonidlePortionMin * stats.animationAttackDrawPortion));
        OnAnimatorSetFloat("releaseSpeed", "greatbow_anim_release", 1f / stats.animationAttackReleaseTime);
    }

    public override void UpdateLevelBasedStats()
    {
        base.UpdateLevelBasedStats();
        if (weapon.combatLevel == 1) {
            healthPercent = stats.healthPercent1;
            range = stats.range1;
        } else if (weapon.combatLevel == 2) {
            healthPercent = stats.healthPercent2;
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
        drawTriggerTime = actionTimePeriod * (1f - animationAttackPortion);
        releaseTriggerTime = actionTimePeriod - stats.animationAttackReleaseTime;
        waitForReleaseTriggerTime = drawTriggerTime + (releaseTriggerTime - drawTriggerTime) * stats.animationAttackDrawPortion;

        OnAnimatorSetFloat("drawSpeed", "greatbow_anim_draw", 1f /
            (((actionTimePeriod / seActionSpeedMultiplier) * animationAttackPortion - stats.animationAttackReleaseTime) * stats.animationAttackDrawPortion));
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
        return CombatFunctions.GetCombatAction(rnd, statusEffects, this, allyCombatWeapons, allyRowsList, 0, 0, healthPercent);
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
