using UnityEngine;

namespace EEGAI.Demo
{
    public class DemoCompactVitalsHUD : MonoBehaviour
    {
        private DemoEmotionSimulator emotionSimulator;
        private DemoAttentionSystem attentionSystem;
        private DemoMeditationSystem meditationSystem;
        private DemoOxygenSystem oxygenSystem;
        private DemoRepairSystem repairSystem;
        private EmotionPanel emotionPanel;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindObjectOfType<DemoCompactVitalsHUD>() != null)
            {
                return;
            }

            var hud = new GameObject("DemoCompactVitalsHUD");
            hud.AddComponent<DemoCompactVitalsHUD>();
        }

        private void Update()
        {
            if (emotionSimulator == null) emotionSimulator = FindAnyObjectByType<DemoEmotionSimulator>();
            if (attentionSystem == null) attentionSystem = FindAnyObjectByType<DemoAttentionSystem>();
            if (meditationSystem == null) meditationSystem = FindAnyObjectByType<DemoMeditationSystem>();
            if (oxygenSystem == null) oxygenSystem = FindAnyObjectByType<DemoOxygenSystem>();
            if (repairSystem == null) repairSystem = FindAnyObjectByType<DemoRepairSystem>();
            if (emotionPanel == null) emotionPanel = FindAnyObjectByType<EmotionPanel>();
        }

        private void OnGUI()
        {
            if (!DemoUIVisibilityController.ShowVisualizations)
            {
                return;
            }

            float width = Mathf.Min(560f, Screen.width - 36f);
            Rect rect = new Rect(18f, Screen.height - 202f, width, 180f);

            GUI.color = new Color(0f, 0f, 0f, 0.58f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;

            GUILayout.BeginArea(rect);
            GUILayout.Space(8);
            DrawEmotionWave();
            DrawSourceStatus();
            DrawBar("OXYGEN", oxygenSystem != null ? oxygenSystem.NormalizedOxygen : 1f, new Color(0.28f, 0.95f, 0.8f, 1f));
            DrawBar("REPAIR", repairSystem != null ? repairSystem.OverallProgress / 100f : 0f, new Color(1f, 0.75f, 0.25f, 1f));
            DrawBar("ATTENTION", attentionSystem != null ? attentionSystem.NormalizedAttention : 0f, new Color(0.45f, 0.72f, 1f, 1f));
            DrawBar("MEDITATION", meditationSystem != null ? meditationSystem.NormalizedMeditation : 0f, new Color(0.55f, 1f, 0.55f, 1f));
            GUILayout.EndArea();
        }

        private void DrawSourceStatus()
        {
            if (emotionPanel == null)
            {
                GUILayout.Label("SRC unknown | no receiver");
                return;
            }

            string live = emotionPanel.currentDeviceConnected ? "LIVE" : "OFFLINE";
            GUILayout.Label($"SRC {emotionPanel.currentSource} | {live} | signal {emotionPanel.currentPoorSignal} | eSense {emotionPanel.currentESenseAgeSeconds:F1}s | raw {emotionPanel.currentRawPacketCount} eSense {emotionPanel.currentESensePacketCount}");
            GUILayout.Label($"RAW {emotionPanel.currentRawEmotion.ToUpperInvariant()} | ATT {emotionPanel.currentAttention} | MED {emotionPanel.currentMeditation}");
        }

        private void DrawEmotionWave()
        {
            string emotion = emotionSimulator != null ? emotionSimulator.GetEmotionString().ToUpperInvariant() : "NORMAL";
            Rect lineRect = GUILayoutUtility.GetRect(1f, 30f, GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(lineRect.x, lineRect.y, 94f, lineRect.height), $"EMOTION {emotion}");

            Color color = GetEmotionColor(emotion);
            float left = lineRect.x + 112f;
            float width = lineRect.width - 120f;
            float centerY = lineRect.y + lineRect.height * 0.5f;

            for (int i = 0; i < 64; i++)
            {
                float t0 = i / 64f;
                float t1 = (i + 1) / 64f;
                float y0 = Mathf.Sin((Time.time * 2.4f + t0 * 8f) * Mathf.PI) * 9f;
                float y1 = Mathf.Sin((Time.time * 2.4f + t1 * 8f) * Mathf.PI) * 9f;
                DrawLine(new Vector2(left + width * t0, centerY + y0), new Vector2(left + width * t1, centerY + y1), color, 2f);
            }
        }

        private static void DrawBar(string label, float value, Color color)
        {
            value = Mathf.Clamp01(value);
            Rect row = GUILayoutUtility.GetRect(1f, 22f, GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(row.x, row.y, 94f, row.height), label);

            Rect bg = new Rect(row.x + 112f, row.y + 5f, row.width - 172f, 12f);
            GUI.color = new Color(1f, 1f, 1f, 0.16f);
            GUI.DrawTexture(bg, Texture2D.whiteTexture);
            GUI.color = color;
            GUI.DrawTexture(new Rect(bg.x, bg.y, bg.width * value, bg.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(row.x + row.width - 52f, row.y, 50f, row.height), $"{value * 100f:F0}%");
        }

        private static Color GetEmotionColor(string emotion)
        {
            switch (emotion)
            {
                case "HAPPY": return new Color(1f, 0.78f, 0.2f, 1f);
                case "SAD": return new Color(0.35f, 0.55f, 1f, 1f);
                default: return new Color(0.62f, 1f, 0.62f, 1f);
            }
        }

        private static void DrawLine(Vector2 start, Vector2 end, Color color, float width)
        {
            Matrix4x4 matrix = GUI.matrix;
            Color previousColor = GUI.color;
            float angle = Vector3.Angle(end - start, Vector2.right) * (start.y > end.y ? -1f : 1f);
            float length = (end - start).magnitude;

            GUI.color = color;
            GUIUtility.RotateAroundPivot(angle, start);
            GUI.DrawTexture(new Rect(start.x, start.y - width * 0.5f, length, width), Texture2D.whiteTexture);
            GUI.matrix = matrix;
            GUI.color = previousColor;
        }
    }
}
