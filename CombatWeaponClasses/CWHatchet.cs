using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CWHatchet : CombatWeapon, ICWThrown
{
    public new WInfoHatchet weaponInfo => base.weaponInfo as WInfoHatchet;
    int damageFixed;
    int damageMin;
    int damageMax;

    float animationPrepareClipLength;

    public CWHatchet(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, ref rnd)
    {
        UpdateLevelBasedStats();
        _30DegreesRotationDuration = weaponInfo._30DegreesRotationDuration;
        ApplyExistingPermanentStatusEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        OnAnimatorSetFloat("prepareSpeed", "hatchet_anim_prepare", weaponInfo.animationThrowPrepareSpeed);
    }

    public override void UpdateLevelBasedStats()
    {
        base.UpdateLevelBasedStats();
        if (weapon.combatLevel == 1) {
            range = weaponInfo.range1;
            damageFixed = base.weaponInfo.damage1Fixed;
            damageMin = base.weaponInfo.damage1Min;
            damageMax = base.weaponInfo.damage1Max;
        } else if (weapon.combatLevel == 2) {
            range = weaponInfo.range2;
            damageFixed = base.weaponInfo.damage2Fixed;
            damageMin = base.weaponInfo.damage2Min;
            damageMax = base.weaponInfo.damage2Max;
        }
    }

    public override void Update(float deltaTime)
    {
        CombatFunctions.HandleStatusEffects(this, deltaTime);
        OnUpdateHealthBar(deltaTime);
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
