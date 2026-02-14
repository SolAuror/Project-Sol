using System;
using UnityEngine;
using TMPro;
using Character.XpSystem;

namespace Character.Sheet
{
    public class CharacterSheet : MonoBehaviour
    {
    #region System References

        private LVLSystem levelSystem;                                 //Reference to the level system, to fetch current level and xp for display on character sheet.

    #endregion

    #region CharacterSheet Variables
        [SerializeField] private string characterName = "Sol";                        //character name, as a string, can be changed in editor
        [SerializeField] private string characterRace = "Developer";                 //character species, as a string, can be changed in editor

        [SerializeField] private int characterAge = 27;                                //character age, as an integer, can be changed in editor
        [SerializeField] private char characterGender = 'M';                           //character gender represented by a character, can be changed in editor
        [SerializeField] private int characterLevel;                                    //character level, as an integer, fetched from the level system to display current level on character sheet.                         
        [SerializeField] private int characterXP;                                                        //character XP, as an integer, fetched from level system to track total XP gains for the character
    #endregion

        void Awake()                                                   //system initialization
        {
            levelSystem = GetComponent<LVLSystem>();                     //set the reference to the levelsystem component on the same game object

            // initialize runtime values that depend on LVLSystem
            if (levelSystem != null)
            {
                // keep local copy in sync and subscribe to future changes
                characterLevel = levelSystem.GetCharacterLevel();
                characterXP = levelSystem.GetLifetimeTotalXP();
                levelSystem.OnCharacterLevelUpdate += HandleLevelUpdate;                             //subscribe to the level update event from LVLSystem, to update character sheet info when level changes
                levelSystem.OnLifetimeXpUpdate += HandleLifetimeXpUpdate;
            }
            else
            {
                Debug.LogWarning("CharacterSheet: LVLSystem component not found on the same GameObject; using inspector/default values.");
            }
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

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
