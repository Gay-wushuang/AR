using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EEGAI.UI.Chat
{
    public enum MessageType
    {
        User,
        AI,
        System
    }

    public class ChatMessageBubble : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image bubbleImage;
        [SerializeField] private VerticalLayoutGroup layoutGroup;
        [SerializeField] private ContentSizeFitter contentSizeFitter;
        [SerializeField] private LayoutElement layoutElement;

        [Header("Style Config")]
        [SerializeField] private Color userBubbleColor = new Color(0.298f, 0.686f, 0.314f, 1f);
        [SerializeField] private Color aiBubbleColor = new Color(0.176f, 0.549f, 1f, 1f);
        [SerializeField] private Color systemBubbleColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        [Header("Emotion Visualization")]
        [SerializeField] private Image emotionIndicator;
        [SerializeField] private bool showEmotionIndicator = true;

        private MessageType currentType;

        public void SetMessage(string text, MessageType type, string emotion = null)
        {
            currentType = type;

            RectTransform rectTransform = (RectTransform)transform;
            rectTransform.sizeDelta = new Vector2(580, rectTransform.sizeDelta.y);
            
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(0, 0.5f);

            if (layoutElement == null)
            {
                layoutElement = gameObject.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = gameObject.AddComponent<LayoutElement>();
                }
            }
            
            layoutElement.preferredWidth = 580;
            layoutElement.flexibleWidth = 0;
            layoutElement.minWidth = 580;

            if (messageText != null)
            {
                messageText.text = text;
                messageText.textWrappingMode = TextWrappingModes.Normal;
                messageText.overflowMode = TextOverflowModes.Overflow;
                
                RectTransform textRect = (RectTransform)messageText.transform;
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
            }

            UpdateBubbleStyle(type);
            UpdateEmotionIndicator(emotion);

            if (contentSizeFitter != null)
            {
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            
            if (layoutGroup != null)
            {
                layoutGroup.childControlWidth = false;
                layoutGroup.childForceExpandWidth = false;
            }
            
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        private void UpdateBubbleStyle(MessageType type)
        {
            if (bubbleImage != null)
            {
                switch (type)
                {
                    case MessageType.User:
                        bubbleImage.color = userBubbleColor;
                        SetAlignment(TextAlignmentOptions.Right);
                        break;
                    case MessageType.AI:
                        bubbleImage.color = aiBubbleColor;
                        SetAlignment(TextAlignmentOptions.Left);
                        break;
                    case MessageType.System:
                        bubbleImage.color = systemBubbleColor;
                        SetAlignment(TextAlignmentOptions.Center);
                        break;
                }
            }
        }

        private void SetAlignment(TextAlignmentOptions alignment)
        {
            if (messageText != null)
            {
                messageText.alignment = alignment;
            }

            if (layoutGroup != null)
            {
                layoutGroup.childAlignment = alignment switch
                {
                    TextAlignmentOptions.Right => TextAnchor.UpperRight,
                    TextAlignmentOptions.Left => TextAnchor.UpperLeft,
                    TextAlignmentOptions.Center => TextAnchor.UpperCenter,
                    _ => TextAnchor.UpperLeft
                };
            }
        }

        private void UpdateEmotionIndicator(string emotion)
        {
            if (emotionIndicator == null || !showEmotionIndicator)
                return;

            if (string.IsNullOrEmpty(emotion))
            {
                emotionIndicator.gameObject.SetActive(false);
                return;
            }

            emotionIndicator.gameObject.SetActive(true);
            var emotionColor = GetEmotionColor(emotion);
            emotionIndicator.color = emotionColor;
        }

        private Color GetEmotionColor(string emotion)
        {
            return emotion.ToLower() switch
            {
                "happy" => new Color(1f, 0.84f, 0f, 1f),
                "sad" => new Color(0.29f, 0.65f, 0.94f, 1f),
                "anxious" => new Color(1f, 0.34f, 0.13f, 1f),
                "calm" => new Color(0.18f, 0.8f, 0.44f, 1f),
                "angry" => new Color(0.95f, 0.1f, 0.1f, 1f),
                _ => new Color(0.8f, 0.8f, 0.8f, 1f)
            };
        }

        public MessageType GetMessageType()
        {
            return currentType;
        }
    }
}
