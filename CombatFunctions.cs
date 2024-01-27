using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoWeapons {

public static class CombatFunctions
{
    //------------------------------------------------------------------------
    public static string GetFormation(List<List<CW>> rowsList)
    {
        string formation = "";
        foreach (var list in rowsList) {
            formation += list.Count;
        }
        return formation;
    }

    //------------------------------------------------------------------------
    public static string GetCombatState(List<List<CW>> rowsList)
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
    public static CombatAction GetCombatAction(System.Random rnd, List<Effect> effects, CW attacker, List<CW> allyCWs, List<List<CW>> allyRowsList,
        int damageMin, int damageMax, int healthPercent = 0, float finalDmgMultiplierParameter = 1f)
    {
        int dmgAddAmount = 0;
        float dmgMultiplier = 1f;
        float finalDmgMultiplier = 1f;
        int damage = -1;
        for (int i = 0; i < effects.Count; i++) {
            switch (effects[i].info.EffectType)
            {
            case EffectType.BonusAttackDmg:
                dmgMultiplier += CombatMain.itemAttributes.bonusDmg_Multiplier - 1f;
                break;
            case EffectType.EachAttackOneLess:
                dmgAddAmount--;
                break;
            case EffectType.EachAttackOneMore:
                dmgAddAmount++;
                break;
            case EffectType.FirstAttacksDoubleDmg:
                dmgMultiplier += CombatMain.itemAttributes.firstAttacksBuff_Multiplier - 1f;
                effects.RemoveAt(i);
                i--;
                break;
            case EffectType.IfAloneBonusDmg:
                if (allyCWs.Count == 1)
                    dmgMultiplier += CombatMain.itemAttributes.ifAloneBonusDmg_Multiplier - 1f;
                break;
            case EffectType.IfPerfectSquareBonusDmg:
                dmgMultiplier += CombatMain.itemAttributes.perfectSquareBonusDmg_Multiplier - 1f;
                break;
            case EffectType.KillAndBoost:
                if (effects[i].killAndBoost_flag) {
                    dmgMultiplier += CombatMain.itemAttributes.killAndBoostBoost_Multiplier - 1f;
                    var se = effects[i];
                    se.killAndBoost_flag = false;
                    effects[i] = se;
                }
                break;
            case EffectType.FirstRowBuff:
                dmgMultiplier += CombatMain.itemAttributes.firstRowBuffDmg_Multiplier - 1f;
                break;
            case EffectType.Gunslinger_OneEyeClosed:
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
    public static CW GetClosestTargetOnRow(List<CW> row, CW attacker, bool redirectActive)
    {
        var targets = new List<CW>();
        if (row.Count == 1 && MathF.Abs(attacker.verticalPosition - 3) <= 1) {
            targets.Add(row[0]);
        }
        else if (row.Count == 2)
        {
            if (attacker.verticalPosition <= 2)
                targets.Add(row[0]);
            else if (attacker.verticalPosition >= 4)
                targets.Add(row[1]);
            else {
                targets.Add(row[redirectActive ? 0 : 1]);
                targets.Add(row[redirectActive ? 1 : 0]);
            }
        }
        else if (row.Count == 3)
        {
            switch(attacker.verticalPosition)
            {
                case 1:
                    targets.Add(row[0]); break;
                case 2:
                    targets.Add(row[redirectActive ? 0 : 1]);
                    targets.Add(row[redirectActive ? 1 : 0]);
                    break;
                case 3:
                    targets.Add(row[1]);
                    break;
                case 4:
                    targets.Add(row[redirectActive ? 1 : 2]);
                    targets.Add(row[redirectActive ? 2 : 1]);
                    break;
                case 5:
                    targets.Add(row[2]);
                    break;
            };
        }

        return targets.FirstOrDefault_(cw => cw.IsTargetable);
    }

    //------------------------------------------------------------------------
    public static CW GetFurthestTargetOnRow(List<CW> row, CW attacker, bool redirectActive)
    {
        var targets = new List<CW>();
        if (row.Count == 1 && MathF.Abs(attacker.verticalPosition - 3) <= 1) {
            targets.Add(row[0]);
        }
        else if (row.Count == 2)
        {
            if (attacker.verticalPosition <= 2)
                targets.Add(row[1]);
            else if (attacker.verticalPosition >= 4)
                targets.Add(row[0]);
            else {
                targets.Add(row[redirectActive ? 0 : 1]);
                targets.Add(row[redirectActive ? 1 : 0]);
            }
        }
        else if (row.Count == 3)
        {
            if (attacker.verticalPosition <= 2)
                targets.Add(row[2]);
            else if (attacker.verticalPosition >= 4)
                targets.Add(row[0]);
            else {
                targets.Add(row[redirectActive ? 0 : 2]);
                targets.Add(row[redirectActive ? 2 : 0]);
            }
        }

        return targets.FirstOrDefault_(cw => cw.IsTargetable);
    }

    //------------------------------------------------------------------------
    public static CW TargetEnemyMelee(CW attacker, CW prevTargetEnemy,
        List<List<CW>> targetRowsList, bool canAttackDownwards = true)
    {
        if (targetRowsList.Count == 0 || attacker.rowNumber != 1) {
            return null;
        }
        if (prevTargetEnemy != null) {
            if (!canAttackDownwards && attacker.verticalPosition > prevTargetEnemy.verticalPosition) {
                return null;
            }
        }

        bool redirectActive = attacker.itemRedirect_active && canAttackDownwards;
        CW targetEnemy = null;
        var targetRow = targetRowsList[0];
        if (prevTargetEnemy == null || !prevTargetEnemy.IsTargetable) {
            targetEnemy = GetClosestTargetOnRow(targetRow, attacker, redirectActive);
        } else if (!targetRow.Contains_(prevTargetEnemy) || MathF.Abs(attacker.verticalPosition - prevTargetEnemy.verticalPosition) > 1) {
            targetEnemy = null;
        } else {
            targetEnemy = prevTargetEnemy;
        }

        return targetEnemy;
    }

    //------------------------------------------------------------------------
    public static CW TargetEnemyRanged(CW attacker, int range, CW prevTargetEnemy,
        List<List<CW>> targetRowsList, float timePassed, bool oneLessRangeForXSeconds)
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
        List<List<CW>> targetRowsList)
    {
        if (attacker.rowNumber > range || targetRowsList.Count == 0)
            return null;

        bool redirectActive = attacker.itemRedirect_active;
        CW targetEnemy = null;
        if (prevTargetEnemy == null || !prevTargetEnemy.IsTargetable) {
            for (int i = 0; i < targetRowsList.Count; i++) {
                targetEnemy = GetClosestTargetOnRow(targetRowsList[i], attacker, redirectActive);
                if (targetEnemy != null)
                    break;
            }
        } else {
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
        List<List<CW>> targetRowsList)
    {
        if (attacker.rowNumber > range || targetRowsList.Count == 0)
            return null;

        bool redirectActive = attacker.itemRedirect_active;
        CW targetEnemy = null;
        if (prevTargetEnemy == null || !prevTargetEnemy.IsTargetable)
        {
            for (int i = targetRowsList.Count - 1; i >= 0; i--) {
                if (range - attacker.rowNumber >= i) {
                    targetEnemy = GetFurthestTargetOnRow(targetRowsList[i], attacker, redirectActive);
                    if (targetEnemy != null)
                        break;
                }
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
    public static int GetCompFormation(List<List<CW>> rowsList)
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
    public static Vec2 GetCHPosition(int coordX, int coordY, bool isPlayer)
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
            if (attacker.effects.Any_(x => x.info.EffectType == EffectType.KillAndShield))
                attacker.damageShield += CombatMain.itemAttributes.killAndShieldShield_Value;
            var killAndBoostEffect = attacker.effects.FirstOrDefault_(x => x.info.EffectType == EffectType.KillAndBoost);
            if (killAndBoostEffect.info.EffectType != EffectType.Null)
                killAndBoostEffect.killAndBoost_flag = true;
        }
    }

    //------------------------------------------------------------------------
    public static void ReceiveAction(CW cw, CombatAction action)
    {
        int damage = action.damage;
        damage += MathCustom.RoundToInt((action.healthPercentDamage * cw.maxHealthPoint) / 100.0f);

        float damageReceivedMultiplier = 1f;
        // if (cw.effects.FirstOrDefault(x => x.effectType == EffectType.___________) != null) {
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

        if (action.effect.info.EffectType != EffectType.Null)
            cw.ApplyEffect(action.effect);
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
            if (attacker.effects.Any_(x => x.info.EffectType == EffectType.KillAndShield))
                attacker.damageShield += CombatMain.itemAttributes.killAndShieldShield_Value;
            var killAndBoostEffect = attacker.effects.FirstOrDefault_(x => x.info.EffectType == EffectType.KillAndBoost);
            if (killAndBoostEffect.info.EffectType == EffectType.KillAndBoost)
                killAndBoostEffect.killAndBoost_flag = true;
        }
    }
}
}
