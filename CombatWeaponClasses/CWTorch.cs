using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CWTorch : CombatWeapon
{
    public StatsTorch stats;
    public int damage;
    public List<CombatWeapon> targetEnemies = new List<CombatWeapon>();

    public event Action onUpdateLines;

    public CWTorch(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, CombatMode combatMode, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, combatMode, ref rnd)
    {
        stats = DataManager.inst.weaponsPackage.torch;
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
            damage = stats.damage1;
        } else if (weapon.combatLevel == 2) {
            damage = stats.damage2;
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
        if (rowNumber > 1 || targetRowsList.Count == 0) {
            targetEnemies = new List<CombatWeapon>();
        } else {
            targetEnemies = targetRowsList[0];
        }
    }

    public override void UpdateAnimator(){}

    public override void ActIfReady()
    {
        if (actionTimePassed >= actionTimePeriod) {
            actionTimePassed = 0f;
            var count = targetEnemies.Count;
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
        return CombatFunctions.GetCombatAction(rnd, statusEffects, this, allyCombatWeapons, allyRowsList, damage, damage);
    }

    public override void ReceiveAction(CombatAction action)
    {
        base.ReceiveAction(action);
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
