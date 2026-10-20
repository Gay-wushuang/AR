using UnityEngine;
using UnityEngine.InputSystem;

namespace EEGAI.Demo
{
    /// <summary>Toolbox repair, console radar, and oxygen warnings for the expedition.</summary>
    public class ConsoleRadarSystem : MonoBehaviour
    {
        [SerializeField] private DemoGameState gameState;
        [SerializeField] private DemoOxygenSystem oxygen;
        [SerializeField] private Transform player;
        [SerializeField] private Transform toolbox;
        [SerializeField] private Transform console;
        [SerializeField] private RepairNode[] targets;
        [SerializeField] private AudioClip warningClip;
        [SerializeField] private AudioClip repairClip;
        [SerializeField] private AudioClip radarClip;
        [SerializeField] private float repairSeconds = 4f;
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private float radarExcerptStart = 0f;
        [SerializeField] private float radarPulseSeconds = 0.6f;
        public static ConsoleRadarSystem Instance { get; private set; }
        public bool ConsoleRepaired { get; private set; }
        public bool RadarEnabled { get; private set; }
        public bool IsRepairing { get; private set; }
        public bool SelectorOpen { get; private set; }
        public int SelectedIndex { get; private set; } = -1;
        public Transform TrackedTarget => RadarEnabled && TargetAvailable(SelectedIndex) ? targets[SelectedIndex].transform : null;
        public float RepairProgress => Mathf.Clamp01(repairElapsed / repairSeconds);
        public bool LowOxygen => oxygen != null && oxygen.NormalizedOxygen < 0.3f && !oxygen.IsDepleted;
        private AudioSource warningSource, repairSource, radarSource;
        private float repairElapsed, nextWarning, nextPulse, pulseEnd;
        private CursorLockMode previousLock;
        private bool previousVisible;
        public bool Collapsed { get; private set; }
        public void ToggleCollapsed() { Collapsed = !Collapsed; }
        private readonly string[] targetNames = { "Gear", "Fuel canister", "Fuel cell" };
        private bool Running => gameState != null && oxygen != null && !oxygen.IsDepleted &&
            (gameState.Status == DemoGameState.GameStatus.Playing || gameState.Status == DemoGameState.GameStatus.ReadyToLaunch);
        private bool CanAct => Running && !EnergyLinkPuzzle.Active && !DemoMainMenu.PauseMenuOpen && Time.timeScale > 0f;

