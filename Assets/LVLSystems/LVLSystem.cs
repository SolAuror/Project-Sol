using System;                                                                                   //using system for Action event handling
using UnityEngine;                                                                              //using unity engine
using UnityEngine.UI;                                                                           //using UnityEngine UI for sliders
using TMPro;                                                                                    //using TextMeshPro for UI text
using Character.Sheet;                                                                          //using character sheet namespace for access to character sheet elements

namespace Character.LevelSystem                                                                 // Namespace for the system so it can be easily accessed.
{
    public class LVLSystem : MonoBehaviour                                                          //public class level system for managing character levels and experience points (XP).
    {     
#region Component References                                                                        //Compnent References
    AttributeSystem _attributeSystem;                                                               //Reference to Attribute system for managing attribute points on level up.
    CharacterSheet _characterSheet;                                                                 //Reference to Charsheet.cs to fetch name                                

                                                                                                    //UI, will move to character.UI (charUI.cs) later
    [SerializeField] TextMeshProUGUI nameText;                                                      //set in editor, Text mesh pro UI Reference for the name text.
    [SerializeField] TextMeshProUGUI lvlText;                                                       //set in editor, Text mesh pro UI Reference for the level text.
    [SerializeField] TextMeshProUGUI xpText;                                                        //set in editor, Text mesh pro UI Reference for the XP text.
    [SerializeField] Slider xpSlider;                                                               //set in editor, Unity Slider UI Reference for XP slider.
#endregion

#region Debug Variables                                                                             //Debug
    [SerializeField] public bool lvlSystemDebugMode;                                                                // set in editor, boolean for enabling debug logs to track XP gains and level ups in the console.
    [SerializeField] TextMeshPro inWorldDebugText;                                                  // set in editor, Text mesh pro UI Reference for diagetic level display.
#endregion

#region LvlSystem Variables                                                                         //Master Level and XP variables
        public string _characterName;                                                              //private string for character name
        private int _lvl;                                                                           // private integer # for Current level of the character
        private int _lifetimeXp;                                                                    // private integer # for Total XP accumulated by the character, only tracks total gains, does not reset on level up.
        private int _currentXP;                                                                     // private integer # for XP accumulated towards the next level (totalXP - pastLvlXP), resets on level up.
        private int _pastLvlXP;                                                                     // private integer # for XP held at the start of the current level
        private int _xpToNextLVL;                                                                   // private integer # for XP required to reach the next level
#endregion

#region Initialization                                                                              //Initialization
        public void Initialize(LevelSystemManager manager)
        {
            _attributeSystem = manager.AttributeSystem;
            _characterSheet = manager.Sheet;
        }

        public void Start()                                                                                     //Start method to initialize character name and UI on game start.
        {
            if (_characterSheet != null){                                                           //if no character sheet component is found
                _characterName = _characterSheet.GetCharacterName();                                       //set character name to "characterName", set within CharSheet.cs
                nameText.text = _characterName;}                                                      //set name text UI to character name

            else                                                                                    //else:
            {
                _characterName = "CharacterSheet404";                                               //set character name to "NoCharacterSheet" if no character sheet component is found on the same GameObject or in parent objects.
                Debug.Log("LVLSystem: CharacterSheet not found on" + 
                          " GameObject (or in scene). Using default" +
                          " name 'CharacterSheet404'.");                                            //print debug warning
            }

            if (GetCharacterLevel() == 0) AddXP(100);                                               //if character level is 0, add 100xp to initialize the system.
        }
#endregion

#region Core LvlSystem
    public LVLSystem()                                                                             //initialize the level system
        {
        _lvl = 0;                                                                                    //set initial level to 0, add 100 xp to player during script start.
        _currentXP = 0;                                                                              //set initial current XP to 0, this is the xp accumulated towards the next level, resets on level up.
        _lifetimeXp = 0;                                                                             //set initial lifetime total XP to 0, this accumalates throughout the game and does not reset on level up, used for tracking lifetime xp gains.
        _pastLvlXP = _lvl * 100;                                                                     //set initial past level up xp for use in calculating xp to next level, the total xp held at the start of the current level.
        _xpToNextLVL = _currentXP + ((_lvl + 1) * 100);                                              //set initial XP required for level up, calculated as totalXP + ((level + 1) * 100) at script start
        }

    public event Action<int> OnCharacterLevelUpdate;                                              //event to notify CharSheet.cs when character info updates, such as level changes, passes the new level as an integer.
    public event Action<int> OnLifetimeXpUpdate;                                                  //event to notify charsheet.cs when lifetime XP changes


