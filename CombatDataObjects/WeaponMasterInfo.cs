using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class WeaponMasterInfo
{
    public string weaponMasterTypeStr;
    public string displayName;
    public TacticInfo passiveTactic;
    public List<TacticInfo> tactics;

    WeaponMasterType _weaponMasterType;

    public WeaponMasterType weaponMasterType { get {
        if (_weaponMasterType == WeaponMasterType.Null) {
            _weaponMasterType = (WeaponMasterType) Enum.Parse(typeof(WeaponMasterType), weaponMasterTypeStr);
        }
        return _weaponMasterType;
    }}
}
