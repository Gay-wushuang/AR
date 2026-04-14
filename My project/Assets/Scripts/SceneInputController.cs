using UnityEngine;
using UnityEngine.InputSystem;

public class SceneInputController : MonoBehaviour
{
    [Header("引用设置")]
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private bool useBGMManager = false;

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        HandleSkyboxControls(keyboard);
        HandleBGMControls(keyboard);
    }

    void HandleSkyboxControls(Keyboard keyboard)
    {
        if (sceneManager == null) return;

        if (keyboard.f1Key.wasPressedThisFrame)
        {
            sceneManager.PreviousSkybox();
        }
        else if (keyboard.f2Key.wasPressedThisFrame)
        {
            sceneManager.NextSkybox();
        }
        else if (keyboard.digit1Key.wasPressedThisFrame)
        {
            sceneManager.SetSkybox(0);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {
            sceneManager.SetSkybox(1);
        }
        else if (keyboard.digit3Key.wasPressedThisFrame)
        {
            sceneManager.SetSkybox(2);
        }
    }

    void HandleBGMControls(Keyboard keyboard)
    {
        if (useBGMManager && BGM_DontDestroy.Instance != null)
        {
            if (keyboard.f3Key.wasPressedThisFrame)
            {
                BGM_DontDestroy.Instance.PreviousBGM();
            }
            else if (keyboard.f4Key.wasPressedThisFrame)
            {
                BGM_DontDestroy.Instance.NextBGM();
            }
            else if (keyboard.digit4Key.wasPressedThisFrame)
            {
                BGM_DontDestroy.Instance.SetBGM(0);
            }
            else if (keyboard.digit5Key.wasPressedThisFrame)
            {
                BGM_DontDestroy.Instance.SetBGM(1);
            }
            else if (keyboard.digit6Key.wasPressedThisFrame)
            {
                BGM_DontDestroy.Instance.SetBGM(2);
            }
            else if (keyboard.mKey.wasPressedThisFrame)
            {
                BGM_DontDestroy.Instance.ToggleMute();
            }
        }
        else if (sceneManager != null)
        {
            if (keyboard.f3Key.wasPressedThisFrame)
            {
                sceneManager.PreviousBGM();
            }
            else if (keyboard.f4Key.wasPressedThisFrame)
            {
                sceneManager.NextBGM();
            }
            else if (keyboard.digit4Key.wasPressedThisFrame)
            {
                sceneManager.SetBGM(0);
            }
            else if (keyboard.digit5Key.wasPressedThisFrame)
            {
                sceneManager.SetBGM(1);
            }
            else if (keyboard.digit6Key.wasPressedThisFrame)
            {
                sceneManager.SetBGM(2);
            }
            else if (keyboard.mKey.wasPressedThisFrame)
            {
                sceneManager.ToggleMute();
            }
        }
    }
}
