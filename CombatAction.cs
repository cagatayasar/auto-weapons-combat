using System.Collections;
using System.Collections.Generic;

namespace AutoWeapons {

public class CombatAction
{
    //------------------------------------------------------------------------
    public int damage;
    public int healthPercentDamage;
    public int healAmount;
    public bool isKnockbackAction;
    public bool isAttackerThrown;

    public bool isSenderPlayersWeapon;
    public int senderId;
    public Effect effect; // Could be changed to a list, if an action has multiple status effects

    public int damageDealt;

    //------------------------------------------------------------------------
    public CombatAction(int damage, bool isSenderPlayersWeapon, int senderId)
    {
        if (damage < 0)
            damage = 0;
        this.damage = damage;
        this.isSenderPlayersWeapon = isSenderPlayersWeapon;
        this.senderId = senderId;
    }

    //------------------------------------------------------------------------
    public CombatAction(){}

    //------------------------------------------------------------------------
    public void SetHealthPercentDamage(int percent, int dmgAddAmount)
    {
        healthPercentDamage = percent;
        damage = dmgAddAmount;
    }
}
}
