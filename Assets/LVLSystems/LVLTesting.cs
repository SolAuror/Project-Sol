using UnityEngine;
using UnityEngine.InputSystem;

public class LVLTesting : MonoBehaviour
{
    private LVLSystem lvlSystem;

    private void Awake()
    {
        lvlSystem =  GetComponent<LVLSystem>();
    }

    private void Update()
    {
        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            lvlSystem.AddXP(60);
        }
    }
}
