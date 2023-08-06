using System;
using System.Collections;
using System.Collections.Generic;

public class WInfoSawedOff : WeaponInfo
{
    public int range1;
    public int range2;
    public int bullets;
    public float doFirstShotAfter;
    public float firstReloadLength;
    public float secondReloadLength;
    public float reloadAnimationLength;
    public float waitLengthAfterReload;
    public float playAfterReloadSoundAfter;

    public float animNonidlePortionMin;
    public float animNonidlePortionMax;
    public float animNonidleSpeedMin;
    public float animNonidleSpeedMax;

    public float projectileSpeed;
    public float _30DegreesRotationDuration;
}
