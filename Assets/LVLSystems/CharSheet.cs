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

        void Awake()                                                   //system initialization
        {
            _levelSystem = GetComponent<LVLSystem>();                     //set the reference to the levelsystem component on the same game object
            _attributeSystem = GetComponent<AttributeSystem>();                     //set the reference to the attributesystem component on the same game object

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
                _attributeSystem.OnStrengthUpdate += HandleStrengthUpdate;
                _attributeSystem.OnIntelligenceUpdate += HandleIntelligenceUpdate;
                _attributeSystem.OnAgilityUpdate += HandleAgilityUpdate;
                _attributeSystem.OnAttributePointUpdate += HandleAttributePointUpdate;
            }
            else
            {
                Debug.LogWarning("CharacterSheet: Attribute System component not found on the same GameObject; using default values.");
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }

    #region Public Getters and Setters
        public string CharacterName                                 //public getter for character name, to allow other scripts to access the character's name if needed.
        {
            get { return characterName; }
            set { characterName = value; }
        }
    #endregion

        
        private void HandleLevelUpdate(int newLevel)                                             // called by LVLSystem when the character levels up
        {
            characterLevel = newLevel;
            Debug.Log("CharacterSheet: level updated to " + newLevel + " via LVLSystem event.");
        }
        private void HandleLifetimeXpUpdate(int newLifetimeXp)                                             // called by LVLSystem when the character's lifetime XP changes
        {
            characterXP = newLifetimeXp;
            Debug.Log("CharacterSheet: lifetime XP updated to " + newLifetimeXp + " via LVLSystem event.");
        }

        private void HandleStrengthUpdate(string attributeName, int newStrength)                                             // called by AttributeSystem when the character's attributes change
        {
            
            characterStrength = newStrength;
            Debug.Log("CharacterSheet: attribute updated - Strength: " + characterStrength);
        }

        private void HandleIntelligenceUpdate(string attributeName, int newIntelligence)                                             // called by AttributeSystem when the character's attributes change
        {
            characterIntelligence = newIntelligence;
            Debug.Log("CharacterSheet: attribute updated - Intelligence: " + characterIntelligence);
        }

        private void HandleAgilityUpdate(string attributeName, int newAgility)                                             // called by AttributeSystem when the character's attributes change
        {
            characterAgility = newAgility;
            Debug.Log("CharacterSheet: attribute updated - Agility: " + characterAgility);
        }

        private void HandleAttributePointUpdate(int newAttributePoints)                                             // called by AttributeSystem when the character's attribute points change
        {
            characterAttributePoints = newAttributePoints;
            Debug.Log("CharacterSheet: attribute updated - Unspent Attribute Points: " + characterAttributePoints);


    }


}
}
