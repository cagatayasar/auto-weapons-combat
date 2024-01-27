using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public struct EffectInfo : IYamlObject
{
    public string effectTypeStr;
    public bool isTimed;
    public float duration;

    public EffectType EffectType { get; set; }

    public void Initialize()
    {
        EffectType = (EffectType) Enum.Parse(typeof(EffectType), effectTypeStr);
    }
}
