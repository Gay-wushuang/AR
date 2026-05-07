using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using WebSocketSharp;

public class EmotionPanel : MonoBehaviour
{
    [Header("WebSocket Settings")]
    public string serverUrl = "ws://localhost:8765";
    public float reconnectDelay = 3f;
    
    [Header("引用")]
    public EnvironmentController environmentController;
    
    [Header("Debug")]
    public bool showDebugInfo = false;  // 默认关闭，避免干扰
    public string currentEmotion = "normal";
    public float currentConfidence = 0f;
    public float transitionProgress = 0f;
    
    private WebSocket ws;
    private Coroutine reconnectCoroutine;
    private string targetEmotion = "normal";
    private bool wasCursorLocked = true;  // 记录光标状态
    
    // 线程安全的消息队列
    private Queue<string> messageQueue = new Queue<string>();
    private object queueLock = new object();

    private void Start()
    {
        if (environmentController == null)
        {
            environmentController = FindObjectOfType<EnvironmentController>();
        }
        
        // 初始化光标状态
        if (!showDebugInfo)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        Connect();
    }
    
    private void Update()
    {
        // ===== 调试面板开关（使用新 Input System）=====
        if (Keyboard.current != null)
        {
            // 检查 F12 键
            if (Keyboard.current.f12Key.wasPressedThisFrame)
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
            if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
            {
                Debug.Log("[TEST] 手动切换到 HAPPY");
                TestEmotion("happy");
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
            {
                Debug.Log("[TEST] 手动切换到 SAD");
                TestEmotion("sad");
            }
            else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
            {
                Debug.Log("[TEST] 手动切换到 NORMAL");
                TestEmotion("normal");
            }
        }
        
        ProcessMessageQueue();
    }

    private void Connect()
    {
        if (ws != null && ws.IsAlive)
        {
            return;
        }

        Debug.Log($"[EEG] Connecting to {serverUrl}...");
        ws = new WebSocket(serverUrl);
        
        ws.OnOpen += OnOpen;
        ws.OnMessage += OnMessage;
        ws.OnError += OnError;
        ws.OnClose += OnClose;
        
        ws.ConnectAsync();
    }

    private void OnOpen(object sender, System.EventArgs e)
    {
        Debug.Log("[EEG] ✅ Connected to emotion server!");
        if (reconnectCoroutine != null)
        {
            StopCoroutine(reconnectCoroutine);
            reconnectCoroutine = null;
        }
    }

    private void OnMessage(object sender, MessageEventArgs e)
    {
        lock (queueLock)
        {
            messageQueue.Enqueue(e.Data);
        }
    }

    private void ProcessEmotionData(string jsonData)
    {
        try
        {
            Debug.Log($"[EEG] 收到数据: {jsonData}");
            
            var emotionData = JsonUtility.FromJson<EmotionData>(jsonData);
            
            if (emotionData == null)
            {
                Debug.LogWarning("[EEG] Received null emotion data");
                return;
            }
            
            currentConfidence = emotionData.confidence;
            transitionProgress = emotionData.transition_progress;
            
            Debug.Log($"[EEG] 解析结果: emotion={emotionData.emotion}, confidence={emotionData.confidence:F2}");
            
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
        List<string> messagesToProcess = new List<string>();
        
        lock (queueLock)
        {
            while (messageQueue.Count > 0)
            {
                messagesToProcess.Add(messageQueue.Dequeue());
            }
        }
        
        foreach (string message in messagesToProcess)
        {
            ProcessEmotionData(message);
        }
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        Debug.LogError($"[EEG] ❌ WebSocket error: {e.Message}");
    }

    private void OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log($"[EEG] Disconnected from server. Code: {e.Code}, Reason: {e.Reason}");
        
        if (reconnectCoroutine == null)
        {
            reconnectCoroutine = StartCoroutine(Reconnect());
        }
    }

    private IEnumerator Reconnect()
    {
        int retryCount = 0;
        while (ws == null || !ws.IsAlive)
        {
            retryCount++;
            Debug.Log($"[EEG] Attempting to reconnect... ({retryCount})");
            yield return new WaitForSeconds(reconnectDelay);
            Connect();
        }
    }

    private void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }
    }

    private void OnGUI()
    {
        if (!showDebugInfo) return;
        
        // 使用旧版 Input System 处理光标状态
        if (Event.current.type == EventType.Layout)
        {
            // 显示面板时释放光标
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        float boxWidth = 350;
        float boxHeight = 240;
        
        GUILayout.BeginArea(new Rect(10, 10, boxWidth, boxHeight));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("🧠 EEG Emotion Receiver", GUI.skin.box);
        GUILayout.Space(10);
        
        bool isConnected = ws != null && ws.IsAlive;
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
        
        if (isConnected)
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
    }

    [System.Serializable]
    private class Probabilities
    {
        public float happy;
        public float sad;
        public float normal;
    }
}
