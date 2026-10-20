using UnityEngine;
using UnityEngine.InputSystem;

namespace EEGAI.Demo
{
    public class DemoMeditationSystem : MonoBehaviour
    {
        [Header("Meditation Settings")]
        [SerializeField] private float maxMeditation = 100f;
        [SerializeField] private float minMeditation = 0f;
        [SerializeField] private float defaultMeditation = 50f;
        [SerializeField] private float decayRate = 1.5f;
        [SerializeField] private float recoveryRate = 3f;
        [SerializeField] private float meditateRate = 20f;
        private const float externalStaleSeconds = 2.5f;

        [Header("Emotion Effects")]
        [SerializeField] private float happyMeditationPenalty = 0.8f;
        [SerializeField] private float sadMeditationBonus = 1.3f;

        public float CurrentMeditation { get; private set; }
        public float NormalizedMeditation => CurrentMeditation / maxMeditation;
        public bool HasExternalData => hasExternalData;
        public bool ExternalDataLive => hasExternalData &&
            Time.unscaledTime - lastExternalUpdate <= externalStaleSeconds;
        public float ExternalDataAge => hasExternalData
            ? Time.unscaledTime - lastExternalUpdate
            : float.PositiveInfinity;

        public event System.Action<float> OnMeditationChanged;

        private DemoEmotionSimulator emotionSimulator;
        private bool isMeditating = false;
        private bool hasExternalData;
        private float lastExternalUpdate;

        private void Awake()
        {
            CurrentMeditation = defaultMeditation;
        }

        private void Start()
        {
            emotionSimulator = FindAnyObjectByType<DemoEmotionSimulator>();
        }

        private void Update()
        {
            HandleInput();
            UpdateMeditation();
        }

        private void HandleInput()
        {
            if (Keyboard.current != null)
            {
                isMeditating = Keyboard.current.leftAltKey.isPressed;
            }
        }

        private void UpdateMeditation()
        {
            // Real eSense meditation is held between device samples; the simulator
            // only runs when no real EEG value has ever been received.
            if (hasExternalData)
            {
                if (ExternalDataLive)
                {
                    OnMeditationChanged?.Invoke(CurrentMeditation);
                }
                return;
            }

            float emotionModifier = 1f;

            if (emotionSimulator != null)
            {
                switch (emotionSimulator.CurrentEmotion)
                {
                    case DemoEmotionSimulator.EmotionType.Happy:
                        emotionModifier = happyMeditationPenalty;
                        break;
                    case DemoEmotionSimulator.EmotionType.Sad:
                        emotionModifier = sadMeditationBonus;
                        break;
                }
            }

            if (isMeditating)
            {
                CurrentMeditation += meditateRate * emotionModifier * Time.deltaTime;
            }
            else
            {
                CurrentMeditation += recoveryRate * emotionModifier * Time.deltaTime;
                CurrentMeditation -= decayRate * Time.deltaTime;
            }

            CurrentMeditation = Mathf.Clamp(CurrentMeditation, minMeditation, maxMeditation);

            OnMeditationChanged?.Invoke(CurrentMeditation);
        }

        public void DrainMeditation(float amount)
        {
            CurrentMeditation -= amount;
            CurrentMeditation = Mathf.Max(CurrentMeditation, minMeditation);
            OnMeditationChanged?.Invoke(CurrentMeditation);
        }

        public void RestoreMeditation(float amount)
        {
            CurrentMeditation += amount;
            CurrentMeditation = Mathf.Min(CurrentMeditation, maxMeditation);
            OnMeditationChanged?.Invoke(CurrentMeditation);
        }

        public void SetExternalMeditation(float value)
        {
            hasExternalData = true;
            lastExternalUpdate = Time.unscaledTime;
            CurrentMeditation = Mathf.Clamp(value, minMeditation, maxMeditation);
            OnMeditationChanged?.Invoke(CurrentMeditation);
        }

        private void OnGUI()
        {
            if (!DemoUIVisibilityController.ShowDebugPanels)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(270, 360, 250, 80));
            GUILayout.BeginVertical("box");

            GUILayout.Label($"🧘 Meditation: {CurrentMeditation:F0}/{maxMeditation:F0}");
            GUILayout.Label($"Normalized: {NormalizedMeditation:P0}");
            GUILayout.Label(hasExternalData
                ? (ExternalDataLive ? "Source: EEG eSense (live)" : $"Source: EEG eSense (stale {ExternalDataAge:F1}s)")
                : "Source: simulator (no EEG yet)");
            GUILayout.Label(isMeditating ? "[ALT] Meditating..." : "Press ALT to meditate (simulator only)");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
