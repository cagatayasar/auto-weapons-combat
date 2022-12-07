using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CWDarkSword : CombatWeapon, ICWCancelTransition, ICWStackWeapon
{
    public MeleeDoubleAttackState meleeDoubleAttackState = MeleeDoubleAttackState.Idle;
    MeleeTargetType targetType = MeleeTargetType.Null;

    StatsDarkSword stats;
    int damageFixed;
    int damageMin;
    int damageMax;
    CombatWeapon damagedTargetEnemy;

    int damagePerStack;

    // animation variables
    float animationUpwardAttackClipLength;
    float animationFrontAttackClipLength;

    float attackTriggerTime;
    float damageUpward1TriggerTime;
    float damageUpward2TriggerTime;
    float damageFront1TriggerTime;
    float damageFront2TriggerTime;

    public event Action onUpdateStackUI;

    public CWDarkSword(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, CombatMode combatMode, ref System.Random rnd)
        : base(weapon, playerEnemyData, id, isPlayer, combatMode, ref rnd)
    {
        stats = DataManager.inst.weaponsPackage.darkSword;
        statsGeneral = stats.statsGeneral;
        UpdateLevelBasedStats();
        ApplyExistingPermanentStatusEffects();
    }

    public override void InvokeInitializationEvents()
    {
        base.InvokeInitializationEvents();
        OnAnimatorSetFloat("attackSpeedUpward", "darksword_anim_upwardattack", 1f / (actionTimePeriod * stats.animationNonidlePortionMin));
        OnAnimatorSetFloat("attackSpeedFront", "darksword_anim_frontattack", 1f / (actionTimePeriod * stats.animationNonidlePortionMin));
    }

    public override void UpdateLevelBasedStats()
    {
        base.UpdateLevelBasedStats();
        if (weapon.combatLevel == 1) {
            damageFixed = statsGeneral.damage1Fixed;
            damageMin = statsGeneral.damage1Min;
            damageMax = statsGeneral.damage1Max;
            maxStacks = stats.maxStackAmount1;
            damagePerStack = stats.damagePerStack1;
        } else if (weapon.combatLevel == 2) {
            damageFixed = statsGeneral.damage2Fixed;
            damageMin = statsGeneral.damage2Min;
            damageMax = statsGeneral.damage2Max;
            maxStacks = stats.maxStackAmount2;
            damagePerStack = stats.damagePerStack2;
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
            UpdateAnimator();
            if (meleeDoubleAttackState == MeleeDoubleAttackState.Canceling)
                UpdateTransition(deltaTime);
            ActIfReady();

            OnUpdateHealthBar(deltaTime);
        }
    }

    public override void UpdateTarget()
    {
        prevTargetEnemy = targetEnemy;
        targetEnemy = CombatFunctions.TargetEnemyMelee(this, targetEnemy, targetRowsList, false);
    }

    public void IncrementStack()
    {
        stacks++;
        onUpdateStackUI?.Invoke();
    }

    public override void UpdateAnimator()
    {
        if (prevSeActionSpeedMultiplier == seActionSpeedMultiplier) return;
        prevSeActionSpeedMultiplier = seActionSpeedMultiplier;

        float actionSpeed = seActionSpeedMultiplier / actionTimePeriod;
        float animationAttackPortion;
        if (actionSpeed < stats.actionSpeedForAnimationMin)
            animationAttackPortion = stats.animationNonidlePortionMin;
        else if (actionSpeed > stats.actionSpeedForAnimationMax)
            animationAttackPortion = stats.animationNonidlePortionMax;
        else {
            float actionSpeedForNonidleNormalized = (actionSpeed - stats.actionSpeedForAnimationMin) / (stats.actionSpeedForAnimationMax - stats.actionSpeedForAnimationMin);
            float actionSpeedForNonidleMapped = actionSpeedForNonidleNormalized * (stats.animationNonidlePortionMax - stats.animationNonidlePortionMin);
            animationAttackPortion = stats.animationNonidlePortionMin + actionSpeedForNonidleMapped;
        }
        attackTriggerTime = actionTimePeriod * (1f - animationAttackPortion);
        damageUpward1TriggerTime = attackTriggerTime + (actionTimePeriod - attackTriggerTime) * stats.animationUpwardDamage1Portion;
        damageUpward2TriggerTime = attackTriggerTime + (actionTimePeriod - attackTriggerTime) * stats.animationUpwardDamage2Portion;
        damageFront1TriggerTime = attackTriggerTime + (actionTimePeriod - attackTriggerTime) * stats.animationFrontDamage1Portion;
        damageFront2TriggerTime = attackTriggerTime + (actionTimePeriod - attackTriggerTime) * stats.animationFrontDamage2Portion;

        OnAnimatorSetFloat("attackSpeedUpward", "darksword_anim_upwardattack", 1f / ((actionTimePeriod / seActionSpeedMultiplier) * animationAttackPortion));
        OnAnimatorSetFloat("attackSpeedFront", "darksword_anim_frontattack", 1f / ((actionTimePeriod / seActionSpeedMultiplier) * animationAttackPortion));
    }

    public override void ActIfReady()
    {
        if (meleeDoubleAttackState == MeleeDoubleAttackState.Attacking1
            && (targetEnemy == null || targetEnemy != prevTargetEnemy
                || (targetType == MeleeTargetType.Upward && targetEnemy.positionFromBottom != positionFromBottom + 1)
                || (targetType == MeleeTargetType.Front  && targetEnemy.positionFromBottom != positionFromBottom)))
        {
            meleeDoubleAttackState = MeleeDoubleAttackState.Canceling;
            CancelTransition();
            return;
        }

        if (meleeDoubleAttackState == MeleeDoubleAttackState.Idle && actionTimePassed >= attackTriggerTime && targetEnemy != null)
        {
            actionTimePassed = attackTriggerTime;
            meleeDoubleAttackState = MeleeDoubleAttackState.Attacking1;
            if (targetEnemy.positionFromBottom > positionFromBottom) {
                targetType = MeleeTargetType.Upward;
                OnAnimatorSetTrigger("attackUpward");
            } else {
                targetType = MeleeTargetType.Front;
                OnAnimatorSetTrigger("attackFront");
            }
        }
        else if (meleeDoubleAttackState == MeleeDoubleAttackState.Attacking1 && targetType == MeleeTargetType.Upward
            && actionTimePassed >= damageUpward1TriggerTime)
        {
            actionTimePassed = damageUpward1TriggerTime;
            CombatFunctions.ApplyActionToTarget(targetEnemy, this, GetCombatAction());
            damagedTargetEnemy = targetEnemy;
            meleeDoubleAttackState = MeleeDoubleAttackState.Attacking2;
        }
        else if (meleeDoubleAttackState == MeleeDoubleAttackState.Attacking1 && targetType == MeleeTargetType.Front
            && actionTimePassed >= damageFront1TriggerTime)
        {
            actionTimePassed = damageFront1TriggerTime;
            CombatFunctions.ApplyActionToTarget(targetEnemy, this, GetCombatAction());
            damagedTargetEnemy = targetEnemy;
            meleeDoubleAttackState = MeleeDoubleAttackState.Attacking2;
        }
        else if (meleeDoubleAttackState == MeleeDoubleAttackState.Attacking2 && targetType == MeleeTargetType.Upward
            && actionTimePassed >= damageUpward2TriggerTime)
        {
            actionTimePassed = damageUpward2TriggerTime;
            meleeDoubleAttackState = MeleeDoubleAttackState.Returning;
            if (targetEnemy != null && targetEnemy == damagedTargetEnemy) {
                CombatFunctions.ApplyActionToTarget(targetEnemy, this, GetCombatAction());
            }
        }
        else if (meleeDoubleAttackState == MeleeDoubleAttackState.Attacking2 && targetType == MeleeTargetType.Front
            && actionTimePassed >= damageFront2TriggerTime)
        {
            actionTimePassed = damageFront2TriggerTime;
            meleeDoubleAttackState = MeleeDoubleAttackState.Returning;
            if (targetEnemy != null && targetEnemy == damagedTargetEnemy) {
                CombatFunctions.ApplyActionToTarget(targetEnemy, this, GetCombatAction());
            }
        }
        else if (meleeDoubleAttackState == MeleeDoubleAttackState.Returning && actionTimePassed >= actionTimePeriod)
        {
            meleeDoubleAttackState = MeleeDoubleAttackState.Idle;
            actionTimePassed = 0f;
        }
    }

    public void OnCancelTransitionEnd()
    {
        meleeDoubleAttackState = MeleeDoubleAttackState.Idle;
    }

    public override void PrepareCombatStart(int roundCount)
    {
        PrepareCombatStartHP();
        PrepareCombatStartOther(roundCount);
        PrepareCombatStartSpeed();
    }

    public override CombatAction GetCombatAction()
    {
        if (DataManager.inst.isRandomized) {
            return CombatFunctions.GetCombatAction(rnd, statusEffects, this, allyCombatWeapons, allyRowsList,
                damageMin + stacks * damagePerStack, damageMax + stacks * damagePerStack);
        } else {
            return CombatFunctions.GetCombatAction(rnd, statusEffects, this, allyCombatWeapons, allyRowsList,
                damageFixed + stacks * damagePerStack, damageFixed + stacks * damagePerStack);
        }
    }

    public override void ReceiveAction(CombatAction action)
    {
        base.ReceiveAction(action);
    }

    public override void ReportClearedRow(int rowNumber, bool isPlayersRow) {}
}
