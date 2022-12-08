using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class TacticInfo
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

    TacticType _tacticType;
    WeaponMasterType _weaponMasterType;
    TacticUseType _useType;

    public TacticType tacticType { get {
        if (_tacticType == TacticType.Null) {
            _tacticType = (TacticType) Enum.Parse(typeof(TacticType), weaponMasterType + "_" + tacticTypeStr);
        }
        return _tacticType;
    }}

    public WeaponMasterType weaponMasterType { get {
        if (_weaponMasterType == WeaponMasterType.Null) {
            _weaponMasterType = (WeaponMasterType) Enum.Parse(typeof(WeaponMasterType), weaponMasterTypeStr);
        }
        return _weaponMasterType;
    }}

    public TacticUseType useType { get {
        if (_useType == TacticUseType.Null) {
            _useType = (TacticUseType) Enum.Parse(typeof(TacticUseType), useTypeStr);
        }
        return _useType;
    }}

    public bool usable => useType != TacticUseType.Nonusable;

    public string insertedDescription { get {
        var desc = description.Clone() as string;
        for (int i = 0; i < 10; i++) {
            if (!ReplaceVariable(ref desc)) break;
        }
        return desc;
    }}

    bool ReplaceVariable(ref string desc)
    {
        var startIndex = desc.IndexOf('<', 0);
        if (startIndex != -1) {
            var endIndex = desc.IndexOf('>', startIndex);
            if (endIndex != -1) {
                desc = desc.Substring(0, startIndex) + GetVariable(desc.Substring(startIndex + 1, endIndex - startIndex - 1)) +
                    desc.Substring(endIndex + 1, desc.Length - endIndex - 1);
                return true;
            }
        }
        return false;
    }

    string GetVariable(string variable)
    {
        var fieldType = this.GetType().GetField(variable).FieldType;
        var value = this.GetType().GetField(variable).GetValue(this);

        if (fieldType == typeof(float)) {
            return Utils.GetWithDecimalZero((float) value);
        } else {
            return value.ToString();
        }
    }
}
