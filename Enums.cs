using System.Collections;
using System.Collections.Generic;

public enum WeaponType : byte
{
    Null,
    Arbalest,
    Axe,
    BlueStaff,
    Bow,
    Buckler,
    Cannon,
    Dagger,
    DarkSword,
    Dynamite,
    Gladius,
    Greataxe,
    Greatbow,
    GreenStaff,
    Hatchet,
    Javelin,
    Katana,
    PumpShotgun,
    RagingBow,
    Rapier,
    Revolver,
    SawedOff,
    Scimitar,
    Shield,
    Shuriken,
    SniperRifle,
    SpikedShield,
    Sword,
    Torch,
}

public enum TacticType : byte
{
    Null,
    Capitalist_Borrow,
    Capitalist_Bribe,
    Capitalist_Productivity,
    Capitalist_Reproduction,
    Capitalist_SkilledLabor_DEPRECATED,
    Capitalist_Savings,
    Gunslinger_45,
    Gunslinger_QuickShot,
    Gunslinger_Showdown,
    Gunslinger_OneEyeClosed,
    Gunslinger_CorrosiveShot,
    Gunslinger_Lasso,
    Gunslinger_Reload,
    Gunslinger_QuickDraw,
    Gunslinger_PlannedShot,
    Colonel_Mobility,
    Colonel_TacticalManeuver,
    Colonel_Ceasefire,
    Colonel_Retreat,
    Colonel_Command,
    Colonel_Fusillade,
    Colonel_DisorientingSmoke,
    Colonel_Promotion,
    Colonel_Mark,
    Guardian_Armored,
    Guardian_Protect,
    Guardian_Fortify,
    Guardian_Defender,
    Guardian_ToTheBitterEnd,
    Guardian_ShieldBash,
    Guardian_Taunt,
    Guardian_BelatedAssistance,
}

public enum TacticUseType {
    Null,
    Unusable,
    NoTarget,
    AnyWeapon,
    AllyWeapon,
    EnemyWeapon,
    AnyRow,
    AllyRow,
    EnemyRow,
    AnyArea,
    AllyArea,
    EnemyArea
}

public enum ItemType : byte
{
    //Passives / other = 7
    DoubleDmgToBase,
    MaxWeaponsOneLess,
    // MovesCarry,
    RemovingGivesMove,
    RowCapacity2,
    StartOneMoreStack,
    // Status effects = 12
    BonusDmg,
    BonusHp,
    BonusSpeed,
    EachAttackOneLess,
    EachAttackOneMore,
    FirstAttacksBuff,
    IfAloneBonusDmg,
    IfAloneInRowHp,
    OneLessRange,
    PerfectSquareBonusDmg,
        RotateSlower,      // Not implemented
    ThirdRowSpeed,
    // Usable no target = 6
    CheaperChangePos,
    FirstRowBuff,
    HealBase,
    OneMaximumMove,
    OneMoreMove,
    SummonAShield,
    // Usable on cw = 6
    Barrier,
    KillAndBoost,
    KillAndShield,
        Redirect,          // Not implemented
    RemoveFromEnemy,
        Sacrifice,         // Not implemented
    // null = 1
    Null,
}

public enum EffectType : byte
{
    Null,
    BlueStaff_SpeedBuff,
    Stack,
    BonusActionSpeed,
    BonusAttackDmg,
    BonusHp,
    EachAttackOneLess,
    EachAttackOneMore,
    FirstAttacksDoubleDmg,
    IfAloneBonusDmg,
    IfAloneInRowBonusHp,
    IfPerfectSquareBonusDmg,
    OneLessRangeForXSeconds,
    RotateSlower,
    ThirdRowBonusActionSpeed,
    // Usable on CW
    KillAndShield,
    KillAndBoost,
    // Usable no target
    FirstRowBuff,
    Gunslinger_OneEyeClosed,
    Gunslinger_CorrosiveShot,
    Gunslinger_QuickDraw,
    Stealth,
    UpgradedForTheMatch,
}

public enum BowState : byte
{
    Idle,
    Drawing,
    WaitingForRelease,
    Releasing,
    Null
}

public enum MeleeState : byte
{
    Idle,
    Attacking,
    Returning,
    Canceling,
    Null
}

public enum MeleeDoubleAttackState : byte
{
    Idle,
    Attacking1,
    Attacking2,
    Returning,
    Canceling,
    Null
}

public enum MeleeTargetType : byte
{
    Upward,
    Front,
    Downward,
    Null
}

public enum ReloadState : byte
{
    Shoot,
    Reload,
    WaitAfterReload
}

public enum TableEventType : byte
{
    WeaponChoice,
    Blacksmith,
}

public enum AttachmentType
{
    Null,
    CostsLess,
    DmgShield,
    Dodge,
    FasterReload,
    FirstAttack,
    Furthest,
    Lifesteal,
    Lucky,
    Range,
    ReceiveLessDmg,
    Repel,
    Round,
    RowMoreDamage,
    Speed,
}
