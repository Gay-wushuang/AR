using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EEGAI.AI.LLM;
using EEGAI.AI.Emotion;
using EEGAI.Game.Memory;

namespace EEGAI.AI.Dialogue
{
    [Serializable]
    public class ConversationContext
    {
        public List<LLMMessage> RecentMessages = new List<LLMMessage>();
        public string CurrentTopic;
        public float MemoryProgress;
        public List<MemoryFragment> UnlockedFragments = new List<MemoryFragment>();
        public Dictionary<string, object> SessionData = new Dictionary<string, object>();

        public List<LLMMessage> GetRecentMessages(int count)
        {
            return RecentMessages.Count <= count
                ? RecentMessages
                : RecentMessages.GetRange(RecentMessages.Count - count, count);
        }

        public void AddMessage(LLMMessage message)
        {
            RecentMessages.Add(message);

            if (RecentMessages.Count > 50)
            {
                RecentMessages.RemoveRange(0, RecentMessages.Count - 50);
            }
        }

        public void UpdateMemoryProgress(MemoryProgress progress)
        {
            MemoryProgress = progress.currentProgress;
        }
    }

    public class DialogueManager : MonoBehaviour
    {
        [Header("Core Services")]
        [SerializeField] private QwenService llmService;
        [SerializeField] private EmotionAnalyzer emotionAnalyzer;
        [SerializeField] private MemorySystem memorySystem;

        [Header("Configuration")]
        [SerializeField] private float typingDelay = 0.05f;

        private ConversationContext context = new ConversationContext();
        private bool isProcessing = false;

        public event Action<string> OnUserMessage;
        public event Action<string> OnAIMessage;
        public event Action<string> OnSystemMessage;
        public event Action OnConversationStarted;

        private void Awake()
        {
            if (llmService == null) llmService = FindAnyObjectByType<QwenService>();
            if (emotionAnalyzer == null) emotionAnalyzer = FindAnyObjectByType<EmotionAnalyzer>();
            if (memorySystem == null) memorySystem = FindAnyObjectByType<MemorySystem>();
        }

        private void Start()
        {
            memorySystem?.LoadProgress();
            UpdateContextFromMemory();
            StartConversation();
        }

        public void ProcessUserInput(string userInput)
        {
            if (isProcessing || string.IsNullOrWhiteSpace(userInput)) return;
            StartCoroutine(ProcessUserInputCoroutine(userInput));
        }

        private IEnumerator ProcessUserInputCoroutine(string userInput)
        {
            isProcessing = true;
            LLMResponse response = null;

            try
            {
                OnUserMessage?.Invoke(userInput);
                context.AddMessage(new LLMMessage("user", userInput));

                var emotionState = emotionAnalyzer.GetCurrentState();
                var enrichedState = emotionAnalyzer.EnrichWithTextSentiment(
                    emotionState, userInput, CalculateSentimentScore(userInput)
                );

                UpdateContextFromMemory();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DialogueManager] Pre-processing error: {ex.Message}");
                OnSystemMessage?.Invoke("系统错误，请稍后再试");
                isProcessing = false;
                yield break;
            }

            yield return StartCoroutine(llmService.SendMessageWithContextCoroutine(
                userInput, emotionAnalyzer.GetCurrentState(), context, r => response = r
            ));

            try
            {
                if (response != null && response.isSuccess)
                {
                    OnAIMessage?.Invoke(response.content);
                    context.AddMessage(new LLMMessage("assistant", response.content));
                }
                else
                {
                    string errorMsg = response != null ? response.errorMessage : "未知错误";
                    OnSystemMessage?.Invoke($"错误: {errorMsg}");
                }

                memorySystem?.SaveProgress();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DialogueManager] Post-processing error: {ex.Message}");
                OnSystemMessage?.Invoke("系统错误，请稍后再试");
            }
            finally
            {
                isProcessing = false;
            }
        }

        private void StartConversation()
        {
            Debug.Log("[DialogueManager] StartConversation() called");
            OnConversationStarted?.Invoke();

            string welcomeMessage = "你好，我是 Lunar，月球基地的智能助手。你现在在月球上，月盾计划失败了，你是唯一的幸存者。你似乎失去了部分记忆，但不用担心，我们会一起找回它们的。你现在感觉怎么样？";

            Debug.Log($"[DialogueManager] Invoking OnAIMessage with: {welcomeMessage.Substring(0, Mathf.Min(30, welcomeMessage.Length))}...");
            OnAIMessage?.Invoke(welcomeMessage);
            context.AddMessage(new LLMMessage("assistant", welcomeMessage));
            Debug.Log("[DialogueManager] StartConversation() completed");
        }

        private void UpdateContextFromMemory()
        {
            if (memorySystem != null)
            {
                var progress = memorySystem.Progress;
                context.MemoryProgress = progress.currentProgress;
                context.UnlockedFragments = memorySystem.UnlockedFragments;
            }
        }

        private float CalculateSentimentScore(string text)
        {
            var positiveWords = new[] { "开心", "高兴", "好", "喜欢", "希望", "成功", "安全" };
            var negativeWords = new[] { "难过", "伤心", "害怕", "焦虑", "失败", "危险", "孤独" };

            int positiveCount = 0;
            int negativeCount = 0;

            foreach (var word in positiveWords)
            {
                if (text.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0) positiveCount++;
            }

            foreach (var word in negativeWords)
            {
                if (text.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0) negativeCount++;
            }

            return Mathf.Clamp((positiveCount - negativeCount) / 5f, -1f, 1f);
        }
    }
}
