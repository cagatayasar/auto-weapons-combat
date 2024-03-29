﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoWeapons {

public class CWGreenStaff : CW
{
    public new WInfoGreenStaff weaponInfo => base.weaponInfo as WInfoGreenStaff;
    public int healAmount;
    public List<CW> targetWeapons;

    public Action onUpdateLines;

    public CWGreenStaff(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, ref System.Random rnd)
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
            healAmount = weaponInfo.heal1;
        } else if (weapon.combatLevel == 2) {
            healAmount = weaponInfo.heal2;
        }
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (!isDead)
        {
            timePassed += deltaTime;
            actionTimePassed += deltaTime * effectSpeedMultiplier;

            UpdateTarget();
            onUpdateLines?.Invoke();
            ActIfReady();
            onUpdateHealthBar?.Invoke(deltaTime);
        }
    }

    public override void UpdateTarget()
    {
        if (rowNumber == 1) {
            targetWeapons = new List<CW>();
        } else if (rowNumber == 2) {
            targetWeapons = allyRowsList[0];
        } else if (rowNumber == 3) {
            targetWeapons = allyRowsList[1];
        }
    }

    public override void UpdateAnimator(){}

    public override void ActIfReady()
    {
        if (actionTimePassed >= actionTimePeriod) {
            actionTimePassed = 0f;
            var count = targetWeapons?.Count;
            for (int i = 0; i < count; i++) {
                targetWeapons[i].ReceiveAction(GetCombatAction());
            }
        }
    }

    public override void PrepareCombatStart(int roundCount)
    {
        PrepareCombatStartHP();
        PrepareCombatStartOther(roundCount);
        PrepareCombatStartSpeed();
    }

    public override CombatAction GetCombatAction()
    {
        var action = new CombatAction(0, isPlayer, weapon.matchRosterIndex);
        action.healAmount = healAmount;

        return action;
    }

    public override void ReceiveAction(CombatAction action)
    {
        base.ReceiveAction(action);
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
}
