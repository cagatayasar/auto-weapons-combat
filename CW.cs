using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public interface ICWCancelTransition {
    void OnCancelTransitionEnd();
}

public interface ICWStackWeapon {
    void IncrementStack();
}

public interface ICWHoldsRowPositions {
    void UpdateRowPositions(bool isPreparationPhase);
}

public interface ICWThrown {
    void Throw_Simulation();
}

public class CW
{
    //------------------------------------------------------------------------
    public float yellowBarShrinkingSpeed = 10f;

    public WeaponInfo weaponInfo;
    public int coordX = 0;
    public int coordY = 0;
    public int id;
    public Weapon weapon;

    public System.Random rnd;

    public List<List<CW>> playerRowsList;
    public List<List<CW>> enemyRowsList;
    public List<List<CW>> allyRowsList;
    public List<List<CW>> targetRowsList;
    public List<CW> playerCWs;
    public List<CW> enemyCWs;
    public List<CW> allyCWs;
    public List<CW> targetCWs;
    public PlayerEnemyData playerEnemyData;

    public List<Effect> effects;
    public int healthPoint;
    public int maxHealthPoint;
    public float actionTimePeriod;
    public int damageFixed;
    public int damageMin;
    public int damageMax;

    public float healthPointForYellowBar;
    public int damageShield;
    public int stacks;
    public int maxStacks;
    public int rowNumber; // between 1-3
    public int verticalPosition;
    public bool isPlayer;
    public float timePassed;
    public float actionTimePassed;
    public bool isDead = false;
    public int roundCount;

    public int range;
    public List<POneRow> pOneRows = new List<POneRow>();
    public List<POneTarget> pOneTargets = new List<POneTarget>();

    public bool itemRedirect_active = false;

    public CW targetEnemy;
    public CW prevTargetEnemy;
    public MeleeState meleeState = MeleeState.Idle;
    public BowState bowState = BowState.Idle;
    public ReloadState reloadState = ReloadState.Shoot;

    // Canceling transition vars
    public float transitionTimeTotal;
    public float transitionTimePassed;

    // rotating to a new target
    public bool isRotating = false;
    public float _30DegreesRotationDuration;
    public float currentAngle = 0f;
    public float wantedAngle;
    public float currentRotationTime;
    public float totalRotationTime;
    public float effectRotationSpeedMultiplier = 1f;

    public int prevAttackerCoordX = -9;
    public int prevAttackerCoordY = -9;
    public Vec2 prevAttackerImaginaryPos;
    public int prevTargetCoordX = -9;
    public int prevTargetCoordY = -9;
    public Vec2 prevTargetImaginaryPos;

    // status effects
    public float prevEffectSpeedMultiplier = -1f;
    public float effectSpeedMultiplier = 1f;
    public bool isInStealth = false;
    public bool oneLessRangeForXSeconds = false;

    // attachments
    public bool attachment_dodgeAvailable = false;
    public bool attachment_firstAttackAvailable = false;

    //------------------------------------------------------------------------
    public bool IsTargetable => !isInStealth;

    //------------------------------------------------------------------------
    public event Action onUpdate;
    public event Action onDestroy;
    public event Action onUpdateAnimator;
    public event Action<float> onUpdateRotation;
    public event Action<float> onUpdateHealthBar;

    public event Action<float> onCancelTransition;
    public event Action onFinishTransition;

    public event Action<string> onAnimatorSetTrigger;
    public event Action<string, string, float> onAnimatorSetFloat;
    public event Action<string> onSfxTrigger;

    public event Action<CW, CombatAction> onReceiveAction;

    //------------------------------------------------------------------------
    public void OnUpdate()                           => onUpdate?.Invoke();
    public void OnDestroy()                          => onDestroy?.Invoke();
    public void OnUpdateAnimator()                   => onUpdateAnimator?.Invoke();
    public void OnUpdateRotation(float currentAngle) => onUpdateRotation?.Invoke(currentAngle);
    public void OnUpdateHealthBar(float deltaTime)   => onUpdateHealthBar?.Invoke(deltaTime);

    public void OnCancelTransition(float transitionTimeTotal) => onCancelTransition?.Invoke(transitionTimeTotal);
    public void OnFinishTransition()                          => onFinishTransition?.Invoke();

