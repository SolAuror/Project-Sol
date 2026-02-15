using System;                                                                                   //using system for Action event handling
using UnityEngine;                                                                              //using unity engine
using UnityEngine.UI;                                                                           //using UnityEngine UI for sliders
using TMPro;                                                                                    //using TextMeshPro for UI text
using Character.Sheet;                                                                          //using character sheet namespace for access to character sheet elements


namespace Character.LevelSystem                                                                 // Namespace for the system so it can be easily accessed.
{
    [RequireComponent(typeof(CharacterSheet))]
    [RequireComponent(typeof(AttributeSystem))]

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
    [SerializeField] public bool debugMode;                                                                // set in editor, boolean for enabling debug logs to track XP gains and level ups in the console.
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
        void Awake()                                                                                //On Script Awake                
        {
            _attributeSystem = GetComponent<AttributeSystem>();                                     //set the attribute system reference to the attribute system component on the same GameObject
            _characterSheet = GetComponent<CharacterSheet>();                                       //set the character sheet reference to the character sheet component on the same GameObject
            if (_characterSheet == null)                                                              //if no character sheet component is found on the same GameObject               
                _characterSheet = GetComponentInParent<CharacterSheet>();                             //search parent object.

            if (_characterSheet != null){                                                           //if no character sheet component is found
                _characterName = _characterSheet.CharacterName;                                       //set character name to "characterName", set within CharSheet.cs
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

        if (debugMode){                                                                           //if Debug mode is True:
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

        if (_attributeSystem != null)                                                               //null guard: check if attribute system is available
        {
            _attributeSystem.UpdateAttributePoints(_attributeSystem.AttributePointPool + _attributeSystem.AttributePointsPerLevel);                                              //add attribute points on level up, based on the defined attribute points per level.
        }

        if (debugMode)
        {
            Debug.Log("Leveled up! You are LVL: " + _lvl +                                          //debug to log new level after level up, overflow xp and Current Attribute Points.
                      " | XP Overflow: " + _xpOverflow +
                        " | Unspent Attribute Points: " + _attributeSystem.AttributePointPool);                                                        
        }
        
        UpdateUI();                                                                              //call UpdateUI Method to display current level.

        OnCharacterLevelUpdate?.Invoke(_lvl);                                                    //notify CharSheet.cs about the info change
    }
#endregion

#region Public Getters
    public int GetCharacterLevel()                                                            //Method to return the current level if needed.
    {
        if (debugMode)
        {
            Debug.Log(_characterName + " is LVL: " + _lvl);                                    //debug to log current level when method is called.             
        }
        return _lvl;   
    }
    
    public int GetLifetimeTotalXP()                                                           //Method to return the lifetime total XP if needed.
    {
        if (debugMode)
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

public class AttributeSystem                                                                                   //initialize the attribute system
    {
    private LVLSystem _levelSystem;                                                                          //reference to the level system to fetch level info for attribute point allocation
    
#region Atrribute System Variables                                                                  //Attribute system variables
    private int _attributePointPool;                                                                 // private integer for attribute points, used to allow allocation of points to attributes on level up
    private int _attributePointsPerLevel = 1;                                                       // private integer for attribute points gained per level
    private int _attributeMaxValue = 99;                                                            // private integer for maximum value of any attribute, used to cap attributes       
    private string[] _attributes = {"Strength", "Intelligence", "Agility"};                         // string array for attribute names, indexed by Name [str, int, agi]
    private int[] _attributeBaseValues = {1, 1, 1};                                                 // integer array to hold current values of each attribute, indexed from 0 to 2 [0, 1, 2] = [str, int, agi]
#endregion

    public AttributeSystem()                                                     //constructor for attribute system, initializes attribute point pool.
        {
            _attributePointPool = 0;                                                                //set initial attribute points to 0, gained on level up and used to increase attributes.
        }

    void Awake()                                                                                    //Initialize attribute system component references
    {
        _levelSystem = GetComponent<LVLSystem>();
    }

    public event Action<int> OnAttributePointUpdate;                                                //event to notify CharSheet.cs when attribute points are gained or spent
    public event Action<string, int> OnStrengthUpdate;                                        //event to notify CharSheet.cs when attribute values are changed
    public event Action<string, int> OnIntelligenceUpdate;                                        //event to notify CharSheet.cs when attribute values are changed
    public event Action<string, int> OnAgilityUpdate;                                        //event to notify CharSheet.cs when attribute values are changed
    public event Action<int, int, int> OnAttributeUpdate;                                                //event to notify CharSheet.cs when attribute values are changed, passes all attribute values for display on character sheet.

    public int AttributePointPool { get { return _attributePointPool; } }                          //public property to access attribute point pool
    public int AttributePointsPerLevel { get { return _attributePointsPerLevel; } }                //public property to access attribute points per level

    public int AddAttributePoint(string attributeName)                                                   //Method to add attribute points to a specific attribute, takes a string for the attribute name, checks if there are unspent points and if the attribute is valid, then adds a point to the specified attribute and updates the point pool.
    {
        if (_attributePointPool <= 0)                                                            //check if there are attribute points available to spend, if not return -1 as an error code.
        {
            if (_levelSystem.debugMode) Debug.LogWarning("No attribute points available to spend.");
            return 0;
        }

        int attributeIndex = Array.IndexOf(_attributes, attributeName);                          //get the index of the specified attribute name from the _attributes array

        if (_attributeBaseValues[attributeIndex] >= _attributeMaxValue)                            //check if the specified attribute is already at or above the maximum value, if so return -1 as an error code.
        {
            if (_levelSystem.debugMode) Debug.LogWarning(attributeName + " is already at maximum value.");
            return 0;
        }

        _attributeBaseValues[attributeIndex]++;                                                     //add a point to the specified attribute based on the index

        UpdateAttributePoints(_attributePointPool - 1);                                                   //subtract one from the attribute point pool and update CharSheet.cs with the new value.

        // Invoke the appropriate event based on which attribute was updated
        if (attributeName == "Strength")
            OnStrengthUpdate?.Invoke(attributeName, _attributeBaseValues[attributeIndex]);
        else if (attributeName == "Intelligence")
            OnIntelligenceUpdate?.Invoke(attributeName, _attributeBaseValues[attributeIndex]);
        else if (attributeName == "Agility")
            OnAgilityUpdate?.Invoke(attributeName, _attributeBaseValues[attributeIndex]);

        return _attributeBaseValues[attributeIndex];                                                     //return the new value of the updated attribute
    }

    public void UpdateAttributePoints(int newAttributePoints)                                       //Method to update attribute points and notify subscribers
    {
        _attributePointPool = newAttributePoints;
        OnAttributePointUpdate?.Invoke(_attributePointPool);
    }

    public void NotifyAllAttributeValues()                                                          //Method to notify CharSheet.cs about all current attribute values, can be called after loading a character or when initializing to sync values.
    {
        OnAttributeUpdate?.Invoke(_attributeBaseValues[0], _attributeBaseValues[1], _attributeBaseValues[2]);
    }

#region Public Getters
    public int GetStrength()                                                                  //Method to return Strength
    {
        if (_levelSystem.debugMode)
        {
            Debug.Log(" Current Strength: " + _attributeBaseValues[0]);                           //debug to log current strength.
        }
        return _attributeBaseValues[0]; 
    }

    public int GetIntelligence()                                                              //Method to return Intelligence
    {
        if (_levelSystem.debugMode)
        {
            Debug.Log(" Current Intelligence: " + _attributeBaseValues[1]);                           //debug to log current intelligence.
        }
        return _attributeBaseValues[1]; 
    }

    public int GetAgility()                                                                   //Method to return Agility
    {
        if (_levelSystem.debugMode)
        {
            Debug.Log(" Current Agility: " + _attributeBaseValues[2]);                           //debug to log current agility.
        }
        return _attributeBaseValues[2]; 
    }

    public int GetAttributePoints()                                                           //Method to return unspent attribute points
    {
        if (_levelSystem.debugMode)
        {
            Debug.Log(" Unspent Attribute Points: " + _attributePointPool);                           //debug to log unspent attribute points.
        }
        return _attributePointPool; 
    }


#endregion

    }

}