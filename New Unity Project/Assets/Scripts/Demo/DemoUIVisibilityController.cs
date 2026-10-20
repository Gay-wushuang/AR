using UnityEngine;
using UnityEngine.InputSystem;
using EEGAI.UI.Visualization;

namespace EEGAI.Demo
{
    public class DemoUIVisibilityController : MonoBehaviour
    {
        public enum DisplayMode
        {
            Visualization,
            DebugPanels
        }

        public static DisplayMode CurrentMode { get; private set; } = DisplayMode.Visualization;
        public static bool ShowVisualizations => IsPlayingState() && !DemoMainMenu.PauseMenuOpen && CurrentMode == DisplayMode.Visualization;
        public static bool ShowDebugPanels => IsPlayingState() && !DemoMainMenu.PauseMenuOpen && CurrentMode == DisplayMode.DebugPanels;

        private static DemoGameState gameState;
        private float refreshTimer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindObjectOfType<DemoUIVisibilityController>() != null)
            {
                return;
            }

            var controller = new GameObject("DemoUIVisibilityController");
            controller.AddComponent<DemoUIVisibilityController>();
        }

        private void Update()
        {
            if (gameState == null)
            {
                gameState = FindAnyObjectByType<DemoGameState>();
            }

            if (Keyboard.current != null &&
                Keyboard.current.f12Key.wasPressedThisFrame &&
                IsPlayingState() &&
                !DemoMainMenu.PauseMenuOpen)
            {
                ToggleMode();
            }

            refreshTimer += Time.unscaledDeltaTime;
            if (refreshTimer >= 0.25f)
            {
                refreshTimer = 0f;
                ApplySceneVisualizationState();
            }
        }

        public static void SetVisualizationMode()
        {
            CurrentMode = DisplayMode.Visualization;
            ApplyCursorState();
        }

        public static void ToggleMode()
        {
            CurrentMode = CurrentMode == DisplayMode.Visualization
                ? DisplayMode.DebugPanels
                : DisplayMode.Visualization;

            Debug.Log($"[DemoUI] Display mode: {CurrentMode}");
            ApplyCursorState();
        }

        private static bool IsPlayingState()
        {
            if (gameState == null)
            {
                gameState = FindAnyObjectByType<DemoGameState>();
            }

            if (gameState == null)
            {
                return Application.isPlaying;
            }

            return gameState.Status == DemoGameState.GameStatus.Playing ||
                gameState.Status == DemoGameState.GameStatus.ReadyToLaunch ||
                gameState.Status == DemoGameState.GameStatus.Launching;
        }

        private static void ApplyCursorState()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (CurrentMode == DisplayMode.DebugPanels)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private static void ApplySceneVisualizationState()
        {
            bool visible = ShowVisualizations;

            foreach (var visualizer in FindObjectsOfType<EmotionVisualizer>(true))
            {
                SetCanvasGroupVisible(visualizer.gameObject, visible);
            }
        }

        private static void SetCanvasGroupVisible(GameObject target, bool visible)
        {
            if (target == null)
            {
                return;
            }

            CanvasGroup group = target.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = target.AddComponent<CanvasGroup>();
            }

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
