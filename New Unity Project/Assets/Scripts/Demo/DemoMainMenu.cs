using UnityEngine;
using UnityEngine.InputSystem;

namespace EEGAI.Demo
{
    public class DemoMainMenu : MonoBehaviour
    {
        private DemoGameState gameState;
        private bool showSettings;
        private bool showTutorial;
        private Vector2 tutorialScroll;
        private float volume = 0.8f;
        private float mouseSensitivity = 2f;
        private string eegStatus = "EEG services will start with mission";
        public static bool PauseMenuOpen { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindObjectOfType<DemoMainMenu>() != null)
            {
                return;
            }

            var menu = new GameObject("DemoMainMenu");
            menu.AddComponent<DemoMainMenu>();
        }

        private void Start()
        {
            AudioListener.volume = volume;
            ApplyMouseSensitivity();
        }

        private void Update()
        {
            if (gameState == null)
            {
                gameState = FindAnyObjectByType<DemoGameState>();
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (PauseMenuOpen)
                {
                    SetPauseMenu(false);
                }
                else if (CanPauseGame())
                {
                    SetPauseMenu(true);
                }
            }

            if (PauseMenuOpen && !CanPauseGame())
            {
                SetPauseMenu(false, false);
            }
        }

        private void OnGUI()
        {
            if (gameState == null)
            {
                gameState = FindAnyObjectByType<DemoGameState>();
            }

            if (gameState == null)
            {
                return;
            }

            if (PauseMenuOpen)
            {
                DrawPauseSettings();
                return;
            }

            if (gameState.Status != DemoGameState.GameStatus.MainMenu)
            {
                return;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            float width = Mathf.Min(520f, Screen.width - 28f);
            float height = Mathf.Min(showSettings || showTutorial ? 520f : 414f, Screen.height - 28f);
            Rect rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUI.color = new Color(0f, 0f, 0f, 0.84f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;

            GUILayout.BeginArea(rect);
            GUILayout.Space(22);

            GUILayout.Label("MOON SURVIVOR", new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height * 0.085f, 30f, 38f)),
                fontStyle = FontStyle.Bold
            });

