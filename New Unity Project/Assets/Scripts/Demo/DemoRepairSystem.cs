using UnityEngine;

namespace EEGAI.Demo
{
    public class DemoRepairSystem : MonoBehaviour
    {
        [Header("Repair Settings")]
        [SerializeField] private int totalNodes = 3;
        [SerializeField] private float repairProgressPerNode = 33.33f;
        [SerializeField] private float baseRepairSpeed = 20f;

        [Header("Attention Bonus")]
        [SerializeField] private float maxAttentionBonus = 2f;

        public int TotalNodes => totalNodes;
        public int RepairedNodes { get; private set; }
        public float CurrentProgress { get; private set; }
        public float CurrentNodeProgressNormalized => Mathf.Clamp01(CurrentProgress / repairProgressPerNode);
        public float OverallProgress => (RepairedNodes + CurrentNodeProgressNormalized) / totalNodes * 100f;
        public bool IsFullyRepaired => RepairedNodes >= totalNodes;

        public event System.Action<int> OnNodeRepaired;
        public event System.Action<float> OnProgressChanged;
        public event System.Action OnRepairComplete;

        private DemoAttentionSystem attentionSystem;
        private DemoGameState gameState;
        private bool isRepairing = false;

        private void Awake()
        {
            RepairedNodes = 0;
            CurrentProgress = 0f;
        }

        private void Start()
        {
            attentionSystem = FindAnyObjectByType<DemoAttentionSystem>();
            gameState = FindAnyObjectByType<DemoGameState>();
        }

        private void Update()
        {
            if (gameState != null && gameState.Status != DemoGameState.GameStatus.Playing)
            {
                isRepairing = false;
                return;
            }

            if (isRepairing && !IsFullyRepaired)
            {
                UpdateRepair();
            }
        }

        public void StartRepair()
        {
            if (gameState != null && gameState.Status != DemoGameState.GameStatus.Playing) return;
            if (IsFullyRepaired) return;
            isRepairing = true;
            Debug.Log("[Demo] Repairing started");
        }

        public void StopRepair()
        {
            isRepairing = false;
            Debug.Log("[Demo] Repairing stopped");
        }

        private void UpdateRepair()
        {
            float attentionModifier = 1f;
            if (attentionSystem != null)
            {
                attentionModifier = 1f + (maxAttentionBonus - 1f) * attentionSystem.NormalizedAttention;
            }

            float repairAmount = baseRepairSpeed * attentionModifier * Time.deltaTime;
            CurrentProgress += repairAmount;

            if (CurrentProgress >= repairProgressPerNode)
            {
                CurrentProgress = 0f;
                RepairedNodes++;
                OnNodeRepaired?.Invoke(RepairedNodes);
                Debug.Log($"[Demo] Node repaired! {RepairedNodes}/{totalNodes}");

                if (IsFullyRepaired)
                {
                    isRepairing = false;
                    OnRepairComplete?.Invoke();
                    Debug.Log("[Demo] 🎉 Repair complete!");
                }
            }

            OnProgressChanged?.Invoke(CurrentProgress);
        }

        public void RepairNodeInstantly()
        {
            if (IsFullyRepaired) return;

            RepairedNodes++;
            CurrentProgress = 0f;
            OnNodeRepaired?.Invoke(RepairedNodes);

            if (IsFullyRepaired)
            {
                OnRepairComplete?.Invoke();
            }
        }

        public void ResetRepair()
        {
            RepairedNodes = 0;
            CurrentProgress = 0f;
            isRepairing = false;
            OnNodeRepaired?.Invoke(RepairedNodes);
            OnProgressChanged?.Invoke(CurrentProgress);
        }

        private void OnGUI()
        {
            if (!DemoUIVisibilityController.ShowDebugPanels)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(270, 450, 250, 80));
            GUILayout.BeginVertical("box");

            GUILayout.Label($"🔧 Repair Progress: {OverallProgress:F0}%");
            GUILayout.Label($"Repaired Nodes: {RepairedNodes}/{totalNodes}");
            GUILayout.Label(isRepairing ? "Repairing..." : "Ready to repair");

            if (IsFullyRepaired)
            {
                GUI.color = Color.green;
                GUILayout.Label("✅ REPAIR COMPLETE!");
                GUI.color = Color.white;
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
