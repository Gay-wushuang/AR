using UnityEngine;
using UnityEngine.InputSystem;

namespace EEGAI.Demo
{
    public class DemoEmotionSimulator : MonoBehaviour
    {
        public enum EmotionType
        {
            Happy,
            Sad,
            Normal
        }

        [Header("Emotion Settings")]
        [SerializeField] private EmotionType initialEmotion = EmotionType.Normal;
        [SerializeField] private bool showDebugPanel = false;

        public EmotionType CurrentEmotion { get; private set; }
        public float Confidence { get; private set; } = 1f;

        public event System.Action<EmotionType> OnEmotionChanged;

        private void Awake()
        {
            CurrentEmotion = initialEmotion;
        }

        private void Update()
        {
            HandleKeyboardInput();
        }

        private void HandleKeyboardInput()
        {
            if (!DemoUIVisibilityController.ShowDebugPanels)
            {
                return;
            }

            if (Keyboard.current != null)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
                {
                    SetEmotion(EmotionType.Happy);
                }
                else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
                {
                    SetEmotion(EmotionType.Sad);
                }
                else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
                {
                    SetEmotion(EmotionType.Normal);
                }
            }
        }

        public void SetEmotion(EmotionType newEmotion)
        {
            if (newEmotion != CurrentEmotion)
            {
                CurrentEmotion = newEmotion;
                Confidence = 1f;
                Debug.Log($"[Demo] Emotion changed to: {newEmotion}");
                OnEmotionChanged?.Invoke(newEmotion);
            }
        }

        public void SetExternalEmotion(string emotion, float confidence)
        {
            if (string.IsNullOrEmpty(emotion) || emotion == "offline")
            {
                return;
            }

            EmotionType parsedEmotion;
            switch (emotion.ToLowerInvariant())
            {
                case "happy":
                    parsedEmotion = EmotionType.Happy;
                    break;
                case "sad":
                    parsedEmotion = EmotionType.Sad;
                    break;
                default:
                    parsedEmotion = EmotionType.Normal;
                    break;
            }

            Confidence = Mathf.Clamp01(confidence);
            if (parsedEmotion != CurrentEmotion)
            {
                CurrentEmotion = parsedEmotion;
                Debug.Log($"[Demo] External emotion changed to: {parsedEmotion} ({Confidence:P0})");
                OnEmotionChanged?.Invoke(parsedEmotion);
            }
        }

        public string GetEmotionString()
        {
            return CurrentEmotion.ToString().ToLower();
        }

        private void OnGUI()
        {
            if (!showDebugPanel)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(10, 200, 300, 150));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("🎮 Demo Emotion Simulator", GUI.skin.box);
            GUILayout.Space(10);
            
            GUILayout.Label($"Current: <color={GetEmotionColor()}>{CurrentEmotion.ToString().ToUpper()}</color>");
            GUILayout.Label($"Confidence: {Confidence:P0}");
            
            GUILayout.Space(10);
            GUILayout.Label("Press:");
            GUILayout.Label("[1] - Happy (快乐)");
            GUILayout.Label("[2] - Sad (悲伤)");
            GUILayout.Label("[3] - Normal (平静)");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private string GetEmotionColor()
        {
            switch (CurrentEmotion)
            {
                case EmotionType.Happy: return "#FFD700";
                case EmotionType.Sad: return "#4169E1";
                case EmotionType.Normal: return "#90EE90";
                default: return "white";
            }
        }
    }
}
