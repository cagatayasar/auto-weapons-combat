using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CWRapier : CombatWeapon
{
    public CWRapier(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, CombatMode combatMode, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, combatMode, ref rnd)
    {
        UpdateLevelBasedStats();
        ApplyExistingPermanentStatusEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
    }

    public override void UpdateLevelBasedStats()
    {
        base.UpdateLevelBasedStats();
    }

    public override void Update(float deltaTime)
    {
        CombatFunctions.HandleStatusEffects(this, deltaTime);
        OnUpdateHealthBar(deltaTime);
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
        base.ReceiveAction(action);
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
