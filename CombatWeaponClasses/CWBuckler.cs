using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CWBuckler : CW
{
    public new WInfoBuckler weaponInfo => base.weaponInfo as WInfoBuckler;
    bool isImmunityActive;
    float immunityTimer;

    public event Action<float> onUpdateImmunity;

    public CWBuckler(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, ref System.Random rnd)
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
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

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
        if (immunityTimer > weaponInfo.immunityLength) {
            isImmunityActive = false;
            immunityTimer = 0f;
        }
        onUpdateImmunity?.Invoke(immunityTimer / weaponInfo.immunityLength);
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
                immunityTimer = weaponInfo.immunityLength * 0.25f;
                UpdateImmunity(0f);
            }
        }

        if (weapon.attachment == AttachmentType.Repel) {
            if (isPlayer == action.isSenderPlayersWeapon)
                return;
            var enemyList = isPlayer ? enemyCWs : playerCWs;
            var target = enemyList.FirstOrDefault(cw => cw.id == action.senderId);
            CombatFunctions.ApplyResponseAction(target, this, CombatMain.attachmentAttributes.repel_Value, CombatMain.attachmentAttributes.repel_Value);
        }
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
