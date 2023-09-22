using System;
using System.Collections;
using System.Collections.Generic;

namespace AutoWeapons {

[Serializable]
public class TacticInfo : IYamlObject
{
    public string tacticTypeStr;
    public string weaponMasterTypeStr;
    public string useTypeStr;

    public int useCost;
    public int uses;
    public int cooldown;
    public string description;

    // Gunslinger
    public int bulletCost;

    // Variables
    public int damage;
    public int shield;
    public int totalHP;
    public int delay;
    public int percent;
    public float speedToAdd;

    public TacticType TacticType { get; set; }
    public WeaponMasterType WeaponMasterType { get; set; }
    public TacticUseType TacticUseType { get; set; }
    public string InsertedDescription { get; set; }

    public bool Usable => TacticUseType != TacticUseType.Nonusable;

    public void Initialize()
    {
        WeaponMasterType = (WeaponMasterType) Enum.Parse(typeof(WeaponMasterType), weaponMasterTypeStr);
        TacticType       = (TacticType)       Enum.Parse(typeof(TacticType), WeaponMasterType + "_" + tacticTypeStr);
        TacticUseType    = (TacticUseType)    Enum.Parse(typeof(TacticUseType), useTypeStr);

        InsertedDescription = description.Clone() as string;
        for (int i = 0; i < 10; i++) {
            InsertedDescription = InsertedDescription.ReplaceVariable(this, '<', '>', out var success);
            if (success) break;
        }
    }
}
}
