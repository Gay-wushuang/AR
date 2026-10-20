using UnityEngine;

namespace EEGAI.Demo
{
    public class DemoMissionGuide : MonoBehaviour
    {
        private DemoGameState gameState;
        private DemoRepairSystem repairSystem;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindObjectOfType<DemoMissionGuide>() != null)
            {
                return;
            }

            var guide = new GameObject("DemoMissionGuide");
            guide.AddComponent<DemoMissionGuide>();
        }

        private void Update()
        {
            if (gameState == null)
            {
                gameState = FindAnyObjectByType<DemoGameState>();
            }

            if (repairSystem == null)
            {
                repairSystem = FindAnyObjectByType<DemoRepairSystem>();
            }
        }

        private void OnGUI()
        {
            if (EnergyLinkPuzzle.Active) return;
            if (!DemoUIVisibilityController.ShowVisualizations)
            {
                return;
            }

            if (gameState == null ||
                gameState.Status == DemoGameState.GameStatus.MainMenu ||
                gameState.Status == DemoGameState.GameStatus.Victory ||
                gameState.Status == DemoGameState.GameStatus.GameOver)
            {
                return;
            }

            int repaired = repairSystem != null ? repairSystem.RepairedNodes : gameState.RepairedNodes;
            int total = repairSystem != null ? repairSystem.TotalNodes : gameState.TotalRepairNodes;

            Rect rect = new Rect(20f, 28f, 410f, 118f);
            GUI.color = new Color(0f, 0f, 0f, 0.68f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;

            GUILayout.BeginArea(rect);
            GUILayout.Space(8);
            GUILayout.Label("MISSION OBJECTIVES", new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold
            });

            DrawTask(ConsoleRadarSystem.Instance != null && ConsoleRadarSystem.Instance.ConsoleRepaired, "1. Repair the control console using the toolbox");
            DrawTask(repaired >= total, $"2. Collect scattered repair parts ({repaired}/{total})");

            string task2 = gameState.Status switch
            {
                DemoGameState.GameStatus.ReadyToLaunch => "3. Approach the rocket and press E",
                DemoGameState.GameStatus.Launching => "3. Rocket launch in progress",
                _ => "3. Repair the rocket and escape the planet"
            };
            DrawTask(gameState.Status == DemoGameState.GameStatus.Launching, task2);

            GUILayout.EndArea();
        }

        private static void DrawTask(bool done, string text)
        {
            GUI.color = done ? new Color(0.4f, 1f, 0.55f, 1f) : Color.white;
            GUILayout.Label($"{(done ? "[OK]" : "[  ]")} {text}", new GUIStyle(GUI.skin.label)
            {
                fontSize = 15
            });
            GUI.color = Color.white;
        }
    }
}
