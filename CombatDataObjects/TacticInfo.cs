using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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
    public bool Usable => TacticUseType != TacticUseType.Nonusable;
    public string InsertedDescription { get; set; }

    public void Initialize()
    {
        WeaponMasterType = (WeaponMasterType) Enum.Parse(typeof(WeaponMasterType), weaponMasterTypeStr);
        TacticType       = (TacticType)       Enum.Parse(typeof(TacticType), WeaponMasterType + "_" + tacticTypeStr);
        TacticUseType    = (TacticUseType)    Enum.Parse(typeof(TacticUseType), useTypeStr);

        InsertedDescription = description.Clone() as string;
        for (int i = 0; i < 10; i++) {
            InsertedDescription = ReplaceVariable(InsertedDescription, out var success);
            if (success) break;
        }
    }

    string ReplaceVariable(string desc, out bool success)
    {
        var startIndex = desc.IndexOf('<', 0);
        if (startIndex != -1) {
            var endIndex = desc.IndexOf('>', startIndex);
            if (endIndex != -1) {
                var sb = new StringBuilder();
                sb.Append(desc.Substring(0, startIndex));
                sb.Append(GetVariable(desc.Substring(startIndex + 1, endIndex - startIndex - 1)));
                sb.Append(desc.Substring(endIndex + 1, desc.Length - endIndex - 1));
                success = true;
                return sb.ToString();
            }
        }
        success = false;
        return desc;
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
