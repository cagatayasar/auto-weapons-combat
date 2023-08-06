using System;
using System.Collections;
using System.Collections.Generic;

public class WInfoPumpShotgun : WeaponInfo
{
    public int range1;
    public int range2;
    public int bullets;
    public float doFirstShotAfter;
    public float reloadTimePerBullet;
    public float reloadAnimationUIPortion;
    public float waitLengthAfterReload;
    public float playAfterReloadSoundAfter;

    public float animationNonidleMultiplierMin;
    public float animationNonidleMultiplierMax;
    public float animNonidleSpeedMin;
    public float animNonidleSpeedMax;

    public float projectileSpeed;
    public float _30DegreesRotationDuration;
}
