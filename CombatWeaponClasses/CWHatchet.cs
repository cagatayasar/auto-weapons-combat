using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoWeapons {

public class CWHatchet : CW, ICWThrown
{
    public new WInfoHatchet weaponInfo => base.weaponInfo as WInfoHatchet;

    float animationPrepareClipLength;

    public CWHatchet(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, ref rnd)
    {
        UpdateLevelBasedStats();
        _30DegreesRotationDuration = weaponInfo._30DegreesRotationDuration;
        ApplyExistingPermanentEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        onAnimatorSetFloat?.Invoke("prepareSpeed", "hatchet_anim_prepare", weaponInfo.animationThrowPrepareSpeed);
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

        onUpdateHealthBar?.Invoke(deltaTime);
    }

    public override void UpdateTarget()
    {
        int rangeToAddOrSubstract = CombatFunctions.GetRangeToAddOrSubstract(this, range, 0f, oneLessRangeForXSeconds);
        if (weapon.attachment == AttachmentType.Furthest) {
            targetEnemy = CombatFunctions.TargetEnemyRangedFurthest(this, range + rangeToAddOrSubstract, targetEnemy, targetRowsList);
        } else {
            targetEnemy = CombatFunctions.TargetEnemyRangedClosest(this, range + rangeToAddOrSubstract, targetEnemy, targetRowsList);
        }
    }

    public override void UpdateAnimator(){}

    public override void ActIfReady(){}

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
        if (targetEnemy != null && targetEnemy.isDead) {
            targetEnemy = null;
        }
        UpdateTarget();

        if (targetEnemy != null) {
            targetEnemy.ReceiveAction(GetCombatAction());
            isDead = true;
        }
    }
}
}