    public void OnAnimatorSetTrigger(string name)                                      => onAnimatorSetTrigger?.Invoke(name);
    public void OnAnimatorSetFloat(string parameterName, string clipName, float value) => onAnimatorSetFloat?.Invoke(parameterName, clipName, value);
    public void OnSfxTrigger(string name) => onSfxTrigger?.Invoke(name);

    public void OnReceiveAction(CombatAction combatAction) => onReceiveAction?.Invoke(this, combatAction);

    //------------------------------------------------------------------------
    public CW(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, ref System.Random rnd)
    {
        this.weapon = weapon;
        this.playerEnemyData = playerEnemyData;
        this.id = id;
        this.isPlayer = isPlayer;
        this.rnd = rnd;

        weaponInfo = CombatMain.weaponInfosDict[weapon.weaponType];

        effects = new List<Effect>();
        if (weapon.attachment == AttachmentType.DmgShield) {
            damageShield += CombatMain.attachmentAttributes.dmgShield_Value;
            OnUpdateHealthBar(0f);
        } else if (weapon.attachment == AttachmentType.Dodge) {
            attachment_dodgeAvailable = true;
        } else if (weapon.attachment == AttachmentType.FirstAttack) {
            attachment_firstAttackAvailable = true;
        }

        playerRowsList = playerEnemyData.playerRowsList;
        enemyRowsList = playerEnemyData.enemyRowsList;
        playerCWs = playerEnemyData.playerCWs;
        enemyCWs = playerEnemyData.enemyCWs;
        targetRowsList = isPlayer ? enemyRowsList : playerRowsList;
        allyRowsList = isPlayer ? playerRowsList : enemyRowsList;
        allyCWs = isPlayer ? playerCWs : enemyCWs;
        targetCWs = isPlayer ? enemyCWs : playerCWs;
    }

    //------------------------------------------------------------------------
    public CW(){}

    //------------------------------------------------------------------------
    // this will be a problem for CWMaster.cs, fix before committing
    // public abstract void UpdateObjectReferencesExclusive();
    public virtual void InvokeInitializationEvents(){}
    public virtual void UpdateTarget(){}
    public virtual void UpdateProjectiles(float deltaTime){}
    public virtual void UpdateReloading(float deltaTime){}
    public virtual void UpdateAnimator(){}
    public virtual void PrepareCombatStart(int roundCount){}
    public virtual void ActIfReady(){}
    public virtual void ReportClearedRow(int rowNumber, bool isPlayersRow){}
    public virtual CombatAction GetCombatAction() { return null; }

    //------------------------------------------------------------------------
    public virtual void Update(float deltaTime)
    {
        UpdateEffects(deltaTime);
    }

    //------------------------------------------------------------------------
    public virtual void ReceiveAction(CombatAction action)
    {
        CombatFunctions.ReceiveAction(this, action);
        OnReceiveAction(action);
        OnUpdateHealthBar(0f);
        if (weapon.attachment == AttachmentType.Repel) {
            if (isPlayer == action.isSenderPlayersWeapon)
                return;
            var opponentCWs = isPlayer ? enemyCWs : playerCWs;
            var target = opponentCWs.FirstOrDefault(cw => cw.id == action.senderId);
            CombatFunctions.ApplyResponseAction(target, this, CombatMain.attachmentAttributes.repel_Value, CombatMain.attachmentAttributes.repel_Value);
        }
    }

    //------------------------------------------------------------------------
    public virtual void UpdateLevelBasedStats()
    {
        healthPoint = weaponInfo.GetHP(weapon.combatLevel);
        actionTimePassed = weapon.combatLevel == 1 ? weaponInfo.startingActionTimePassed1 : weaponInfo.startingActionTimePassed2;
        maxHealthPoint = healthPoint;
        healthPointForYellowBar = healthPoint;

        if (weapon.combatLevel == 1) {
            actionTimePeriod = weaponInfo.actionTimePeriod1;
            damageFixed = weaponInfo.damageFixed1;
            damageMin = weaponInfo.damageMin1;
            damageMax = weaponInfo.damageMax1;
        } else {
            actionTimePeriod = weaponInfo.actionTimePeriod2;
            damageFixed = weaponInfo.damageFixed2;
            damageMin = weaponInfo.damageMin2;
            damageMax = weaponInfo.damageMax2;
        }
    }

