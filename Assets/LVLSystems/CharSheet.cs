using System;                                                                                       //using c# system
using UnityEngine;                                                                                  //using Unity engine
using TMPro;                                                                                        //using TextMeshPro for text display                      
using Character.LevelSystem;                                                                        //using LevelSystem.cs

namespace Character.Sheet                                                                           //namespace for charSheet for reference elsewhere.
{
    public class CharacterSheet : MonoBehaviour                                                     //public class for CharacterSheet, responsible for displaying character info such as name, level, XP, and attributes on the character sheet
    {
#region System References                                                                           //System references

        private LVLSystem _levelSystem;                                                             //Reference to the level system, to fetch current level and xp for display on character sheet.
        private AttributeSystem _attributeSystem;                                                   //Reference to the attribute system, to fetch current attribute values for display on character sheet.

    #endregion

#region CharacterSheet Variables
        [SerializeField] private string characterName = "Sol";                                      //character name, as a string, can be changed in editor
        [SerializeField] private string characterRace = "Developer";                                //character species, as a string, can be changed in editor
        [SerializeField] private int characterAge = 27;                                             //character age, as an integer, can be changed in editor
        [SerializeField] private char characterGender = 'M';                                        //character gender represented by a character, can be changed in editor

        [SerializeField] private int characterLevel;                                                //character level, as an integer, fetched from the level system to display current level on character sheet.                         
        [SerializeField] private int characterXP;                                                   //character XP, as an integer, fetched from level system to track total XP gains for the character

        [SerializeField] private int characterStrength;                                             //character strength attribute, as an integer
        [SerializeField] private int characterIntelligence;                                         //character intelligence attribute, as an integer
        [SerializeField] private int characterAgility;                                              //character agility attribute, as an integer
        [SerializeField] private int characterAttributePoints;                                      //unspent attribute points, as an integer, that can be allocated to strength, intelligence, or agility
    #endregion

#region Initialization
        void Awake()                                                                                //system initialization
        {
            _levelSystem = GetComponent<LVLSystem>();                                               //get reference to level system component on the same GameObject, if it exists
            _attributeSystem = GetComponent<AttributeSystem>();                                     //get reference to attribute system component on the same GameObject, if it exists
            // initialize runtime values that depend on LVLSystem
            if (_levelSystem != null)
            {
                // keep local copy in sync and subscribe to future changes
                characterLevel = _levelSystem.GetCharacterLevel();
                characterXP = _levelSystem.GetLifetimeTotalXP();
                _levelSystem.OnCharacterLevelUpdate += HandleLevelUpdate;                           //subscribe to the level update event from LVLSystem, to update character sheet info when level changes
                _levelSystem.OnLifetimeXpUpdate += HandleLifetimeXpUpdate;
            }
            else
            {
                Debug.LogWarning("CharacterSheet: Level System component not found on the same GameObject; using inspector/default values.");
            }

            if (_attributeSystem != null)
            {
                characterStrength = _attributeSystem.GetStrength();
                characterIntelligence = _attributeSystem.GetIntelligence();
                characterAgility = _attributeSystem.GetAgility();
                characterAttributePoints = _attributeSystem.GetAttributePoints();

                _attributeSystem.OnAttributeUpdate += HandleAttributeUpdate;                        //subscribe to the attribute update event from AttributeSystem, to update character sheet info when attributes change
                _attributeSystem.OnAttributePointUpdate += HandleAttributePointUpdate;
            }
            else
            {
                Debug.LogWarning("CharacterSheet: Attribute System component not found on the same GameObject; using default values.");
            }
        }
        #endregion

        // Update is called once per frame
        void Update()
        {
        
        }

#region Public Getters and Setters                                                                  //public getters and setters
        public string GetCharacterName() { return characterName; }                                  //Method for returning Character Name
        public string GetCharacterRace() { return characterRace; }                                  //Method for returning Character Name
        public int GetCharacterAge() { return characterAge; }                                       //Method for returning Character Name
        public char GetCharacterGender() { return characterGender; }                                //Method for returning Character Name

        public int GetCharacterLevel() { return characterLevel; }                                   //Method for returning Character Name
        public int GetCharacterXP() { return characterXP; }                                         //Method for returning Character Name

        public int GetCharacterStrength() { return characterStrength; }                             //Method for returning Character Name
        public int GetCharacterIntelligence() { return characterIntelligence; }                     //Method for returning Character Name
        public int GetCharacterAgility() { return characterAgility; }                               //Method for returning Character Name
        public int GetCharacterAttributePoints() { return characterAttributePoints; }               //Method for returning Character Name
    #endregion

        
        private void HandleLevelUpdate(int newLevel)                                               // Method to handle level update events from LVLSystem
        {
            characterLevel = newLevel;
        }
        private void HandleLifetimeXpUpdate(int newLifetimeXp)                                     // Method to handle lifetime XP update events from LVLSystem
        {
            characterXP = newLifetimeXp;
        }

        private void HandleAttributeUpdate(string attributeName, int newValue)                     // Method to handle attribute updates events from AttributeSystem
        {
            switch (attributeName)                                                                //switch statement to determine which attribute was updated
            {
                case "Strength":                                                                  //case for strength updates, to update character sheet's local strength variable and logs the new value
                    characterStrength = newValue;
                    Debug.Log("CharacterSheet: Updated Strength: " + characterStrength);
                    break;                                                                          //case break
                case "Intelligence":                                                             //case for Intelligence updates, ^^        
                    characterIntelligence = newValue;
                    Debug.Log("CharacterSheet: Updated Intelligence: " + characterIntelligence);
                    break;                                                                          //case break                          
                case "Agility":                                                                 //case for Agility updates, ^^               
                    characterAgility = newValue;
                    Debug.Log("CharacterSheet: Updated Agility: " + characterAgility);
                    break;                                                                        //case break                  
            }  
        }

        private void HandleAttributePointUpdate(int newAttributePoints)                         //Method for handling Attribute point  updates 
        {
            characterAttributePoints = newAttributePoints;
            Debug.Log("CharacterSheet: Unspent Attribute Points: " + characterAttributePoints);

        }


    }
}
