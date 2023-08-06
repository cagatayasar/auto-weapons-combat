using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum GreataxeState
{
    IdleUpward,
    IdleDownward,
    AttackingDownwardPhase1,
    AttackingDownwardPhase2,
    AttackingDownwardPhase3,
    AttackingDownwardPhase4,
    AttackingUpwardPhase1,
    AttackingUpwardPhase2,
    AttackingUpwardPhase3,
    AttackingUpwardPhase4,
    Null
}

public class CWGreataxe : CW
{
    public new WInfoGreataxe weaponInfo => base.weaponInfo as WInfoGreataxe;

    public GreataxeState greataxeState = GreataxeState.IdleUpward;


    CW damagedEnemy1;
    CW damagedEnemy2;

    // animation variables
    float animationAttackClipLength;
    float animationAttackPortionMin;
    float animationAttackPortionMax;
    float actionSpeedForAnimationMin;
    float actionSpeedForAnimationMax;

    public float attackTriggerTime;
    float damage1TriggerTime;
    float damage2TriggerTime;
    float damage3TriggerTime;

    public CWGreataxe(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, ref rnd)
    {
        UpdateLevelBasedStats();
        ApplyExistingPermanentEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        OnAnimatorSetFloat("attackSpeed", "greataxe_anim_upwardattack", 1f / (actionTimePeriod * weaponInfo.animNonidlePortionMin));
    }

    public override void UpdateLevelBasedStats()
    {
        base.UpdateLevelBasedStats();
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (!isDead)
        {
            timePassed += deltaTime;
            actionTimePassed += deltaTime * effectSpeedMultiplier;

            UpdateAnimator();
            ActIfReady();

            OnUpdateHealthBar(deltaTime);
        }
    }

    public override void UpdateTarget(){}

    public override void UpdateAnimator()
    {
        if (prevEffectSpeedMultiplier == effectSpeedMultiplier) return;
        prevEffectSpeedMultiplier = effectSpeedMultiplier;

        float actionSpeed = effectSpeedMultiplier / actionTimePeriod;
        float animationAttackPortion;
        if (actionSpeed < weaponInfo.animNonidleSpeedMin)
            animationAttackPortion = weaponInfo.animNonidlePortionMin;
        else if (actionSpeed > weaponInfo.animNonidleSpeedMax)
            animationAttackPortion = weaponInfo.animNonidlePortionMax;
        else {
            float actionSpeedForNonidleNormalized = (actionSpeed - weaponInfo.animNonidleSpeedMin) / (weaponInfo.animNonidleSpeedMax - weaponInfo.animNonidleSpeedMin);
            float actionSpeedForNonidleMapped = actionSpeedForNonidleNormalized * (weaponInfo.animNonidlePortionMax - weaponInfo.animNonidlePortionMin);
            animationAttackPortion = weaponInfo.animNonidlePortionMin + actionSpeedForNonidleMapped;
        }
        attackTriggerTime = actionTimePeriod * (1f - animationAttackPortion);
        damage1TriggerTime = attackTriggerTime + (actionTimePeriod - attackTriggerTime) * weaponInfo.attack1DamageEnemyPortion;
        damage2TriggerTime = attackTriggerTime + (actionTimePeriod - attackTriggerTime) * weaponInfo.attack2DamageEnemyPortion;
        damage3TriggerTime = attackTriggerTime + (actionTimePeriod - attackTriggerTime) * weaponInfo.attack3DamageEnemyPortion;

        OnAnimatorSetFloat("attackSpeed", "greataxe_anim_upwardattack", 1f / ((actionTimePeriod / effectSpeedMultiplier) * animationAttackPortion));
    }

