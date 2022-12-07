using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CWGreenStaff : CombatWeapon
{
    public StatsGreenStaff stats;
    public int healAmount;
    public List<CombatWeapon> targetWeapons = new List<CombatWeapon>();

    public event Action onUpdateLines;

    public CWGreenStaff(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, CombatMode combatMode, ref Unity.Mathematics.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, combatMode, ref rnd)
    {
        stats = DataManager.inst.weaponsPackage.greenStaff;
        statsGeneral = stats.statsGeneral;
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
        if (weapon.combatLevel == 1) {
            healAmount = stats.heal1;
        } else if (weapon.combatLevel == 2) {
            healAmount = stats.heal2;
        }
    }

    public override void Update(float deltaTime)
    {
        seActionSpeedMultiplier = CombatFunctions.HandleStatusEffects(this, deltaTime);
        if (!isDead)
        {
            timePassed += deltaTime;
            actionTimePassed += deltaTime * seActionSpeedMultiplier;

            UpdateTarget();
            onUpdateLines?.Invoke();
            ActIfReady();
            OnUpdateHealthBar(deltaTime);
        }
    }

    public override void UpdateTarget()
    {
        if (rowNumber == 1) {
            targetWeapons = new List<CombatWeapon>();
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
            int count = targetWeapons.Count;
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
        CombatAction action = new CombatAction(0, isPlayer, weapon.matchRosterIndex);
        action.SetHealAmount(healAmount);

        return action;
    }

    public override void ReceiveAction(CombatAction action)
    {
        base.ReceiveAction(action);
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
