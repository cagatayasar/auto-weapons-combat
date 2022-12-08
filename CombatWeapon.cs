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

public interface ICWThrown_Object {
    IEnumerator Throw_Object(System.Action callback = null);
}

public class CombatWeapon
{
    //------------------------------------------------------------------------
    public float yellowBarShrinkingSpeed = 10f;

    public _StatsGeneral statsGeneral;
    public int coordX = 0;
    public int coordY = 0;
    public int id;
    public Weapon weapon;
    public CombatMode combatMode;

    public System.Random rnd;

    public List<List<CombatWeapon>> playerRowsList;
    public List<List<CombatWeapon>> enemyRowsList;
    public List<List<CombatWeapon>> allyRowsList;
    public List<List<CombatWeapon>> targetRowsList;
    public List<CombatWeapon> playerCombatWeapons;
    public List<CombatWeapon> enemyCombatWeapons;
    public List<CombatWeapon> allyCombatWeapons;
    public List<CombatWeapon> targetCombatWeapons;
    public PlayerEnemyData playerEnemyData;

    public List<StatusEffect> statusEffects;
    public int healthPoint;
    public int maxHealthPoint;
    public float actionTimePeriod;
    public float healthPointForYellowBar;
    public int damageShield;
    public int stacks;
    public int maxStacks;
    public int rowNumber; // between 1-3
    public int positionFromBottom;
    public bool isPlayer;
    public float timePassed;
    public float actionTimePassed;
    public bool isDead = false;
    public int roundCount;

    public int range;
    public List<POneRow> pOneRows = new List<POneRow>();
    public List<POneTarget> pOneTargets = new List<POneTarget>();

    public bool itemRedirect_active = false;

    public CombatWeapon targetEnemy;
    public CombatWeapon prevTargetEnemy;
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
    public float seRotationSpeedMultiplier = 1f;
    public float prevSeActionSpeedMultiplier = -1f;

    public int prevAttackerCoordX = -9;
    public int prevAttackerCoordY = -9;
    public Vec2 prevAttackerImaginaryPos;
    public int prevTargetCoordX = -9;
    public int prevTargetCoordY = -9;
    public Vec2 prevTargetImaginaryPos;

    // status effects
    public float seActionSpeedMultiplier = 1f;
    public bool oneLessRangeForXSeconds = false;

    // attachments
    public bool attachment_dodgeAvailable = false;
    public bool attachment_firstAttackAvailable = false;

    //------------------------------------------------------------------------
    public event System.Action onUpdate;
    public event System.Action onDestroy;
    public event System.Action onUpdateAnimator;
    public event System.Action<float> onUpdateRotation;
    public event System.Action<float> onUpdateHealthBar;

    public event System.Action<float> onCancelTransition;
    public event System.Action onFinishTransition;

    public event System.Action<string> onAnimatorSetTrigger;
    public event System.Action<string, string, float> onAnimatorSetFloat;
    public event System.Action<string> onSfxTrigger;

    public event System.Action<CombatWeapon, CombatAction> onReceiveAction;

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
    public CombatWeapon(Weapon weapon, PlayerEnemyData playerEnemyData, int id, bool isPlayer, CombatMode combatMode, ref System.Random rnd)
    {
        this.weapon = weapon;
        this.playerEnemyData = playerEnemyData;
        this.id = id;
        this.isPlayer = isPlayer;
        this.combatMode = combatMode;
        this.rnd = rnd;

        statusEffects = new List<StatusEffect>();
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
        playerCombatWeapons = playerEnemyData.playerCombatWeapons;
        enemyCombatWeapons = playerEnemyData.enemyCombatWeapons;
        targetRowsList = isPlayer ? enemyRowsList : playerRowsList;
        allyRowsList = isPlayer ? playerRowsList : enemyRowsList;
        allyCombatWeapons = isPlayer ? playerCombatWeapons : enemyCombatWeapons;
        targetCombatWeapons = isPlayer ? enemyCombatWeapons : playerCombatWeapons;
    }

    //------------------------------------------------------------------------
    public CombatWeapon(){}

    //------------------------------------------------------------------------
    // this will be a problem for CombatWeaponMaster.cs, fix before committing
    // public abstract void UpdateObjectReferencesExclusive();
    public virtual void InvokeInitializationEvents(){}
    public virtual void Update(float deltaTime){}
    public virtual void UpdateTarget(){}
    public virtual void UpdateProjectiles(float deltaTime){}
    public virtual void UpdateReloading(float deltaTime){}
    public virtual void UpdateAnimator(){}
    public virtual void PrepareCombatStart(int roundCount){}
    public virtual void ActIfReady(){}
    public virtual void ReportClearedRow(int rowNumber, bool isPlayersRow){}
    public virtual CombatAction GetCombatAction() { return null; }

    public virtual void ReceiveAction(CombatAction action)
    {
        CombatFunctions.ReceiveAction(this, action);
        OnReceiveAction(action);
        OnUpdateHealthBar(0f);
        if (weapon.attachment == AttachmentType.Repel) {
            if (isPlayer == action.isSenderPlayersWeapon)
                return;
            var enemyList = isPlayer ? enemyCombatWeapons : playerCombatWeapons;
            var target = enemyList.FirstOrDefault(cw => cw.id == action.senderId);
            CombatFunctions.ApplyResponseAction(target, this, CombatMain.attachmentAttributes.repel_Value, CombatMain.attachmentAttributes.repel_Value);
        }
    }

