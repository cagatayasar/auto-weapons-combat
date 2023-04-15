using System.Collections;
using System.Collections.Generic;

public class CombatAction
{
    //------------------------------------------------------------------------
    public int damage;
    public int healthPercentDamage;
    public int healAmount;
    public bool respondable;
    public bool isKnockbackAction;
    public bool isAttackerThrown;

    public bool isSenderPlayersWeapon;
    public int senderId;
    public StatusEffect statusEffect;// Could be changed to a list, if an action has multiple status effects

    public int damageDealt = 0;

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
    public CombatAction()
    {
        respondable = false;
    }

    //------------------------------------------------------------------------
    public void SetHealthPercentDamage(int percent, int dmgAddAmount = 0)
    {
        healthPercentDamage = percent;
        damage = dmgAddAmount;
    }

    //------------------------------------------------------------------------
    public void SetHealAmount(int healAmount)
    {
        this.healAmount = healAmount;
    }

    //------------------------------------------------------------------------
    public void SetStatusEffect(StatusEffect statusEffect)
    {
        this.statusEffect = statusEffect;
        statusEffect.isSenderPlayersWeapon = isSenderPlayersWeapon;
        statusEffect.senderMatchRosterIndex = senderId;
    }
}
