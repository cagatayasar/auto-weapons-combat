using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CWBuckler : CombatWeapon
{
    public StatsBuckler stats;
    bool isImmunityActive;
    float immunityTimer;

    public event Action<float> onUpdateImmunity;

    public CWBuckler(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, CombatMode combatMode, ref Unity.Mathematics.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, combatMode, ref rnd)
    {
        stats = DataManager.inst.weaponsPackage.buckler;
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
    }

    public override void Update(float deltaTime)
    {
        CombatFunctions.HandleStatusEffects(this, deltaTime);
        if (!isDead) {
            if (isImmunityActive) {
                UpdateImmunity(deltaTime);
            }
            OnUpdateHealthBar(deltaTime);
        }
    }

    public override void UpdateTarget(){}

    public override void UpdateAnimator(){}

    public void UpdateImmunity(float deltaTime)
    {
        immunityTimer += deltaTime;
        if (immunityTimer > stats.immunityLength) {
            isImmunityActive = false;
            immunityTimer = 0f;
        }
        onUpdateImmunity?.Invoke(immunityTimer / stats.immunityLength);
    }

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
        if (action.isSenderPlayersWeapon == isPlayer) {
            CombatFunctions.ReceiveAction(this, action);
            OnReceiveAction(action);
            OnUpdateHealthBar(0f);
        } else if (!isImmunityActive) {
            CombatFunctions.ReceiveAction(this, action);
            OnReceiveAction(action);
            OnUpdateHealthBar(0f);
            isImmunityActive = true;
            if (action.isAttackerThrown) {
                immunityTimer = stats.immunityLength * 0.25f;
                UpdateImmunity(0f);
            }
        }

        if (weapon.attachment == AttachmentType.Repel) {
            if (isPlayer == action.isSenderPlayersWeapon)
                return;
            var enemyList = isPlayer ? enemyCombatWeapons : playerCombatWeapons;
            var target = enemyList.FirstOrDefault(cw => cw.id == action.senderId);
            CombatFunctions.ApplyResponseAction(target, this, CombatInfos.attachmentAttributes.repel_Value, CombatInfos.attachmentAttributes.repel_Value);
        }
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
