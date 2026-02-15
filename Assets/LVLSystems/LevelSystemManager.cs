using UnityEngine;
using Character.Sheet;

namespace Character.LevelSystem
{
    [RequireComponent(typeof(LVLSystem))]
    [RequireComponent(typeof(AttributeSystem))]
    [RequireComponent(typeof(CharacterSheet))]
    public class LevelSystemManager : MonoBehaviour
    {
        public LVLSystem LevelSystem { get; private set; }
        public AttributeSystem AttributeSystem { get; private set; }
        public CharacterSheet Sheet { get; private set; }

        void Awake()
        {
            LevelSystem = GetComponent<LVLSystem>();
            AttributeSystem = GetComponent<AttributeSystem>();
            Sheet = GetComponent<CharacterSheet>();

            // Wire cross-references so subsystems don't hunt for each other
            LevelSystem.Initialize(this);
            AttributeSystem.Initialize(this);
        }
    }
}
