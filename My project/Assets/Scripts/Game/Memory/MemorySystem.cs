using System;
using System.Collections.Generic;
using UnityEngine;

namespace EEGAI.Game.Memory
{
    public class MemorySystem : MonoBehaviour
    {
        [Header("Memory Configuration")]
        [SerializeField] private List<MemoryFragment> memoryFragments = new List<MemoryFragment>();
        [SerializeField] private float baseProgressIncrement = 5f;
        [SerializeField] private float emotionBonusMultiplier = 1.5f;

        [Header("Game Events")]
        [SerializeField] private string completionEventName = "MemoryRecoveryComplete";

        private MemoryProgress progress;
        private Dictionary<string, MemoryFragment> fragmentLookup = new Dictionary<string, MemoryFragment>();

        public MemoryProgress Progress => progress;
        public List<MemoryFragment> UnlockedFragments => memoryFragments.FindAll(f => f.isUnlocked);

        public event Action<MemoryFragment> OnMemoryUnlocked;
        public event Action OnMemoryRecoveryComplete;

        private void Awake()
        {
            progress = new MemoryProgress();
            progress.totalFragments = memoryFragments.Count;

            foreach (var fragment in memoryFragments)
            {
                fragmentLookup[fragment.id] = fragment;
            }
        }

        private void Start()
        {
            InitializeDefaultFragments();
        }

        private void InitializeDefaultFragments()
        {
            if (memoryFragments.Count == 0)
            {
                memoryFragments.Add(new MemoryFragment("M001", "苏醒", "你从低温睡眠中醒来，头很痛，周围是陌生的月球基地环境。", 0f)
                {
                    category = "觉醒",
                    triggerKeywords = new[] { "醒来", "头痛", "基地", "睡眠" }
                });

                memoryFragments.Add(new MemoryFragment("M002", "月盾计划", "你模糊记得自己是月盾计划的宇航员，任务是在月球建立防御系统。", 10f)
                {
                    category = "觉醒",
                    triggerKeywords = new[] { "月盾", "计划", "任务", "宇航员", "防御" }
                });

                memoryFragments.Add(new MemoryFragment("M003", "通讯中断", "基地的通讯系统完全静默，你尝试联系地球但没有回应。", 20f)
                {
                    category = "觉醒",
                    triggerKeywords = new[] { "通讯", "地球", "联系", "静默", "无线电" }
                });

                memoryFragments.Add(new MemoryFragment("M004", "孤独感", "你意识到自己可能是唯一的幸存者，巨大的孤独感袭来。", 30f)
                {
                    category = "觉醒",
                    triggerKeywords = new[] { "孤独", "幸存", "唯一", "一个人" }
                });

                memoryFragments.Add(new MemoryFragment("M005", "基地日志", "你找到基地的日志系统，记录显示其他宇航员在撤离时遭遇了意外。", 40f)
                {
                    category = "探索",
                    triggerKeywords = new[] { "日志", "撤离", "意外", "记录", "其他" }
                });

                memoryFragments.Add(new MemoryFragment("M006", "防御系统", "月盾计划的核心是一个先进的防御系统，用于抵御可能的小行星撞击。", 50f)
                {
                    category = "探索",
                    triggerKeywords = new[] { "防御", "系统", "小行星", "撞击", "保护" }
                });

                memoryFragments.Add(new MemoryFragment("M007", "故障", "防御系统出现了严重故障，导致计划失败，这可能与你的失忆有关。", 60f)
                {
                    category = "探索",
                    triggerKeywords = new[] { "故障", "失败", "失忆", "错误", "崩溃" }
                });

                memoryFragments.Add(new MemoryFragment("M008", "自我怀疑", "你开始怀疑自己是否对故障负有责任，记忆碎片开始变得混乱。", 70f)
                {
                    category = "探索",
                    triggerKeywords = new[] { "责任", "怀疑", "混乱", "我的错", "我做了什么" }
                });

                memoryFragments.Add(new MemoryFragment("M009", "关键决策", "你回忆起自己在危机时刻做出了一个关键决策，可能拯救了地球。", 80f)
                {
                    category = "真相",
                    triggerKeywords = new[] { "决策", "危机", "拯救", "选择", "我决定" }
                });

                memoryFragments.Add(new MemoryFragment("M010", "牺牲", "为了修复防御系统，你主动留在了月球，放弃了撤离的机会。", 90f)
                {
                    category = "真相",
                    triggerKeywords = new[] { "牺牲", "修复", "撤离", "留下", "放弃" }
                });

                memoryFragments.Add(new MemoryFragment("M011", "希望", "尽管计划失败，你仍然相信人类的未来，这是你坚持下去的动力。", 100f)
                {
                    category = "真相",
                    triggerKeywords = new[] { "希望", "未来", "坚持", "相信", "人类" }
                });

                progress.totalFragments = memoryFragments.Count;
                fragmentLookup.Clear();
                foreach (var fragment in memoryFragments)
                {
                    fragmentLookup[fragment.id] = fragment;
                }
            }
        }

