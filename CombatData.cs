using System.Collections;
using System.Collections.Generic;

namespace AutoWeapons {

public class CombatData
{
    //------------------------------------------------------------------------
    public List<Weapon> playerWeapons = new List<Weapon>();
    public List<Weapon> enemyWeapons = new List<Weapon>();
    public Match match;

    //------------------------------------------------------------------------
    public CombatData(List<Weapon> playerWeapons, List<Weapon> enemyWeapons, Match match)
    {
        this.playerWeapons = playerWeapons;
        this.enemyWeapons = enemyWeapons;
        this.match = match;
    }

    //------------------------------------------------------------------------
    public CombatData(List<Weapon> playerWeapons, List<Weapon> enemyWeapons)
    {
        this.playerWeapons = playerWeapons;
        this.enemyWeapons = enemyWeapons;
    }
}
}