    //------------------------------------------------------------------------
    public void UpdateEffects(float deltaTime)
    {
        float speedMultiplier = 1f;
        isInStealth = false;

        for (int i = 0; i < effects.Count; i++)
        {
            var effect = effects[i];
            if (effect.info.EffectType == EffectType.BlueStaff_SpeedBuff) {
                speedMultiplier += effect.actionSpeedMultiplier - 1f;
            }

            if (effect.info.EffectType == EffectType.Stealth) {
                isInStealth = true;
            }

            if (effect.info.isTimed) {
                effect.timeLeft -= deltaTime;
                if (effect.timeLeft < 0f) {
                    effects.RemoveAt(i);
                    i--;
                }
            }
        }

        effectSpeedMultiplier = speedMultiplier;
    }

    //------------------------------------------------------------------------
    public void PrepareCombatStartHP() {
        int damageReceived = maxHealthPoint - healthPoint;
        float hpMultiplier = 1f;
        if (effects.Any_(x => x.info.EffectType == EffectType.BonusHp)) // item12
            hpMultiplier += CombatMain.itemAttributes.bonusHp_Multiplier - 1f;
        if (effects.Any_(x => x.info.EffectType == EffectType.IfAloneInRowBonusHp)) { // item17
            if (playerRowsList[rowNumber - 1].Count == 1) {
                hpMultiplier += CombatMain.itemAttributes.ifAloneInRowHp_Multiplier - 1f;
            }
        }
        maxHealthPoint = MathCustom.RoundToInt(maxHealthPoint * hpMultiplier);
        healthPoint = maxHealthPoint - damageReceived;
    }

    //------------------------------------------------------------------------
    public void PrepareCombatStartSpeed() {
        float actionSpeedMultiplier = 1f;
        float actionSpeedToAdd = 0f;
        if (effects.Any_(x => x.info.EffectType == EffectType.BonusActionSpeed)) // item13
            actionSpeedMultiplier += CombatMain.itemAttributes.bonusSpeed_Multiplier - 1f;
        if (effects.Any_(x => x.info.EffectType == EffectType.ThirdRowBonusActionSpeed)) // item16
            actionSpeedMultiplier += CombatMain.itemAttributes.thirdRowSpeed_Multiplier - 1f;
        if (effects.Any_(x => x.info.EffectType == EffectType.Gunslinger_QuickDraw))
            actionSpeedToAdd += CombatMain.GetTacticInfo(WeaponMasterType.Gunslinger, TacticType.Gunslinger_QuickDraw).speedToAdd;
        if (weapon.attachment == AttachmentType.Speed)
            actionSpeedToAdd += CombatMain.attachmentAttributes.speed_Value;
        actionTimePeriod /= (actionSpeedMultiplier + actionSpeedToAdd);
    }

    //------------------------------------------------------------------------
    public void PrepareCombatStartRange() {
        if (effects.Any_(x => x.info.EffectType == EffectType.OneLessRangeForXSeconds))
            oneLessRangeForXSeconds = true;
    }

    //------------------------------------------------------------------------
    public void PrepareCombatStartOther(int roundCount) {
        if (weapon.attachment == AttachmentType.Round) {
            this.roundCount = roundCount;
        }
    }

    //------------------------------------------------------------------------
    public void ApplyEffect(Effect se)
    {
        switch (se.info.EffectType)
        {
            case EffectType.BlueStaff_SpeedBuff: //@Test
                bool contains = false;
                foreach (var effect in effects) {
                    if (effect.isSenderPlayersWeapon == se.isSenderPlayersWeapon &&
                        effect.senderMatchRosterIndex == se.senderMatchRosterIndex) {
                        effect.ResetTime();
                        contains = true;
                        break;
                    }
                }
                if (!contains) {
                    effects.Add(se);
                }
                break;
            case EffectType.RotateSlower:
                effectRotationSpeedMultiplier *= CombatMain.itemAttributes.rotateSlower_Multiplier;
                break;
            case EffectType.Gunslinger_OneEyeClosed:
                if (!effects.Exists(e => e.info.EffectType == se.info.EffectType)) {
                    effects.Add(se);
                }
                break;
            default:
                effects.Add(se);
                break;
        }
    }

