﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace AutoWeapons {

public class WInfoRevolver : WeaponInfo
{
    public int range1;
    public int range2;
    public int bullets;
    public float reloadTimePerBullet;
    public float reloadAnimationUIPortion;
    public float waitLengthAfterReload;
    public float playAfterReloadSoundAfter;

    public float animNonidlePortionMin;
    public float animNonidlePortionMax;
    public float animNonidleSpeedMin;
    public float animNonidleSpeedMax;

    public float projectileSpeed;
    public float _30DegreesRotationDuration;
}
}
