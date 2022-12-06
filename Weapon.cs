using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public struct WeaponStruct
{
    //------------------------------------------------------------------------
    public int level;
    public int matchRosterIndex;
    public AttachmentType attachment;
    public WeaponType weaponType;
    public bool isSummonedWeapon;
    public bool isPermanentlySummonedWeapon;
    public bool isUpgradedForTheMatch;

    //------------------------------------------------------------------------
    public WeaponStruct(Weapon weapon)
    {
        this.level                       = weapon.level;
        this.matchRosterIndex            = weapon.matchRosterIndex;
        this.attachment                  = weapon.attachment;
        this.weaponType                  = weapon.weaponType;
        this.isSummonedWeapon            = weapon.isSummonedWeapon;
        this.isPermanentlySummonedWeapon = weapon.isPermanentlySummonedWeapon;
        this.isUpgradedForTheMatch       = weapon.isUpgradedForTheMatch;
    }
}

[Serializable]
public class Weapon
{
    //------------------------------------------------------------------------
    public int level;
    public int matchRosterIndex = -1;
    public AttachmentType attachment = AttachmentType.Null;
    public WeaponType weaponType;
    public bool isSummonedWeapon = false;
    public bool isPermanentlySummonedWeapon = false;
    public bool isUpgradedForTheMatch = false;
    public List<StatusEffect> permanentStatusEffects;

    public int combatLevel => isUpgradedForTheMatch ? 2 : level;

    //------------------------------------------------------------------------
    public Weapon(WeaponType weaponType, int level)
    {
        this.weaponType = weaponType;
        this.level = level;

        matchRosterIndex = -1;
        permanentStatusEffects = new List<StatusEffect>();
    }

    //------------------------------------------------------------------------
    public Weapon(WeaponStruct weaponStruct, List<StatusEffect> permanentStatusEffects)
    {
        this.level                       = weaponStruct.level;
        this.matchRosterIndex            = weaponStruct.matchRosterIndex;
        this.attachment                  = weaponStruct.attachment;
        this.weaponType                  = weaponStruct.weaponType;
        this.isSummonedWeapon            = weaponStruct.isSummonedWeapon;
        this.isPermanentlySummonedWeapon = weaponStruct.isPermanentlySummonedWeapon;
        this.isUpgradedForTheMatch       = weaponStruct.isUpgradedForTheMatch;
        this.permanentStatusEffects      = permanentStatusEffects;
    }

    //------------------------------------------------------------------------
    public void ResetPermanentStatusEffects() {
        permanentStatusEffects = new List<StatusEffect>();
    }
}
