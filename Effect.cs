using System;
using System.Collections;
using System.Collections.Generic;

namespace AutoWeapons {

public struct Effect
{
    //------------------------------------------------------------------------
    public EffectInfo info;
    public bool isSenderPlayersWeapon;
    public int senderMatchRosterIndex;
    public float actionSpeedMultiplier;
    public float timeLeft;

    public bool killAndBoost_flag;

    //------------------------------------------------------------------------
    public Effect(EffectInfo info)
    {
        this.info = info;
        timeLeft = info.duration;

        isSenderPlayersWeapon = false;
        senderMatchRosterIndex = -1;
        actionSpeedMultiplier = 1f;
        killAndBoost_flag = false;
    }

    //------------------------------------------------------------------------
    public Effect(EffectType type)
    {
        this.info = new EffectInfo { EffectType = type };
        timeLeft = 0f;

        isSenderPlayersWeapon = false;
        senderMatchRosterIndex = -1;
        actionSpeedMultiplier = 1f;
        killAndBoost_flag = false;
    }

    //------------------------------------------------------------------------
    public void ResetTime()
    {
        if (!info.isTimed) return;

        timeLeft = info.duration;
    }
}
}
