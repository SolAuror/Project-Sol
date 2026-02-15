using UnityEngine;
using UnityEngine.InputSystem;
using Character.LevelSystem;

public class LVLTesting : MonoBehaviour
{
    private LVLSystem lvlSystem;
    private AttributeSystem atrSystem;
    private int xpToNext;
    private int currentLVL;
    [SerializeField] private int XpGain = 50;

    private void Awake()
    {
        lvlSystem = gameObject.GetComponent<LVLSystem>();
        atrSystem = gameObject.GetComponent<AttributeSystem>();
    }

    private void Update()
    {
        if (Keyboard.current.xKey.isPressed)
        {
             lvlSystem.AddXP(XpGain);
        }
        if (Keyboard.current.vKey.isPressed)
        {
            atrSystem.AddAttributePoint("Strength");
            atrSystem.AddAttributePoint("Intelligence");
            atrSystem.AddAttributePoint("Agility");
        }

        
    }
}
