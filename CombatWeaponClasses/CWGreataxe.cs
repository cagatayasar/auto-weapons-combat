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

    int damageFixed;
    int damageMin;
    int damageMax;
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
        ApplyExistingPermanentStatusEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        OnAnimatorSetFloat("attackSpeed", "greataxe_anim_upwardattack", 1f / (actionTimePeriod * weaponInfo.animationNonidlePortionMin));
    }

    public override void UpdateLevelBasedStats()
    {
        base.UpdateLevelBasedStats();
        if (weapon.combatLevel == 1) {
            damageFixed = base.weaponInfo.damageFixed1;
            damageMin = base.weaponInfo.damageMin1;
            damageMax = base.weaponInfo.damageMax1;
        } else if (weapon.combatLevel == 2) {
            damageFixed = base.weaponInfo.damageFixed2;
            damageMin = base.weaponInfo.damageMin2;
            damageMax = base.weaponInfo.damageMax2;
        }
    }

    public override void Update(float deltaTime)
    {
        seActionSpeedMultiplier = CombatFunctions.HandleStatusEffects(this, deltaTime);
        if (!isDead)
        {
            timePassed += deltaTime;
            actionTimePassed += deltaTime * seActionSpeedMultiplier;

            UpdateAnimator();
            ActIfReady();

            OnUpdateHealthBar(deltaTime);
        }
    }

    public override void UpdateTarget(){}

    public override void UpdateAnimator()
    {
        if (prevSeActionSpeedMultiplier == seActionSpeedMultiplier) return;
        prevSeActionSpeedMultiplier = seActionSpeedMultiplier;

        float actionSpeed = seActionSpeedMultiplier / actionTimePeriod;
        float animationAttackPortion;
        if (actionSpeed < weaponInfo.actionSpeedForNonidleMin)
            animationAttackPortion = weaponInfo.animationNonidlePortionMin;
        else if (actionSpeed > weaponInfo.actionSpeedForNonidleMax)
            animationAttackPortion = weaponInfo.animationNonidlePortionMax;
        else {
            float actionSpeedForNonidleNormalized = (actionSpeed - weaponInfo.actionSpeedForNonidleMin) / (weaponInfo.actionSpeedForNonidleMax - weaponInfo.actionSpeedForNonidleMin);
            float actionSpeedForNonidleMapped = actionSpeedForNonidleNormalized * (weaponInfo.animationNonidlePortionMax - weaponInfo.animationNonidlePortionMin);
            animationAttackPortion = weaponInfo.animationNonidlePortionMin + actionSpeedForNonidleMapped;
        }
        attackTriggerTime = actionTimePeriod * (1f - animationAttackPortion);
        damage1TriggerTime = attackTriggerTime + (actionTimePeriod - attackTriggerTime) * weaponInfo.attack1DamageEnemyPortion;
        damage2TriggerTime = attackTriggerTime + (actionTimePeriod - attackTriggerTime) * weaponInfo.attack2DamageEnemyPortion;
        damage3TriggerTime = attackTriggerTime + (actionTimePeriod - attackTriggerTime) * weaponInfo.attack3DamageEnemyPortion;

        OnAnimatorSetFloat("attackSpeed", "greataxe_anim_upwardattack", 1f / ((actionTimePeriod / seActionSpeedMultiplier) * animationAttackPortion));
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
                    damagedEnemy1 = GetTargetWithRelativePos(targetRowsList[0], positionFromBottom + 1);
                    if (damagedEnemy1 != null) {
                        CombatFunctions.ApplyActionToTarget(damagedEnemy1, this, GetCombatAction());
                    }
                } break;
            }
            case GreataxeState.AttackingDownwardPhase2:
            {
                if (actionTimePassed >= damage2TriggerTime) {
                    greataxeState = GreataxeState.AttackingDownwardPhase3;
                    damagedEnemy2 = GetTargetWithRelativePos(targetRowsList[0], positionFromBottom);
                    if (damagedEnemy2 != null && damagedEnemy2 != damagedEnemy1) {
                        CombatFunctions.ApplyActionToTarget(damagedEnemy2, this, GetCombatAction());
                    }
                } break;
            }
            case GreataxeState.AttackingDownwardPhase3:
            {
                if (actionTimePassed >= damage3TriggerTime) {
                    greataxeState = GreataxeState.AttackingDownwardPhase4;
                    var targetEnemy = GetTargetWithRelativePos(targetRowsList[0], positionFromBottom - 1);
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
                    damagedEnemy1 = GetTargetWithRelativePos(targetRowsList[0], positionFromBottom - 1);
                    if (damagedEnemy1 != null) {
                        CombatFunctions.ApplyActionToTarget(damagedEnemy1, this, GetCombatAction());
                    }
                } break;
            }
            case GreataxeState.AttackingUpwardPhase2:
            {
                if (actionTimePassed >= damage2TriggerTime) {
                    greataxeState = GreataxeState.AttackingUpwardPhase3;
                    damagedEnemy2 = GetTargetWithRelativePos(targetRowsList[0], positionFromBottom);
                    if (damagedEnemy2 != null && damagedEnemy2 != damagedEnemy1) {
                        CombatFunctions.ApplyActionToTarget(damagedEnemy2, this, GetCombatAction());
                    }
                } break;
            }
            case GreataxeState.AttackingUpwardPhase3:
            {
                if (actionTimePassed >= damage3TriggerTime) {
                    greataxeState = GreataxeState.AttackingUpwardPhase4;
                    var targetEnemy = GetTargetWithRelativePos(targetRowsList[0], positionFromBottom + 1);
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

    public static CW GetTargetWithRelativePos(List<CW> row, int positionFromBottom)
    {
        for (int i = 0; i < row.Count; i++) {
            if (row[i].positionFromBottom == positionFromBottom) {
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
            return CombatFunctions.GetCombatAction(rnd, statusEffects, this, allyCWs, allyRowsList, damageMin, damageMax);
        } else {
            return CombatFunctions.GetCombatAction(rnd, statusEffects, this, allyCWs, allyRowsList, damageFixed, damageFixed);
        }
    }

    public override void ReceiveAction(CombatAction action)
    {
        base.ReceiveAction(action);
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
