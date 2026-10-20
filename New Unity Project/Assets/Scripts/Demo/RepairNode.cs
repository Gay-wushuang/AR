using UnityEngine;
using UnityEngine.InputSystem;

namespace EEGAI.Demo
{
    public class RepairNode : MonoBehaviour
    {
        [Header("Repair Node Settings")]
        [SerializeField] private string nodeName = "Repair Node";
        [SerializeField] private string nodeDescription = "A damaged ship component";
        [SerializeField] private float repairDistance = 3f;
        [SerializeField] private KeyCode repairKey = KeyCode.E;

        [Header("Visual Effects")]
        [SerializeField] private float floatAmplitude = 0.3f;
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private bool tintRendererDuringRepair = false;

        [Header("Particles")]
        [SerializeField] private ParticleSystem repairParticles;
        [SerializeField] private ParticleSystem idleParticles;

        public bool IsRepaired { get; private set; } = false;
        public bool IsInRange { get; private set; } = false;

        public event System.Action<RepairNode> OnRepaired;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Vector3 initialScale;
        private Transform player;
        private DemoRepairSystem repairSystem;
        private DemoOxygenSystem oxygenSystem;
        private DemoGameState gameState;
        private Renderer nodeRenderer;
        private MaterialPropertyBlock propertyBlock;
        private bool hasHighlight;

        private void Awake()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            initialScale = transform.localScale;
            player = Camera.main?.transform;
            nodeRenderer = GetComponent<Renderer>();
            if (nodeRenderer == null)
            {
                nodeRenderer = GetComponentInChildren<Renderer>();
            }
            propertyBlock = new MaterialPropertyBlock();
        }

        private void Start()
        {
            repairSystem = FindAnyObjectByType<DemoRepairSystem>();
            oxygenSystem = FindAnyObjectByType<DemoOxygenSystem>();
            gameState = FindAnyObjectByType<DemoGameState>();

            if (idleParticles != null)
            {
                idleParticles.Play();
            }
        }

        private void Update()
        {
            if (EnergyLinkPuzzle.Active) return;
            if (IsRepaired) return;
            if (gameState != null && gameState.Status != DemoGameState.GameStatus.Playing) return;

            AnimateNode();
            CheckPlayerRange();
            HandleRepairInput();
        }

        private void AnimateNode()
        {
            float floatOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = initialPosition + Vector3.up * floatOffset;
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        private void CheckPlayerRange()
        {
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.position);
            IsInRange = distance <= repairDistance;

            if (tintRendererDuringRepair && nodeRenderer != null)
            {
                SetHighlightColor(IsInRange ? Color.yellow : Color.red);
            }
        }

        private void HandleRepairInput()
        {
            if (!IsInRange || IsRepaired) return;
            if (ConsoleRadarSystem.Instance != null && (!ConsoleRadarSystem.Instance.ConsoleRepaired || ConsoleRadarSystem.Instance.SelectorOpen)) return;
            if (gameState != null && gameState.Status != DemoGameState.GameStatus.Playing) return;

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                StartRepair();
            }
        }

        private void StartRepair()
        {
            if (IsRepaired) return;

            if (ConsoleRadarSystem.Instance != null && !ConsoleRadarSystem.Instance.ConsoleRepaired) return;
            if (DemoMainMenu.PauseMenuOpen || Time.timeScale <= 0f) return;
            Debug.Log($"[Demo] Started repairing node: {nodeName}");

            if (idleParticles != null)
            {
                idleParticles.Stop();
            }

            if (repairParticles != null)
            {
                repairParticles.Play();
            }

            if (repairSystem != null)
            {
                repairSystem.StartRepair();
            }

            if (oxygenSystem != null)
            {
                oxygenSystem.SetRepairing(true);
            }

            StartCoroutine(RepairAnimation());
        }

        private System.Collections.IEnumerator RepairAnimation()
        {
            int startRepairedCount = repairSystem != null ? repairSystem.RepairedNodes : 0;
            float fallbackDuration = 2f;
            float elapsed = 0f;
            Color startColor = Color.yellow;
            Color endColor = Color.green;

            while (!IsRepaired)
            {
                elapsed += Time.deltaTime;
                float t = repairSystem != null
                    ? repairSystem.CurrentNodeProgressNormalized
                    : Mathf.Clamp01(elapsed / fallbackDuration);

                if (tintRendererDuringRepair && nodeRenderer != null)
                {
                    SetHighlightColor(Color.Lerp(startColor, endColor, t));
                }

                if (repairSystem != null && repairSystem.RepairedNodes > startRepairedCount)
                {
                    break;
                }

                if (repairSystem == null && elapsed >= fallbackDuration)
                {
                    break;
                }

                yield return null;
            }

            CompleteRepair();
        }

        private void CompleteRepair()
        {
            IsRepaired = true;

            if (repairParticles != null)
            {
                repairParticles.Stop();
            }

            if (repairSystem != null)
            {
                repairSystem.StopRepair();
            }

            if (oxygenSystem != null)
            {
                oxygenSystem.SetRepairing(false);
            }

            transform.localScale = Vector3.zero;
            OnRepaired?.Invoke(this);

            Debug.Log($"[Demo] Repaired node: {nodeName}");
        }

        /// <summary>Sets the round's stable pose so animation cannot restore the old location.</summary>
        public void SetSpawnPosition(Vector3 position)
        {
            initialPosition = position;
            transform.position = position;
            // Keep the collider and ground placement stable for these physical props.
            floatAmplitude = 0f;
            rotationSpeed = 0f;
        }
        public void ResetNode()
        {
            IsRepaired = false;
            IsInRange = false;
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            transform.localScale = initialScale;

            if (repairParticles != null)
            {
                repairParticles.Stop();
            }

            if (idleParticles != null)
            {
                idleParticles.Play();
            }

            if (nodeRenderer != null && hasHighlight)
            {
                nodeRenderer.SetPropertyBlock(null);
                hasHighlight = false;
            }
        }

        private void SetHighlightColor(Color color)
        {
            if (nodeRenderer == null) return;

            nodeRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_Color", color);
            nodeRenderer.SetPropertyBlock(propertyBlock);
            hasHighlight = true;
        }

        private void OnDisable()
        {
            if (nodeRenderer != null && hasHighlight)
            {
                nodeRenderer.SetPropertyBlock(null);
                hasHighlight = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, repairDistance);
        }

        private void OnGUI()
        {
            if (EnergyLinkPuzzle.Active) return;
            if (DemoMainMenu.PauseMenuOpen) return;
            if (!IsInRange || IsRepaired) return;
            if (ConsoleRadarSystem.Instance != null && (!ConsoleRadarSystem.Instance.ConsoleRepaired || ConsoleRadarSystem.Instance.SelectorOpen)) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            screenPos.y = Screen.height - screenPos.y;

            GUILayout.BeginArea(new Rect(screenPos.x - 75, screenPos.y, 150, 40));
            GUILayout.BeginVertical("box");
            GUILayout.Label($"Press [E] to repair", GUILayout.Width(150));
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        public string GetNodeName() => nodeName;
        public string GetNodeDescription() => nodeDescription;
    }
}
