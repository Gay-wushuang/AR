using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace EEGAI.Demo
{
    public class DemoGameState : MonoBehaviour
    {
        public enum GameStatus
        {
            MainMenu,
            Playing,
            ReadyToLaunch,
            Launching,
            Victory,
            GameOver
        }

        [Header("Game Settings")]
        [SerializeField] private int totalRepairNodes = 3;
        [Tooltip("Failsafe: force victory if the launch coroutine is interrupted and the game stays in Launching.")]
        [SerializeField] private float launchTimeout = 15f;

        private float launchStartedAt;

        public GameStatus Status { get; private set; } = GameStatus.MainMenu;
        public int RepairedNodes { get; private set; } = 0;
        public int TotalRepairNodes => totalRepairNodes;

        public event System.Action OnNodeRepaired;
        public event System.Action OnVictory;
        public event System.Action OnGameOver;
        public event System.Action OnReset;
        public event System.Action OnStarted;

        public static DemoGameState Instance { get; private set; }

        private DemoOxygenSystem oxygenSystem;
        private DemoRepairSystem repairSystem;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            oxygenSystem = FindAnyObjectByType<DemoOxygenSystem>();
            repairSystem = FindAnyObjectByType<DemoRepairSystem>();

            if (oxygenSystem != null)
            {
                oxygenSystem.OnOxygenDepleted += HandleOxygenDepleted;
            }

            if (repairSystem != null)
            {
                repairSystem.OnRepairComplete += HandleRepairComplete;
                repairSystem.OnNodeRepaired += HandleNodeRepaired;
            }

            StartCoroutine(ResetToMenuAfterSceneStartup());
        }

        private IEnumerator ResetToMenuAfterSceneStartup()
        {
            yield return null;
            ResetGame();
        }

        private void Update()
        {
            if (Status == GameStatus.MainMenu)
            {
                return;
            }

            if (Keyboard.current != null)
            {
                if (Keyboard.current.vKey.wasPressedThisFrame)
                {
                    WinGame();
                    return;
                }

                if (Keyboard.current.gKey.wasPressedThisFrame)
                {
                    GameOver();
                    return;
                }
            }

            if (Status == GameStatus.Launching)
            {
                // The launch coroutine normally completes after ~5 seconds. If a runtime
                // exception or a stopped coroutine leaves the game in Launching, force
                // the victory state so the mission can never soft-lock here.
                if (launchTimeout > 0f && Time.unscaledTime - launchStartedAt >= launchTimeout)
                {
                    Debug.LogWarning("[Demo] Launch sequence timed out; forcing victory state.");
                    CompleteLaunch();
                }

                return;
            }

            if (Status != GameStatus.Playing && Status != GameStatus.ReadyToLaunch)
            {
                return;
            }

            if (oxygenSystem != null && oxygenSystem.IsDepleted)
            {
                GameOver();
                return;
            }

            if (repairSystem != null)
            {
                RepairedNodes = repairSystem.RepairedNodes;
                if (repairSystem.IsFullyRepaired && Status == GameStatus.Playing)
                {
                    ReadyForLaunch();
                }
            }
        }

        private void HandleOxygenDepleted()
        {
            if (Status == GameStatus.Playing || Status == GameStatus.ReadyToLaunch)
            {
                GameOver();
            }
        }

        private void HandleRepairComplete()
        {
            if (Status == GameStatus.Playing)
            {
                ReadyForLaunch();
            }
        }

        private void HandleNodeRepaired(int count)
        {
            RepairedNodes = count;
            OnNodeRepaired?.Invoke();
        }

        public void RepairNode()
        {
            if (Status != GameStatus.Playing) return;

            RepairedNodes++;
            Debug.Log($"[Demo] Node repaired! {RepairedNodes}/{TotalRepairNodes}");
            OnNodeRepaired?.Invoke();

            if (RepairedNodes >= TotalRepairNodes)
            {
                ReadyForLaunch();
            }
        }

        private void ReadyForLaunch()
        {
            if (Status != GameStatus.Playing) return;
            Status = GameStatus.ReadyToLaunch;
            Debug.Log("[Demo] Repairs complete. Rocket ready for launch.");
        }

        public void BeginLaunch()
        {
            if (Status != GameStatus.ReadyToLaunch) return;
            Status = GameStatus.Launching;
            launchStartedAt = Time.unscaledTime;
            Debug.Log("[Demo] Rocket launch sequence started.");
        }

        public void CompleteLaunch()
        {
            WinGame();
        }

        public void StartGame()
        {
            ResetGame();
            DemoEEGServiceLauncher.StartServices();
            Status = GameStatus.Playing;
            DemoUIVisibilityController.SetVisualizationMode();
            Debug.Log("[Demo] Game started");
            OnStarted?.Invoke();
        }

        private void WinGame()
        {
            if (Status == GameStatus.Victory) return;
            Status = GameStatus.Victory;
            Debug.Log("[Demo] Victory! Rocket launched successfully.");
            OnVictory?.Invoke();
        }

        private void GameOver()
        {
            if (Status == GameStatus.GameOver) return;
            Status = GameStatus.GameOver;
            Debug.Log("[Demo] Game Over! Oxygen depleted.");
            OnGameOver?.Invoke();
        }

        public void ResetGame()
        {
            Status = GameStatus.MainMenu;
            RepairedNodes = 0;

            if (oxygenSystem == null)
            {
                oxygenSystem = FindAnyObjectByType<DemoOxygenSystem>();
            }

            if (repairSystem == null)
            {
                repairSystem = FindAnyObjectByType<DemoRepairSystem>();
            }

            if (repairSystem != null)
            {
                repairSystem.ResetRepair();
            }

            if (oxygenSystem != null)
            {
                oxygenSystem.ResetOxygen();
            }

            foreach (var node in FindObjectsOfType<RepairNode>(true))
            {
                node.ResetNode();
            }

            foreach (var controller in FindObjectsOfType<DemoPlayerController>(true))
            {
                controller.ResetPose();
            }

            foreach (var controller in FindObjectsOfType<AstronautController>(true))
            {
                controller.ResetPose();
            }

            foreach (var rocket in FindObjectsOfType<RocketLaunchController>(true))
            {
                rocket.ResetLaunch();
            }

            Debug.Log("[Demo] Game reset");
            OnReset?.Invoke();
        }

        public float GetCompletionPercentage()
        {
            return (float)RepairedNodes / TotalRepairNodes * 100f;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (oxygenSystem != null)
            {
                oxygenSystem.OnOxygenDepleted -= HandleOxygenDepleted;
            }

            if (repairSystem != null)
            {
                repairSystem.OnRepairComplete -= HandleRepairComplete;
                repairSystem.OnNodeRepaired -= HandleNodeRepaired;
            }
        }
    }
}