            GUILayout.Space(6);
            GUILayout.Label("Repair the rocket. Protect your oxygen. Escape the moon.", new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                wordWrap = true
            });

            GUILayout.Space(20);

            if (showTutorial)
            {
                DrawTutorial(rect);
            }
            else if (!showSettings)
            {
                if (GUILayout.Button("Start Mission", GUILayout.Height(40)))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    gameState.StartGame();
                }

                GUILayout.Space(6);
                if (GUILayout.Button("Tutorial", GUILayout.Height(38)))
                {
                    showTutorial = true;
                    tutorialScroll = Vector2.zero;
                }

                GUILayout.Space(6);
                if (GUILayout.Button("Settings", GUILayout.Height(38)))
                {
                    showSettings = true;
                }

                GUILayout.Space(6);
                if (GUILayout.Button("Quit", GUILayout.Height(38)))
                {
                    DemoEEGServiceLauncher.StopServices();
                    Application.Quit();
                }
            }
            else
            {
                DrawSettingsControls();

                GUILayout.Space(14);
                if (GUILayout.Button("Back", GUILayout.Height(38)))
                {
                    showSettings = false;
                }
            }

            if (!showTutorial)
            {
                GUILayout.Space(18);
                GUILayout.Label("Tasks:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
                GUILayout.Space(4);
                GUILayout.Label("1. Repair the control console using the toolbox");
                GUILayout.Label("2. Collect 3 scattered repair parts");
                GUILayout.Space(3);
                GUILayout.Label("3. Repair the rocket and escape the planet");
            }

            GUILayout.EndArea();
        }

        private void DrawTutorial(Rect panelRect)
        {
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };

            var bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                wordWrap = true
            };

            float scrollHeight = Mathf.Max(180f, panelRect.height - 194f);
            tutorialScroll = GUILayout.BeginScrollView(tutorialScroll, false, true, GUILayout.Height(scrollHeight));

            GUILayout.Label("How To Survive", titleStyle);
            GUILayout.Space(6);
            GUILayout.Label("Use the toolbox to repair the console. Activate the console radar, collect 3 scattered parts, then approach the rocket and press E to launch. A fixed oxygen canister remains at base; more supplies are scattered outside.", bodyStyle);

            GUILayout.Space(14);
            GUILayout.Label("Controls", titleStyle);
            GUILayout.Label("WASD / Arrow Keys: Move", bodyStyle);
            GUILayout.Label("Mouse: Look around", bodyStyle);
            GUILayout.Label("E: Use toolbox / console / parts / oxygen / rocket", bodyStyle);
            GUILayout.Label("Hold TAB + click: Select one radar target", bodyStyle);
            GUILayout.Label("Low oxygen alarm: refill when below 30%", bodyStyle);
            GUILayout.Label("Space: Low-gravity hop", bodyStyle);
            GUILayout.Label("F5: Switch camera view", bodyStyle);
            GUILayout.Label("ESC: Settings / pause", bodyStyle);
            GUILayout.Label("F12: Toggle EEG debug panels", bodyStyle);

            GUILayout.Space(14);
            GUILayout.Label("EEG Effects", titleStyle);
            GUILayout.Label("Attention increases repair speed.", bodyStyle);
            GUILayout.Label("Meditation slows oxygen loss.", bodyStyle);
            GUILayout.Label("Emotion changes ambience and movement speed.", bodyStyle);

            GUILayout.EndScrollView();

            GUILayout.Space(10);
            if (GUILayout.Button("Back", GUILayout.Height(38)))
            {
                showTutorial = false;
            }
        }

        private void DrawPauseSettings()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            float width = Mathf.Min(520f, Screen.width - 28f);
            float height = Mathf.Min(500f, Screen.height - 28f);
            Rect rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUI.color = new Color(0f, 0f, 0f, 0.86f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;

            GUILayout.BeginArea(rect);
            GUILayout.Space(20);
            GUILayout.Label("SETTINGS", new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 30,
                fontStyle = FontStyle.Bold
            });

            GUILayout.Space(18);
            DrawSettingsControls();

            GUILayout.Space(22);
            if (GUILayout.Button("Resume", GUILayout.Height(38)))
            {
                SetPauseMenu(false);
            }

            GUILayout.Space(6);
            if (GUILayout.Button("Restart Mission", GUILayout.Height(38)))
            {
                SetPauseMenu(false);
                gameState.StartGame();
            }

            GUILayout.Space(6);
            if (GUILayout.Button("Return To Main Menu", GUILayout.Height(38)))
            {
                SetPauseMenu(false, false);
                gameState.ResetGame();
            }

            GUILayout.EndArea();
        }

        private void DrawSettingsControls()
        {
            GUILayout.Label($"Volume: {volume:P0}");
            volume = GUILayout.HorizontalSlider(volume, 0f, 1f);
            AudioListener.volume = volume;

            GUILayout.Space(16);
            GUILayout.Label($"Mouse Sensitivity: {mouseSensitivity:F1}");
            mouseSensitivity = GUILayout.HorizontalSlider(mouseSensitivity, 0.5f, 6f);
            ApplyMouseSensitivity();

            GUILayout.Space(24);
            GUILayout.Label("EEG Device");
            GUILayout.Label(eegStatus, new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true
            });

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Test Data Flow", GUILayout.Height(34)))
            {
                bool live = DemoEEGServiceLauncher.TestRealtimeDataFlow();
                eegStatus = live ? "Realtime EEG data is live" : DemoEEGServiceLauncher.LastStatus;
            }

            if (GUILayout.Button("Restart EEG Services", GUILayout.Height(34)))
            {
                DemoEEGServiceLauncher.RestartServices();
                eegStatus = DemoEEGServiceLauncher.LastStatus;
            }
            GUILayout.EndHorizontal();
        }

        private void ApplyMouseSensitivity()
        {
            foreach (var astronaut in FindObjectsOfType<AstronautController>(true))
            {
                astronaut.mouseSensitivity = mouseSensitivity;
            }
        }

        private bool CanPauseGame()
        {
            return gameState != null &&
                (gameState.Status == DemoGameState.GameStatus.Playing ||
                 gameState.Status == DemoGameState.GameStatus.ReadyToLaunch);
        }

        private static void SetPauseMenu(bool open, bool lockCursorWhenClosing = true)
        {
            PauseMenuOpen = open;
            Time.timeScale = open ? 0f : 1f;

            if (open)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (lockCursorWhenClosing)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnDestroy()
        {
            if (PauseMenuOpen)
            {
                SetPauseMenu(false, false);
            }
        }
    }
}
