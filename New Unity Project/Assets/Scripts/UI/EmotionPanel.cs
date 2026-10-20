using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using WebSocketSharp;
using EEGAI.Demo;

public class EmotionPanel : MonoBehaviour
{
    [Header("WebSocket Settings")]
    public string serverUrl = "ws://127.0.0.1:8765";
    public float reconnectDelay = 3f;
    
    [Header("引用")]
    public EnvironmentController environmentController;
    
    [Header("Debug")]
    public bool showDebugInfo = false;  // 默认关闭，避免干扰
    public string currentEmotion = "normal";
    public float currentConfidence = 0f;
    public float transitionProgress = 0f;
    public bool currentDeviceConnected = false;
    public bool currentStreamConnected = false;
    public float currentDataAgeSeconds = 999999f;
    public int currentPoorSignal = 200;
    public int currentAttention = 0;
    public int currentMeditation = 0;
    public string currentSource = "unknown";
    public string currentRawEmotion = "unknown";
    public float currentESenseAgeSeconds = 999999f;
    public float currentPowerAgeSeconds = 999999f;
    public int currentRawPacketCount = 0;
    public int currentESensePacketCount = 0;
    public int currentPowerPacketCount = 0;

    // Runtime-only state. Kept static so adding it does not change the
    // serialization layout of this scene component.
    public static int[] CurrentRawSamples { get; private set; } = System.Array.Empty<int>();
    public static string CurrentConfigMode { get; private set; } = "unknown";
    public static string CurrentConfigPort { get; private set; } = "";
    public static string CurrentServerVersion { get; private set; } = "unknown";
    public static bool CurrentInputComplete { get; private set; }
    
    private WebSocket ws;
    private float nextReconnectTime;
    private float lastMessageTime = -1f;
    private static bool configRestartRequested;
    public bool WebSocketConnected => ws != null && ws.ReadyState == WebSocketState.Open;
    public int ReceivedMessageCount { get; private set; }
    public float MessageAgeSeconds => lastMessageTime < 0f ? 999999f : Time.realtimeSinceStartup - lastMessageTime;
    public string ConnectionStatus { get; private set; } = "Not connected";
    private string targetEmotion = "normal";
    private DemoAttentionSystem demoAttentionSystem;
    private DemoMeditationSystem demoMeditationSystem;
    private DemoEmotionSimulator demoEmotionSimulator;
    private bool allowReconnect = true;
    private bool isShuttingDown = false;
    // WebSocket callbacks run on worker threads. Only Update may touch Unity objects.
    private readonly Queue<System.Action> mainThreadActions = new Queue<System.Action>();
    private readonly object queueLock = new object();

    private void OnEnable()
    {
        isShuttingDown = false;
        allowReconnect = true;
        nextReconnectTime = 0f;

        if (Application.isPlaying && ws == null)
        {
            Connect();
        }
    }

    private void Start()
    {
        if (environmentController == null)
        {
            environmentController = FindObjectOfType<EnvironmentController>();
        }

        demoAttentionSystem = FindAnyObjectByType<DemoAttentionSystem>();
        demoMeditationSystem = FindAnyObjectByType<DemoMeditationSystem>();
        demoEmotionSimulator = FindAnyObjectByType<DemoEmotionSimulator>();
        
        Connect();
    }
    
