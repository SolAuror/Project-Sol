using System;                                               //using system for c# functionality.
using UnityEngine;                                          //using unity engine
using TMPro;                                                //using TextMeshPro for UI text
using UnityEngine.UI;
using TMPro.Examples;                                       //using UnityEngine UI for sliders

namespace Character.LVLSystem                               // Namespace for the level system so it can be easily accessed.
{
    public class LVLSystem : MonoBehaviour                          //public class level system for managing character levels and experience points (XP).
    {                                                               
    private int lvl;                                                // private integer # for Current level of the character
    private int lifetimeTotalXP;                                    // private integer # for Total XP accumulated by the character, only tracks total gains, does not reset on level up.
    private int currentXP;                                          // private integer # for XP accumulated towards the next level (totalXP - pastLvlXP), resets on level up.
    private int pastLvlXP;                                          // private integer # for XP held at the start of the current level
    private int xpToNextLVL;                                        // private integer # for XP required to reach the next level

    [SerializeField] TextMeshProUGUI lvlText;                       // set in editor, Text mesh pro UI Reference for the level text.
    [SerializeField] TextMeshProUGUI xpText;                        // set in editor, Text mesh pro UI Reference for the XP text.
    [SerializeField] Slider xpSlider;                               // set in editor, Unity Slider UI Reference for XP slider.

    [SerializeField] bool debugMode;                                // set in editor, boolean for enabling debug logs to track XP gains and level ups in the console.
    [SerializeField] bool diageticMode;                             // set in editor, boolean for enabling non UI mode, which enables Diagetic UI updates.
    [SerializeField] TextMeshPro diageticText;          // set in editor, Text mesh pro prefab reference for diagetic floating text, used to display xp gains in diagetic mode.


    public LVLSystem()                                          //initialize the level system
    {
        lvl = 0;                                                  //set initial level to 0, add 100 xp to player during script start.
        currentXP = 0;                                            //set initial current XP to 0, this is the xp accumulated towards the next level, resets on level up.
        lifetimeTotalXP = 0;                                      //set initial lifetime total XP to 0, this accumalates throughout the game and does not reset on level up, used for tracking lifetime xp gains.
        pastLvlXP = lvl * 100;                                    //set initial past level up xp for use in calculating xp to next level, the total xp held at the start of the current level.
        xpToNextLVL = currentXP + ((lvl + 1) * 100);              //set initial XP required for level up, calculated as totalXP + ((level + 1) * 100) at script start
    }

    public void AddXP(int amount)                                                                       // Method to add XP to the character, checks for level up and updates UI accordingly.
    {
        currentXP += amount;                                                                                  //set current xp, equal to itself plus the integer amount added
        lifetimeTotalXP += amount;                                                                            //set lifetime total xp, equal to itself plus the integer amount added

        if (debugMode)
        {
            Debug.Log("Added " + amount + " XP. Lifetime Total XP: " + lifetimeTotalXP + " | Current XP: " + currentXP);         //debug to log xp amount and current xp total after addition.
        }

        if (currentXP >= xpToNextLVL)                                                                         //if statement to check for level ups (if total xp is greater than or equal to xp required for next level)
        {
            LevelUp();                                                                                          //call LevelUp Method 
        }

        UpdateUI(); //potentially create a seperate component for handling ui updates and elements          //call update UI method to display current xp values.
    }

    public void LevelUp()                                                                               //Method for handling Level Ups 
    {
        int XpOverflow = currentXP - xpToNextLVL;                                                           //set local integer for XP overflow, equal to currentXP - xpToNextLvl, this is the amount of XP that exceeds the requirement for leveling up.
        currentXP = XpOverflow;                                                                             //set current XP equal to the overflow amount, this allows excess XP to carry over towards the next level.
        pastLvlXP = lvl * 100;                                                                              //set past lvl xp equal to current level x 100

        lvl++;                                                                                              //increment the current level
                               
        xpToNextLVL = (lvl + 1) * 100;                                                          //set xp to next level = (new level + 1) x 100.

        if (debugMode)
        {
            Debug.Log("Leveled up! You are LVL: " + lvl + " | XP Overflow: " + XpOverflow);                     //debug to log new level after level up and display overflow xp.                
        }
        
        UpdateUI(); //potentially create a seperate component for handling ui updates and elements           //call UpdateUI Method to display current level.
    }

    public int GetCurrentLVL()                                                                          //Method to return the current level if needed.
    {
        if (debugMode)
        {
            Debug.Log("Current LVL: " + lvl);                                                                   //debug to log current level when method is called.             
        }
        return lvl;                                                                                         //return the current level of the character.   
    }

        void UpdateUI()                                                                                     //Method for updating Ui elements, consider moving to its own component.
        {
        int start = currentXP;                                                                              //set local integer for start = currentXP.      
        int end = xpToNextLVL;                                                                              //set local integer for end = xpToNextLvl - pastLvlXp                     


        if (diageticMode)
        {
            diageticText.text = lvl.ToString();
        }

        else if (!diageticMode)
        {
            lvlText.text = lvl.ToString();                                                                      //update level text to display current level as a string.                                        
            xpText.text = start + " xp / " + end + " xp";                                                       //update xp text to display current xp and xp required for next level in the format "current xp / xp to next level" as a string.                    
        
            float fillValue = (float)start / (float)end;                                                        //set local float fillValue = the result of start divided by end, to determine the amount of fill on the bar.
            xpSlider.value = fillValue;                                                                         //update xp slider value to reflect the current progress towards the next level based on the calculated fill value.

            if (debugMode)
            {
                Debug.Log("Fill Value: " + fillValue + " | Start: " + start + " | End: " + end +                    //debug to log fill value and current xp values used for UI updates.
                          " | lifetimeTotalXP: " + lifetimeTotalXP + " | pastLvlXP: " + pastLvlXP + 
                          " | currentXP: " + currentXP + " | xpToNextLVL: " + xpToNextLVL);
            }   
        }

        }
    }
}