    public override void ActIfReady()
    {
        switch (greataxeState)
        {
            case GreataxeState.IdleUpward:
            {
                if (rowNumber == 1 && actionTimePassed >= attackTriggerTime) {
                    damagedEnemy1 = null;
                    damagedEnemy2 = null;
                    greataxeState = GreataxeState.AttackingDownwardPhase1;
                    OnAnimatorSetTrigger("attackDownward");
                } break;
            }
            case GreataxeState.AttackingDownwardPhase1:
            {
                if (actionTimePassed >= damage1TriggerTime) {
                    greataxeState = GreataxeState.AttackingDownwardPhase2;
                    damagedEnemy1 = GetTargetWithRelativePos(targetRowsList[0], verticalPosition + 1);
                    if (damagedEnemy1 != null) {
                        CombatFunctions.ApplyActionToTarget(damagedEnemy1, this, GetCombatAction());
                    }
                } break;
            }
            case GreataxeState.AttackingDownwardPhase2:
            {
                if (actionTimePassed >= damage2TriggerTime) {
                    greataxeState = GreataxeState.AttackingDownwardPhase3;
                    damagedEnemy2 = GetTargetWithRelativePos(targetRowsList[0], verticalPosition);
                    if (damagedEnemy2 != null && damagedEnemy2 != damagedEnemy1) {
                        CombatFunctions.ApplyActionToTarget(damagedEnemy2, this, GetCombatAction());
                    }
                } break;
            }
            case GreataxeState.AttackingDownwardPhase3:
            {
                if (actionTimePassed >= damage3TriggerTime) {
                    greataxeState = GreataxeState.AttackingDownwardPhase4;
                    var targetEnemy = GetTargetWithRelativePos(targetRowsList[0], verticalPosition - 1);
                    if (targetEnemy != null && targetEnemy != damagedEnemy1 && targetEnemy != damagedEnemy2) {
                        CombatFunctions.ApplyActionToTarget(targetEnemy, this, GetCombatAction());
                    }
                } break;
            }
            case GreataxeState.AttackingDownwardPhase4:
            {
                if (actionTimePassed >= actionTimePeriod) {
                    greataxeState = GreataxeState.IdleDownward;
                    actionTimePassed = 0f;
                } break;
            }
            case GreataxeState.IdleDownward:
            {
                if (rowNumber == 1 && actionTimePassed >= attackTriggerTime) {
                    damagedEnemy1 = null;
                    damagedEnemy2 = null;
                    greataxeState = GreataxeState.AttackingUpwardPhase1;
                    OnAnimatorSetTrigger("attackUpward");
                } break;
            }
            case GreataxeState.AttackingUpwardPhase1:
            {
                if (actionTimePassed >= damage1TriggerTime) {
                    greataxeState = GreataxeState.AttackingUpwardPhase2;
                    damagedEnemy1 = GetTargetWithRelativePos(targetRowsList[0], verticalPosition - 1);
                    if (damagedEnemy1 != null) {
                        CombatFunctions.ApplyActionToTarget(damagedEnemy1, this, GetCombatAction());
                    }
                } break;
            }
            case GreataxeState.AttackingUpwardPhase2:
            {
                if (actionTimePassed >= damage2TriggerTime) {
                    greataxeState = GreataxeState.AttackingUpwardPhase3;
                    damagedEnemy2 = GetTargetWithRelativePos(targetRowsList[0], verticalPosition);
                    if (damagedEnemy2 != null && damagedEnemy2 != damagedEnemy1) {
                        CombatFunctions.ApplyActionToTarget(damagedEnemy2, this, GetCombatAction());
                    }
                } break;
            }
            case GreataxeState.AttackingUpwardPhase3:
            {
                if (actionTimePassed >= damage3TriggerTime) {
                    greataxeState = GreataxeState.AttackingUpwardPhase4;
                    var targetEnemy = GetTargetWithRelativePos(targetRowsList[0], verticalPosition + 1);
                    if (targetEnemy != null && targetEnemy != damagedEnemy1 && targetEnemy != damagedEnemy2) {
                        CombatFunctions.ApplyActionToTarget(targetEnemy, this, GetCombatAction());
                    }
                } break;
            }
            case GreataxeState.AttackingUpwardPhase4:
            {
                if (actionTimePassed >= actionTimePeriod) {
                    greataxeState = GreataxeState.IdleUpward;
                    actionTimePassed = 0f;
                }
            } break;
            default: break;
        }
    }

    public static CW GetTargetWithRelativePos(List_<CW> row, int positionFromBottom)
    {
        for (int i = 0; i < row.Count; i++) {
            if (row[i].verticalPosition == positionFromBottom) {
                return row[i];
            }
        }
        return null;
    }

    public override void PrepareCombatStart(int roundCount)
    {
        PrepareCombatStartHP();
        PrepareCombatStartOther(roundCount);
        PrepareCombatStartSpeed();
    }

    public override CombatAction GetCombatAction()
    {
        if (CombatMain.isRandomized) {
            return CombatFunctions.GetCombatAction(rnd, effects, this, allyCWs, allyRowsList, damageMin, damageMax);
        } else {
            return CombatFunctions.GetCombatAction(rnd, effects, this, allyCWs, allyRowsList, damageFixed, damageFixed);
        }
    }

    public override void ReceiveAction(CombatAction action)
    {
        base.ReceiveAction(action);
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
