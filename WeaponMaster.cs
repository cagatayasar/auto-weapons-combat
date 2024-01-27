using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoWeapons {

public enum WeaponMasterType : byte {
    Null,
    Capitalist,
    Colonel,
    Gunslinger,
    Guardian,
    Liner,
    Swordsman,
    Rogue,
    Corsair,
    Shieldmaiden,
    Blacksmith,
    Alchemist,
    Gambler,
}

[System.Serializable]
public class WeaponMaster
{
    //------------------------------------------------------------------------
    public WeaponMasterType weaponMasterType;
    public bool[] unlockedTactics = new bool[10];
    public int unlocksAvailable;
    public bool isPlayer;

    public int TotalSkillsUnlocked => unlockedTactics.Sum(t => t.ToBinary());

    //------------------------------------------------------------------------
    public WeaponMaster(WeaponMasterType weaponMasterType, bool isPlayer)
    {
        this.weaponMasterType = weaponMasterType;
        this.isPlayer = isPlayer;
    }
}
}
