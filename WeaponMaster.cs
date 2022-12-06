using System.Collections;
using System.Collections.Generic;

public enum WeaponMasterType : byte {
    Null,
    Capitalist,
    Colonel,
    Gunslinger,
    Guardian,
    Liner,
}

[System.Serializable]
public class WeaponMaster
{
    //------------------------------------------------------------------------
    public WeaponMasterType weaponMasterType;
    public List<bool> unlockedTactics;
    public int unlocksAvailable;
    public bool isPlayer;

    public int totalSkillsUnlocked {
        get {
            var i = 0;
            foreach (var isUnlocked in unlockedTactics) {
                if (isUnlocked)
                    i++;
            }
            return i;
        }
    }

    //------------------------------------------------------------------------
    public WeaponMaster(WeaponMasterType weaponMasterType, bool isPlayer)
    {
        this.weaponMasterType = weaponMasterType;
        this.isPlayer = isPlayer;

        unlockedTactics = new List<bool>();
        for (int i = 0; i < 10; i++) {
            unlockedTactics.Add(false);
        }
        unlocksAvailable = 0;
    }
}
