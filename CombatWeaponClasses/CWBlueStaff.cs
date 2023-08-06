using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CWBlueStaff : CW
{
    public new WInfoBlueStaff weaponInfo => base.weaponInfo as WInfoBlueStaff;
    public float actionSpeedMultiplier;
    public List_<CW> targetWeapons;

    public event Action onUpdateLines;

    public CWBlueStaff(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, ref rnd)
    {
        UpdateLevelBasedStats();
        targetWeapons = new List_<CW>(3);
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
            actionSpeedMultiplier = weaponInfo.actionSpeedMultiplier1;
        } else if (weapon.combatLevel == 2) {
            actionSpeedMultiplier = weaponInfo.actionSpeedMultiplier2;
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
            OnUpdateHealthBar(deltaTime);
        }
    }

    public override void UpdateTarget()
    {
        if (rowNumber == 1) {
            targetWeapons = new List_<CW>(3);
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
        var effect = new Effect(weaponInfo.effectInfo_speedBuff);
        effect.isSenderPlayersWeapon = isPlayer;
        effect.senderMatchRosterIndex = weapon.matchRosterIndex;
        effect.actionSpeedMultiplier = actionSpeedMultiplier;

        var action = new CombatAction(0, isPlayer, weapon.matchRosterIndex);
        action.effect = effect;

        return action;
    }

    public override void ReceiveAction(CombatAction action)
    {
        CombatFunctions.ReceiveAction(this, action);
        OnReceiveAction(action);
        if (weapon.attachment == AttachmentType.Repel) {
            if (isPlayer == action.isSenderPlayersWeapon)
                return;
            var enemyList = isPlayer ? enemyCWs : playerCWs;
            var target = enemyList.FirstOrDefault(cw => cw.id == action.senderId);
            CombatFunctions.ApplyResponseAction(target, this, CombatMain.attachmentAttributes.repel_Value, CombatMain.attachmentAttributes.repel_Value);
        }
        OnUpdateHealthBar(0f);
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
