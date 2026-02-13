using UnityEngine;
using UnityEngine.InputSystem;
using Character.LVLSystem;

public class LVLTesting : MonoBehaviour
{
    private LVLSystem lvlSystem;
    private int xpToNext;
    private int currentLVL;
    [SerializeField] private int XpGain = 50;

    private void Awake()
    {
        lvlSystem = gameObject.GetComponent<LVLSystem>();
    }

    private void Update()
    {
        if (Keyboard.current.xKey.isPressed)
        {
             lvlSystem.AddXP(XpGain);
        }
        if (Keyboard.current.vKey.isPressed)
        {
            currentLVL = lvlSystem.GetCurrentLVL();
            xpToNext = currentLVL * 100;
            lvlSystem.AddXP(xpToNext);
            Debug.Log("V Key Pressed: Added " + (xpToNext * 100) + " XP");
        }

        
    }
}
