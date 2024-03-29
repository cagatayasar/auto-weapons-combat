﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoWeapons {

public class CWTorch : CW
{
    public new WInfoTorch weaponInfo => base.weaponInfo as WInfoTorch;
    public int damage;
    public List<CW> targetEnemies;

    public Action onUpdateLines;

    public CWTorch(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, ref System.Random rnd)
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
            damage = weaponInfo.damage1;
        } else if (weapon.combatLevel == 2) {
            damage = weaponInfo.damage2;
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
        if (rowNumber > 1 || targetRowsList.Count == 0) {
            targetEnemies = new List<CW>();
        } else {
            targetEnemies = targetRowsList[0];
        }
    }

    public override void UpdateAnimator(){}

    public override void ActIfReady()
    {
        if (actionTimePassed >= actionTimePeriod) {
            actionTimePassed = 0f;
            var count = targetEnemies?.Count;
            for (int i = 0; i < count; i++) {
                CombatFunctions.ApplyActionToTarget(targetEnemies[i], this, GetCombatAction());
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
        return CombatFunctions.GetCombatAction(rnd, effects, this, allyCWs, allyRowsList, damage, damage);
    }

    public override void ReceiveAction(CombatAction action)
    {
        base.ReceiveAction(action);
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
}
