using UnityEngine;

namespace EEGAI.Demo
{
    public class DemoManager : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private DemoEmotionSimulator emotionSimulator;
        [SerializeField] private DemoAttentionSystem attentionSystem;
        [SerializeField] private DemoMeditationSystem meditationSystem;
        [SerializeField] private DemoOxygenSystem oxygenSystem;
        [SerializeField] private DemoRepairSystem repairSystem;
        [SerializeField] private DemoGameState gameState;
        [SerializeField] private EmotionGameAdapter emotionGameAdapter;
        [SerializeField] private DemoHUD demoHUD;

        [Header("Repair Nodes")]
        [SerializeField] private RepairNode[] repairNodes;

        public static DemoManager Instance { get; private set; }

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
            FindMissingReferences();
            InitializeNodes();
            LogSetupInfo();
        }

        private void FindMissingReferences()
        {
            if (emotionSimulator == null) emotionSimulator = FindAnyObjectByType<DemoEmotionSimulator>();
            if (attentionSystem == null) attentionSystem = FindAnyObjectByType<DemoAttentionSystem>();
            if (meditationSystem == null) meditationSystem = FindAnyObjectByType<DemoMeditationSystem>();
            if (oxygenSystem == null) oxygenSystem = FindAnyObjectByType<DemoOxygenSystem>();
            if (repairSystem == null) repairSystem = FindAnyObjectByType<DemoRepairSystem>();
            if (gameState == null) gameState = FindAnyObjectByType<DemoGameState>();
            if (emotionGameAdapter == null) emotionGameAdapter = FindAnyObjectByType<EmotionGameAdapter>();
            if (demoHUD == null) demoHUD = FindAnyObjectByType<DemoHUD>();

            if (repairNodes.Length == 0)
            {
                repairNodes = FindObjectsOfType<RepairNode>();
            }
        }

        private void InitializeNodes()
        {
            foreach (var node in repairNodes)
            {
                node.OnRepaired += HandleNodeRepaired;
            }

            Debug.Log($"[Demo] Found {repairNodes.Length} repair nodes in scene");
        }

        private void HandleNodeRepaired(RepairNode node)
        {
            Debug.Log($"[Demo] Repair node completed: {node.GetNodeName()}");
        }

        private void LogSetupInfo()
        {
            Debug.Log("[Demo] 🚀 Moon Survivor Demo v2.0 initialized!");
            Debug.Log("[Demo] ===================");
            Debug.Log("[Demo] CONTROLS:");
            Debug.Log("[Demo]   [1] - Happy (快乐)");
            Debug.Log("[Demo]   [2] - Sad (悲伤)");
            Debug.Log("[Demo]   [3] - Normal (平静)");
            Debug.Log("[Demo]   [WASD] - Move");
            Debug.Log("[Demo]   [Mouse] - Rotate");
            Debug.Log("[Demo]   [E] - Repair node");
            Debug.Log("[Demo]   [SHIFT] - Focus (increase attention)");
            Debug.Log("[Demo]   [ALT] - Meditate (increase meditation)");
            Debug.Log("[Demo] ===================");
            Debug.Log("[Demo] GAMEPLAY LOOP:");
            Debug.Log("[Demo]   1. Manage oxygen (depletes over time)");
            Debug.Log("[Demo]   2. Use meditation (ALT) to reduce oxygen consumption");
            Debug.Log("[Demo]   3. Use attention (SHIFT) to speed up repairs");
            Debug.Log("[Demo]   4. Repair 3 nodes by pressing E near them");
            Debug.Log("[Demo]   5. Emotions affect movement speed and system rates");
            Debug.Log("[Demo] ===================");
        }

        private void OnDestroy()
        {
            foreach (var node in repairNodes)
            {
                node.OnRepaired -= HandleNodeRepaired;
            }
        }
    }
}
