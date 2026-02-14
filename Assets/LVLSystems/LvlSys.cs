using System;                                               //using system for Action event handling
using UnityEngine;                                          //using unity engine
using UnityEngine.UI;                                       //using UnityEngine UI for sliders
using TMPro;                                                //using TextMeshPro for UI text
using Character.Sheet;                                      //using character sheet namespace for access to character sheet elements


namespace Character.XpSystem                               // Namespace for the level system so it can be easily accessed.
{
    [RequireComponent(typeof(CharacterSheet))]

    public class LVLSystem : MonoBehaviour                          //public class level system for managing character levels and experience points (XP).
    {     
#region System References
    CharacterSheet _characterSheet;                                 //Reference to Charsheet.cs to fetch name                                

//UI, will move to character.UI (charUI.cs) later
    [SerializeField] TextMeshProUGUI nameText;                       // set in editor, Text mesh pro UI Reference for the name text.
    [SerializeField] TextMeshProUGUI lvlText;                       // set in editor, Text mesh pro UI Reference for the level text.
    [SerializeField] TextMeshProUGUI xpText;                        // set in editor, Text mesh pro UI Reference for the XP text.
    [SerializeField] Slider xpSlider;                               // set in editor, Unity Slider UI Reference for XP slider.
//end UI
#endregion

#region LvlSystem Variables
    private string _characterName;                                 //private string for character name
    private int _lvl;                                                // private integer # for Current level of the character
    private int _lifetimeXp;                                    // private integer # for Total XP accumulated by the character, only tracks total gains, does not reset on level up.
    private int _currentXP;                                          // private integer # for XP accumulated towards the next level (totalXP - pastLvlXP), resets on level up.
    private int _pastLvlXP;                                          // private integer # for XP held at the start of the current level
    private int _xpToNextLVL;                                        // private integer # for XP required to reach the next level

//debug
    [SerializeField] bool debugMode;                                // set in editor, boolean for enabling debug logs to track XP gains and level ups in the console.
    [SerializeField] TextMeshPro inWorldDebugText;                 // set in editor, Text mesh pro UI Reference for diagetic level display.
#endregion

#region Initialization
        void Awake()
        {
            // prefer same-GameObject reference, fall back to parent search and a safe default
            _characterSheet = GetComponent<CharacterSheet>();

            if (_characterSheet == null)                                    //if no character sheet component is found on the same GameObject               
                _characterSheet = GetComponentInParent<CharacterSheet>();     //search parent object.

            if (_characterSheet != null)                                    //if no character sheet component is found
            {
                _characterName = _characterSheet.CharacterName;               //set character name to "characterName", set within CharSheet.cs
            }
            else
            {
                _characterName = "CharacterSheet404";                     //set character name to "NoCharacterSheet" if no character sheet component is found on the same GameObject or in parent objects.
                Debug.Log("LVLSystem: CharacterSheet not found on" + 
                          " GameObject (or in scene). Using default" +
                          " name 'CharacterSheet404'.");                  //print debug warning
            }
        }
        
        public LVLSystem()                                          //initialize the level system
        {
        _lvl = 0;                                                  //set initial level to 0, add 100 xp to player during script start.
        _currentXP = 0;                                            //set initial current XP to 0, this is the xp accumulated towards the next level, resets on level up.
        _lifetimeXp = 0;                                           //set initial lifetime total XP to 0, this accumalates throughout the game and does not reset on level up, used for tracking lifetime xp gains.
        _pastLvlXP = _lvl * 100;                                   //set initial past level up xp for use in calculating xp to next level, the total xp held at the start of the current level.
        _xpToNextLVL = _currentXP + ((_lvl + 1) * 100);            //set initial XP required for level up, calculated as totalXP + ((level + 1) * 100) at script start
        }
#endregion

#region Public Properties
    public event Action<int> OnCharacterLevelUpdate;             //event to notify CharSheet.cs when character info updates, such as level changes, passes the new level as an integer.
    public event Action<int> OnLifetimeXpUpdate;               //event to notify charsheet.cs when lifetime XP changes