        public void ProcessDialogue(string userInput, float emotionalIntensity)
        {
            float progressGain = baseProgressIncrement;

            if (emotionalIntensity > 0.7f)
            {
                progressGain *= emotionBonusMultiplier;
            }

            foreach (var fragment in memoryFragments)
            {
                if (!fragment.isUnlocked)
                {
                    foreach (var keyword in fragment.triggerKeywords)
                    {
                        if (userInput.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            UnlockFragment(fragment.id);
                            progressGain += 10f;
                            break;
                        }
                    }
                }
            }

            progress.currentProgress = Mathf.Min(progress.currentProgress + progressGain, 100f);
            progress.lastUpdate = DateTime.UtcNow;

            CheckThresholdUnlocks();

            if (progress.IsComplete())
            {
                TriggerCompletionEvent();
            }
        }

        public void UnlockFragment(string fragmentId)
        {
            if (fragmentLookup.TryGetValue(fragmentId, out var fragment) && !fragment.isUnlocked)
            {
                fragment.isUnlocked = true;
                fragment.unlockedAt = DateTime.UtcNow;
                progress.unlockedFragments++;

                Debug.Log($"🧠 记忆碎片解锁: {fragment.title}");
                OnMemoryUnlocked?.Invoke(fragment);
            }
        }

        private void CheckThresholdUnlocks()
        {
            foreach (var fragment in memoryFragments)
            {
                if (!fragment.isUnlocked && progress.currentProgress >= fragment.unlockThreshold)
                {
                    UnlockFragment(fragment.id);
                }
            }
        }

        private void TriggerCompletionEvent()
        {
            Debug.Log("🎉 记忆恢复完成！游戏结局触发");
            OnMemoryRecoveryComplete?.Invoke();
        }

        public List<MemoryFragment> GetFragmentsByPhase(string phase)
        {
            return memoryFragments.FindAll(f => f.category == phase && f.isUnlocked);
        }

        public void SaveProgress()
        {
            string json = JsonUtility.ToJson(progress);
            PlayerPrefs.SetString("MemoryProgress", json);

            var unlockedIds = new List<string>();
            foreach (var fragment in memoryFragments)
            {
                if (fragment.isUnlocked)
                {
                    unlockedIds.Add(fragment.id);
                }
            }
            PlayerPrefs.SetString("UnlockedFragments", string.Join(",", unlockedIds));
            PlayerPrefs.Save();
        }

        public void LoadProgress()
        {
            if (PlayerPrefs.HasKey("MemoryProgress"))
            {
                string json = PlayerPrefs.GetString("MemoryProgress");
                progress = JsonUtility.FromJson<MemoryProgress>(json);
            }

            if (PlayerPrefs.HasKey("UnlockedFragments"))
            {
                string unlockedStr = PlayerPrefs.GetString("UnlockedFragments");
                var unlockedIds = unlockedStr.Split(',');

                foreach (var id in unlockedIds)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        UnlockFragment(id);
                    }
                }
            }
        }
    }
}
