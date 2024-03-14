using System;
using System.Collections;
using System.Collections.Generic;

namespace AutoWeapons {

[Serializable]
public class Match
{
    //------------------------------------------------------------------------
    public int playerScore;
    public int enemyScore;
    public List<Weapon> enemyWeapons;

    public bool playerWon => playerScore > enemyScore;

    //------------------------------------------------------------------------
    public Match(List<Weapon> enemyWeapons)
    {
        this.enemyWeapons = enemyWeapons;
    }

    //------------------------------------------------------------------------
    public void SetScore(int playerScore, int enemyScore)
    {
        this.playerScore = playerScore;
        this.enemyScore = enemyScore;
    }
}
}
