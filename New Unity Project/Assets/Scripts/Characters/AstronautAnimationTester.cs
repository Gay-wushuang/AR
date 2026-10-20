using UnityEngine;
using UnityEngine.InputSystem;

public class AstronautAnimationTester : MonoBehaviour
{
    private AstronautAnimationController _animationController;

    private void Awake()
    {
        _animationController = GetComponent<AstronautAnimationController>();
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            _animationController.SetIdle();
            Debug.Log("Playing: Idle");
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {
            _animationController.SetWalk();
            Debug.Log("Playing: Walk");
        }
        else if (keyboard.digit3Key.wasPressedThisFrame)
        {
            _animationController.SetFloat();
            Debug.Log("Playing: Float");
        }
        else if (keyboard.digit4Key.wasPressedThisFrame)
        {
            _animationController.SetWave();
            Debug.Log("Playing: Wave");
        }
    }
}
