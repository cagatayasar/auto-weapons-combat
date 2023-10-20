using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoWeapons {

public static class CombatMain
{
    public static ItemAttributes itemAttributes;
    public static AttachmentAttributes attachmentAttributes;
    public static RosterAttributes rosterAttributes;

    public class WeaponTypePair
    {
        public WeaponType weaponType;
        public Type cwType;
        public Type infoType;
    }

    // put this inside a general/combat attributes yaml
    public static List<WeaponMasterInfo> weaponMasterInfos = new List<WeaponMasterInfo>();
    public static Dictionary<WeaponType, WeaponInfo> weaponInfosDict = new Dictionary<WeaponType, WeaponInfo>();
    public static List<WeaponTypePair> weaponTypePairs = new List<WeaponTypePair> {
        new WeaponTypePair { weaponType=WeaponType.Arbalest,     cwType=typeof(CWArbalest),     infoType=typeof(WInfoArbalest)},
        new WeaponTypePair { weaponType=WeaponType.Axe,          cwType=typeof(CWAxe),          infoType=typeof(WInfoAxe)},
        new WeaponTypePair { weaponType=WeaponType.BlueStaff,    cwType=typeof(CWBlueStaff),    infoType=typeof(WInfoBlueStaff)},
        new WeaponTypePair { weaponType=WeaponType.Bow,          cwType=typeof(CWBow),          infoType=typeof(WInfoBow)},
        new WeaponTypePair { weaponType=WeaponType.Buckler,      cwType=typeof(CWBuckler),      infoType=typeof(WInfoBuckler)},
        new WeaponTypePair { weaponType=WeaponType.Cannon,       cwType=typeof(CWCannon),       infoType=typeof(WInfoCannon)},
        new WeaponTypePair { weaponType=WeaponType.Dagger,       cwType=typeof(CWDagger),       infoType=typeof(WInfoDagger)},
        new WeaponTypePair { weaponType=WeaponType.DarkSword,    cwType=typeof(CWDarkSword),    infoType=typeof(WInfoDarkSword)},
        new WeaponTypePair { weaponType=WeaponType.Dynamite,     cwType=typeof(CWDynamite),     infoType=typeof(WInfoDynamite)},
        new WeaponTypePair { weaponType=WeaponType.Gladius,      cwType=typeof(CWGladius),      infoType=typeof(WInfoGladius)},
        new WeaponTypePair { weaponType=WeaponType.Greataxe,     cwType=typeof(CWGreataxe),     infoType=typeof(WInfoGreataxe)},
        new WeaponTypePair { weaponType=WeaponType.Greatbow,     cwType=typeof(CWGreatbow),     infoType=typeof(WInfoGreatbow)},
        new WeaponTypePair { weaponType=WeaponType.GreenStaff,   cwType=typeof(CWGreenStaff),   infoType=typeof(WInfoGreenStaff)},
        new WeaponTypePair { weaponType=WeaponType.Hatchet,      cwType=typeof(CWHatchet),      infoType=typeof(WInfoHatchet)},
        new WeaponTypePair { weaponType=WeaponType.Javelin,      cwType=typeof(CWJavelin),      infoType=typeof(WInfoJavelin)},
        new WeaponTypePair { weaponType=WeaponType.Katana,       cwType=typeof(CWKatana),       infoType=typeof(WInfoKatana)},
        new WeaponTypePair { weaponType=WeaponType.PumpShotgun,  cwType=typeof(CWPumpShotgun),  infoType=typeof(WInfoPumpShotgun)},
        new WeaponTypePair { weaponType=WeaponType.RagingBow,    cwType=typeof(CWRagingBow),    infoType=typeof(WInfoRagingBow)},
        new WeaponTypePair { weaponType=WeaponType.Rapier,       cwType=typeof(CWRapier),       infoType=typeof(WInfoRapier)},
        new WeaponTypePair { weaponType=WeaponType.Revolver,     cwType=typeof(CWRevolver),     infoType=typeof(WInfoRevolver)},
        new WeaponTypePair { weaponType=WeaponType.SawedOff,     cwType=typeof(CWSawedOff),     infoType=typeof(WInfoSawedOff)},
        new WeaponTypePair { weaponType=WeaponType.Scimitar,     cwType=typeof(CWScimitar),     infoType=typeof(WInfoScimitar)},
        new WeaponTypePair { weaponType=WeaponType.Shield,       cwType=typeof(CWShield),       infoType=typeof(WInfoShield)},
        new WeaponTypePair { weaponType=WeaponType.Shuriken,     cwType=typeof(CWShuriken),     infoType=typeof(WInfoShuriken)},
        new WeaponTypePair { weaponType=WeaponType.SniperRifle,  cwType=typeof(CWSniperRifle),  infoType=typeof(WInfoSniperRifle)},
        new WeaponTypePair { weaponType=WeaponType.SpikedShield, cwType=typeof(CWSpikedShield), infoType=typeof(WInfoSpikedShield)},
        new WeaponTypePair { weaponType=WeaponType.Sword,        cwType=typeof(CWSword),        infoType=typeof(WInfoSword)},
        new WeaponTypePair { weaponType=WeaponType.Torch,        cwType=typeof(CWTorch),        infoType=typeof(WInfoTorch)},
    };

    public static float combatAreaScale = 1.56f;

    public static bool isRandomized;
    public static Random rnd = new Random();
    public static ILogger logger;

    //------------------------------------------------------------------------
    public static WeaponMasterInfo GetWeaponMasterInfo(WeaponMasterType weaponMasterType)
    {
        return weaponMasterInfos.First(x => x.WeaponMasterType == weaponMasterType);
    }

    //------------------------------------------------------------------------
    public static TacticInfo GetTacticInfo(WeaponMasterType weaponMasterType, TacticType tacticType)
    {
        var wmInfo = GetWeaponMasterInfo(weaponMasterType);
        return wmInfo.tactics.First(t => t.TacticType == tacticType);
    }
}
}
