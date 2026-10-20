using System;
using System.Collections.Generic;
using UnityEngine;

namespace EEGAI.Game.Memory
{
    [Serializable]
    public class MemoryFragment
    {
        public string id;
        public string title;
        public string description;
        public string content;
        public float unlockThreshold;
        public string[] triggerKeywords;
        public string category;
        public bool isUnlocked;
        public DateTime unlockedAt;

        public string imagePath;
        public string audioPath;
        public float emotionalIntensity;

        public MemoryFragment()
        {
            id = Guid.NewGuid().ToString();
            triggerKeywords = new string[0];
            isUnlocked = false;
            emotionalIntensity = 0.5f;
        }

        public MemoryFragment(string id, string title, string content, float threshold) : this()
        {
            this.id = id;
            this.title = title;
            this.content = content;
            this.unlockThreshold = threshold;
        }
    }

    [Serializable]
    public class MemoryProgress
    {
        public float currentProgress;
        public int unlockedFragments;
        public int totalFragments;
        public string currentPhase;
        public DateTime lastUpdate;

        public MemoryProgress()
        {
            currentProgress = 0f;
            unlockedFragments = 0;
            totalFragments = 0;
            currentPhase = "觉醒";
            lastUpdate = DateTime.UtcNow;
        }

        public float GetCompletionPercentage()
        {
            return totalFragments > 0 ? (float)unlockedFragments / totalFragments * 100f : 0f;
        }

        public bool IsComplete()
        {
            return currentProgress >= 100f || (totalFragments > 0 && unlockedFragments >= totalFragments);
        }
    }
}
