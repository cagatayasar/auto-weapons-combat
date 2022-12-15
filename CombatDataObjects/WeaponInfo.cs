using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class WeaponInfo
{
    public string className;
    public string actionDescription;
    public string otherDescription;
    public int cost1;
    public int cost2;
    public bool isMelee;
    public bool isRanged;
    public bool isThrown;

    public int healthPoint1;
    public int healthPoint2;
    public float actionSpeed1;
    public float actionSpeed2;
    public float startingActionTimePassed1;
    public float startingActionTimePassed2;

    public int damageFixed1;
    public int damageFixed2;
    public int damageMin1;
    public int damageMax1;
    public int damageMin2;
    public int damageMax2;

    public int attackPerAnimation;

    public float cancelingSpeed;

    public float commandAttackWait1;
    public float commandAttackWait2;

    public float actionTimePeriod1 => actionSpeed1 > 0f ? (1f / actionSpeed1) : 0f;
    public float actionTimePeriod2 => actionSpeed2 > 0f ? (1f / actionSpeed2) : 0f;

    public int GetHP(int level) => level == 1 ? healthPoint1 : healthPoint2;

    public string GetActionDescription(int level) => GetDescriptionWithLevel(actionDescription?.Clone() as string, level);
    public string GetOtherDescription(int level)  => GetDescriptionWithLevel(otherDescription?.Clone() as string, level);

    protected string GetDescriptionWithLevel(string desc, int level)
    {
        if (string.IsNullOrEmpty(desc))
            return desc;

        var success = true;
        while (success) {
            desc = desc.ReplaceVariable(this, '(', ')', out success, str => str + level);
        }
        return desc;
    }
}
