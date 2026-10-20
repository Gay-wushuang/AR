using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EEGAI.AI.Emotion;

namespace EEGAI.UI.Visualization
{
    public class EmotionVisualizer : MonoBehaviour
    {
        [Header("EEG Wave Display")]
        [SerializeField] private GameObject waveDisplayContainer;
        [SerializeField] private WaveGraph alphaWaveGraph;
        [SerializeField] private WaveGraph betaWaveGraph;
        [SerializeField] private WaveGraph thetaWaveGraph;
        [SerializeField] private WaveGraph deltaWaveGraph;

        [Header("Emotion State Display")]
        [SerializeField] private Image emotionIcon;
        [SerializeField] private TextMeshProUGUI emotionLabel;
        [SerializeField] private Slider emotionIntensitySlider;
        [SerializeField] private Gradient emotionColorGradient;

        [Header("History Display")]
        [SerializeField] private RectTransform historyContainer;
        [SerializeField] private GameObject historyItemPrefab;
        [SerializeField] private int maxHistoryItems = 10;

        [Header("Settings")]
        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private bool showEEGWaves = true;
        [SerializeField] private bool showHistory = true;

        private EmotionAnalyzer emotionAnalyzer;
        private float updateTimer;
        private List<EmotionHistoryItem> historyItems = new List<EmotionHistoryItem>();

        private void Awake()
        {
            emotionAnalyzer = FindAnyObjectByType<EmotionAnalyzer>();
        }

        private void Start()
        {
            InitializeDisplay();
        }

        private void Update()
        {
            if (emotionAnalyzer == null) return;

            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                UpdateDisplay();
                updateTimer = 0f;
            }
        }

        private void InitializeDisplay()
        {
            if (waveDisplayContainer != null)
            {
                waveDisplayContainer.SetActive(showEEGWaves);
            }

            if (historyContainer != null)
            {
                historyContainer.gameObject.SetActive(showHistory);
            }
        }

        private void UpdateDisplay()
        {
            var state = emotionAnalyzer.GetCurrentState();

            UpdateEmotionIcon(state);
            UpdateEmotionLabel(state);
            UpdateEmotionIntensity(state);
            UpdateEEGWaves(state);

            if (showHistory)
            {
                AddToHistory(state);
            }
        }

        private void UpdateEmotionIcon(EmotionState state)
        {
            if (emotionIcon == null) return;

            var color = GetEmotionColor(state.PrimaryEmotion);
            emotionIcon.color = color;

            var sprite = GetEmotionSprite(state.PrimaryEmotion);
            if (sprite != null)
            {
                emotionIcon.sprite = sprite;
            }
        }

        private void UpdateEmotionLabel(EmotionState state)
        {
            if (emotionLabel == null) return;
            emotionLabel.text = state.GetDescription();
        }

        private void UpdateEmotionIntensity(EmotionState state)
        {
            if (emotionIntensitySlider == null) return;

            emotionIntensitySlider.value = state.Intensity;

            if (emotionColorGradient != null)
            {
                var fill = emotionIntensitySlider.fillRect;
                if (fill != null)
                {
                    var fillImage = fill.GetComponent<Image>();
                    if (fillImage != null)
                    {
                        fillImage.color = emotionColorGradient.Evaluate(state.Intensity);
                    }
                }
            }
        }

        private void UpdateEEGWaves(EmotionState state)
        {
            if (!showEEGWaves) return;

            alphaWaveGraph?.UpdateWave(state.AlphaWaveRatio);
            betaWaveGraph?.UpdateWave(state.BetaWaveRatio);
            thetaWaveGraph?.UpdateWave(state.ThetaWaveRatio);
            deltaWaveGraph?.UpdateWave(0.1f);
        }

