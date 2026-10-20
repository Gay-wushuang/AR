using UnityEngine;
using UnityEngine.InputSystem;

namespace EEGAI.Demo
{
    public class DemoEndStateOverlay : MonoBehaviour
    {
        private DemoGameState gameState;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindObjectOfType<DemoEndStateOverlay>() != null)
            {
                return;
            }

            var overlay = new GameObject("DemoEndStateOverlay");
            overlay.AddComponent<DemoEndStateOverlay>();
        }

        private void Update()
        {
            if (gameState == null)
            {
                gameState = FindAnyObjectByType<DemoGameState>();
            }

            if (gameState == null ||
                gameState.Status == DemoGameState.GameStatus.MainMenu ||
                gameState.Status == DemoGameState.GameStatus.Playing ||
                gameState.Status == DemoGameState.GameStatus.ReadyToLaunch ||
                gameState.Status == DemoGameState.GameStatus.Launching)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                gameState.ResetGame();
            }
        }

        private void OnGUI()
        {
            if (DemoMainMenu.PauseMenuOpen)
            {
                return;
            }

            if (gameState == null)
            {
                gameState = FindAnyObjectByType<DemoGameState>();
            }

            if (gameState == null)
            {
                return;
            }

            if (gameState.Status == DemoGameState.GameStatus.ReadyToLaunch)
            {
                DrawStatusHint("REPAIRS COMPLETE", "Approach the rocket and press E to launch.");
                return;
            }

            if (gameState.Status == DemoGameState.GameStatus.Launching)
            {
                DrawStatusHint("LAUNCHING", "Rocket ignition sequence in progress.");
                return;
            }

            if (gameState.Status == DemoGameState.GameStatus.Playing)
            {
                return;
            }

            if (gameState.Status != DemoGameState.GameStatus.Victory &&
                gameState.Status != DemoGameState.GameStatus.GameOver)
            {
                return;
            }

            float width = Mathf.Min(560f, Screen.width - 40f);
            float height = 260f;
            Rect rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

            GUI.color = new Color(0f, 0f, 0f, 0.82f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;

            GUILayout.BeginArea(rect);
            GUILayout.FlexibleSpace();

            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 40,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };

            var bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                wordWrap = true
            };

            if (gameState.Status == DemoGameState.GameStatus.Victory)
            {
                GUI.color = new Color(0.25f, 1f, 0.55f, 1f);
                GUILayout.Label("VICTORY", titleStyle);
                GUI.color = Color.white;
                GUILayout.Label("All repair nodes completed. Ship systems restored.", bodyStyle);
            }
            else
            {
                GUI.color = new Color(1f, 0.28f, 0.16f, 1f);
                GUILayout.Label("MISSION FAILED", titleStyle);
                GUI.color = Color.white;
                GUILayout.Label("Oxygen depleted before the ship was repaired.", bodyStyle);
            }

            GUILayout.Space(18);
            GUILayout.Label("Press R to return to menu", bodyStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
        }

        private void DrawStatusHint(string title, string body)
        {
            Rect rect = new Rect(Screen.width * 0.5f - 220f, 24f, 440f, 86f);
            GUI.color = new Color(0f, 0f, 0f, 0.68f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;

            GUILayout.BeginArea(rect);
            GUILayout.Space(8);
            GUILayout.Label(title, new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold
            });
            GUILayout.Label(body, new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15
            });
            GUILayout.EndArea();
        }
    }
}
