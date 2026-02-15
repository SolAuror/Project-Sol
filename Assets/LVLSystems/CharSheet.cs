using System;
using UnityEngine;
using TMPro;
using Character.LevelSystem;

namespace Character.Sheet
{
    public class CharacterSheet : MonoBehaviour
    {
#region System References

        private LVLSystem _levelSystem;                                 //Reference to the level system, to fetch current level and xp for display on character sheet.
        private AttributeSystem _attributeSystem;                                 //Reference to the attribute system, to fetch current attribute values for display on character sheet.

    #endregion

#region CharacterSheet Variables
        [SerializeField] private string characterName = "Sol";                        //character name, as a string, can be changed in editor
        [SerializeField] private string characterRace = "Developer";                 //character species, as a string, can be changed in editor
        [SerializeField] private int characterAge = 27;                                //character age, as an integer, can be changed in editor
        [SerializeField] private char characterGender = 'M';                           //character gender represented by a character, can be changed in editor

        [SerializeField] private int characterLevel;                                    //character level, as an integer, fetched from the level system to display current level on character sheet.                         
        [SerializeField] private int characterXP;                                       //character XP, as an integer, fetched from level system to track total XP gains for the character

        [SerializeField] private int characterStrength;                                                            //character strength attribute, as an integer
        [SerializeField] private int characterIntelligence;                                                        //character intelligence attribute, as an integer
        [SerializeField] private int characterAgility;                                                             //character agility attribute, as an integer
        [SerializeField] private int characterAttributePoints;                                                      //unspent attribute points, as an integer, that can be allocated to strength, intelligence, or agility
    #endregion

#region Initialization
        void Awake()                                                   //system initialization
        {
            _levelSystem = GetComponent<LVLSystem>();                 //get reference to level system component on the same GameObject, if it exists
            _attributeSystem = GetComponent<AttributeSystem>();       //get reference to attribute system component on the same GameObject, if it exists
            // initialize runtime values that depend on LVLSystem
            if (_levelSystem != null)
            {
                // keep local copy in sync and subscribe to future changes
                characterLevel = _levelSystem.GetCharacterLevel();
                characterXP = _levelSystem.GetLifetimeTotalXP();
                _levelSystem.OnCharacterLevelUpdate += HandleLevelUpdate;                             //subscribe to the level update event from LVLSystem, to update character sheet info when level changes
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

                _attributeSystem.OnAttributeUpdate += HandleAttributeUpdate;                             //subscribe to the attribute update event from AttributeSystem, to update character sheet info when attributes change
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

#region Public Getters and Setters
        public string GetCharacterName() { return characterName; }
        public string GetCharacterRace() { return characterRace; }
        public int GetCharacterAge() { return characterAge; }
        public char GetCharacterGender() { return characterGender; }

        public int GetCharacterLevel() { return characterLevel; }
        public int GetCharacterXP() { return characterXP; }

        public int GetCharacterStrength() { return characterStrength; }
        public int GetCharacterIntelligence() { return characterIntelligence; }
        public int GetCharacterAgility() { return characterAgility; }
        public int GetCharacterAttributePoints() { return characterAttributePoints; }
    #endregion

        
        private void HandleLevelUpdate(int newLevel)                                                        // called by LVLSystem when the character levels up
        {
            characterLevel = newLevel;
        }
        private void HandleLifetimeXpUpdate(int newLifetimeXp)                                             // called by LVLSystem when the character's lifetime XP changes
        {
            characterXP = newLifetimeXp;
        }

        private void HandleAttributeUpdate(string attributeName, int newValue)                             // called by AttributeSystem when the character's attributes change
        {
            switch (attributeName)
            {
                case "Strength":
                    characterStrength = newValue;
                    Debug.Log("CharacterSheet: Updated Strength: " + characterStrength);
                    break;
                case "Intelligence":
                    characterIntelligence = newValue;
                    Debug.Log("CharacterSheet: Updated Intelligence: " + characterIntelligence);
                    break;
                case "Agility":
                    characterAgility = newValue;
                    Debug.Log("CharacterSheet: Updated Agility: " + characterAgility);
                    break;
            }
        }

        private void HandleAttributePointUpdate(int newAttributePoints)                                             // called by AttributeSystem when the character's attribute points change
        {
            characterAttributePoints = newAttributePoints;
            Debug.Log("CharacterSheet: Unspent Attribute Points: " + characterAttributePoints);

        }


    }
}
