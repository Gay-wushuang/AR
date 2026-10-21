using UnityEngine;

namespace EEGAI.Demo
{
    public class DemoCompactVitalsHUD : MonoBehaviour
    {
        private const int TraceSamples = 96;
        private static readonly string[] FrequencyBandNames =
        {
            "DELTA", "THETA", "ALPHA 1", "ALPHA 2",
            "BETA 1", "BETA 2", "GAMMA 1", "GAMMA 2"
        };
        private static readonly Color[] FrequencyBandColors =
        {
            new Color(0.62f, 0.38f, 1f), new Color(0.30f, 0.55f, 1f),
            new Color(0.12f, 0.82f, 1f), new Color(0.20f, 0.92f, 0.62f),
            new Color(0.86f, 0.94f, 0.28f), new Color(1f, 0.62f, 0.18f),
            new Color(1f, 0.32f, 0.28f), new Color(1f, 0.38f, 0.72f)
        };

        private DemoEmotionSimulator emotionSimulator;
        private DemoAttentionSystem attentionSystem;
        private DemoMeditationSystem meditationSystem;
        private DemoOxygenSystem oxygenSystem;
        private DemoRepairSystem repairSystem;
        private EmotionPanel emotionPanel;

        private readonly float[] attentionTrace = new float[TraceSamples];
        private readonly float[] meditationTrace = new float[TraceSamples];
        private int traceHead;
        private int traceCount;
        private float nextTraceSampleTime;

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

            SampleTrace();
        }

        private void SampleTrace()
        {
            if (Time.unscaledTime < nextTraceSampleTime)
            {
                return;
            }

            nextTraceSampleTime = Time.unscaledTime + 0.1f;
            attentionTrace[traceHead] = attentionSystem != null ? attentionSystem.NormalizedAttention : 0f;
            meditationTrace[traceHead] = meditationSystem != null ? meditationSystem.NormalizedMeditation : 0f;
            traceHead = (traceHead + 1) % TraceSamples;
            if (traceCount < TraceSamples)
            {
                traceCount++;
            }
        }