    //------------------------------------------------------------------------
    public void ApplyNewPermanentEffect(Effect se)
    {
        weapon.permanentEffects.Add(se);

        switch (se.info.EffectType)
        {
            case EffectType.Stack:
                if (stacks < maxStacks) {
                    (this as ICWStackWeapon).IncrementStack();
                }
                break;
            case EffectType.Gunslinger_CorrosiveShot:
                var dmg = CombatMain.GetTacticInfo(WeaponMasterType.Gunslinger, TacticType.Gunslinger_CorrosiveShot).damage;
                ReceiveAction(new CombatAction(dmg, false, -1));
                OnUpdateHealthBar(0f);
                break;
        }
    }

    //------------------------------------------------------------------------
    public void ApplyExistingPermanentEffects()
    {
        if (weapon.permanentEffects == null) {
            weapon.permanentEffects = new List<Effect>();
            return;
        }
        for (int i = 0; i < weapon.permanentEffects.Count; i++) {
            var se = weapon.permanentEffects[i];
            switch (se.info.EffectType)
            {
                case EffectType.Stack:
                    if (stacks < maxStacks) {
                        (this as ICWStackWeapon).IncrementStack();
                    }
                    break;
                case EffectType.Gunslinger_CorrosiveShot:
                    var dmg = CombatMain.GetTacticInfo(WeaponMasterType.Gunslinger, TacticType.Gunslinger_CorrosiveShot).damage;
                    ReceiveAction(new CombatAction(dmg, false, -1));
                    OnUpdateHealthBar(0f);
                    break;
            }
        }
    }

    //------------------------------------------------------------------------
    public void UpdateTransition(float deltaTime)
    {
        transitionTimePassed += deltaTime;
        if (transitionTimePassed >= transitionTimeTotal)
        {
            transitionTimePassed = transitionTimeTotal;
            (this as ICWCancelTransition).OnCancelTransitionEnd();
            actionTimePassed = 0f;
            OnFinishTransition();
        }
    }

    //------------------------------------------------------------------------
    public void CancelTransition()
    {
        transitionTimeTotal = effectSpeedMultiplier / (actionTimePeriod * weaponInfo.cancelingSpeed);
        transitionTimePassed = 0f;
        OnCancelTransition(transitionTimeTotal);
    }

    //------------------------------------------------------------------------
    public void UpdateRotating(float deltaTime)
    {
        currentRotationTime += deltaTime;
        if (currentRotationTime >= totalRotationTime)
        {
            isRotating = false;
            currentRotationTime = 0f;
            currentAngle = wantedAngle;
            OnUpdateRotation(currentAngle);
            return;
        }

        float degreesToAdd = deltaTime * (30f / (_30DegreesRotationDuration / effectRotationSpeedMultiplier)) * MathF.Sign(wantedAngle - currentAngle);
        currentAngle += degreesToAdd;

        OnUpdateRotation(currentAngle);
    }

    //------------------------------------------------------------------------
    public void RotateIfNeeded(CW targetEnemy)
    {
        // rotate to default position
        float nextAngle = 0f;
        if (targetEnemy != null) {
            if (prevTargetCoordX != targetEnemy.coordX || prevTargetCoordY != targetEnemy.coordY) {
                prevTargetCoordX = targetEnemy.coordX;
                prevTargetCoordY = targetEnemy.coordY;
                prevTargetImaginaryPos = CombatFunctions.GetCHPosition(targetEnemy.coordX, targetEnemy.coordY, targetEnemy.isPlayer);
            }
            if (prevAttackerCoordX != coordX || prevAttackerCoordY != coordY) {
                prevAttackerCoordX = coordX;
                prevAttackerCoordY = coordY;
                prevAttackerImaginaryPos = CombatFunctions.GetCHPosition(coordX, coordY, isPlayer);
            }

            nextAngle = isPlayer.ToMultiplier() * MathCustom.Rad2Deg *
                MathF.Atan((prevTargetImaginaryPos.y - prevAttackerImaginaryPos.y) / (prevTargetImaginaryPos.x - prevAttackerImaginaryPos.x));
        }

        if (nextAngle > (wantedAngle-0.1f) && nextAngle < (wantedAngle+0.1f))
            return;

        wantedAngle = nextAngle;
        if (currentAngle < (wantedAngle-1f) || currentAngle > (wantedAngle+1f))
        {
            isRotating = true;
            currentRotationTime = 0.0f;
            totalRotationTime = MathF.Abs(wantedAngle - currentAngle) * (_30DegreesRotationDuration / effectRotationSpeedMultiplier / 30f);
        }
    }
}
