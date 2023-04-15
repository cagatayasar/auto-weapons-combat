using System;
using System.Collections;
using System.Collections.Generic;

public interface IProjectile
{
    float speed { get; set; }
    CombatAction combatAction { get; set; }
}

public class POneTarget : IProjectile
{
    public CW target;
    public float currentTravelTime;
    public float totalTravelTime;

    public CombatAction combatAction { get; set; }
    public float speed { get; set; }

    public POneTarget(CW target, CombatAction combatAction, float speed, float totalTravelTime)
    {
        this.target = target;
        this.combatAction = combatAction;
        this.speed = speed;
        this.totalTravelTime = totalTravelTime;
        currentTravelTime = 0f;
    }
}

public class PArbalest : IProjectile
{
    public Vec3 position;
    public Vec3 directionVector;
    public int positionFromBottom;
    public List<int> damagedIDs;
    public int flyingRowToDamage;

    public CombatAction combatAction { get; set; }
    public float speed { get; set; }

    public PArbalest(CombatAction combatAction, Vec3 position, Vec3 directionVector, float speed, int positionFromBottom)
    {
        this.combatAction = combatAction;
        this.position = position;
        this.directionVector = directionVector;
        this.speed = speed;
        this.positionFromBottom = positionFromBottom;
        damagedIDs = new List<int>();
        flyingRowToDamage = 0;
    }
}

public class POneRow : IProjectile
{
    public int targetRowNumber;
    public Vec3 startPos;
    public float endX;
    public float currentTravelTime;
    public float totalTravelTime;

    public CombatAction combatAction { get; set; }
    public float speed { get; set; }

    public POneRow(int targetRowNumber, CombatAction combatAction, float speed, Vec3 startPos, float endX, float totalTravelTime)
    {
        this.targetRowNumber = targetRowNumber;
        this.combatAction = combatAction;
        this.speed = speed;
        this.totalTravelTime = totalTravelTime;
        this.startPos = startPos;
        this.endX = endX;
        currentTravelTime = 0f;
    }
}
