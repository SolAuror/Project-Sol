using System;
using UnityEngine;
using Character.Sheet;

namespace Character.LevelSystem
{
    public class AttributeSystem : MonoBehaviour                                                                                  //initialize the attribute system
    {
        private LVLSystem _levelSystem;                                                                          //reference to the LVLSystem Class to fetch level info for attribute point allocation
        private CharacterSheet _characterSheet;                                                                  //reference to the character sheet for fetching and updating character info such as name, and for sending attribute updates to be displayed on the character sheet.

#region Atrribute System Variables                                                                  //Attribute system variables
        private int _attributePointPool;                                                                 // private integer for attribute points, used to allow allocation of points to attributes on level up
        private int _attributePointsPerLevel = 1;                                                       // private integer for attribute points gained per level
        private int _attributeMaxValue = 99;                                                            // private integer for maximum value of any attribute, used to cap attributes       
        private string[] _attributes = {"Strength", "Intelligence", "Agility"};                         // string array for attribute names, indexed by Name [str, int, agi]
        private int[] _attributeBaseValues = {1, 1, 1};                                                 // integer array to hold current values of each attribute, indexed from 0 to 2 [0, 1, 2] = [str, int, agi]
#endregion

#region Debug Variables                                                                             //Debug
        [SerializeField] public bool attributeSystemDebugMode;                                                                // set in editor, boolean for enabling debug logs to track attribute point gains and allocations in the console.
#endregion

#region Initialization
        public void Initialize(LevelSystemManager manager)
        {
            _levelSystem = manager.LevelSystem;
            _characterSheet = manager.Sheet;
        }
        public void Start()                                                                                     //Start method to initialize attribute system, can be expanded to include loading saved attribute values, or applying racial/class bonuses, etc.
        {
            _levelSystem.OnCharacterLevelUpdate += HandleLevelUp;                                               //subscribe to level up event from level system to gain attribute points on level up.

            // Notify CharacterSheet of initial attribute values
            NotifyAllAttributeValues();
        }
#endregion

#region Core Attribute System
        public AttributeSystem()                                                     //constructor for attribute system, initializes attribute point pool.
        {
            _attributePointPool = 0;                                                                //set initial attribute points to 0, gained on level up and used to increase attributes.
        }
        public event Action<int> OnAttributePointUpdate;                                                //event to notify CharSheet.cs when attribute points are gained or spent
        public event Action<string, int> OnAttributeUpdate;                                                //event to notify CharSheet.cs when attribute values are changed, passes all attribute values for display on character sheet.

        public int AttributePointPool { get { return _attributePointPool; } }                          //public property to access attribute point pool
        public int AttributePointsPerLevel { get { return _attributePointsPerLevel; } }                //public property to access attribute points per level

        public int AddAttributePoint(string attributeName)                                                   //Method to add attribute points to a specific attribute, takes a string for the attribute name, checks if there are unspent points and if the attribute is valid, then adds a point to the specified attribute and updates the point pool.
        {
            if (_attributePointPool <= 0)                                                            //check if there are attribute points available to spend, if not return -1 as an error code.
            {
                if (attributeSystemDebugMode) Debug.LogWarning("No attribute points available to spend.");
                return 0;
            }

            int attributeIndex = Array.IndexOf(_attributes, attributeName);                          //get the index of the specified attribute name from the _attributes array

            if (_attributeBaseValues[attributeIndex] >= _attributeMaxValue)                            //check if the specified attribute is already at or above the maximum value, if so return -1 as an error code.
            {
                if (attributeSystemDebugMode) Debug.LogWarning(attributeName + " is already at maximum value.");
                return 0;
            }

            _attributeBaseValues[attributeIndex]++;                                                     //add a point to the specified attribute based on the index

            UpdateAttributePoints(_attributePointPool - 1);                                                   //subtract one from the attribute point pool and update CharSheet.cs with the new value.

            OnAttributeUpdate?.Invoke(attributeName, _attributeBaseValues[attributeIndex]);
            return _attributeBaseValues[attributeIndex];                                                     //return the new value of the updated attribute

        }

        public void UpdateAttributePoints(int newAttributePoints)                                       //Method to update attribute points and notify subscribers
        {
            _attributePointPool = newAttributePoints;
            OnAttributePointUpdate?.Invoke(_attributePointPool);
        }

        public void NotifyAllAttributeValues()                                                          //Method to notify CharSheet.cs about all current attribute values, can be called after loading a character or when initializing to sync values.
        {
            OnAttributeUpdate?.Invoke("Strength", _attributeBaseValues[0]);
            OnAttributeUpdate?.Invoke("Intelligence", _attributeBaseValues[1]);
            OnAttributeUpdate?.Invoke("Agility", _attributeBaseValues[2]);
        }

        public void HandleLevelUp(int newLevel)                                                            //Method to handle level up events from LVLSystem, adds attribute points to the pool based on the defined points per level and updates CharSheet.cs with the new point pool value.
        {
            UpdateAttributePoints(_attributePointPool + _attributePointsPerLevel);                                                   //add attribute points on level up, based on the defined attribute points per level.
        }
#endregion

#region Public Getters
        public int GetStrength()                                                                  //Method to return Strength
        {
            if (attributeSystemDebugMode)
            {
                Debug.Log(" Current Strength: " + _attributeBaseValues[0]);                           //debug to log current strength.
            }
            return _attributeBaseValues[0]; 
        }

        public int GetIntelligence()                                                              //Method to return Intelligence
        {
            if (attributeSystemDebugMode)
            {
                Debug.Log(" Current Intelligence: " + _attributeBaseValues[1]);                           //debug to log current intelligence.
            }
            return _attributeBaseValues[1]; 
        }

        public int GetAgility()                                                                   //Method to return Agility
        {
            if (attributeSystemDebugMode)
            {
                Debug.Log(" Current Agility: " + _attributeBaseValues[2]);                           //debug to log current agility.
            }
            return _attributeBaseValues[2]; 
        }

        public int GetAttributePoints()                                                           //Method to return unspent attribute points
        {
            if (attributeSystemDebugMode)
            {
                Debug.Log(" Unspent Attribute Points: " + _attributePointPool);                           //debug to log unspent attribute points.
            }
            return _attributePointPool; 
        }


#endregion

    }
}
