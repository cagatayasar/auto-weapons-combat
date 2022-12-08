using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class CombatMain
{
    public static ItemAttributes itemAttributes;
    public static AttachmentAttributes attachmentAttributes;
    public static List<WeaponMasterInfo> weaponMasterInfos;

    // put this inside a general/combat attributes yaml
    public static float combatAreaScale = 1.56f;

    public static Random rnd = new Random();

    //------------------------------------------------------------------------
    public static WeaponMasterInfo GetWeaponMasterInfo(WeaponMasterType weaponMasterType)
    {
        return weaponMasterInfos.First(x => x.weaponMasterType == weaponMasterType);
    }

    //------------------------------------------------------------------------
    public static TacticInfo GetTacticInfo(WeaponMasterType weaponMasterType, TacticType tacticType)
    {
        var wmInfo = GetWeaponMasterInfo(weaponMasterType);
        return wmInfo.tactics.First(t => t.tacticType == tacticType);
    }
}
