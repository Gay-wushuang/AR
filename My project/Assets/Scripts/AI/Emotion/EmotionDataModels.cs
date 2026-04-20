using System;
using System.Collections.Generic;

namespace EEGAI.AI.Emotion
{
    [Serializable]
    public enum EmotionType
    {
        Calm,
        Happy,
        Anxious,
        Sad,
        Angry,
        Stressed,
        Confused,
        Neutral
    }

    [Serializable]
    public class EmotionState
    {
        public EmotionType PrimaryEmotion;
        public float Confidence;
        public float Intensity;
        public double DurationMinutes;
        public DateTime DetectedAt;
        public Dictionary<EmotionType, float> SecondaryEmotions;

        public float AlphaWaveRatio;
        public float BetaWaveRatio;
        public float ThetaWaveRatio;

        public EmotionState()
        {
            PrimaryEmotion = EmotionType.Neutral;
            Confidence = 0.3f;
            Intensity = 0.5f;
            DetectedAt = DateTime.UtcNow;
            SecondaryEmotions = new Dictionary<EmotionType, float>();
        }

        public string GetDescription()
        {
            return $"{PrimaryEmotion} (强度:{Intensity:F2})";
        }

        public bool IsNegativeEmotion()
        {
            return PrimaryEmotion == EmotionType.Anxious ||
                   PrimaryEmotion == EmotionType.Sad ||
                   PrimaryEmotion == EmotionType.Angry ||
                   PrimaryEmotion == EmotionType.Stressed;
        }

        public bool RequiresIntervention()
        {
            return IsNegativeEmotion() && Intensity > 0.7f && DurationMinutes > 5.0;
        }
    }

    [Serializable]
    public class EEGRawData
    {
        public float[] channels;
        public float samplingRate;
        public DateTime timestamp;

        public EEGRawData()
        {
            channels = new float[8];
            samplingRate = 256f;
            timestamp = DateTime.UtcNow;
        }
    }
}
