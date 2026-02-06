using System;
using UnityEngine;

public class LVLSystem
{
    private int lvl;
    private int currentXP;
    private int xpToNextLVL;

    public LVLSystem()
    {
        lvl = 0;
        currentXP = 0;
        xpToNextLVL = (int)(lvl * 100 *1.25f);
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        Debug.Log("Current XP: " + currentXP);
        if (currentXP >= xpToNextLVL)
        {
            LevelUp();
        }
        
    }

    public void LevelUp()
    {
        lvl++;
        Debug.Log("Leveled up! You are LVL: " + lvl);
        xpToNextLVL = (int)(lvl * 100 *1.25f);
    }

    public int GetCurrentLVL()
    {
        Debug.Log("Current LVL: " + lvl);
        return lvl;
    }
}
