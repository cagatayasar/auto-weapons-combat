using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoWeapons {

public class CWJavelin : CW, ICWThrown, ICWHoldsRowPositions
{
    public new WInfoJavelin weaponInfo => base.weaponInfo as WInfoJavelin;

    public float touchDamageMultiplier;
    public float projectileSpeed;
    public Vec3 damagePointOffset;

    public Vec3[] leftAreaRows = new Vec3[3];
    public Vec3[] rightAreaRows = new Vec3[3];
    public float combatAreaBoundaryLeft;
    public float combatAreaBoundaryRight;

    public CWJavelin(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, ref rnd)
    {
        UpdateLevelBasedStats();
        projectileSpeed = weaponInfo.projectileSpeed * CombatMain.combatAreaScale;
        damagePointOffset = Vec3.right * weaponInfo.damagePointOffset * CombatMain.combatAreaScale * isPlayer.ToMultiplier();
        UpdateRowPositions(false);
        var distancePerRow = MathF.Abs(leftAreaRows[0].x - leftAreaRows[1].x);
        combatAreaBoundaryLeft = leftAreaRows[2].x - distancePerRow;
        combatAreaBoundaryRight = rightAreaRows[2].x + distancePerRow;
        ApplyExistingPermanentEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
    }

    public override void UpdateLevelBasedStats()
    {
        base.UpdateLevelBasedStats();
        if (weapon.combatLevel == 1) {
            touchDamageMultiplier = weaponInfo.touchDamageMultiplier1;
        } else if (weapon.combatLevel == 2) {
            touchDamageMultiplier = weaponInfo.touchDamageMultiplier2;
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

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        onUpdateHealthBar?.Invoke(deltaTime);
    }

    public override void UpdateTarget(){}

    public override void UpdateAnimator(){}

    public override void ActIfReady(){}

    public CombatAction ModifyCombatAction(CW target, CombatAction combatAction, bool isTouched)
    {
        var multiplier = isTouched ? touchDamageMultiplier : 1f;
        if (target.isPlayer == isPlayer) {
            multiplier *= weaponInfo.friendlyDamageMultiplier;
        }
        return new CombatAction((int)(combatAction.damage * multiplier), combatAction.isSenderPlayersWeapon, combatAction.senderId);
    }

    public override void PrepareCombatStart(int roundCount)
    {
        PrepareCombatStartHP();
        PrepareCombatStartOther(roundCount);
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

    public void Throw_Simulation()
    {
        var rowsCount = targetRowsList.Count;
        for (int i = 0; i < rowsCount; i++) {
            var row = targetRowsList[i];
            var combatAction = GetCombatAction();

            var cwCount = row.Count;
            for (int j = 0; j < cwCount; j++) {
                var target = row[j];
                var positionBottomDifference = target.verticalPosition - verticalPosition;
                if (positionBottomDifference == 0) {
                    CombatFunctions.ApplyActionToTarget(target, this, ModifyCombatAction(target, combatAction, false));
                } else if (positionBottomDifference == 1 || positionBottomDifference == -1) {
                    CombatFunctions.ApplyActionToTarget(target, this, ModifyCombatAction(target, combatAction, true));
                }
            }
        }
    }
}
}
