using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CWBlueStaff : CombatWeapon
{
    public StatsBlueStaff stats;
    public float actionSpeedMultiplier;
    public List<CombatWeapon> targetWeapons;

    public event Action onUpdateLines;

    public CWBlueStaff(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, CombatMode combatMode, ref Unity.Mathematics.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, combatMode, ref rnd)
    {
        stats = DataManager.inst.weaponsPackage.blueStaff;
        statsGeneral = stats.statsGeneral;
        UpdateLevelBasedStats();
        targetWeapons = new List<CombatWeapon>();
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
            actionSpeedMultiplier = stats.actionSpeedMultiplier1;
        } else if (weapon.combatLevel == 2) {
            actionSpeedMultiplier = stats.actionSpeedMultiplier2;
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
        if (actionTimePassed > 0.2f) {
            actionTimePassed = 0f;
            var count = targetWeapons.Count;
            for (int i = 0; i < count; i++) {
                targetWeapons[i].ReceiveAction(GetCombatAction());
            }
        }
    }

    public override void PrepareCombatStart(int roundCount)
    {
        PrepareCombatStartHP();
        PrepareCombatStartOther(roundCount);
    }

    public override CombatAction GetCombatAction()
    {
        CombatAction action = new CombatAction(0, isPlayer, weapon.matchRosterIndex);
        StatusEffect statusEffect = new StatusEffect(StatusEffectType.BlueStaffBoost, true);
        statusEffect.isSenderPlayersWeapon = action.isSenderPlayersWeapon;
        statusEffect.senderMatchRosterIndex = action.senderId;
        statusEffect.SetActionSpeedMultiplier(actionSpeedMultiplier);
        action.SetStatusEffect(statusEffect);

        return action;
    }

    public override void ReceiveAction(CombatAction action)
    {
        CombatFunctions.ReceiveAction(this, action);
        OnReceiveAction(action);
        if (weapon.attachment == AttachmentType.Repel) {
            if (isPlayer == action.isSenderPlayersWeapon)
                return;
            var enemyList = isPlayer ? enemyCombatWeapons : playerCombatWeapons;
            var target = enemyList.FirstOrDefault(cw => cw.id == action.senderId);
            CombatFunctions.ApplyResponseAction(target, this, CombatInfos.attachmentAttributes.repel_Value, CombatInfos.attachmentAttributes.repel_Value);
        }
        OnUpdateHealthBar(0f);
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
