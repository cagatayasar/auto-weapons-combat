using System;
using System.Collections;
using System.Collections.Generic;

namespace AutoWeapons {

public class WInfoKatana : WeaponInfo
{
    public int maxStackAmount1;
    public int maxStackAmount2;

    public float firstAttackLength;
    public float upwardAttackLength;
    public float downwardAttackLength;
    public float firstAttackDamageEnemyPortion;
    public float upwardAttackDamageEnemyPortion;
    public float downwardAttackDamageEnemyPortion;

    public float waitTimeBetweenAttacks;
    public float _30DegreesRotationDuration;
}
}
