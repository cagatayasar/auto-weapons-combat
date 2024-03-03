using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoWeapons {

public class CWArbalest : CW, ICWHoldsRowPositions
{
    public new WInfoArbalest weaponInfo => base.weaponInfo as WInfoArbalest;
    float touchDamageMultiplier;

    // animation variables
    float animationDrawClipLength;
    float animationReleaseClipLength;

    float drawTriggerTime;
    float waitForReleaseTriggerTime;
    float releaseTriggerTime;

    float arrowImaginaryStartingOffset;
    float projectileSpeed;
    public List<PArbalest> pArbalests = new List<PArbalest>();

    Vec3[] leftAreaRows = new Vec3[3];
    Vec3[] rightAreaRows = new Vec3[3];
    float combatAreaBoundaryLeft;
    float combatAreaBoundaryRight;

    List<CW> targets = new List<CW>(3);
    List<CW> touched = new List<CW>(3);

    public Action<PArbalest> onReleaseProjectile;
    public Action<PArbalest> onDestroyProjectile;
    public Action<PArbalest, float> onUpdateProjectile;

    //------------------------------------------------------------------------
    public CWArbalest(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, ref rnd)
    {
        UpdateLevelBasedStats();
        arrowImaginaryStartingOffset = weaponInfo.arrowImaginaryStartingOffset * CombatMain.combatAreaScale;
        projectileSpeed = weaponInfo.projectileSpeed * CombatMain.combatAreaScale;
        UpdateRowPositions(false);
        var distancePerRow = MathF.Abs(leftAreaRows[0].x - leftAreaRows[1].x);
        combatAreaBoundaryLeft = leftAreaRows[2].x - distancePerRow;
        combatAreaBoundaryRight = rightAreaRows[2].x + distancePerRow;
        ApplyExistingPermanentEffects();
    }

    //------------------------------------------------------------------------
    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        onAnimatorSetFloat?.Invoke("drawSpeed", "arbalest_anim_draw", 1f / (actionTimePeriod * weaponInfo.animNonidlePortionMin * weaponInfo.animAttackDrawTime));
        onAnimatorSetFloat?.Invoke("releaseSpeed", "arbalest_anim_release", 1f / weaponInfo.animAttackReleaseTime);
    }

    //------------------------------------------------------------------------
    public override void UpdateLevelBasedStats()
    {
        base.UpdateLevelBasedStats();
        if (weapon.combatLevel == 1) {
            touchDamageMultiplier = weaponInfo.touchDamageMultiplier1;
        } else if (weapon.combatLevel == 2) {
            touchDamageMultiplier = weaponInfo.touchDamageMultiplier2;
        }
    }

    //------------------------------------------------------------------------
    public void UpdateRowPositions(bool isPreparationPhase)
    {
        if (isPreparationPhase) {
            CombatFunctions.UpdateRowPositionsPreparation(leftAreaRows, rightAreaRows, playerRowsList.Count, enemyRowsList.Count);
        } else {
            CombatFunctions.UpdateRowPositionsCombat(leftAreaRows, rightAreaRows);
        }
    }

    //------------------------------------------------------------------------
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (!isDead)
        {
            timePassed += deltaTime;
            actionTimePassed += deltaTime * effectSpeedMultiplier;

            UpdateAnimator();
            ActIfReady();

            onUpdateHealthBar?.Invoke(deltaTime);
            UpdateProjectiles(deltaTime);
        }
    }

    //------------------------------------------------------------------------
    public override void UpdateTarget(){}

    //------------------------------------------------------------------------
    public override void UpdateProjectiles(float deltaTime)
    {
        for (int i = 0; i < pArbalests.Count; i++)
        {
            var projectile = pArbalests[i];
            Vec3 added = projectile.position + projectile.directionVector * (deltaTime * projectile.Speed);

            if ((projectile.directionVector.x > 0 && added.x > combatAreaBoundaryRight) ||
                (projectile.directionVector.x < 0 && added.x < combatAreaBoundaryLeft))
            {
                onDestroyProjectile?.Invoke(projectile);
                pArbalests.RemoveAt(i);
                i--;
                continue;
            }

            int rowToDamage = 0;
            if (projectile.directionVector.x > 0)
            {
                     if (added.x >= leftAreaRows[1].x  && projectile.position.x <= leftAreaRows[1].x)  rowToDamage = -2;
                else if (added.x >= leftAreaRows[0].x  && projectile.position.x <= leftAreaRows[0].x)  rowToDamage = -1;
                else if (added.x >= rightAreaRows[0].x && projectile.position.x <= rightAreaRows[0].x) rowToDamage =  1;
                else if (added.x >= rightAreaRows[1].x && projectile.position.x <= rightAreaRows[1].x) rowToDamage =  2;
                else if (added.x >= rightAreaRows[2].x && projectile.position.x <= rightAreaRows[2].x) rowToDamage =  3;
            }
            else
            {
                     if (added.x <= rightAreaRows[1].x && projectile.position.x >= rightAreaRows[1].x) rowToDamage =  2;
                else if (added.x <= rightAreaRows[0].x && projectile.position.x >= rightAreaRows[0].x) rowToDamage =  1;
                else if (added.x <= leftAreaRows[0].x  && projectile.position.x >= leftAreaRows[0].x)  rowToDamage = -1;
                else if (added.x <= leftAreaRows[1].x  && projectile.position.x >= leftAreaRows[1].x)  rowToDamage = -2;
                else if (added.x <= leftAreaRows[2].x  && projectile.position.x >= leftAreaRows[2].x)  rowToDamage = -3;
            }
            projectile.position = added;
            onUpdateProjectile?.Invoke(projectile, deltaTime);

            targets.Clear();
            touched.Clear();
            if (rowToDamage > 0 && enemyRowsList.Count >= rowToDamage)
            {
                var rowSize = enemyRowsList[rowToDamage - 1].Count;
                for (int j = 0; j < rowSize; j++) {
                    var cw = enemyRowsList[rowToDamage - 1][j];
                    if (cw.verticalPosition == projectile.positionFromBottom)
                        targets.Add(cw);
                    else if (cw.verticalPosition == projectile.positionFromBottom + 1 ||
                             cw.verticalPosition == projectile.positionFromBottom - 1)
                        touched.Add(cw);
                }
            }
            else if (rowToDamage < 0 && playerRowsList.Count >= -rowToDamage)
            {
                var rowSize = playerRowsList[-rowToDamage - 1].Count;
                for (int j = 0; j < rowSize; j++) {
                    var cw = playerRowsList[-rowToDamage - 1][j];
                    if (cw.verticalPosition == projectile.positionFromBottom)
                        targets.Add(cw);
                    else if (cw.verticalPosition == projectile.positionFromBottom + 1 ||
                             cw.verticalPosition == projectile.positionFromBottom - 1)
                        touched.Add(cw);
                }
            }

            if (projectile.flyingRowToDamage > 0)
            {
                if (projectile.directionVector.x > 0)
                {
                    if ((projectile.flyingRowToDamage == 1 && added.x >= (3*rightAreaRows[0].x + rightAreaRows[1].x) / 4f) ||
                        (projectile.flyingRowToDamage == 2 && added.x >= (3*rightAreaRows[1].x + rightAreaRows[2].x) / 4f))
                    {
                        if (!(projectile.flyingRowToDamage > enemyRowsList.Count))
                        {
                            var rowSize = enemyRowsList[projectile.flyingRowToDamage - 1].Count;
                            for (int j = 0; j < rowSize; j++)
                            {
                                var cw = enemyRowsList[projectile.flyingRowToDamage - 1][j];
                                if (cw.verticalPosition == projectile.positionFromBottom)
                                    targets.Add(cw);
                                else if (cw.verticalPosition == projectile.positionFromBottom + 1 ||
                                         cw.verticalPosition == projectile.positionFromBottom - 1)
                                    touched.Add(cw);
                            }
                            projectile.flyingRowToDamage = 0;
                        }
                    }
                }
                else
                {
                    if ((projectile.flyingRowToDamage == 1 && added.x <= (3*leftAreaRows[0].x + leftAreaRows[1].x) / 4f) ||
                        (projectile.flyingRowToDamage == 2 && added.x <= (3*leftAreaRows[1].x + leftAreaRows[2].x) / 4f))
                    {
                        if (!(projectile.flyingRowToDamage > playerRowsList.Count))
                        {
                            var rowSize = playerRowsList[projectile.flyingRowToDamage - 1].Count;
                            for (int j = 0; j < rowSize; j++) {
                                var cw = playerRowsList[projectile.flyingRowToDamage - 1][j];
                                if (cw.verticalPosition == projectile.positionFromBottom)
                                    targets.Add(cw);
                                else if (cw.verticalPosition == projectile.positionFromBottom + 1 ||
                                         cw.verticalPosition == projectile.positionFromBottom - 1)
                                    touched.Add(cw);
                            }
                            projectile.flyingRowToDamage = 0;
                        }
                    }
                }
            }

            if (targets.Count == 0 && touched.Count == 0)
                continue;

            CombatAction action = GetCombatAction();
            for (int j = 0; j < targets.Count; j++) {
                var target = targets[j];
                if (projectile.damagedIDs.Contains(target.id) || target == this)
                    continue;
                CombatFunctions.ApplyActionToTarget(target, this, ModifyCombatAction(target, action, isTouched: false));
                projectile.damagedIDs.Add(target.id);
            }

            for (int j = 0; j < touched.Count; j++) {
                var target = touched[j];
                if (projectile.damagedIDs.Contains(target.id) || target == this) continue;
                CombatFunctions.ApplyActionToTarget(target, this, ModifyCombatAction(target, action, isTouched: true));
                projectile.damagedIDs.Add(target.id);
            }
        }
    }

    //------------------------------------------------------------------------
    public override void UpdateAnimator()
    {
        if (prevEffectSpeedMultiplier == effectSpeedMultiplier) return;
        prevEffectSpeedMultiplier = effectSpeedMultiplier;

        float actionSpeed = effectSpeedMultiplier / actionTimePeriod;
        float animationNonidlePortion;
        if (actionSpeed < weaponInfo.animNonidleSpeedMin)
            animationNonidlePortion = weaponInfo.animNonidlePortionMin;
        else if (actionSpeed > weaponInfo.animNonidleSpeedMax)
            animationNonidlePortion = weaponInfo.animNonidlePortionMax;
        else {
            float actionSpeedForNonidleNormalized = (actionSpeed - weaponInfo.animNonidleSpeedMin) / (weaponInfo.animNonidleSpeedMax - weaponInfo.animNonidleSpeedMin);
            float actionSpeedForNonidleMapped = actionSpeedForNonidleNormalized * (weaponInfo.animNonidlePortionMax - weaponInfo.animNonidlePortionMin);
            animationNonidlePortion = weaponInfo.animNonidlePortionMin + actionSpeedForNonidleMapped;
        }
        drawTriggerTime = actionTimePeriod * (1f - animationNonidlePortion);
        releaseTriggerTime = actionTimePeriod - weaponInfo.animAttackReleaseTime;
        waitForReleaseTriggerTime = drawTriggerTime + (releaseTriggerTime - drawTriggerTime) * weaponInfo.animAttackDrawTime;

        onAnimatorSetFloat?.Invoke("drawSpeed", "arbalest_anim_draw", 1f /
            (((actionTimePeriod / effectSpeedMultiplier) * animationNonidlePortion - weaponInfo.animAttackReleaseTime) * weaponInfo.animAttackDrawTime));
    }

    //------------------------------------------------------------------------
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
        else if (bowState == BowState.WaitingForRelease && actionTimePassed >= releaseTriggerTime)
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

    //------------------------------------------------------------------------
    public void ReleaseProjectile()
    {
        var val1 = Vec3.right * 2;
        var val2 = 2 * Vec3.right;
        var directionVector = Vec3.right * isPlayer.ToMultiplier();
        var attackerImaginaryPos = isPlayer ? leftAreaRows[rowNumber-1] : rightAreaRows[rowNumber-1];
        attackerImaginaryPos += directionVector * arrowImaginaryStartingOffset;

        var projectile = new PArbalest(GetCombatAction(), attackerImaginaryPos, directionVector, projectileSpeed, verticalPosition);
        pArbalests.Add(projectile);
        onReleaseProjectile?.Invoke(projectile);
    }

    //------------------------------------------------------------------------
    public CombatAction ModifyCombatAction(CW target, CombatAction combatAction, bool isTouched)
    {
        var multiplier = isTouched ? touchDamageMultiplier : 1f;
        if (target.isPlayer == isPlayer) {
            multiplier *= weaponInfo.friendlyDamageMultiplier;
        }
        return new CombatAction((int)(combatAction.damage * multiplier), combatAction.isSenderPlayersWeapon, combatAction.senderId);
    }

    //------------------------------------------------------------------------
    public override void PrepareCombatStart(int roundCount)
    {
        PrepareCombatStartHP();
        PrepareCombatStartOther(roundCount);
        PrepareCombatStartSpeed();
    }

    // @nextday starts here:
    // Copy GetCombatAction() to other CW classes
    //------------------------------------------------------------------------
    public override CombatAction GetCombatAction()
    {
        if (CombatMain.isRandomized) {
            return CombatFunctions.GetCombatAction(rnd, effects, this, allyCWs, allyRowsList, damageMin, damageMax);
        } else {
            return CombatFunctions.GetCombatAction(rnd, effects, this, allyCWs, allyRowsList, damageFixed, damageFixed);
        }
    }

    //------------------------------------------------------------------------
    public override void ReceiveAction(CombatAction action)
    {
        base.ReceiveAction(action);
        if (isDead) {
            foreach (var projectile in pArbalests) {
                onDestroyProjectile?.Invoke(projectile);
            }
        }
    }

    //------------------------------------------------------------------------
    public override void ReportClearedRow(int rowNumber, bool isPlayersRow)
    {
        if (isPlayersRow == isPlayer)
            return;

        if (isPlayer) {
            for (int i = 0; i < pArbalests.Count; i++) {
                var p = pArbalests[i];
                if (p.flyingRowToDamage == 1)
                    continue;
                if (rowNumber == 1 && p.position.x >= rightAreaRows[0].x && p.position.x <= rightAreaRows[1].x)
                    p.flyingRowToDamage = 1;
                else if (rowNumber != 3 && p.position.x >= rightAreaRows[1].x && p.position.x <= rightAreaRows[2].x)
                    p.flyingRowToDamage = 2;
                else
                    p.flyingRowToDamage = 0;
            }
        } else {
            for (int i = 0; i < pArbalests.Count; i++) {
                var p = pArbalests[i];
                if (p.flyingRowToDamage == 1)
                    continue;
                if (rowNumber == 1 && p.position.x <= leftAreaRows[0].x && p.position.x >= leftAreaRows[1].x)
                    p.flyingRowToDamage = 1;
                else if (rowNumber != 3 && p.position.x <= leftAreaRows[1].x && p.position.x >= leftAreaRows[2].x)
                    p.flyingRowToDamage = 2;
                else
                    p.flyingRowToDamage = 0;
            }
        }
    }
}
}
