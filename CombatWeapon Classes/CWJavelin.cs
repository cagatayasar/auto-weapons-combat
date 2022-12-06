using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CWJavelin : CombatWeapon, ICWThrown, ICWHoldsRowPositions
{
    public StatsJavelin stats;
    int damageFixed;
    int damageMin;
    int damageMax;

    public float touchDamageMultiplier;
    public float projectileSpeed;
    public Vec3 damagePointOffset;

    public Vec3[] leftAreaRows = new Vec3[3];
    public Vec3[] rightAreaRows = new Vec3[3];
    public float combatAreaBoundaryLeft;
    public float combatAreaBoundaryRight;

    public CWJavelin(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, CombatMode combatMode, ref Unity.Mathematics.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, combatMode, ref rnd)
    {
        stats = DataManager.inst.weaponsPackage.javelin;
        statsGeneral = stats.statsGeneral;
        UpdateLevelBasedStats();
        projectileSpeed = stats.projectileSpeed * DataManager.inst.globalAttributes.combatAreaScale;
        if (combatMode == CombatMode.Object) {
            damagePointOffset = Vec3.right * stats.damagePointOffset * DataManager.inst.globalAttributes.combatAreaScale * isPlayer.ToMultiplier();
        }
        UpdateRowPositions(false);
        var distancePerRow = MathF.Abs(leftAreaRows[0].x - leftAreaRows[1].x);
        combatAreaBoundaryLeft = leftAreaRows[2].x - distancePerRow;
        combatAreaBoundaryRight = rightAreaRows[2].x + distancePerRow;
        ApplyExistingPermanentStatusEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
    }

    public override void UpdateLevelBasedStats()
    {
        base.UpdateLevelBasedStats();
        if (weapon.combatLevel == 1) {
            damageFixed = statsGeneral.damage1Fixed;
            damageMin = statsGeneral.damage1Min;
            damageMax = statsGeneral.damage1Max;
            touchDamageMultiplier = stats.touchDamageMultiplier1;
        } else if (weapon.combatLevel == 2) {
            damageFixed = statsGeneral.damage2Fixed;
            damageMin = statsGeneral.damage2Min;
            damageMax = statsGeneral.damage2Max;
            touchDamageMultiplier = stats.touchDamageMultiplier2;
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
        CombatFunctions.HandleStatusEffects(this, deltaTime);
        OnUpdateHealthBar(deltaTime);
    }

    public override void UpdateTarget(){}

    public override void UpdateAnimator(){}

    public override void ActIfReady(){}

    public CombatAction ModifyCombatAction(CombatWeapon target, CombatAction combatAction, bool isTouched)
    {
        var multiplier = isTouched ? touchDamageMultiplier : 1f;
        if (target.isPlayer == isPlayer) {
            multiplier *= stats.friendlyDamageMultiplier;
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
        if (DataManager.inst.isRandomized) {
            return CombatFunctions.GetCombatAction(rnd, statusEffects, this, allyCombatWeapons, allyRowsList, damageMin, damageMax);
        } else {
            return CombatFunctions.GetCombatAction(rnd, statusEffects, this, allyCombatWeapons, allyRowsList, damageFixed, damageFixed);
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
                var positionBottomDifference = target.positionFromBottom - positionFromBottom;
                if (positionBottomDifference == 0) {
                    CombatFunctions.ApplyActionToTarget(target, this, ModifyCombatAction(target, combatAction, false));
                } else if (positionBottomDifference == 1 || positionBottomDifference == -1) {
                    CombatFunctions.ApplyActionToTarget(target, this, ModifyCombatAction(target, combatAction, true));
                }
            }
        }
    }
}
