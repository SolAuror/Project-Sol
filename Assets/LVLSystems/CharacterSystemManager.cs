using UnityEngine;
using Character.Sheet;

namespace Character.LevelSystem                                                         //namespace for the level system
{
    [RequireComponent(typeof(LVLSystem))]                                               //require component for LVLSystem, AttributeSystem, and CharacterSheet, 
    [RequireComponent(typeof(AttributeSystem))]                                         //to ensure they are all present on the same GameObject for easy 
    [RequireComponent(typeof(CharacterSheet))]                                          //reference and communication between systems.
    public class CharacterSystemManager : MonoBehaviour                                 //public class CharacterSystemManager, serves as a hub to communicate bewtween subsystems.
    {
        public LVLSystem LevelSystem { get; private set; }                              //public property for accessing the LVLSystem component, with a private setter to prevent external modification.   
        public AttributeSystem AttributeSystem { get; private set; }                    //public property for accessing the AttributeSystem component ^^
        public CharacterSheet Sheet { get; private set; }                               //public property for accessing the CharacterSheet component ^^

        void Awake()                                                                    //on Awake
        {
            LevelSystem = GetComponent<LVLSystem>();                                    //get reference to LVLSystem, AttributeSystem, and CharacterSheet component on the same GameObject, if it exists    
            AttributeSystem = GetComponent<AttributeSystem>();               
            Sheet = GetComponent<CharacterSheet>();                  

            LevelSystem.Initialize(this);                                               // Wire cross-references so subsystems don't hunt for each other    
            AttributeSystem.Initialize(this);
        }
    }
}