    //------------------------------------------------------------------------
    public virtual void UpdateLevelBasedStats()
    {
        healthPoint = statsGeneral.GetHP(weapon.combatLevel);
        actionTimePassed = weapon.combatLevel == 1 ? statsGeneral.startingActionTimePassed1 : statsGeneral.startingActionTimePassed2;
        maxHealthPoint = healthPoint;
        healthPointForYellowBar = healthPoint;

        if (weapon.combatLevel == 1) {
            actionTimePeriod = statsGeneral.actionTimePeriod1;
        } else {
            actionTimePeriod = statsGeneral.actionTimePeriod2;
        }
    }

    //------------------------------------------------------------------------
    public void PrepareCombatStartHP() {
        int damageReceived = maxHealthPoint - healthPoint;
        float hpMultiplier = 1f;
        if (statusEffects.Any_(x => x.statusEffectType == StatusEffectType.BonusHp)) // item12
            hpMultiplier += CombatMain.itemAttributes.bonusHp_Multiplier - 1f;
        if (statusEffects.Any_(x => x.statusEffectType == StatusEffectType.IfAloneInRowBonusHp)) { // item17
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
        if (statusEffects.Any_(x => x.statusEffectType == StatusEffectType.BonusActionSpeed)) // item13
            actionSpeedMultiplier += CombatMain.itemAttributes.bonusSpeed_Multiplier - 1f;
        if (statusEffects.Any_(x => x.statusEffectType == StatusEffectType.ThirdRowBonusActionSpeed)) // item16
            actionSpeedMultiplier += CombatMain.itemAttributes.thirdRowSpeed_Multiplier - 1f;
        if (statusEffects.Any_(x => x.statusEffectType == StatusEffectType.Gunslinger_QuickDraw))
            actionSpeedToAdd += CombatMain.GetTacticInfo(WeaponMasterType.Gunslinger, TacticType.Gunslinger_QuickDraw).speedToAdd;
        if (weapon.attachment == AttachmentType.Speed)
            actionSpeedToAdd += CombatMain.attachmentAttributes.speed_Value;
        actionTimePeriod /= (actionSpeedMultiplier + actionSpeedToAdd);
    }

    //------------------------------------------------------------------------
    public void PrepareCombatStartRange() {
        if (statusEffects.Any_(x => x.statusEffectType == StatusEffectType.OneLessRangeForXSeconds))
            oneLessRangeForXSeconds = true;
    }

    //------------------------------------------------------------------------
    public void PrepareCombatStartOther(int roundCount) {
        if (weapon.attachment == AttachmentType.Round) {
            this.roundCount = roundCount;
        }
    }

    //------------------------------------------------------------------------
    public void ApplyStatusEffect(StatusEffect se)
    {
        switch (se.statusEffectType)
        {
            case StatusEffectType.BlueStaffBoost: //@Test
                bool contains = false;
                foreach (var statusEffect in statusEffects) {
                    if (statusEffect.isSenderPlayersWeapon == se.isSenderPlayersWeapon &&
                        statusEffect.senderMatchRosterIndex == se.senderMatchRosterIndex) {
                        statusEffect.ResetTime();
                        contains = true;
                        break;
                    }
                }
                if (!contains) {
                    statusEffects.Add(se);
                }
                break;
            case StatusEffectType.RotateSlower:
                seRotationSpeedMultiplier *= CombatMain.itemAttributes.rotateSlower_Multiplier;
                break;
            case StatusEffectType.Gunslinger_OneEyeClosed:
                if (!statusEffects.Exists(e => e.statusEffectType == se.statusEffectType)) {
                    statusEffects.Add(se);
                }
                break;
            default:
                statusEffects.Add(se);
                break;
        }
    }

    //------------------------------------------------------------------------
    public void ApplyNewPermanentStatusEffect(StatusEffect se)
    {
        weapon.permanentStatusEffects.Add(se);

        switch (se.statusEffectType)
        {
            case StatusEffectType.Stack:
                if (stacks < maxStacks) {
                    (this as ICWStackWeapon).IncrementStack();
                }
                break;
            case StatusEffectType.Gunslinger_CorrosiveShot:
                var dmg = CombatMain.GetTacticInfo(WeaponMasterType.Gunslinger, TacticType.Gunslinger_CorrosiveShot).damage;
                ReceiveAction(new CombatAction(dmg, false, -1));
                OnUpdateHealthBar(0f);
                break;
        }
    }

    //------------------------------------------------------------------------
    public void ApplyExistingPermanentStatusEffects()
    {
        if (weapon.permanentStatusEffects == null) {
            weapon.permanentStatusEffects = new List<StatusEffect>();
            return;
        }
        for (int i = 0; i < weapon.permanentStatusEffects.Count; i++) {
            var se = weapon.permanentStatusEffects[i];
            switch (se.statusEffectType)
            {
                case StatusEffectType.Stack:
                    if (stacks < maxStacks) {
                        (this as ICWStackWeapon).IncrementStack();
                    }
                    break;
                case StatusEffectType.Gunslinger_CorrosiveShot:
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
        transitionTimeTotal = seActionSpeedMultiplier / (actionTimePeriod * statsGeneral.cancelingSpeed);
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

        float degreesToAdd = deltaTime * (30f / (_30DegreesRotationDuration / seRotationSpeedMultiplier)) * MathF.Sign(wantedAngle - currentAngle);
        currentAngle += degreesToAdd;

        OnUpdateRotation(currentAngle);
    }

    //------------------------------------------------------------------------
    public void RotateIfNeeded(CombatWeapon targetEnemy)
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

        if (nextAngle.IsInBetween(wantedAngle-0.1f, wantedAngle+0.1f))
            return;

        wantedAngle = nextAngle;
        if (!currentAngle.IsInBetween(wantedAngle-1f, wantedAngle+1f))
        {
            isRotating = true;
            currentRotationTime = 0.0f;
            totalRotationTime = MathF.Abs(wantedAngle - currentAngle) * (_30DegreesRotationDuration / seRotationSpeedMultiplier / 30f);
        }
    }
}
