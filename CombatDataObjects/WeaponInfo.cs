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

    public int damage1Fixed;
    public int damage2Fixed;
    public int damage1Min;
    public int damage1Max;
    public int damage2Min;
    public int damage2Max;

    public int attackPerAnimation;

    public float cancelingSpeed;

    public float commandAttackWait1;
    public float commandAttackWait2;

    public float actionTimePeriod1 => actionSpeed1 > 0f ? (1f / actionSpeed1) : 0f;
    public float actionTimePeriod2 => actionSpeed2 > 0f ? (1f / actionSpeed2) : 0f;

    public int GetHP(int level) => level == 1 ? healthPoint1 : healthPoint2;
}
