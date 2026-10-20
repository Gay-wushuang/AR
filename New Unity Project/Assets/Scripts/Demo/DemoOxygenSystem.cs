using UnityEngine;
using UnityEngine.InputSystem;

namespace EEGAI.Demo
{
    public class DemoOxygenSystem : MonoBehaviour
    {
        [Header("Oxygen Settings")]
        [SerializeField] private float maxOxygen = 100f;
        [SerializeField] private float minOxygen = 0f;
        [SerializeField] private float defaultOxygen = 100f;
        [SerializeField] private float baseConsumptionRate = 1.2f;
        [SerializeField] private float repairConsumptionRate = 3f;
        [SerializeField] private float movementConsumptionRate = 0.8f;

        [Header("Meditation Bonus")]
        [SerializeField] private float meditationOxygenReduction = 0.5f;

        public float CurrentOxygen { get; private set; }
        public float NormalizedOxygen => CurrentOxygen / maxOxygen;
        public bool IsDepleted => CurrentOxygen <= minOxygen;

        public event System.Action<float> OnOxygenChanged;
        public event System.Action OnOxygenDepleted;

        private DemoMeditationSystem meditationSystem;
        private DemoGameState gameState;
        private bool isRepairing = false;
        private bool isMoving = false;
        private bool depletionNotified = false;

        private void Awake()
        {
            CurrentOxygen = defaultOxygen;
        }

        private void Start()
        {
            meditationSystem = FindAnyObjectByType<DemoMeditationSystem>();
            gameState = FindAnyObjectByType<DemoGameState>();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
            {
                DrainOxygen(maxOxygen);
            }

            UpdateOxygen();
        }

        private void UpdateOxygen()
        {
            if (EnergyLinkPuzzle.Active) return;
            bool canDrain = gameState == null ||
                gameState.Status == DemoGameState.GameStatus.Playing ||
                gameState.Status == DemoGameState.GameStatus.ReadyToLaunch;

            if (depletionNotified || !canDrain)
            {
                return;
            }

            float totalConsumption = baseConsumptionRate;

            if (isRepairing)
            {
                totalConsumption += repairConsumptionRate;
            }

            if (isMoving)
            {
                totalConsumption += movementConsumptionRate;
            }

            float meditationModifier = 1f;
            if (meditationSystem != null && meditationSystem.NormalizedMeditation > 0.5f)
            {
                meditationModifier = meditationOxygenReduction + 
                    (1f - meditationOxygenReduction) * (1f - meditationSystem.NormalizedMeditation);
            }

            totalConsumption *= meditationModifier;

            CurrentOxygen -= totalConsumption * Time.deltaTime;
            CurrentOxygen = Mathf.Clamp(CurrentOxygen, minOxygen, maxOxygen);

            OnOxygenChanged?.Invoke(CurrentOxygen);

            if (IsDepleted)
            {
                depletionNotified = true;
                isRepairing = false;
                isMoving = false;
                OnOxygenDepleted?.Invoke();
            }
        }

        public void SetRepairing(bool repairing)
        {
            isRepairing = repairing;
        }

        public void SetMoving(bool moving)
        {
            isMoving = moving;
        }

        public void RestoreOxygen(float amount)
        {
            CurrentOxygen += amount;
            CurrentOxygen = Mathf.Min(CurrentOxygen, maxOxygen);
            if (CurrentOxygen > minOxygen)
            {
                depletionNotified = false;
            }
            OnOxygenChanged?.Invoke(CurrentOxygen);
        }

        public void DrainOxygen(float amount)
        {
            CurrentOxygen -= amount;
            CurrentOxygen = Mathf.Max(CurrentOxygen, minOxygen);
            OnOxygenChanged?.Invoke(CurrentOxygen);
        }

        public void ResetOxygen()
        {
            CurrentOxygen = defaultOxygen;
            depletionNotified = false;
            isRepairing = false;
            isMoving = false;
            OnOxygenChanged?.Invoke(CurrentOxygen);
        }

        private void OnGUI()
        {
            if (!DemoUIVisibilityController.ShowDebugPanels)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(10, 450, 250, 80));
            GUILayout.BeginVertical("box");

            GUILayout.Label($"💨 Oxygen: {CurrentOxygen:F0}/{maxOxygen:F0}");
            GUILayout.Label($"Normalized: {NormalizedOxygen:P0}");
            GUILayout.Label($"Meditation Modifier: {(meditationSystem != null ? meditationSystem.NormalizedMeditation : 0f):P0}");

            if (IsDepleted)
            {
                GUI.color = Color.red;
                GUILayout.Label("⚠️ OXYGEN DEPLETED!");
                GUI.color = Color.white;
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
