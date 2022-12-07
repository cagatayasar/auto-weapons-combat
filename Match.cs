using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Match
{
    //------------------------------------------------------------------------
    public int playerScore;
    public int enemyScore;
    public bool playerWon = false;
    public List<Weapon> enemyWeapons;
    public bool hasBeenPlayed = false;
    public int weekNum = 0;

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
        hasBeenPlayed = true;
        if (playerScore > enemyScore) {
            playerWon = true;
        }
    }
}