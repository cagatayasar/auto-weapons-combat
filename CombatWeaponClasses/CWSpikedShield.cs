﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoWeapons {

public class CWSpikedShield : CW
{
    public new WInfoSpikedShield weaponInfo => base.weaponInfo as WInfoSpikedShield;
    int responseDamageMin;
    int responseDamageMax;

    public CWSpikedShield(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, ref rnd)
    {
        UpdateLevelBasedStats();
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
            responseDamageMin = weaponInfo.responseDamageMin1;
            responseDamageMax = weaponInfo.responseDamageMax1;
        } else if (weapon.combatLevel == 2) {
            responseDamageMin = weaponInfo.responseDamageMin2;
            responseDamageMax = weaponInfo.responseDamageMax2;
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

    public override void PrepareCombatStart(int roundCount)
    {
        PrepareCombatStartHP();
        PrepareCombatStartOther(roundCount);
    }

    public override CombatAction GetCombatAction()
    {
        return new CombatAction();
    }

    public override void ReceiveAction(CombatAction action)
    {
        CombatFunctions.ReceiveAction(this, action);
        onReceiveAction?.Invoke(this, action);
        onUpdateHealthBar?.Invoke(0f);
        var addToResponseDamage = 0;
        if (weapon.attachment == AttachmentType.Repel) {
            addToResponseDamage = CombatMain.attachmentAttributes.repel_Value;
        }
        if (isPlayer == action.isSenderPlayersWeapon)
            return;

        var enemyList = isPlayer ? enemyCWs : playerCWs;
        var target = enemyList.FirstOrDefault(cw => cw.id == action.senderId);
        if (target != null) {
            var responseAction = new CombatAction(rnd.Next(responseDamageMin + addToResponseDamage, responseDamageMax + addToResponseDamage + 1),
                isPlayer, weapon.matchRosterIndex);
            CombatFunctions.ApplyActionToTarget(target, this, responseAction);
        }
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
}
