using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LVLSystem : MonoBehaviour
{
    private int lvl;
    private int pastLvlXP;
    private int xpToNextLVL;
    private int totalXP;

    [SerializeField] TextMeshProUGUI lvlText;
    [SerializeField] TextMeshProUGUI xpText;
    [SerializeField] Slider xpSlider;


    public LVLSystem()
    {
        lvl = 0;
        pastLvlXP = 0;
        totalXP = 0;
        xpToNextLVL = totalXP + ((lvl + 1) * 100);
    }

    public void AddXP(int amount)
    {
        totalXP += amount;
        Debug.Log("Added " + amount + " XP. Total XP: " + totalXP);
        if (totalXP >= xpToNextLVL)
        {
            LevelUp();
        }
        UpdateUI();
    }

    public void LevelUp()
    {
        lvl++;
        Debug.Log("Leveled up! You are LVL: " + lvl);
        pastLvlXP = totalXP;
        xpToNextLVL = totalXP + (lvl * 100);
        
        UpdateUI();
    }

    public int GetCurrentLVL()
    {
        Debug.Log("Current LVL: " + lvl);
        return lvl;
    }

    void UpdateUI()
    {
        int start = totalXP - pastLvlXP;
        int end = xpToNextLVL - pastLvlXP;

        lvlText.text = lvl.ToString();
        xpText.text = start + " xp / " + end + " xp";
        
        float fillValue = (float)start / (float)end;
        xpSlider.value = fillValue;
        Debug.Log("Fill Value: " + fillValue + " | Start: " + start + " | End: " + end + " | totalXP: " + totalXP + " | pastLvlXP: " + pastLvlXP);
    }
}