        private void AddToHistory(EmotionState state)
        {
            if (historyItemPrefab == null || historyContainer == null) return;

            if (historyItems.Count >= maxHistoryItems)
            {
                var oldest = historyItems[0];
                historyItems.RemoveAt(0);
                if (oldest.gameObject != null)
                {
                    Destroy(oldest.gameObject);
                }
            }

            GameObject historyObj = Instantiate(historyItemPrefab, historyContainer);
            var historyItem = historyObj.GetComponent<EmotionHistoryItem>();

            if (historyItem != null)
            {
                historyItem.SetData(state);
                historyItems.Add(historyItem);
            }
        }

        private Color GetEmotionColor(EmotionType emotion)
        {
            return emotion switch
            {
                EmotionType.Happy => new Color(1f, 0.84f, 0f, 1f),
                EmotionType.Sad => new Color(0.29f, 0.65f, 0.94f, 1f),
                EmotionType.Anxious => new Color(1f, 0.34f, 0.13f, 1f),
                EmotionType.Calm => new Color(0.18f, 0.8f, 0.44f, 1f),
                EmotionType.Angry => new Color(0.95f, 0.1f, 0.1f, 1f),
                _ => new Color(0.8f, 0.8f, 0.8f, 1f)
            };
        }

        private Sprite GetEmotionSprite(EmotionType emotion)
        {
            return null;
        }

        public void ToggleEEGWaves(bool show)
        {
            showEEGWaves = show;
            if (waveDisplayContainer != null)
            {
                waveDisplayContainer.SetActive(showEEGWaves);
            }
        }

        public void ToggleHistory(bool show)
        {
            showHistory = show;
            if (historyContainer != null)
            {
                historyContainer.gameObject.SetActive(showHistory);
            }
        }

        public void ClearHistory()
        {
            foreach (var item in historyItems)
            {
                if (item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            historyItems.Clear();
        }
    }

    public class WaveGraph : MonoBehaviour
    {
        [SerializeField] private RectTransform graphContainer;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private int maxPoints = 100;
        [SerializeField] private Color lineColor = Color.green;
        [SerializeField] private float lineWidth = 2f;

        private List<float> waveData = new List<float>();

        private void Awake()
        {
            if (lineRenderer != null)
            {
                lineRenderer.startColor = lineColor;
                lineRenderer.endColor = lineColor;
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = lineWidth;
            }
        }

        public void UpdateWave(float value)
        {
            waveData.Add(value);
            if (waveData.Count > maxPoints)
            {
                waveData.RemoveAt(0);
            }

            RenderGraph();
        }

        private void RenderGraph()
        {
            if (lineRenderer == null || graphContainer == null) return;

            lineRenderer.positionCount = waveData.Count;

            for (int i = 0; i < waveData.Count; i++)
            {
                float x = (i / (float)waveData.Count) * graphContainer.rect.width;
                float y = waveData[i] * graphContainer.rect.height;
                lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            }
        }
    }

    public class EmotionHistoryItem : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private TextMeshProUGUI timestamp;

        public void SetData(EmotionState state)
        {
            if (label != null)
            {
                label.text = state.GetDescription();
            }

            if (timestamp != null)
            {
                timestamp.text = DateTime.Now.ToString("HH:mm:ss");
            }

            if (icon != null)
            {
                var color = GetEmotionColor(state.PrimaryEmotion);
                icon.color = color;
            }
        }

        private Color GetEmotionColor(EmotionType emotion)
        {
            return emotion switch
            {
                EmotionType.Happy => new Color(1f, 0.84f, 0f, 1f),
                EmotionType.Sad => new Color(0.29f, 0.65f, 0.94f, 1f),
                EmotionType.Anxious => new Color(1f, 0.34f, 0.13f, 1f),
                EmotionType.Calm => new Color(0.18f, 0.8f, 0.44f, 1f),
                EmotionType.Angry => new Color(0.95f, 0.1f, 0.1f, 1f),
                _ => new Color(0.8f, 0.8f, 0.8f, 1f)
            };
        }
    }
}
