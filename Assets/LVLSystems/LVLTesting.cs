using UnityEngine;
using UnityEngine.InputSystem;

public class LVLTesting : MonoBehaviour
{
    private LVLSystem lvlSystem;

    private void Awake()
    {
        lvlSystem = new LVLSystem();
        lvlSystem.AddXP(50);
        lvlSystem.AddXP(60);
    }

    private void Update()
    {
        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            lvlSystem.AddXP(60);
        }
    }
}
