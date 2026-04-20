using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EEGAI.AI.Dialogue;
using EEGAI.AI.Emotion;
using EEGAI.Audio.TTS;

namespace EEGAI.UI.Chat
{
    public class EnhancedChatUI : MonoBehaviour
    {
        [Header("Core Components")]
        [SerializeField] private Transform messageContent;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendButton;

        [Header("Message Prefabs")]
        [SerializeField] private GameObject userMessagePrefab;
        [SerializeField] private GameObject aiMessagePrefab;
        [SerializeField] private GameObject systemMessagePrefab;

        [Header("Services")]
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private EmotionAnalyzer emotionAnalyzer;
        [SerializeField] private TTSService ttsService;

        [Header("Settings")]
        [SerializeField] private bool enableTTS = true;
        [SerializeField] private bool enableEmotionVisualization = true;
        [SerializeField] private float typingDelay = 0.05f;

        [Header("UI Elements")]
        [SerializeField] private Image emotionIndicator;
        [SerializeField] private TextMeshProUGUI emotionText;
        [SerializeField] private Slider progressBar;

        private bool isProcessing = false;
        private Coroutine typingCoroutine;

        private void Awake()
        {
            if (dialogueManager == null)
                dialogueManager = FindAnyObjectByType<DialogueManager>();

            if (emotionAnalyzer == null)
                emotionAnalyzer = FindAnyObjectByType<EmotionAnalyzer>();

            if (ttsService == null)
                ttsService = FindAnyObjectByType<TTSService>();
        }

        private void Start()
        {
            Debug.Log("[EnhancedChatUI] Start() called");
            
            sendButton.onClick.AddListener(SendMessage);
            if (inputField != null)
            {
                inputField.onEndEdit.AddListener(OnInputEndEdit);
                inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
                inputField.shouldHideMobileInput = false;
                inputField.interactable = true;
                Debug.Log("[EnhancedChatUI] InputField configured");
            }
            else
            {
                Debug.LogError("[EnhancedChatUI] InputField is NULL!");
            }

            if (dialogueManager != null)
            {
                dialogueManager.OnUserMessage += AddUserMessage;
                dialogueManager.OnAIMessage += AddAIMessage;
                dialogueManager.OnSystemMessage += AddSystemMessage;
                Debug.Log("[EnhancedChatUI] DialogueManager events subscribed");
            }
            else
            {
                Debug.LogError("[EnhancedChatUI] DialogueManager is NULL!");
            }
            
            Debug.Log($"[EnhancedChatUI] messageContent: {(messageContent != null ? "OK" : "NULL")}");
            Debug.Log($"[EnhancedChatUI] userMessagePrefab: {(userMessagePrefab != null ? "OK" : "NULL")}");
        }

        private void Update()
        {
            UpdateEmotionDisplay();
        }

        private void OnDestroy()
        {
            if (dialogueManager != null)
            {
                dialogueManager.OnUserMessage -= AddUserMessage;
                dialogueManager.OnAIMessage -= AddAIMessage;
                dialogueManager.OnSystemMessage -= AddSystemMessage;
            }

            sendButton.onClick.RemoveListener(SendMessage);
            if (inputField != null)
            {
                inputField.onEndEdit.RemoveListener(OnInputEndEdit);
            }
        }

        public void AddUserMessage(string text)
        {
            AddMessage(text, MessageType.User);
        }

        public void AddAIMessage(string text)
        {
            var emotion = emotionAnalyzer?.GetCurrentState().PrimaryEmotion.ToString();
            AddMessage(text, MessageType.AI, emotion);

            if (enableTTS && ttsService != null)
            {
                ttsService.Speak(text);
            }
        }

        public void AddSystemMessage(string text)
        {
            AddMessage(text, MessageType.System);
        }

        private void AddMessage(string text, MessageType type, string emotion = null)
        {
            Debug.Log($"[EnhancedChatUI] AddMessage called - Type: {type}, Text: {text.Substring(0, Mathf.Min(20, text.Length))}...");
            
            GameObject prefab = GetMessagePrefab(type);
            if (prefab == null) 
            {
                Debug.LogError($"[EnhancedChatUI] Prefab is NULL for type: {type}");
                return;
            }

            if (messageContent == null)
            {
                Debug.LogError("[EnhancedChatUI] messageContent is NULL!");
                return;
            }

            GameObject messageObj = Instantiate(prefab, messageContent);
            Debug.Log($"[EnhancedChatUI] Message object instantiated: {messageObj.name}");
            
            var bubble = messageObj.GetComponent<ChatMessageBubble>();

            if (bubble != null)
            {
                Debug.Log("[EnhancedChatUI] ChatMessageBubble found, setting message...");
                string displayEmotion = enableEmotionVisualization ? emotion : null;
                bubble.SetMessage(text, type, displayEmotion);
            }
            else
            {
                Debug.LogWarning("[EnhancedChatUI] ChatMessageBubble not found, using fallback...");
                var textComponent = messageObj.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = text;
                }
            }

            if (typingDelay > 0 && type == MessageType.AI)
            {
                if (typingCoroutine != null)
                    StopCoroutine(typingCoroutine);
                typingCoroutine = StartCoroutine(ShowTypingEffect(messageObj, text, type));
            }

            ScrollToBottom();
            Debug.Log("[EnhancedChatUI] AddMessage completed");
        }

        private GameObject GetMessagePrefab(MessageType type)
        {
            return type switch
            {
                MessageType.User => userMessagePrefab,
                MessageType.AI => aiMessagePrefab,
                MessageType.System => systemMessagePrefab,
                _ => null
            };
        }

        private IEnumerator ShowTypingEffect(GameObject messageObj, string text, MessageType type)
        {
            var bubble = messageObj.GetComponent<ChatMessageBubble>();
            var textComponent = messageObj.GetComponentInChildren<TextMeshProUGUI>();

            if (textComponent == null) yield break;

            textComponent.text = "";

            foreach (char c in text)
            {
                textComponent.text += c;
                yield return new WaitForSeconds(typingDelay);
                ScrollToBottom();
            }
        }

        private void UpdateEmotionDisplay()
        {
            if (emotionAnalyzer == null) return;

            var state = emotionAnalyzer.GetCurrentState();

            if (emotionText != null)
            {
                emotionText.text = state.GetDescription();
            }

            if (emotionIndicator != null)
            {
                emotionIndicator.color = GetEmotionColor(state.PrimaryEmotion);
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

        private void ScrollToBottom()
        {
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0;
            }
        }

        private void SendMessage()
        {
            if (isProcessing) return;

            string text = inputField != null ? inputField.text.Trim() : "";
            if (!string.IsNullOrEmpty(text))
            {
                dialogueManager?.ProcessUserInput(text);

                if (inputField != null)
                {
                    inputField.text = "";
                    inputField.ActivateInputField();
                }
            }
        }

        private void OnInputEndEdit(string text)
        {
            SendMessage();
        }

        public void ToggleTTS(bool enabled)
        {
            enableTTS = enabled;
        }

        public void ToggleEmotionVisualization(bool enabled)
        {
            enableEmotionVisualization = enabled;
        }
    }
}