        private void Awake()
        {
            Instance = this;
            warningSource = CreateSource("Low oxygen warning", warningClip, 0.8f, false);
            repairSource = CreateSource("Console repair sound", repairClip, 0.65f, true);
            radarSource = CreateSource("Single radar signal", radarClip, 0.65f, false);
        }
        private AudioSource CreateSource(string name, AudioClip clip, float volume, bool loop)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false; source.clip = clip; source.volume = volume;
            source.loop = loop; source.spatialBlend = 0f;
            return source;
        }
        private void OnEnable() { gameState.OnReset += ResetMission; }
        private void OnDisable()
        {
            if (gameState != null) gameState.OnReset -= ResetMission;
            StopSounds(); CloseSelector(true);
        }
        private void OnDestroy() { if (Instance == this) Instance = null; }
        private void ResetMission()
        {
            ConsoleRepaired = false; RadarEnabled = false; IsRepairing = false;
            Collapsed = false;
            SelectedIndex = -1; repairElapsed = 0f; nextWarning = nextPulse = 0f;
            StopSounds(); CloseSelector(false);
        }
        private bool Near(Transform target) => target != null && Vector3.Distance(player.position, target.position) <= interactionDistance;
        public bool TryStartRepair()
        {
            if (!CanAct || ConsoleRepaired || IsRepairing || !Near(toolbox)) return false;
            IsRepairing = true; repairElapsed = 0f;
            return true;
        }
        public bool TryToggleRadar()
        {
            if (!CanAct || !ConsoleRepaired || !Near(console)) return false;
            RadarEnabled = !RadarEnabled;
            radarSource.Stop(); nextPulse = 0f;
            if (RadarEnabled && !TargetAvailable(SelectedIndex))
            {
                SelectedIndex = -1;
                for (int i = 0; i < targets.Length; i++) if (TargetAvailable(i)) { SelectedIndex = i; break; }
            }
            return true;
        }
        private bool TargetAvailable(int i) => i >= 0 && i < targets.Length && targets[i] != null &&
            targets[i].gameObject.activeInHierarchy && !targets[i].IsRepaired;
        public bool SelectTarget(int index)
        {
            if (!CanAct || !RadarEnabled || (index != -1 && !TargetAvailable(index))) return false;
            SelectedIndex = index; radarSource.Stop(); nextPulse = 0f;
            return true;
        }
        public float GetPulseInterval(float distance) => Mathf.Lerp(0.85f, 5f, Mathf.Clamp01(distance / 100f));
        private void Update()
        {
            if (!CanAct)
            {
                StopSounds(); nextWarning = nextPulse = Time.time;
                if (!Running) { IsRepairing = false; repairElapsed = 0f; }
                return;
            }
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && !SelectorOpen)
            {
                if (!ConsoleRepaired) TryStartRepair();
                else TryToggleRadar();
            }
            if (IsRepairing)
            {
                if (!Near(toolbox)) { IsRepairing = false; repairElapsed = 0f; repairSource.Stop(); }
                else
                {
                    if (!repairSource.isPlaying) repairSource.Play();
                    repairElapsed += Time.deltaTime;
                    if (repairElapsed >= repairSeconds)
                    {
                        ConsoleRepaired = true; IsRepairing = false; repairSource.Stop();
                    }
                }
            }
            if (!LowOxygen) { warningSource.Stop(); nextWarning = 0f; }
            else if (Time.time >= nextWarning && !warningSource.isPlaying)
            {
                warningSource.Play();
                nextWarning = Time.time + Mathf.Max(5f, warningClip != null ? warningClip.length + 1f : 5f);
            }
            if (!TargetAvailable(SelectedIndex)) { SelectedIndex = -1; radarSource.Stop(); }
            if (!RadarEnabled || SelectedIndex < 0 || warningSource.isPlaying || IsRepairing)
            { radarSource.Stop(); return; }
            if (radarSource.isPlaying && Time.time >= pulseEnd) radarSource.Stop();
            if (Time.time >= nextPulse && !radarSource.isPlaying && radarClip != null)
            {
                float distance = Vector3.Distance(player.position, targets[SelectedIndex].transform.position);
                radarSource.time = Mathf.Clamp(radarExcerptStart, 0f, Mathf.Max(0f, radarClip.length - radarPulseSeconds));
                radarSource.Play();
                pulseEnd = Time.time + radarPulseSeconds;
                nextPulse = Time.time + GetPulseInterval(distance);
            }
        }
        private void StopSounds()
        {
            if (warningSource != null) warningSource.Stop();
            if (repairSource != null) repairSource.Stop();
            if (radarSource != null) radarSource.Stop();
        }
        private void LateUpdate()
        {
            bool open = CanAct && DemoUIVisibilityController.ShowVisualizations &&
                Keyboard.current != null && Keyboard.current.tabKey.isPressed;
            if (open)
            {
                if (!SelectorOpen) { previousLock = Cursor.lockState; previousVisible = Cursor.visible; SelectorOpen = true; }
                Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            }
            else CloseSelector(CanAct);
        }
        private void CloseSelector(bool restore)
        {
            if (!SelectorOpen) return;
            SelectorOpen = false;
            if (restore) { Cursor.lockState = previousLock; Cursor.visible = previousVisible; }
        }
        private string Guidance()
        {
            if (!ConsoleRepaired) return IsRepairing ? $"Repairing console... {RepairProgress:P0} (stay nearby)" :
                Near(toolbox) ? "[E] Use toolbox to repair console (4 sec)" : "First: find the toolbox and repair the console";
            if (Near(console)) return RadarEnabled ? "[E] Switch radar off" : "[E] Activate console radar";
            return RadarEnabled ? "Hold TAB to select one radar target" : "Console repaired. Approach console and press E";
        }
        private void OnGUI()
        {
            if (EnergyLinkPuzzle.Active || !Running || !DemoUIVisibilityController.ShowVisualizations) return;
            var previous = GUI.color;
            var rect = new Rect(Mathf.Max(12f, Screen.width - 282f), 48f, 270f, Collapsed ? 66f : 260f);
            GUI.color = new Color(0.025f, 0.055f, 0.08f, 0.18f); GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = Color.white;
            var oldBackground = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 1f, 1f, .25f);
            if (GUI.Button(new Rect(rect.xMax - 40, rect.y + 6, 30, 24), Collapsed ? "+" : "-")) ToggleCollapsed();
            GUILayout.BeginArea(new Rect(rect.x + 12, rect.y + 8, rect.width - 24, rect.height - 16));
            GUILayout.Label("CONSOLE RADAR", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 16 });
            GUILayout.Label(!ConsoleRepaired ? "LOCKED - repair console first" : RadarEnabled ? "ONLINE - single target" : "OFFLINE - activate at console");
            if (!Collapsed)
            {
            bool enabled = GUI.enabled;
            for (int i = 0; i < targets.Length; i++)
            {
                GUI.enabled = CanAct && RadarEnabled && TargetAvailable(i);
                string name = i < targetNames.Length ? targetNames[i] : targets[i].GetNodeName();
                if (GUILayout.Button((SelectedIndex == i ? "[TRACKING] " : "") + name + (targets[i].IsRepaired ? " [collected]" : ""), GUILayout.Height(24))) SelectTarget(i);
            }
            GUI.enabled = CanAct && RadarEnabled;
            if (GUILayout.Button("Stop tracking", GUILayout.Height(22))) SelectTarget(-1);
            GUI.enabled = enabled;
            if (TargetAvailable(SelectedIndex) && RadarEnabled)
            {
                Vector3 delta = targets[SelectedIndex].transform.position - player.position;
                Transform view = Camera.main != null ? Camera.main.transform : player;
                float bearing = Vector3.SignedAngle(Vector3.ProjectOnPlane(view.forward, Vector3.up), Vector3.ProjectOnPlane(delta, Vector3.up), Vector3.up);
                string direction = Mathf.Abs(bearing) < 15 ? "Ahead" : Mathf.Abs(bearing) > 150 ? "Behind" : bearing > 0 ? "Right" : "Left";
                Vector3 point = targets[SelectedIndex].transform.position;
                GUILayout.Label($"{delta.magnitude:F0} m | {direction} {Mathf.Abs(bearing):F0} deg");
                GUILayout.Label($"X {point.x:F1} / Z {point.z:F1}");
            }
            else GUILayout.Label("No active target");
            GUILayout.Label("Hold TAB + click to choose");
            }
            GUILayout.EndArea();
            GUI.backgroundColor = oldBackground;
            GUI.Box(new Rect(Screen.width * 0.5f - 230, 8f, 460, 28), Guidance());
            if (LowOxygen)
            {
                GUI.color = new Color(1f, 0.3f, 0.2f);
                GUI.Box(new Rect(Screen.width * 0.5f - 170, 40f, 340, 28), "LOW OXYGEN < 30% - FIND A CANISTER");
            }
            GUI.color = previous;
        }
    }
}