        private void OnGUI()
        {
            if (!DemoUIVisibilityController.ShowVisualizations)
            {
                return;
            }

            float width = Mathf.Min(560f, Screen.width - 36f);
            Rect rect = new Rect(18f, Screen.height - 322f, width, 300f);

            GUI.color = new Color(0f, 0f, 0f, 0.58f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;

            GUILayout.BeginArea(rect);
            GUILayout.Space(8);
            DrawEegTrace();
            DrawSourceStatus();

            bool eegLive = IsEegLive();
            DrawBar("OXYGEN", oxygenSystem != null ? oxygenSystem.NormalizedOxygen : 1f, new Color(0.28f, 0.95f, 0.8f, 1f), true);
            DrawBar("REPAIR", repairSystem != null ? repairSystem.OverallProgress / 100f : 0f, new Color(1f, 0.75f, 0.25f, 1f), true);
            DrawBar("ATTENTION", attentionSystem != null ? attentionSystem.NormalizedAttention : 0f, new Color(0.45f, 0.72f, 1f, 1f), eegLive);
            DrawBar("MEDITATION", meditationSystem != null ? meditationSystem.NormalizedMeditation : 0f, new Color(0.55f, 1f, 0.55f, 1f), eegLive);
            DrawFrequencyBands();
            GUILayout.EndArea();
        }

        private bool HasFreshPowerBands()
        {
            return emotionPanel != null && emotionPanel.currentStreamConnected &&
                emotionPanel.MessageAgeSeconds <= 3f && emotionPanel.currentPowerPacketCount > 0 &&
                emotionPanel.currentPowerAgeSeconds <= 3f && emotionPanel.currentPowerBands != null &&
                emotionPanel.currentPowerBands.Length >= FrequencyBandNames.Length;
        }

        private bool HasLivePowerBands()
        {
            return HasFreshPowerBands() && emotionPanel.currentDeviceConnected &&
                emotionPanel.currentPoorSignal < 200;
        }

        private void DrawFrequencyBands()
        {
            bool live = HasLivePowerBands();
            bool fresh = HasFreshPowerBands();
            GUILayout.Space(3f);
            Rect title = GUILayoutUtility.GetRect(1f, 18f, GUILayout.ExpandWidth(true));
            string titleText = live
                ? "EEG SPECTRUM  |  RELATIVE ASIC BAND POWER"
                : fresh
                    ? $"EEG SPECTRUM  |  POOR SIGNAL ({emotionPanel.currentPoorSignal}); CHECK HEADSET CONTACT"
                    : "EEG SPECTRUM  |  WAITING FOR BAND DATA";
            GUI.Label(title, titleText);

            Rect grid = GUILayoutUtility.GetRect(1f, 62f, GUILayout.ExpandWidth(true));
            int[] powers = live ? emotionPanel.currentPowerBands : null;
            long total = 0;
            int maximum = 0;
            if (powers != null)
            {
                for (int i = 0; i < FrequencyBandNames.Length; i++)
                {
                    int power = Mathf.Max(0, powers[i]);
                    total += power;
                    maximum = Mathf.Max(maximum, power);
                }
            }

            float cellWidth = grid.width * 0.25f;
            float rowHeight = grid.height * 0.5f;
            for (int i = 0; i < FrequencyBandNames.Length; i++)
            {
                int column = i % 4;
                int row = i / 4;
                Rect cell = new Rect(grid.x + column * cellWidth + 2f, grid.y + row * rowHeight + 1f,
                    cellWidth - 4f, rowHeight - 2f);
                Color bandColor = FrequencyBandColors[i];
                GUI.color = new Color(1f, 1f, 1f, 0.10f);
                GUI.DrawTexture(cell, Texture2D.whiteTexture);

                float relativePower = live && maximum > 0
                    ? Mathf.Sqrt(Mathf.Max(0, powers[i]) / (float)maximum)
                    : 0f;
                float share = live && total > 0 ? Mathf.Max(0, powers[i]) / (float)total : 0f;
                Rect bar = new Rect(cell.x + 5f, cell.y + 18f, cell.width - 10f, 7f);
                GUI.color = new Color(bandColor.r, bandColor.g, bandColor.b, live ? 0.24f : 0.10f);
                GUI.DrawTexture(bar, Texture2D.whiteTexture);
                GUI.color = new Color(bandColor.r, bandColor.g, bandColor.b,
                    live ? 0.35f + 0.65f * relativePower : 0.12f);
                GUI.DrawTexture(new Rect(bar.x, bar.y, bar.width * relativePower, bar.height), Texture2D.whiteTexture);
                GUI.color = live ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.7f);
                GUI.Label(new Rect(cell.x + 5f, cell.y + 1f, cell.width - 10f, 16f),
                    live ? $"{FrequencyBandNames[i]}  {share * 100f:F0}%" : FrequencyBandNames[i]);
            }
            GUI.color = Color.white;
        }

        private bool IsEegLive()
        {
            if (emotionPanel == null || !emotionPanel.currentDeviceConnected || emotionPanel.MessageAgeSeconds > 3f)
            {
                return false;
            }

            bool attentionLive = attentionSystem == null || attentionSystem.ExternalDataLive;
            bool meditationLive = meditationSystem == null || meditationSystem.ExternalDataLive;
            return attentionLive && meditationLive;
        }

        private void DrawEegTrace()
        {
            Rect lineRect = GUILayoutUtility.GetRect(1f, 34f, GUILayout.ExpandWidth(true));
            bool live = IsEegLive();
            int[] rawSamples = emotionPanel != null ? EmotionPanel.CurrentRawSamples : null;
            bool hasRaw = live && rawSamples != null && rawSamples.Length >= 8;

            string mode = !live ? "OFFLINE" : (hasRaw ? "RAW" : "eSense");
            GUI.Label(new Rect(lineRect.x, lineRect.y + 8f, 100f, 18f), $"EEG {mode}");

            float left = lineRect.x + 112f;
            float width = lineRect.width - 120f;
            float top = lineRect.y + 3f;
            float height = lineRect.height - 6f;

            if (!live)
            {
                float middle = top + height * 0.5f;
                DrawLine(new Vector2(left, middle), new Vector2(left + width, middle),
                    new Color(1f, 1f, 1f, 0.22f), 1f);
                GUI.Label(new Rect(left, lineRect.y + 8f, width, 18f), "NO EEG SIGNAL");
                return;
            }

            if (hasRaw)
            {
                DrawRawTrace(rawSamples, left, top, width, height);
                return;
            }

            DrawSeries(attentionTrace, new Color(0.45f, 0.72f, 1f, 0.95f), left, top, width, height);
            DrawSeries(meditationTrace, new Color(0.55f, 1f, 0.55f, 0.95f), left, top, width, height);
        }

