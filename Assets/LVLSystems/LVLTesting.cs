using UnityEngine;
using UnityEngine.InputSystem;
using Character.LevelSystem;

public class LVLTesting : MonoBehaviour
{
    private LVLSystem lvlSystem;
    private int xpToNext;
    private int currentLVL;
    [SerializeField] private int XpGain = 50;

    private void Awake()
    {
        lvlSystem = gameObject.GetComponent<LVLSystem>();
        if (lvlSystem.GetCharacterLevel() == 0)
        {
            lvlSystem.AddXP(100);
        }
    }

    private void Update()
    {
        if (Keyboard.current.xKey.isPressed)
        {
             lvlSystem.AddXP(XpGain);
        }
        if (Keyboard.current.vKey.isPressed)
        {
            //add a debug key for increasing attributes
        }

        
    }
}
