using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CWCannon : CombatWeapon, ICWHoldsRowPositions
{
    StatsCannon stats;
    int damageFixed;
    int damageMin;
    int damageMax;
    public int targetRowNumber;

    bool didFirstShot;
    float firstShotTimer;

    float projectileSpeed;
    public float projectileMaxHeightOverDistance;

    Vec3[] leftAreaRows = new Vec3[3];
    Vec3[] rightAreaRows = new Vec3[3];

    public event Action<POneRow> onReleaseProjectile;
    public event Action<POneRow> onDestroyProjectile;
    public event Action<POneRow, float> onUpdateProjectile;

    public CWCannon(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, CombatMode combatMode, ref Unity.Mathematics.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, combatMode, ref rnd)
    {
        stats = DataManager.inst.weaponsPackage.cannon;
        statsGeneral = stats.statsGeneral;
        UpdateLevelBasedStats();
        projectileSpeed = stats.projectileSpeed * DataManager.inst.globalAttributes.combatAreaScale;
        projectileMaxHeightOverDistance = stats.projectileMaxHeightOverDistance * DataManager.inst.globalAttributes.combatAreaScale;
        UpdateRowPositions(false);
        ApplyExistingPermanentStatusEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        OnAnimatorSetFloat("speed", "cannon_anim_attack", 1f / (actionTimePeriod * stats.animationNonidlePortionMin));
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
                if (firstShotTimer > stats.doFirstShotAfter) {
                    didFirstShot = true;
                }
            }

            UpdateTarget();
            UpdateAnimator();
            if (didFirstShot)
                ActIfReady();

            OnUpdateHealthBar(deltaTime);
            UpdateProjectiles(deltaTime);
        }
    }

    public void UpdateRowPositions(bool isPreparationPhase)
    {
        if (isPreparationPhase) {
            CombatFunctions.UpdateRowPositionsPreparation(leftAreaRows, rightAreaRows, playerRowsList.Count, enemyRowsList.Count);
        } else {
            CombatFunctions.UpdateRowPositionsCombat(leftAreaRows, rightAreaRows);
        }
    }

    public override void UpdateTarget()
    {
        int rangeToAddOrSubstract = CombatFunctions.GetRangeToAddOrSubstract(this, range, timePassed, oneLessRangeForXSeconds);
        if (rowNumber > range + rangeToAddOrSubstract || playerRowsList.Count == 0) {
            targetRowNumber = 0;
        } else {
            targetRowNumber = 1;
        }
    }

    public override void UpdateAnimator()
    {
        if (prevSeActionSpeedMultiplier == seActionSpeedMultiplier) return;
        prevSeActionSpeedMultiplier = seActionSpeedMultiplier;

        float actionSpeed = seActionSpeedMultiplier / actionTimePeriod;
        float actionSpeedClamped = actionSpeed;
        float animationAttackPortion;
        if (actionSpeed < stats.actionSpeedForAnimationMin) {
            animationAttackPortion = stats.animationNonidlePortionMin;
            actionSpeedClamped = stats.actionSpeedForAnimationMin;
        }
        else if (actionSpeed > stats.actionSpeedForAnimationMax) {
            animationAttackPortion = stats.animationNonidlePortionMax;
        }
        else {
            float actionSpeedForNonidleNormalized = (actionSpeed - stats.actionSpeedForAnimationMin) / (stats.actionSpeedForAnimationMax - stats.actionSpeedForAnimationMin);
            float actionSpeedForNonidleMapped = actionSpeedForNonidleNormalized * (stats.animationNonidlePortionMax - stats.animationNonidlePortionMin);
            animationAttackPortion = stats.animationNonidlePortionMin + actionSpeedForNonidleMapped;
        }

        OnAnimatorSetFloat("speed", "cannon_anim_attack", 1f * actionSpeedClamped / animationAttackPortion);
    }

    public override void UpdateProjectiles(float deltaTime)
    {
        for (int i = 0; i < pOneRows.Count; i++)
        {
            var projectile = pOneRows[i];
            var targetRowsList = isPlayer ? enemyRowsList : playerRowsList;
            if (targetRowsList.Count == 0)
            {
                onDestroyProjectile?.Invoke(projectile);
                pOneRows.RemoveAt(i);
                i--;
                continue;
            }

            if (projectile.currentTravelTime + deltaTime >= projectile.totalTravelTime)
            {
                foreach (var target in targetRowsList[projectile.targetRowNumber - 1]) {
                    CombatFunctions.ApplyActionToTarget(target, this, projectile.combatAction);
                }
                onDestroyProjectile?.Invoke(projectile);
                pOneRows.RemoveAt(i);
                i--;
                continue;
            }

            onUpdateProjectile?.Invoke(projectile, deltaTime);
            projectile.currentTravelTime += deltaTime;
        }
    }

    public override void ActIfReady()
    {
        if (actionTimePassed >= actionTimePeriod && targetRowNumber >= 1)
        {
            actionTimePassed = 0f;
            ReleaseProjectile();
            OnSfxTrigger("shotSound");
            OnAnimatorSetTrigger("attack");
        }
    }

    public void ReleaseProjectile()
    {
        // this may be a problem
        var targetImaginaryPos   = isPlayer ? rightAreaRows[targetRowNumber-1] : leftAreaRows[targetRowNumber-1];
        var attackerImaginaryPos = isPlayer ? leftAreaRows[rowNumber-1] : rightAreaRows[rowNumber-1];

        float totalTravelTime = (targetImaginaryPos - attackerImaginaryPos).magnitude / projectileSpeed;
        var projectile = new POneRow(targetRowNumber, GetCombatAction(), projectileSpeed, attackerImaginaryPos, targetImaginaryPos.x, totalTravelTime);
        pOneRows.Add(projectile);
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
            foreach (var projectile in pOneRows) {
                onDestroyProjectile?.Invoke(projectile);
            }
        }
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
