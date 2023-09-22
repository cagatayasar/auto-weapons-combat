using System;
using System.Collections;
using System.Collections.Generic;

namespace AutoWeapons {

public class CWSaved
{
    public Weapon weapon;
    public int coordX = 0;
    public int coordY = 0;
    public int id = 0;
    public int matchRosterIndex = -1;
    public bool stayedAlive = false;
    public int lastCoordX;
    public int lastCoordY;

    //------------------------------------------------------------------------
    public CWSaved(Weapon weapon, int coordX, int coordY, int id, int matchRosterIndex)
    {
        this.weapon = weapon;
        this.coordX = coordX;
        this.coordY = coordY;
        this.id = id;
        this.matchRosterIndex = matchRosterIndex;
    }
}
}
