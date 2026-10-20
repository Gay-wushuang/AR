using UnityEngine;
using UnityEngine.InputSystem;

namespace EEGAI.Demo
{
    public class DemoAttentionSystem : MonoBehaviour
    {
        [Header("Attention Settings")]
        [SerializeField] private float maxAttention = 100f;
        [SerializeField] private float minAttention = 0f;
        [SerializeField] private float defaultAttention = 70f;
        [SerializeField] private float decayRate = 2f;
        [SerializeField] private float recoveryRate = 5f;
        [SerializeField] private float focusBonusRate = 15f;

        [Header("Emotion Effects")]
        [SerializeField] private float happyAttentionBonus = 1.2f;
        [SerializeField] private float sadAttentionPenalty = 0.7f;

        public float CurrentAttention { get; private set; }
        public float NormalizedAttention => CurrentAttention / maxAttention;

        public event System.Action<float> OnAttentionChanged;

        private DemoEmotionSimulator emotionSimulator;
        private bool isFocusing = false;

        private void Awake()
        {
            CurrentAttention = defaultAttention;
        }

        private void Start()
        {
            emotionSimulator = FindAnyObjectByType<DemoEmotionSimulator>();
        }

        private void Update()
        {
            HandleInput();
            UpdateAttention();
        }

        private void HandleInput()
        {
            if (Keyboard.current != null)
            {
                isFocusing = Keyboard.current.leftShiftKey.isPressed;
            }
        }

        private void UpdateAttention()
        {
            float emotionModifier = 1f;

            if (emotionSimulator != null)
            {
                switch (emotionSimulator.CurrentEmotion)
                {
                    case DemoEmotionSimulator.EmotionType.Happy:
                        emotionModifier = happyAttentionBonus;
                        break;
                    case DemoEmotionSimulator.EmotionType.Sad:
                        emotionModifier = sadAttentionPenalty;
                        break;
                }
            }

            if (isFocusing)
            {
                CurrentAttention += focusBonusRate * emotionModifier * Time.deltaTime;
            }
            else
            {
                CurrentAttention += recoveryRate * emotionModifier * Time.deltaTime;
                CurrentAttention -= decayRate * Time.deltaTime;
            }

            CurrentAttention = Mathf.Clamp(CurrentAttention, minAttention, maxAttention);

            OnAttentionChanged?.Invoke(CurrentAttention);
        }

        public void DrainAttention(float amount)
        {
            CurrentAttention -= amount;
            CurrentAttention = Mathf.Max(CurrentAttention, minAttention);
            OnAttentionChanged?.Invoke(CurrentAttention);
        }

        public void RestoreAttention(float amount)
        {
            CurrentAttention += amount;
            CurrentAttention = Mathf.Min(CurrentAttention, maxAttention);
            OnAttentionChanged?.Invoke(CurrentAttention);
        }

        public void SetExternalAttention(float value)
        {
            CurrentAttention = Mathf.Clamp(value, minAttention, maxAttention);
            OnAttentionChanged?.Invoke(CurrentAttention);
        }

        private void OnGUI()
        {
            if (!DemoUIVisibilityController.ShowDebugPanels)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(10, 360, 250, 80));
            GUILayout.BeginVertical("box");

            GUILayout.Label($"🎯 Attention: {CurrentAttention:F0}/{maxAttention:F0}");
            GUILayout.Label($"Normalized: {NormalizedAttention:P0}");
            GUILayout.Label(isFocusing ? "[SHIFT] Focusing..." : "Press SHIFT to focus");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
