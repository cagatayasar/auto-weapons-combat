using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class CombatFunctions
{
    //------------------------------------------------------------------------
    public static float HandleStatusEffects(CW cw, float deltaTime)
    {
        float seActionSpeedMultiplier = 1f;
        for (int i = 0; i < cw.statusEffects.Count; i++)
        {
            var se = cw.statusEffects[i];
            if (se.statusEffectType == StatusEffectType.BlueStaffBoost) {
                seActionSpeedMultiplier += se.actionSpeedMultiplier - 1f;
            }

            if (se.isTimed) {
                se.timeLeft -= deltaTime;
                if (se.timeLeft < 0f) {
                    cw.statusEffects.RemoveAt(i);
                    i--;
                }
            }
        }

        return seActionSpeedMultiplier;
    }

    //------------------------------------------------------------------------
    public static string GetFormation(List_<List_<CW>> rowsList)
    {
        string formation = "";
        foreach (var list in rowsList) {
            formation += list.Count;
        }
        return formation;
    }

    //------------------------------------------------------------------------
    public static string GetCombatState(List_<List_<CW>> rowsList)
    {
        string state = GetFormation(rowsList) + "_";

        foreach (var list in rowsList) {
            foreach (var cw in list)
                state += cw.weapon.matchRosterIndex;
        }

        return state;
    }

    //------------------------------------------------------------------------
    public static int GetRangeToAddOrSubstract(CW cw, int range, float timePassed, bool oneLessRangeForXSeconds)
    {
        int rangeToAddOrSubstract = 0;
        if (timePassed < CombatMain.itemAttributes.oneLessRange_Duration && oneLessRangeForXSeconds)
            rangeToAddOrSubstract--; // item5
        if (range + rangeToAddOrSubstract < 1)
            rangeToAddOrSubstract = 1 - range;
        if (cw.weapon.attachment == AttachmentType.Range)
            rangeToAddOrSubstract++;
        return rangeToAddOrSubstract;
    }

    //------------------------------------------------------------------------
    public static CombatAction GetCombatAction(System.Random rnd, List<StatusEffect> statusEffects, CW attacker, List_<CW> allyCWs, List_<List_<CW>> allyRowsList,
        int damageMin, int damageMax, int healthPercent = 0, float finalDmgMultiplierParameter = 1f)
    {
        int dmgAddAmount = 0;
        float dmgMultiplier = 1f;
        float finalDmgMultiplier = 1f;
        int damage = -1;
        for (int i = 0; i < statusEffects.Count; i++) {
            switch (statusEffects[i].statusEffectType)
            {
            case StatusEffectType.BonusAttackDmg:
                dmgMultiplier += CombatMain.itemAttributes.bonusDmg_Multiplier - 1f;
                break;
            case StatusEffectType.EachAttackOneLess:
                dmgAddAmount--;
                break;
            case StatusEffectType.EachAttackOneMore:
                dmgAddAmount++;
                break;
            case StatusEffectType.FirstAttacksDoubleDmg:
                dmgMultiplier += CombatMain.itemAttributes.firstAttacksBuff_Multiplier - 1f;
                statusEffects.RemoveAt(i);
                i--;
                break;
            case StatusEffectType.IfAloneBonusDmg:
                if (allyCWs.Count == 1)
                    dmgMultiplier += CombatMain.itemAttributes.ifAloneBonusDmg_Multiplier - 1f;
                break;
            case StatusEffectType.IfPerfectSquareBonusDmg:
                dmgMultiplier += CombatMain.itemAttributes.perfectSquareBonusDmg_Multiplier - 1f;
                break;
            case StatusEffectType.KillAndBoost:
                if (statusEffects[i].killAndBoost_flag) {
                    dmgMultiplier += CombatMain.itemAttributes.killAndBoostBoost_Multiplier - 1f;
                    var se = statusEffects[i];
                    se.killAndBoost_flag = false;
                    statusEffects[i] = se;
                }
                break;
            case StatusEffectType.FirstRowBuff:
                dmgMultiplier += CombatMain.itemAttributes.firstRowBuffDmg_Multiplier - 1f;
                break;
            case StatusEffectType.Gunslinger_OneEyeClosed:
                damage = damageMax;
                break;
            }
        }
        finalDmgMultiplier *= finalDmgMultiplierParameter;

        if (attacker.weapon.attachment == AttachmentType.Lucky) {
            damage = damageMax;
        } else if (attacker.attachment_firstAttackAvailable) {
            attacker.attachment_firstAttackAvailable = false;
            finalDmgMultiplier *= 2f;
        } else if (attacker.weapon.attachment == AttachmentType.Round) {
            dmgAddAmount += attacker.roundCount;
        } else if (attacker.weapon.attachment == AttachmentType.RowMoreDamage) {
            dmgAddAmount += (allyRowsList[attacker.rowNumber - 1].Count - 1) * CombatMain.attachmentAttributes.rowMoreDamage_Value;
        }

        if (finalDmgMultiplier < 0f)
            finalDmgMultiplier = 0f;
        if (damage == -1) {
            damage = rnd.Next(damageMin, damageMax + 1);
        }
        var action = new CombatAction(MathCustom.RoundToInt(finalDmgMultiplier * (dmgMultiplier * damage + dmgAddAmount)), attacker.isPlayer, attacker.id);
        action.isAttackerThrown = attacker is ICWThrown;
        if (healthPercent > 0) {
            action.SetHealthPercentDamage(MathCustom.RoundToInt(finalDmgMultiplier * dmgMultiplier * healthPercent), dmgAddAmount);
        }
        return action;
    }

    //------------------------------------------------------------------------
    public static CW TargetEnemyMelee(CW attacker, CW prevTargetEnemy,
        List_<List_<CW>> targetRowsList, bool canAttackDownwards = true)
    {
        if (targetRowsList.Count == 0)
            return null;

        bool redirectActive = attacker.itemRedirect_active && canAttackDownwards;
        CW targetEnemy = null;
        if (attacker.rowNumber == 1)
        {
            var targetRow = targetRowsList[0];
            if (prevTargetEnemy == null)
            {
                var targetRowCount = targetRow.Count;
                if (targetRowCount == 1 && MathF.Abs(attacker.positionFromBottom - 3) <= 1)
                    targetEnemy = targetRow[0];
                else if (targetRowCount == 2)
                {
                    if (attacker.positionFromBottom <= 2)
                        targetEnemy = targetRow[0];
                    else if (attacker.positionFromBottom >= 4)
                        targetEnemy = targetRow[1];
                    else
                        targetEnemy = targetRow[redirectActive ? 0 : 1];
                }
                else if (targetRowCount == 3)
                {
                    targetEnemy = attacker.positionFromBottom switch
                    {
                        1 => targetRow[0],
                        2 => targetRow[redirectActive ? 0 : 1],
                        3 => targetRow[1],
                        4 => targetRow[redirectActive ? 1 : 2],
                        5 => targetRow[2],
                        _ => targetEnemy
                    };
                }
            }
            else if (!targetRow.Contains_(prevTargetEnemy)
                   || MathF.Abs(attacker.positionFromBottom - prevTargetEnemy.positionFromBottom) > 1)
                targetEnemy = null;
            else
                targetEnemy = prevTargetEnemy;

            if (prevTargetEnemy != null) {
                if (!canAttackDownwards && attacker.positionFromBottom > prevTargetEnemy?.positionFromBottom) {
                    targetEnemy = null;
                }
            }
        }

        return targetEnemy;
    }

    //------------------------------------------------------------------------
    public static CW TargetEnemyRanged(CW attacker, int range, CW prevTargetEnemy,
        List_<List_<CW>> targetRowsList, float timePassed, bool oneLessRangeForXSeconds)
    {
        int rangeToAddOrSubstract = CombatFunctions.GetRangeToAddOrSubstract(attacker, range, timePassed, oneLessRangeForXSeconds);
        if (attacker.weapon.attachment == AttachmentType.Furthest) {
            return CombatFunctions.TargetEnemyRangedFurthest(attacker, range + rangeToAddOrSubstract, prevTargetEnemy, targetRowsList);
        } else {
            return CombatFunctions.TargetEnemyRangedClosest(attacker, range + rangeToAddOrSubstract, prevTargetEnemy, targetRowsList);
        }
    }

    //------------------------------------------------------------------------
    public static CW TargetEnemyRangedClosest(CW attacker, int range, CW prevTargetEnemy,
        List_<List_<CW>> targetRowsList)
    {
        if (attacker.rowNumber > range || targetRowsList.Count == 0)
            return null;

        bool redirectActive = attacker.itemRedirect_active;
        CW targetEnemy = null;
        if (prevTargetEnemy == null)
        {
            var targetRow = targetRowsList[0];

            if (targetRow.Count == 1)
                targetEnemy = targetRow[0];
            else if (targetRow.Count == 2)
            {
                if (attacker.positionFromBottom <= 2)
                    targetEnemy = targetRow[0];
                else if (attacker.positionFromBottom >= 4)
                    targetEnemy = targetRow[1];
                else
                    targetEnemy = targetRow[redirectActive ? 0 : 1];
            }
            else if (targetRow.Count == 3)
            {
                targetEnemy = attacker.positionFromBottom switch
                {
                    1 => targetRow[0],
                    2 => targetRow[redirectActive ? 0 : 1],
                    3 => targetRow[1],
                    4 => targetRow[redirectActive ? 1 : 2],
                    5 => targetRow[2],
                    _ => targetEnemy
                };
            }
        }
        else
        {
            bool contains = false;
            for (int i = 0; i < targetRowsList.Count; i++) {
                if (targetRowsList[i].Contains_(prevTargetEnemy)) {
                    contains = true;
                    break;
                }
            }

            if (!contains || (prevTargetEnemy.rowNumber + attacker.rowNumber - range > 1))
                targetEnemy = null;
            else
                targetEnemy = prevTargetEnemy;
        }
        return targetEnemy;
    }

    //------------------------------------------------------------------------
    public static CW TargetEnemyRangedFurthest(CW attacker, int range, CW prevTargetEnemy,
        List_<List_<CW>> targetRowsList)
    {
        if (attacker.rowNumber > range || targetRowsList.Count == 0)
            return null;

        bool redirectActive = attacker.itemRedirect_active;
        CW targetEnemy = null;
        if (prevTargetEnemy == null)
        {
            List_<CW> targetRow = null;
            for (int i = targetRowsList.Count - 1; i >= 0; i--)
            {
                if (range - attacker.rowNumber >= i)
                {
                    targetRow = targetRowsList[i];
                    break;
                }
            }

            var targetRowCount = targetRow?.Count;
            if (targetRowCount == 1)
                targetEnemy = targetRow[0];
            else if (targetRowCount == 2)
            {
                if (attacker.positionFromBottom <= 2)
                    targetEnemy = targetRow[1];
                else if (attacker.positionFromBottom >= 4)
                    targetEnemy = targetRow[0];
                else
                    targetEnemy = targetRow[redirectActive ? 0 : 1];
            }
            else if (targetRowCount == 3)
            {
                if (attacker.positionFromBottom <= 2)
                    targetEnemy = targetRow[2];
                else if (attacker.positionFromBottom >= 4)
                    targetEnemy = targetRow[0];
                else
                    targetEnemy = targetRow[redirectActive ? 0 : 2];
            }
        }
        else
        {
            bool contains = false;
            for (int i = 0; i < targetRowsList.Count; i++) {
                if (targetRowsList[i].Contains_(prevTargetEnemy)) {
                    contains = true;
                    break;
                }
            }

            if (!contains || (prevTargetEnemy.rowNumber + attacker.rowNumber - range > 1))
                targetEnemy = null;
            else
                targetEnemy = prevTargetEnemy;
        }
        return targetEnemy;
    }

    //------------------------------------------------------------------------
    public static void UpdateRowPositionsPreparation(Vec3[] leftAreaRows, Vec3[] rightAreaRows, int playerRowsCount, int enemyRowsCount)
    {
        if (playerRowsCount == 1) {
            leftAreaRows[1]  = CombatFunctions.GetCHPosition(-1, 0, true);
            leftAreaRows[0]  = CombatFunctions.GetCHPosition( 0, 0, true);
        } else if (playerRowsCount == 2) {
            leftAreaRows[1]  = CombatFunctions.GetCHPosition(-1, 0, true);
            leftAreaRows[0]  = CombatFunctions.GetCHPosition( 1, 0, true);
        }
        if (enemyRowsCount == 1) {
            rightAreaRows[0] = CombatFunctions.GetCHPosition( 0, 0, false);
            rightAreaRows[1] = CombatFunctions.GetCHPosition( 1, 0, false);
        } else if (enemyRowsCount == 2) {
            rightAreaRows[0] = CombatFunctions.GetCHPosition(-1, 0, false);
            rightAreaRows[1] = CombatFunctions.GetCHPosition( 1, 0, false);
        }
    }

    //------------------------------------------------------------------------
    public static void UpdateRowPositionsCombat(Vec3[] leftAreaRows, Vec3[] rightAreaRows)
    {
        leftAreaRows[2]  = CombatFunctions.GetCHPosition(-2, 0, true);
        leftAreaRows[1]  = CombatFunctions.GetCHPosition( 0, 0, true);
        leftAreaRows[0]  = CombatFunctions.GetCHPosition( 2, 0, true);
        rightAreaRows[0] = CombatFunctions.GetCHPosition(-2, 0, false);
        rightAreaRows[1] = CombatFunctions.GetCHPosition( 0, 0, false);
        rightAreaRows[2] = CombatFunctions.GetCHPosition( 2, 0, false);
    }

    //------------------------------------------------------------------------
    public static int GetCompFormation(List_<List_<CW>> rowsList)
    {
        int formation = 0;
        if (rowsList.Count == 0)
            return formation;
        for (int i = 0; i < rowsList.Count; i++) {
            var digit = rowsList[i].Count;
            for (int j = rowsList.Count-i-1; j > 0; j--) {
                digit *= 10;
            }
            formation += digit;
        }
        return formation;
    }

    //------------------------------------------------------------------------
    public static Vec3 GetCHPosition(int coordX, int coordY, bool isPlayer)
    {
        int rowIndex = coordX + 2;
        int colIndex = coordY + 2;
        Vec2 pos = CombatArea.combatAreaPositions[rowIndex][colIndex];

        Vec2 differenceVector = Vec2.right * CombatArea.combatAreaWidth * (isPlayer ? -1f/4f : 1f/4f);
        return pos + differenceVector;
    }

    //------------------------------------------------------------------------
    public static void ApplyActionToTarget(CW target, CW attacker, CombatAction combatAction)
    {
        target.ReceiveAction(combatAction);
        if (attacker.weapon.attachment == AttachmentType.Lifesteal) {
            attacker.healthPoint += MathCustom.RoundToInt(combatAction.damageDealt * CombatMain.attachmentAttributes.lifesteal_Multiplier);
            if (attacker.healthPoint > attacker.maxHealthPoint) {
                attacker.healthPoint = attacker.maxHealthPoint;
            }
        }
        if (target.isDead) {
            if (attacker.statusEffects.Any_(x => x.statusEffectType == StatusEffectType.KillAndShield))
                attacker.damageShield += CombatMain.itemAttributes.killAndShieldShield_Value;
            var killAndBoostEffect = attacker.statusEffects.FirstOrDefault_(x => x.statusEffectType == StatusEffectType.KillAndBoost);
            if (killAndBoostEffect.statusEffectType != StatusEffectType.Null)
                killAndBoostEffect.killAndBoost_flag = true;
        }
    }

    //------------------------------------------------------------------------
    public static void ReceiveAction(CW cw, CombatAction action)
    {
        int damage = action.damage;
        damage += MathCustom.RoundToInt((action.healthPercentDamage * cw.maxHealthPoint) / 100.0f);

        float damageReceivedMultiplier = 1f;
        // if (cw.statusEffects.FirstOrDefault(x => x.statusEffectType == StatusEffectType.___________) != null) {
        //     damageReceivedMultiplier *= ___________;
        // }
        damage = MathCustom.RoundToInt(damage * damageReceivedMultiplier);
        if (cw.weapon.attachment == AttachmentType.ReceiveLessDmg) {
            damage -= CombatMain.attachmentAttributes.receiveLessDmg_Value;
        }

        if (cw.attachment_dodgeAvailable) {
            cw.attachment_dodgeAvailable = false;
            damage = 0;
        }

        if (damage > 0) {
            action.damageDealt = damage;
            cw.damageShield -= damage;
            if (cw.damageShield < 0) {
                cw.healthPoint += cw.damageShield;
                cw.damageShield = 0;
            }
        }
        cw.healthPoint += action.healAmount;

        if (cw.healthPoint > cw.maxHealthPoint)
            cw.healthPoint = cw.maxHealthPoint;

        if (cw.healthPoint <= 0) {
            cw.healthPoint = 0;
            cw.isDead = true;
            return;
        }

        if (action.statusEffect.statusEffectType != StatusEffectType.Null)
            cw.ApplyStatusEffect(action.statusEffect);
    }

    //------------------------------------------------------------------------
    public static void ApplyResponseAction(CW target, CW attacker, int responseDamageMin, int responseDamageMax)
    {
        if (target == null) return;

        var responseAction = new CombatAction(CombatMain.rnd.Next(responseDamageMin, responseDamageMax + 1), attacker.isPlayer, attacker.weapon.matchRosterIndex);
        target.ReceiveAction(responseAction);
        if (attacker.weapon.attachment == AttachmentType.Lifesteal) {
            attacker.healthPoint += MathCustom.RoundToInt(responseAction.damageDealt * CombatMain.attachmentAttributes.lifesteal_Multiplier);
            if (attacker.healthPoint > attacker.maxHealthPoint) {
                attacker.healthPoint = attacker.maxHealthPoint;
            }
        }
        if (target.isDead) {
            if (attacker.statusEffects.Any_(x => x.statusEffectType == StatusEffectType.KillAndShield))
                attacker.damageShield += CombatMain.itemAttributes.killAndShieldShield_Value;
            var killAndBoostEffect = attacker.statusEffects.FirstOrDefault_(x => x.statusEffectType == StatusEffectType.KillAndBoost);
            if (killAndBoostEffect.statusEffectType == StatusEffectType.KillAndBoost)
                killAndBoostEffect.killAndBoost_flag = true;
        }
    }
}