    private void Update()
    {
        // ===== 调试面板开关（使用新 Input System）=====
        if (Keyboard.current != null)
        {
            // 检查 F12 键
            if (false && Keyboard.current.f12Key.wasPressedThisFrame)
            {
                showDebugInfo = !showDebugInfo;
                Debug.Log($"[EmotionPanel] 调试面板 {(showDebugInfo ? "显示" : "隐藏")}");
                
                // 根据面板状态处理光标锁定
                if (showDebugInfo)
                {
                    // 显示面板时释放光标
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    // 隐藏面板时恢复光标锁定
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
            
            // ===== 手动情绪切换 =====
            if (DemoUIVisibilityController.ShowDebugPanels &&
                (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame))
            {
                Debug.Log("[TEST] 手动切换到 HAPPY");
                TestEmotion("happy");
            }
            else if (DemoUIVisibilityController.ShowDebugPanels &&
                (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame))
            {
                Debug.Log("[TEST] 手动切换到 SAD");
                TestEmotion("sad");
            }
            else if (DemoUIVisibilityController.ShowDebugPanels &&
                (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame))
            {
                Debug.Log("[TEST] 手动切换到 NORMAL");
                TestEmotion("normal");
            }
        }
        
        ProcessMessageQueue();
        UpdateConnection();
    }

    private void Connect()
    {
        if (!allowReconnect || isShuttingDown || !isActiveAndEnabled)
        {
            return;
        }

        if (ws != null && (ws.ReadyState == WebSocketState.Open || ws.ReadyState == WebSocketState.Connecting))
        {
            return;
        }

        ReleaseSocket();
        nextReconnectTime = Time.realtimeSinceStartup + Mathf.Max(0.25f, reconnectDelay);
        ConnectionStatus = $"Connecting to {serverUrl}";
        Debug.Log($"[EEG] {ConnectionStatus}...");
        try
        {
            ws = new WebSocket(serverUrl);
            ws.OnOpen += OnOpen;
            ws.OnMessage += OnMessage;
            ws.OnError += OnError;
            ws.OnClose += OnClose;
            ws.ConnectAsync();
        }
        catch (System.Exception ex)
        {
            ConnectionStatus = $"Connection failed: {ex.Message}";
            Debug.LogWarning($"[EEG] {ConnectionStatus}");
            ReleaseSocket();
        }
    }

    private void OnOpen(object sender, System.EventArgs e)
    {
        EnqueueSocketAction(sender, () =>
        {
            ConnectionStatus = "Connected; waiting for EEG data";
            Debug.Log("[EEG] Connected to emotion server.");
        });
    }

    private void OnMessage(object sender, MessageEventArgs e)
    {
        if (e.IsText)
            EnqueueSocketAction(sender, () => ProcessEmotionData(e.Data));
    }

    private void ProcessEmotionData(string jsonData)
    {
        try
        {
            var emotionData = JsonUtility.FromJson<EmotionData>(jsonData);
            
            if (emotionData == null || string.IsNullOrEmpty(emotionData.emotion))
            {
                Debug.LogWarning("[EEG] Received null emotion data");
                return;
            }

            lastMessageTime = Time.realtimeSinceStartup;
            ReceivedMessageCount++;
            ConnectionStatus = "Receiving EEG data";
            if (ReceivedMessageCount == 1)
                Debug.Log($"[EEG] First data received: source={emotionData.source}, emotion={emotionData.emotion}, att={emotionData.attention}, med={emotionData.meditation}");
            
            currentConfidence = emotionData.confidence;
            transitionProgress = emotionData.transition_progress;
            currentDeviceConnected = emotionData.device_connected && emotionData.emotion != "offline";
            currentStreamConnected = emotionData.stream_connected;
            currentDataAgeSeconds = emotionData.data_age_seconds;
            currentPoorSignal = emotionData.poor_signal;
            currentAttention = emotionData.attention;
            currentMeditation = emotionData.meditation;
            currentSource = string.IsNullOrEmpty(emotionData.source) ? "unknown" : emotionData.source;
            currentRawEmotion = string.IsNullOrEmpty(emotionData.raw_emotion) ? "unknown" : emotionData.raw_emotion;
            currentESenseAgeSeconds = emotionData.esense_age_seconds;
            currentPowerAgeSeconds = emotionData.power_age_seconds;
            currentRawPacketCount = emotionData.raw_packet_count;
            currentESensePacketCount = emotionData.esense_packet_count;
            currentPowerPacketCount = emotionData.power_packet_count;
            CurrentRawSamples = emotionData.raw_samples ?? System.Array.Empty<int>();
            CurrentConfigMode = string.IsNullOrEmpty(emotionData.config_mode) ? "unknown" : emotionData.config_mode;
            CurrentConfigPort = string.IsNullOrEmpty(emotionData.config_port) ? string.Empty : emotionData.config_port;
            CurrentInputComplete = emotionData.input_complete;
            CurrentServerVersion = string.IsNullOrEmpty(emotionData.server_version) ? "unknown" : emotionData.server_version;

            // Validate even while offline: a stale Python process with the wrong
            // config (for example tcp instead of serial COM8) must be replaced.
            ValidateEegConfiguration();

            if (!currentDeviceConnected)
            {
                currentEmotion = "offline";
                targetEmotion = "offline";
                return;
            }


            if (demoAttentionSystem != null)
            {
                demoAttentionSystem.SetExternalAttention(currentAttention);
            }

            if (demoMeditationSystem != null)
            {
                demoMeditationSystem.SetExternalMeditation(currentMeditation);
            }

            if (demoEmotionSimulator != null)
            {
                demoEmotionSimulator.SetExternalEmotion(emotionData.emotion, emotionData.confidence);
            }

            if (!string.IsNullOrEmpty(emotionData.emotion) && emotionData.emotion != targetEmotion)
            {
                targetEmotion = emotionData.emotion;
                currentEmotion = emotionData.emotion;
                Debug.Log($"[EEG] 🎭 New emotion detected: {emotionData.emotion} (confidence: {emotionData.confidence:F2})");
                
                if (environmentController != null)
                {
                    environmentController.SetEmotion(emotionData.emotion);
                }
            }
            else if (!string.IsNullOrEmpty(emotionData.emotion))
            {
                currentEmotion = emotionData.emotion;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[EEG] Failed to parse emotion data: {ex.Message}\nRaw data: {jsonData}");
        }
    }


    
    private void ValidateEegConfiguration()
    {
        bool configOk = DemoEEGServiceLauncher.IsExpectedConfig(CurrentConfigMode, CurrentConfigPort);
        bool sourceOk = DemoEEGServiceLauncher.IsExpectedSource(currentSource);
        bool versionOk = string.Equals(CurrentServerVersion, DemoEEGServiceLauncher.ExpectedServerVersion,
            System.StringComparison.OrdinalIgnoreCase);
        if (configOk && sourceOk && versionOk)
        {
            configRestartRequested = false;
            return;
        }

        if (configRestartRequested)
        {
            return;
        }

        configRestartRequested = true;
        Debug.LogWarning(
            $"[EEG] Source/config/version mismatch: got {CurrentConfigMode}:{CurrentConfigPort} / " +
            $"{currentSource} / v{CurrentServerVersion}, expected {DemoEEGServiceLauncher.ExpectedConfigLabel} / " +
            $"{DemoEEGServiceLauncher.ExpectedSourcePrefix}* / v{DemoEEGServiceLauncher.ExpectedServerVersion}. " +
            "Restarting EEG services once.");
        DemoEEGServiceLauncher.RestartServices();
    }

    private void TestEmotion(string emotion)
    {
        currentEmotion = emotion;
        targetEmotion = emotion;
        
        if (environmentController != null)
        {
            Debug.Log($"[EmotionPanel] 调用 EnvironmentController 切换到 {emotion}");
            environmentController.SetEmotion(emotion);
        }
        else
        {
            Debug.LogError("[EmotionPanel] ❌ 找不到 EnvironmentController！请检查场景配置！");
            // 如果没有 EnvironmentController，直接简单测试天空盒
            SimpleSkyboxTest(emotion);
        }
    }
    
    private void SimpleSkyboxTest(string emotion)
    {
        Debug.Log($"[EmotionPanel] 简单测试：直接切换天空盒到 {emotion}");
        
        // 简单直接切换，没有过渡
        if (emotion == "happy")
        {
            // 你可以在这里指定一个测试用的天空盒材质
            // RenderSettings.skybox = someHappyMaterial;
            RenderSettings.ambientLight = new Color(1f, 0.8f, 0.6f);
            Debug.Log("[EmotionPanel] 设置为 HAPPY 光照");
        }
        else if (emotion == "sad")
        {
            RenderSettings.ambientLight = new Color(0.4f, 0.5f, 0.7f);
            Debug.Log("[EmotionPanel] 设置为 SAD 光照");
        }
        else
        {
            RenderSettings.ambientLight = new Color(0.7f, 0.7f, 0.7f);
            Debug.Log("[EmotionPanel] 设置为 NORMAL 光照");
        }
        
        DynamicGI.UpdateEnvironment();
    }

    private void ProcessMessageQueue()
    {
        // Bound work per frame so a backlog cannot stall the game.
        for (int i = 0; i < 256; i++)
        {
            System.Action action;
            lock (queueLock)
            {
                if (mainThreadActions.Count == 0) break;
                action = mainThreadActions.Dequeue();
            }
            action();
        }
    }

    private void EnqueueSocketAction(object sender, System.Action action)
    {
        lock (queueLock)
        {
            mainThreadActions.Enqueue(() =>
            {
                // A late callback from a disposed socket must not affect its replacement.
                if (!isShuttingDown && ReferenceEquals(sender, ws))
                    action();
            });
        }
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        EnqueueSocketAction(sender, () =>
        {
            ConnectionStatus = $"Connection error: {e.Message}";
            Debug.LogWarning($"[EEG] {ConnectionStatus}");
        });
    }

    private void OnClose(object sender, CloseEventArgs e)
    {
        EnqueueSocketAction(sender, () =>
        {
            ConnectionStatus = $"Disconnected ({e.Code}); retrying";
            MarkDataOffline();
            nextReconnectTime = Time.realtimeSinceStartup + Mathf.Max(0.25f, reconnectDelay);
            Debug.Log($"[EEG] {ConnectionStatus}: {e.Reason}");
        });
    }

    private void UpdateConnection()
    {
        if (lastMessageTime >= 0f)
        {
            currentDataAgeSeconds = Mathf.Min(999999f, currentDataAgeSeconds + Time.unscaledDeltaTime);
            currentESenseAgeSeconds = Mathf.Min(999999f, currentESenseAgeSeconds + Time.unscaledDeltaTime);
            currentPowerAgeSeconds = Mathf.Min(999999f, currentPowerAgeSeconds + Time.unscaledDeltaTime);
        }

        if (!WebSocketConnected || MessageAgeSeconds > 3f || currentDataAgeSeconds > 3f)
            MarkDataOffline();

        // Real time continues while the menu pauses gameplay (timeScale == 0).
        if (allowReconnect && !isShuttingDown && Time.realtimeSinceStartup >= nextReconnectTime &&
            (ws == null || ws.ReadyState == WebSocketState.Closed))
            Connect();
    }

    private void MarkDataOffline()
    {
        currentDeviceConnected = false;
        currentStreamConnected = false;
        currentEmotion = "offline";
        targetEmotion = "offline";
        CurrentRawSamples = System.Array.Empty<int>();
        CurrentInputComplete = false;
    }

    private void OnDisable()
    {
        ShutdownConnection();
    }

    private void OnDestroy()
    {
        ShutdownConnection();
    }

    private void ShutdownConnection()
    {
        isShuttingDown = true;
        allowReconnect = false;

        ReleaseSocket();
        lock (queueLock) mainThreadActions.Clear();
        MarkDataOffline();
        lastMessageTime = -1f;
        ConnectionStatus = "Disconnected";
    }

    private void ReleaseSocket()
    {
        if (ws != null)
        {
            ws.OnOpen -= OnOpen;
            ws.OnMessage -= OnMessage;
            ws.OnError -= OnError;
            ws.OnClose -= OnClose;
            ws.CloseAsync();
            ws = null;
        }
    }

    private void OnGUI()
    {
        showDebugInfo = DemoUIVisibilityController.ShowDebugPanels;
        if (!showDebugInfo) return;
        
        // 使用旧版 Input System 处理光标状态
        if (Event.current.type == EventType.Layout)
        {
            // 显示面板时释放光标
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        float boxWidth = 350;
        float boxHeight = 300;
        
        GUILayout.BeginArea(new Rect(10, 10, boxWidth, boxHeight));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("🧠 EEG Emotion Receiver", GUI.skin.box);
        GUILayout.Space(10);
        
        bool isConnected = WebSocketConnected;
        GUILayout.Label($"🔗 Connection: {(isConnected ? "✅ Connected" : "❌ Disconnected")}");
        GUILayout.Label($"😊 Current Emotion: {currentEmotion.ToUpper()}");
        GUILayout.Label($"📊 Confidence: {currentConfidence:P1}");
        GUILayout.Label($"⏱️ Transition: {transitionProgress:P1}");
        
        if (environmentController != null)
        {
            GUILayout.Space(5);
            GUI.color = Color.green;
            GUILayout.Label("✅ Environment Ready");
            GUI.color = Color.white;
        }
        
        GUILayout.Space(5);
        
        GUILayout.Label($"Device: {(currentDeviceConnected ? "LIVE" : "OFFLINE")} | Source: {currentSource}");
        GUILayout.Label($"Poor Signal: {currentPoorSignal} | Data Age: {currentDataAgeSeconds:F1}s | eSense Age: {currentESenseAgeSeconds:F1}s");
        GUILayout.Label($"Attention: {currentAttention} | Meditation: {currentMeditation}");
        GUILayout.Label($"Raw Emotion: {currentRawEmotion} | Packets raw/eSense/power: {currentRawPacketCount}/{currentESensePacketCount}/{currentPowerPacketCount}");

        if (currentDeviceConnected)
        {
            GUI.color = Color.green;
            GUILayout.Label("● LIVE", GUI.skin.box);
            GUI.color = Color.white;
        }
        else
        {
            GUI.color = Color.red;
            GUILayout.Label("○ OFFLINE", GUI.skin.box);
            GUI.color = Color.white;
        }
        
        GUILayout.Space(5);
        GUILayout.Label("[F12]=Toggle Panel");
        GUILayout.Label("[1=HAPPY] [2=SAD] [3=NORMAL]");
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    [System.Serializable]
    private class EmotionData
    {
        public string emotion;
        public float confidence;
        public float transition_progress;
        public Probabilities probabilities;
        public float timestamp;
        public bool device_connected;
        public bool stream_connected;
        public float data_age_seconds;
        public float packet_age_seconds;
        public int poor_signal;
        public int attention;
        public int meditation;
        public string source;
        public string raw_emotion;
        public float esense_age_seconds;
        public float power_age_seconds;
        public int raw_packet_count;
        public int esense_packet_count;
        public int power_packet_count;
        public int[] raw_samples;
        public string config_mode;
        public string config_port;
        public bool input_complete;
        public string server_version;
    }

    [System.Serializable]
    private class Probabilities
    {
        public float happy;
        public float sad;
        public float normal;
    }
}
