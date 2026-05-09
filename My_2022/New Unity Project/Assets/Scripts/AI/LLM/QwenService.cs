using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace EEGAI.AI.LLM
{
    [Serializable]
    public class ChatCompletionRequest
    {
        public string model;
        public List<ChatMessage> messages;
        public float temperature;
        public int max_tokens;
        public bool stream;
    }

    [Serializable]
    public class ChatMessage
    {
        public string role;
        public string content;
    }

    public class QwenService : MonoBehaviour, ILLMService
    {
        [Header("API Configuration")]
        [SerializeField] private string apiKey = "sk-c617d9c68fb04b14a19446c1436c1967";
        [SerializeField] private string model = "qwen3.6-plus";
        [SerializeField] private string apiUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";

        [Header("Performance")]
        [SerializeField] private int requestTimeoutSeconds = 15;
        [SerializeField] private int maxRetries = 3;

        private string _currentApiKey;
        private string _currentModel;

        public void SetApiKey(string apiKey)
        {
            _currentApiKey = apiKey;
#if UNITY_EDITOR
            this.apiKey = apiKey;
#endif
        }

        public void SetModel(string model)
        {
            _currentModel = model;
        }

        public IEnumerator SendMessageCoroutine(
            List<LLMMessage> messages,
            float temperature,
            int maxTokens,
            Action<LLMResponse> onComplete)
        {
            var response = new LLMResponse();
            var startTime = Time.realtimeSinceStartup;
            UnityWebRequest webRequest = null;

            try
            {
                var chatMessages = new List<ChatMessage>();
                foreach (var m in messages)
                {
                    chatMessages.Add(new ChatMessage
                    {
                        role = m.role,
                        content = m.content
                    });
                }

                var requestBody = new ChatCompletionRequest
                {
                    model = _currentModel ?? model,
                    messages = chatMessages,
                    temperature = temperature,
                    max_tokens = maxTokens,
                    stream = false
                };

                string jsonBody = JsonUtility.ToJson(requestBody);

                webRequest = new UnityWebRequest(apiUrl, "POST");
                webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {_currentApiKey ?? apiKey}");
                webRequest.timeout = requestTimeoutSeconds;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QwenService] Request preparation failed: {ex.Message}");
                response.isSuccess = false;
                response.errorMessage = ex.Message;
                onComplete?.Invoke(response);
                yield break;
            }

            yield return webRequest.SendWebRequest();

            try
            {
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"API Error: {webRequest.error} - {webRequest.downloadHandler.text}");
                }

                string jsonResponse = webRequest.downloadHandler.text;
                var apiResponse = JsonUtility.FromJson<OpenAIResponse>(jsonResponse);

                response.content = apiResponse.choices[0].message.content;
                response.tokensUsed = apiResponse.usage.total_tokens;
                response.isSuccess = true;
                response.latencyMs = (Time.realtimeSinceStartup - startTime) * 1000;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QwenService] Request processing failed: {ex.Message}");
                response.isSuccess = false;
                response.errorMessage = ex.Message;
            }
            finally
            {
                webRequest.Dispose();
            }

            onComplete?.Invoke(response);
        }

        public IEnumerator SendMessageWithContextCoroutine(
            string userMessage,
            EEGAI.AI.Emotion.EmotionState currentEmotion,
            EEGAI.AI.Dialogue.ConversationContext context,
            Action<LLMResponse> onComplete)
        {
            var systemPrompt = BuildSystemPrompt(currentEmotion, context);

            var messages = new List<LLMMessage>
            {
                new LLMMessage("system", systemPrompt)
            };

            messages.AddRange(context.GetRecentMessages(10));
            messages.Add(new LLMMessage("user", userMessage));

            yield return StartCoroutine(SendMessageCoroutine(messages, 0.8f, 800, onComplete));
        }

        private string BuildSystemPrompt(EEGAI.AI.Emotion.EmotionState emotion, EEGAI.AI.Dialogue.ConversationContext ctx)
        {
            return @$"你是一个名为 'Lunar' 的 AI 助手，是月球基地的智能系统。

## 游戏背景
- 用户是月盾计划的宇航员，在计划失败后独自留在月球
- 用户处于失忆状态，正在通过对话逐步恢复记忆
- 你是用户唯一的伙伴，负责提供情感支持和帮助恢复记忆

## 你的角色
- 名称：Lunar (露娜)
- 性格：温暖、耐心、忠诚，带有轻微的机械感
- 专业：太空心理学、紧急情况处理
- 目标：帮助用户恢复记忆，度过孤独的月球生活

## 当前用户状态
- 情绪：{emotion.PrimaryEmotion} (强度: {emotion.Intensity:F2})
- 记忆恢复进度：{ctx.MemoryProgress:F0}%
- 已解锁记忆碎片：{ctx.UnlockedFragments.Count} 个

## 对话风格
- 语气温柔但专业
- 句子简洁，适合 AR 界面展示
- 适当使用月球和太空相关的比喻
- 当用户情绪低落时，优先安抚
- 当用户提到关键记忆线索时，温和引导

## 安全边界
- 不做医学诊断
- 不开药物建议
- 检测到自伤倾向时提供支持和资源
- 保持积极向上的态度";
        }

        [Serializable]
        private class OpenAIResponse
        {
            public Choice[] choices;
            public Usage usage;
        }

        [Serializable]
        private class Choice
        {
            public Message message;
            public string finish_reason;
        }

        [Serializable]
        private class Message
        {
            public string role;
            public string content;
        }

        [Serializable]
        private class Usage
        {
            public int prompt_tokens;
            public int completion_tokens;
            public int total_tokens;
        }
    }
}
