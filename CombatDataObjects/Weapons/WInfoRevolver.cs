using System;
using System.Collections;
using System.Collections.Generic;

public class WInfoRevolver : WeaponInfo
{
    public int range1;
    public int range2;
    public int bullets;
    public float reloadTimePerBullet;
    public float reloadAnimationUIPortion;
    public float waitLengthAfterReload;
    public float playAfterReloadSoundAfter;

    public float animationNonidlePortionMin;
    public float animationNonidlePortionMax;
    public float actionSpeedForNonidleMin;
    public float actionSpeedForNonidleMax;

    public float projectileSpeed;
    public float _30DegreesRotationDuration;
}