    public string CharacterName                                 //public getter for character name, allows other scripts to access the character's name if needed.
    {
        get { return _characterName; }
        set 
            { 
              _characterName = value; 
              UpdateUI();                                       //update UI when name is set to reflect changes.
            } 
    }
#endregion

#region Core System Methods
    public void AddXP(int amount)                                                                       // Method to add XP to the character, checks for level up and updates UI accordingly.
    {
        _currentXP += amount;                                                                                  //set current xp, equal to itself plus the integer amount added
        _lifetimeXp += amount;                                                                            //set lifetime total xp, equal to itself plus the integer amount added
        OnLifetimeXpUpdate?.Invoke(_lifetimeXp);                                                               // notify CharSheet.cs about lifetime XP change

        if (debugMode)
        {
            Debug.Log("Added " + amount + " XP. Lifetime Total XP: " + _lifetimeXp + " | Current XP: " + _currentXP);         //debug to log xp amount and current xp total after addition.
        }

        if (_currentXP >= _xpToNextLVL)                                                                         //if statement to check for level ups (if total xp is greater than or equal to xp required for next level)
        {
            LevelUp();                                                                                          //call LevelUp Method 
        }

        UpdateUI(); //potentially create a seperate component for handling ui updates and elements          //call update UI method to display current xp values.
    }

    public void LevelUp()                                                                               //Method for handling Level Ups 
    {
        int _xpOverflow = _currentXP - _xpToNextLVL;                                                           //set local integer for XP overflow, equal to currentXP - xpToNextLvl, this is the amount of XP that exceeds the requirement for leveling up.
        _currentXP = _xpOverflow;                                                                             //set current XP equal to the overflow amount, this allows excess XP to carry over towards the next level.
        _pastLvlXP = _lvl * 100;                                                                              //set past lvl xp equal to current level x 100

        _lvl++;                                                                                              //increment the current level
                               
        _xpToNextLVL = (_lvl + 1) * 100;                                                          //set xp to next level = (new level + 1) x 100.

        if (debugMode)
        {
            Debug.Log("Leveled up! You are LVL: " + _lvl + " | XP Overflow: " + _xpOverflow);                     //debug to log new level after level up and display overflow xp.                
        }
        
        UpdateUI(); //potentially create a seperate component for handling ui updates and elements          //call UpdateUI Method to display current level.

        OnCharacterLevelUpdate?.Invoke(_lvl);                                                                    // notify CharSheet.cs about the info change
    }
    #endregion

#region Public Value Getters
    public int GetCharacterLevel()                                                                                     //Method to return the current level if needed.
    {
        if (debugMode)
        {
            Debug.Log(_characterName + " is LVL: " + _lvl);                                                               //debug to log current level when method is called.             
        }
        return _lvl;   
    }

    public int GetLifetimeTotalXP()                                                                                   //Method to return the lifetime total XP if needed.
    {
        if (debugMode)
        {
            Debug.Log(_characterName + " Lifetime Total XP: " + _lifetimeXp);                                               //debug to log lifetime total xp when method is called.             
        }
        return _lifetimeXp;   
    }
    #endregion

    #region UI Updater
        void UpdateUI()                                                                                     //Method for updating Ui elements, consider moving to its own component.
        {
            if (nameText == null || lvlText == null || xpText == null || xpSlider == null)                     //if statement: if any UI reference is not found throw a warning and return.
            {
                if (debugMode) Debug.LogWarning("LVLSystem.UpdateUI: UI references not assigned; skipping UpdateUI.");
                return;
            }

            int _start = _currentXP;                                                                              //set local integer for start = currentXP.      
            int _end = _xpToNextLVL;                                                                              //set local integer for end = xpToNextLVL - pastLvlXp                     

            if (debugMode)                                                                                         //if statement to check for debug mode and run debug checks.
            {
                inWorldDebugText.text = _characterName + " LVL: " + _lvl;                                                                       
            }

            nameText.text = _characterName;
            lvlText.text = _lvl.ToString();                                                                      //update level text to display current level as a string.                                        
            xpText.text = _start + " xp / " + _end + " xp";                                                       //update xp text to display current xp and xp required for next level in the format "current xp / xp to next level" as a string.                    
        
            float fillValue = (float)_start / (float)_end;                                                        //set local float fillValue = the result of start divided by end, to determine the amount of fill on the bar.
            xpSlider.value = fillValue;                                                                         //update xp slider value to reflect the current progress towards the next level based on the calculated fill value.

            if (debugMode)
            {
                Debug.Log("Fill Value: " + fillValue + " | Start: " + _start + " | End: " + _end +                    //debug to log fill value and current xp values used for UI updates.
                          " | lifetimeXp: " + _lifetimeXp + " | pastLvlXP: " + _pastLvlXP + 
                          " | currentXP: " + _currentXP + " | xpToNextLVL: " + _xpToNextLVL);
            }  
        }
        #endregion
    }
}