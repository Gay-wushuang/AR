using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EEGAI.AI.Emotion
{
    public class EmotionAnalyzer : MonoBehaviour
    {
        [Header("EEG Thresholds")]
        [SerializeField] private float highAnxietyBetaThreshold = 0.4f;
        [SerializeField] private float calmAlphaThreshold = 0.35f;

        [Header("Time Windows")]
        [SerializeField] private float analysisIntervalSeconds = 30f;
        [SerializeField] private float interventionThresholdMinutes = 5f;

        [Header("Memory Integration")]
        [SerializeField] private Game.Memory.MemorySystem memorySystem;

        private Queue<EmotionState> emotionHistory = new Queue<EmotionState>();
        private EmotionState currentState;
        private float lastAnalysisTime;

        private void Awake()
        {
            if (memorySystem == null)
            {
                memorySystem = FindAnyObjectByType<Game.Memory.MemorySystem>();
            }
        }

        public EmotionState AnalyzeFromEEG(EEGRawData eegData)
        {
            var state = new EmotionState
            {
                DetectedAt = DateTime.UtcNow,
                AlphaWaveRatio = CalculateBandPower(eegData, "alpha"),
                BetaWaveRatio = CalculateBandPower(eegData, "beta"),
                ThetaWaveRatio = CalculateBandPower(eegData, "theta")
            };

            if (state.BetaWaveRatio > highAnxietyBetaThreshold)
            {
                state.PrimaryEmotion = EmotionType.Anxious;
                state.Confidence = Mathf.Min(state.BetaWaveRatio * 1.5f, 1.0f);
                state.Intensity = MapRange(state.BetaWaveRatio, 0.3f, 0.6f, 0.3f, 0.9f);
            }
            else if (state.AlphaWaveRatio > calmAlphaThreshold)
            {
                state.PrimaryEmotion = EmotionType.Calm;
                state.Confidence = state.AlphaWaveRatio;
                state.Intensity = 1.0f - MapRange(state.AlphaWaveRatio, 0.35f, 0.55f, 0.2f, 0.8f);
            }
            else
            {
                state.PrimaryEmotion = EmotionType.Neutral;
                state.Confidence = 0.5f;
                state.Intensity = 0.5f;
            }

            UpdateHistory(state);
            return state;
        }

        public EmotionState EnrichWithTextSentiment(EmotionState baseState, string userText, float sentimentScore)
        {
            if (Mathf.Abs(sentimentScore) > 0.6f)
            {
                baseState.Confidence = Mathf.Min(baseState.Confidence + 0.2f, 1.0f);

                if (sentimentScore < -0.6f)
                {
                    baseState.PrimaryEmotion = EmotionType.Sad;
                    baseState.Intensity = Mathf.Min(Mathf.Abs(sentimentScore), 1.0f);
                }
                else if (sentimentScore > 0.6f)
                {
                    baseState.PrimaryEmotion = EmotionType.Happy;
                    baseState.Intensity = Mathf.Min(sentimentScore, 1.0f);
                }
            }

            if (memorySystem != null)
            {
                memorySystem.ProcessDialogue(userText, baseState.Intensity);
            }

            return baseState;
        }

        public EmotionState GetCurrentState()
        {
            return currentState ?? new EmotionState
            {
                PrimaryEmotion = EmotionType.Neutral,
                Confidence = 0.3f,
                Intensity = 0.5f
            };
        }

        public string GetEmotionTrend()
        {
            if (emotionHistory.Count < 3) return "数据不足";

            var recent = new List<EmotionState>();
            int startIndex = Math.Max(0, emotionHistory.Count - 5);
            var historyArray = new List<EmotionState>(emotionHistory);
            
            for (int i = startIndex; i < emotionHistory.Count; i++)
            {
                recent.Add(historyArray[i]);
            }
            
            int negativeCount = 0;
            foreach (var e in recent)
            {
                if (e.IsNegativeEmotion())
                {
                    negativeCount++;
                }
            }

            if (negativeCount >= 4) return "持续负面，建议关注";
            if (negativeCount >= 2) return "波动较大";
            return "相对稳定";
        }

        private void UpdateHistory(EmotionState newState)
        {
            currentState = newState;
            emotionHistory.Enqueue(newState);

            if (emotionHistory.Count > 20)
                emotionHistory.Dequeue();
        }

        private float CalculateBandPower(EEGRawData data, string band)
        {
            switch (band)
            {
                case "alpha":
                    return UnityEngine.Random.Range(0.25f, 0.45f);
                case "beta":
                    return UnityEngine.Random.Range(0.2f, 0.5f);
                case "theta":
                    return UnityEngine.Random.Range(0.15f, 0.3f);
                default:
                    return 0.3f;
            }
        }

        private float MapRange(float value, float inMin, float inMax, float outMin, float outMax)
        {
            return Mathf.Clamp((value - inMin) / (inMax - inMin) * (outMax - outMin) + outMin, outMin, outMax);
        }
    }
}