    public void AddXP(int amount)                                                                 // Method to add XP to the character, checks for level up and updates UI accordingly.
    {
        _currentXP += amount;                                                                        //set current xp, equal to itself plus the integer amount added
        _lifetimeXp += amount;                                                                       //set lifetime total xp, equal to itself plus the integer amount added
        OnLifetimeXpUpdate?.Invoke(_lifetimeXp);                                                     // notify CharSheet.cs about lifetime XP change

        if (lvlSystemDebugMode){                                                                           //if Debug mode is True:
            Debug.Log("Added " + amount +                                                            //debug to log xp amount and current xp total after addition.
                      " XP. Lifetime Total XP: " + _lifetimeXp +
                      " | Current XP: " + _currentXP);}                                              
        

        if (_currentXP >= _xpToNextLVL)                                                           //if total xp is greater than or equal to xp to next level:
        {
            LevelUp();                                                                               //call LevelUp Method 
        }

        UpdateUI();                                                                               //call update UI method to display current xp values.potentially create a seperate component for handling ui updates and elements
    }

    public void LevelUp()                                                                        //Method for handling Level Ups 
    {
        int _xpOverflow = _currentXP - _xpToNextLVL;                                               //set local integer for XP overflow, equal to currentXP - xpToNextLvl, this is the amount of XP that exceeds the requirement for leveling up.
        _currentXP = _xpOverflow;                                                                  //set current XP equal to the overflow amount, this allows excess XP to carry over towards the next level.
        _pastLvlXP = _lvl * 100;                                                                   //set past lvl xp equal to current level x 100

        _lvl++;                                                                                    //increment the current level
                               
        _xpToNextLVL = (_lvl + 1) * 100;                                                           //set xp to next level = (new level + 1) x 100.
        OnCharacterLevelUpdate?.Invoke(_lvl);                                                      //notify CharSheet.cs about the info change
        
        if (lvlSystemDebugMode)
        {
            Debug.Log("Leveled up! You are LVL: " + _lvl +                                          //debug to log new level after level up, overflow xp and Current Attribute Points.
                      " | XP Overflow: " + _xpOverflow +
                        " | Unspent Attribute Points: " + _attributeSystem.AttributePointPool);                                                        
        }
        
        UpdateUI();                                                                              //call UpdateUI Method to display current level.

        
    }
#endregion

#region Public Getters
    public int GetCharacterLevel()                                                            //Method to return the current level if needed.
    {
        if (lvlSystemDebugMode)
        {
            Debug.Log(_characterName + " is LVL: " + _lvl);                                    //debug to log current level when method is called.             
        }
        return _lvl;   
    }
    
    public int GetLifetimeTotalXP()                                                           //Method to return the lifetime total XP if needed.
    {
        if (lvlSystemDebugMode)
        {
            Debug.Log(_characterName + " Lifetime Total XP: " + _lifetimeXp);                  //debug to log lifetime total xp when method is called.             
        }
        return _lifetimeXp;   
    }
#endregion

#region UI Update
    void UpdateUI()                                                                                     //Method for updating Ui elements, consider moving to its own component.
        {
            if (nameText == null || lvlText == null || xpText == null || xpSlider == null)                     //if statement: if any UI reference is not found throw a warning and return.
            {
                if (lvlSystemDebugMode) Debug.LogWarning("LVLSystem.UpdateUI: UI references not assigned; skipping UpdateUI.");
                return;
            }

            int _start = _currentXP;                                                                              //set local integer for start = currentXP.      
            int _end = _xpToNextLVL;                                                                              //set local integer for end = xpToNextLVL - pastLvlXp                     

            if (lvlSystemDebugMode)                                                                                         //if statement to check for debug mode and run debug checks.
            {
                inWorldDebugText.text = _characterName + " LVL: " + _lvl;                                                                       
            }

            nameText.text = _characterName;
            lvlText.text = _lvl.ToString();                                                                      //update level text to display current level as a string.                                        
            xpText.text = _start + " xp / " + _end + " xp";                                                       //update xp text to display current xp and xp required for next level in the format "current xp / xp to next level" as a string.                    
        
            float fillValue = (float)_start / (float)_end;                                                        //set local float fillValue = the result of start divided by end, to determine the amount of fill on the bar.
            xpSlider.value = fillValue;                                                                         //update xp slider value to reflect the current progress towards the next level based on the calculated fill value.

            if (lvlSystemDebugMode)
            {
                Debug.Log("Fill Value: " + fillValue + " | Start: " + _start + " | End: " + _end +                    //debug to log fill value and current xp values used for UI updates.
                          " | lifetimeXp: " + _lifetimeXp + " | pastLvlXP: " + _pastLvlXP + 
                          " | currentXP: " + _currentXP + " | xpToNextLVL: " + _xpToNextLVL);
            }  
        }
#endregion
    }
}
