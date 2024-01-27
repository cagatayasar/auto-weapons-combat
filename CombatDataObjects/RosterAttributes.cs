using System;
using System.Collections;
using System.Collections.Generic;

namespace AutoWeapons {

public class RosterAttributes : IYamlObject
{
    public List<string> activeWeapons;
    public List<string> activeWeaponMasters;

    public List<WeaponType> ActiveWeaponTypes { get; set; }
    public List<WeaponMasterType> ActiveWeaponMasterTypes { get; set; }

    public void Initialize()
    {
        ActiveWeaponTypes = new List<WeaponType>();
        ActiveWeaponMasterTypes = new List<WeaponMasterType>();

        foreach (var weaponStr in activeWeapons)
            ActiveWeaponTypes.Add((WeaponType) Enum.Parse(typeof(WeaponType), weaponStr));
        foreach (var wmStr in activeWeaponMasters)
            ActiveWeaponMasterTypes.Add((WeaponMasterType) Enum.Parse(typeof(WeaponMasterType), wmStr));
    }
}
}
