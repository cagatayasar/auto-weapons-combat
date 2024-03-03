using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoWeapons {

public class CWCannon : CW, ICWHoldsRowPositions
{
    public new WInfoCannon weaponInfo => base.weaponInfo as WInfoCannon;

    public int targetRowNumber;

    bool didFirstShot;
    float firstShotTimer;

    float projectileSpeed;
    public float projectileMaxHeightOverDistance;

    Vec3[] leftAreaRows = new Vec3[3];
    Vec3[] rightAreaRows = new Vec3[3];

    public Action<POneRow> onReleaseProjectile;
    public Action<POneRow> onDestroyProjectile;
    public Action<POneRow, float> onUpdateProjectile;

    public CWCannon(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, ref rnd)
    {
        UpdateLevelBasedStats();
        projectileSpeed = weaponInfo.projectileSpeed * CombatMain.combatAreaScale;
        projectileMaxHeightOverDistance = weaponInfo.projectileMaxHeightOverDistance * CombatMain.combatAreaScale;
        UpdateRowPositions(false);
        ApplyExistingPermanentEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        onAnimatorSetFloat?.Invoke("speed", "cannon_anim_attack", 1f / (actionTimePeriod * weaponInfo.animNonidlePortionMin));
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
            if (!didFirstShot) {
                firstShotTimer += deltaTime * effectSpeedMultiplier;
                if (firstShotTimer > weaponInfo.doFirstShotAfter) {
                    didFirstShot = true;
                }
            }

            UpdateTarget();
            UpdateAnimator();
            if (didFirstShot)
                ActIfReady();

            onUpdateHealthBar?.Invoke(deltaTime);
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
        if (prevEffectSpeedMultiplier == effectSpeedMultiplier) return;
        prevEffectSpeedMultiplier = effectSpeedMultiplier;

        float actionSpeed = effectSpeedMultiplier / actionTimePeriod;
        float actionSpeedClamped = actionSpeed;
        float animationAttackPortion;
        if (actionSpeed < weaponInfo.actionSpeedForAnimationMin) {
            animationAttackPortion = weaponInfo.animNonidlePortionMin;
            actionSpeedClamped = weaponInfo.actionSpeedForAnimationMin;
        }
        else if (actionSpeed > weaponInfo.actionSpeedForAnimationMax) {
            animationAttackPortion = weaponInfo.animNonidlePortionMax;
        }
        else {
            float actionSpeedForNonidleNormalized = (actionSpeed - weaponInfo.actionSpeedForAnimationMin) / (weaponInfo.actionSpeedForAnimationMax - weaponInfo.actionSpeedForAnimationMin);
            float actionSpeedForNonidleMapped = actionSpeedForNonidleNormalized * (weaponInfo.animNonidlePortionMax - weaponInfo.animNonidlePortionMin);
            animationAttackPortion = weaponInfo.animNonidlePortionMin + actionSpeedForNonidleMapped;
        }

        onAnimatorSetFloat?.Invoke("speed", "cannon_anim_attack", 1f * actionSpeedClamped / animationAttackPortion);
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
                    CombatFunctions.ApplyActionToTarget(target, this, projectile.CombatAction);
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
            onSfxTrigger?.Invoke("shotSound");
            onAnimatorSetTrigger?.Invoke("attack");
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
            foreach (var projectile in pOneRows) {
                onDestroyProjectile?.Invoke(projectile);
            }
        }
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
}
