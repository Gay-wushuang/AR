using System;
using System.Collections;
using System.Collections.Generic;

namespace EEGAI.AI.LLM
{
    [Serializable]
    public class LLMMessage
    {
        public string role;
        public string content;

        public LLMMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }

    [Serializable]
    public class LLMResponse
    {
        public string content;
        public int tokensUsed;
        public double latencyMs;
        public bool isSuccess;
        public string errorMessage;

        public string emotionTag;
        public float confidenceScore;
        public Dictionary<string, object> metadata;

        public LLMResponse()
        {
            metadata = new Dictionary<string, object>();
            isSuccess = false;
        }
    }

    public interface ILLMService
    {
        IEnumerator SendMessageCoroutine(
            List<LLMMessage> messages,
            float temperature,
            int maxTokens,
            Action<LLMResponse> onComplete
        );

        IEnumerator SendMessageWithContextCoroutine(
            string userMessage,
            EEGAI.AI.Emotion.EmotionState currentEmotion,
            EEGAI.AI.Dialogue.ConversationContext context,
            Action<LLMResponse> onComplete
        );

        void SetApiKey(string apiKey);
        void SetModel(string model);
    }
}
