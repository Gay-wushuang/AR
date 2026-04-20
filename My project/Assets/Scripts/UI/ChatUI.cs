using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using EEGAI.AI.Dialogue;

namespace EEGAI.UI
{
    public class ChatUI : MonoBehaviour
    {
        [SerializeField] private Transform messageContainer;
        [SerializeField] private InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private GameObject userMessagePrefab;
        [SerializeField] private GameObject aiMessagePrefab;
        [SerializeField] private GameObject systemMessagePrefab;

        [Header("Typing Effect")]
        [SerializeField] private float typingDelay = 0.05f;

        private DialogueManager dialogueManager;
        private ScrollRect scrollRect;

        private void Awake()
        {
            dialogueManager = FindAnyObjectByType<DialogueManager>();
            scrollRect = messageContainer.GetComponentInParent<ScrollRect>();
        }

        private void Start()
        {
            sendButton.onClick.AddListener(SendMessage);
            if (inputField != null)
            {
                inputField.onEndEdit.AddListener(OnInputEndEdit);
                inputField.lineType = InputField.LineType.MultiLineNewline;
                inputField.interactable = true;
            }

            if (dialogueManager != null)
            {
                dialogueManager.OnUserMessage += AddUserMessage;
                dialogueManager.OnAIMessage += AddAIMessage;
                dialogueManager.OnSystemMessage += AddSystemMessage;
            }
        }

        private void OnDestroy()
        {
            if (dialogueManager != null)
            {
                dialogueManager.OnUserMessage -= AddUserMessage;
                dialogueManager.OnAIMessage -= AddAIMessage;
                dialogueManager.OnSystemMessage -= AddSystemMessage;
            }
        }

        public void AddUserMessage(string text)
        {
            var message = Instantiate(userMessagePrefab, messageContainer);
            SetMessageText(message, text);
            ScrollToBottom();
        }

        public void AddAIMessage(string text)
        {
            var message = Instantiate(aiMessagePrefab, messageContainer);

            if (typingDelay > 0)
            {
                StartCoroutine(ShowMessageWithTypingEffect(message, text, typingDelay));
            }
            else
            {
                SetMessageText(message, text);
            }

            ScrollToBottom();
        }

        public void AddSystemMessage(string text)
        {
            var message = Instantiate(systemMessagePrefab, messageContainer);
            SetMessageText(message, text);
            ScrollToBottom();
        }

        private void SetMessageText(GameObject messageObj, string text)
        {
            var textComponent = messageObj.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.text = text;
            }
        }

        private IEnumerator ShowMessageWithTypingEffect(GameObject messageObj, string text, float delay)
        {
            var textComponent = messageObj.GetComponentInChildren<Text>();
            if (textComponent == null)
            {
                SetMessageText(messageObj, text);
                yield break;
            }

            textComponent.text = "";
            foreach (char c in text)
            {
                textComponent.text += c;
                yield return new WaitForSeconds(delay);
                ScrollToBottom();
            }
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
    }
}
