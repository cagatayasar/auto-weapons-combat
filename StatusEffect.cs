using System;
using System.Collections;
using System.Collections.Generic;

public struct StatusEffect
{
    //------------------------------------------------------------------------
    public bool isSenderPlayersWeapon;
    public int senderMatchRosterIndex;
    public float actionSpeedMultiplier;
    public StatusEffectType statusEffectType;
    public bool isTimed;
    public float timeLeft;

    public bool killAndBoost_flag;

    //------------------------------------------------------------------------
    public StatusEffect(StatusEffectType statusEffectType, bool isTimed = false)
    {
        this.statusEffectType = statusEffectType;
        this.isTimed = isTimed;

        isSenderPlayersWeapon = false;
        senderMatchRosterIndex = -1;
        actionSpeedMultiplier = 1f;
        timeLeft = 0.3f;
        killAndBoost_flag = false;
    }

    //------------------------------------------------------------------------
    public void ResetTime()
    {
        timeLeft = 0.3f;
    }

    //------------------------------------------------------------------------
    public void SetActionSpeedMultiplier (float actionSpeedMultiplier)
    {
        this.actionSpeedMultiplier = actionSpeedMultiplier;
    }
}
