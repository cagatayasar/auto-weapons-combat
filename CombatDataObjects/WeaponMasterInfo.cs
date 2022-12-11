using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class WeaponMasterInfo : IYamlObject
{
    public string weaponMasterTypeStr;
    public string displayName;
    public TacticInfo passiveTactic;
    public List<TacticInfo> tactics;

    public WeaponMasterType WeaponMasterType { get; set; }

    public void Initialize()
    {
        WeaponMasterType = (WeaponMasterType) Enum.Parse(typeof(WeaponMasterType), weaponMasterTypeStr);

        passiveTactic.Initialize();
        tactics.ForEach(t => t.Initialize());
    }
}