        private static void DrawRawTrace(int[] samples, float left, float top, float width, float height)
        {
            int count = Mathf.Min(TraceSamples, samples.Length);
            if (count < 2)
            {
                return;
            }

            int start = samples.Length - count;
            float min = samples[start];
            float max = samples[start];
            for (int i = start + 1; i < samples.Length; i++)
            {
                min = Mathf.Min(min, samples[i]);
                max = Mathf.Max(max, samples[i]);
            }

            float span = Mathf.Max(1f, max - min);
            Vector2 previous = Vector2.zero;
            for (int i = 0; i < count; i++)
            {
                float value = (samples[start + i] - min) / span;
                Vector2 point = new Vector2(
                    left + width * (i / (float)(count - 1)),
                    top + height * (1f - value));
                if (i > 0)
                {
                    DrawLine(previous, point, new Color(0.95f, 0.75f, 0.25f, 0.95f), 1f);
                }
                previous = point;
            }
        }

        private void DrawSeries(float[] values, Color color, float left, float top, float width, float height)
        {
            if (traceCount < 2)
            {
                return;
            }

            Vector2 previous = Vector2.zero;
            for (int i = 0; i < traceCount; i++)
            {
                int index = (traceHead - traceCount + i + TraceSamples) % TraceSamples;
                Vector2 point = new Vector2(
                    left + width * (i / (float)(traceCount - 1)),
                    top + height * (1f - Mathf.Clamp01(values[index])));
                if (i > 0)
                {
                    DrawLine(previous, point, color, 1.5f);
                }
                previous = point;
            }
        }

        private void DrawSourceStatus()
        {
            if (emotionPanel == null)
            {
                GUILayout.Label("SRC unknown | no receiver");
                return;
            }

            bool live = emotionPanel.currentDeviceConnected;
            string state = live ? (IsEegLive() ? "LIVE" : "STALE") : "OFFLINE";
            string config = $"{EmotionPanel.CurrentConfigMode}:{EmotionPanel.CurrentConfigPort} v{EmotionPanel.CurrentServerVersion}";
            bool configOk = DemoEEGServiceLauncher.IsExpectedConfig(
                EmotionPanel.CurrentConfigMode, EmotionPanel.CurrentConfigPort);
            string configWarning = configOk ? string.Empty : " (MISMATCH)";

            string emotion = emotionPanel.currentEmotion.ToUpperInvariant();
            if (live && !EmotionPanel.CurrentInputComplete)
            {
                emotion += " (estimate)";
            }

            GUILayout.Label(
                $"SRC {emotionPanel.currentSource} | {state} | CFG {config}{configWarning} | " +
                $"signal {emotionPanel.currentPoorSignal} | eSense {emotionPanel.currentESenseAgeSeconds:F1}s");
            GUILayout.Label(
                $"RAW pkt {emotionPanel.currentRawPacketCount} | eSense pkt {emotionPanel.currentESensePacketCount} | " +
                $"ATT {emotionPanel.currentAttention} | MED {emotionPanel.currentMeditation} | EMO {emotion}");
        }

        private static void DrawBar(string label, float value, Color color, bool valid)
        {
            value = valid ? Mathf.Clamp01(value) : 0f;
            Rect row = GUILayoutUtility.GetRect(1f, 22f, GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(row.x, row.y, 94f, row.height), label);

            Rect bg = new Rect(row.x + 112f, row.y + 5f, row.width - 172f, 12f);
            GUI.color = new Color(1f, 1f, 1f, 0.16f);
            GUI.DrawTexture(bg, Texture2D.whiteTexture);
            GUI.color = valid ? color : new Color(0.55f, 0.55f, 0.55f, 0.6f);
            GUI.DrawTexture(new Rect(bg.x, bg.y, bg.width * value, bg.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(row.x + row.width - 52f, row.y, 50f, row.height), valid ? $"{value * 100f:F0}%" : "--");
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
