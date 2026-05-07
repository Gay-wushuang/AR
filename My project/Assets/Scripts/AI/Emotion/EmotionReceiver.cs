using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WebSocketSharp;

public class EmotionReceiver : MonoBehaviour
{
    [Header("WebSocket Settings")]
    public string serverUrl = "ws://localhost:8765";
    public float reconnectDelay = 3f;
    
    [Header("Emotion Settings")]
    [Tooltip("天空盒过渡时长（秒）")]
    public float skyboxTransitionTime = 4.0f;  // 增加到4秒
    
    [Tooltip("音乐过渡时长（秒）")]
    public float musicTransitionTime = 5.0f;  // 增加到5秒
    
    [Tooltip("情绪防抖时间（秒）- 在这个时间内不会再次切换情绪")]
    public float emotionCooldownTime = 3.0f;  // 防抖3秒
    
    [Tooltip("情绪切换所需的最小置信度")]
    public float minConfidenceToSwitch = 0.7f;  // 至少70%置信度才切换
    
    [Header("Skybox Settings")]
    public Material happySkybox;
    public Material sadSkybox;
    public Material normalSkybox;
    
    [Header("Music Settings")]
    public AudioClip happyMusic;
    public AudioClip sadMusic;
    public AudioClip normalMusic;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public string currentEmotion = "normal";
    public float currentConfidence = 0f;
    public float transitionProgress = 0f;
    public float cooldownRemaining = 0f;
    public bool isTransitioning = false;
    
    private WebSocket ws;
    private Coroutine reconnectCoroutine;
    private string targetEmotion = "normal";
    private string pendingEmotion = null;
    private AudioSource audioSource;
    private AudioSource crossfadeSource;
    private Material currentSkybox;
    private Material targetSkybox;
    private Material blendedSkyboxMaterial;
    private float skyboxBlend = 0f;
    private Dictionary<string, Material> emotionSkyboxes;
    private Dictionary<string, AudioClip> emotionMusic;
    private Coroutine musicCrossfadeCoroutine;
    
    // 线程安全的消息队列
    private Queue<string> messageQueue = new Queue<string>();
    private object queueLock = new object();

    private void Start()
    {
        InitializeEmotionMaps();
        InitializeAudio();
        Connect();
    }

    private void InitializeEmotionMaps()
    {
        emotionSkyboxes = new Dictionary<string, Material>
        {
            {"happy", happySkybox},
            {"sad", sadSkybox},
            {"normal", normalSkybox}
        };
        
        emotionMusic = new Dictionary<string, AudioClip>
        {
            {"happy", happyMusic},
            {"sad", sadMusic},
            {"normal", normalMusic}
        };
        
        currentSkybox = normalSkybox;
        targetSkybox = normalSkybox;
        
        if (currentSkybox != null)
        {
            RenderSettings.skybox = currentSkybox;
        }
    }
    
    // 平滑过渡曲线：SmoothStep - 更自然的缓动效果
    private float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private void InitializeAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = 1f;
        audioSource.playOnAwake = false;
        
        crossfadeSource = gameObject.AddComponent<AudioSource>();
        crossfadeSource.loop = true;
        crossfadeSource.volume = 0f;
        crossfadeSource.playOnAwake = false;
        
        if (normalMusic != null)
        {
            audioSource.clip = normalMusic;
            audioSource.Play();
        }
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
        // 将消息加入队列（线程安全）
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
            
            // 检查是否可以切换情绪
            if (!string.IsNullOrEmpty(emotionData.emotion) && emotionData.emotion != targetEmotion)
            {
                // 检查置信度是否足够
                if (emotionData.confidence < minConfidenceToSwitch)
                {
                    Debug.Log($"[EEG] ⏸️ 置信度不足 ({emotionData.confidence:F2} < {minConfidenceToSwitch:F2})，忽略情绪切换");
                    return;
                }
                
                // 检查是否在冷却时间内
                if (cooldownRemaining > 0f)
                {
                    Debug.Log($"[EEG] ⏸️ 情绪防抖冷却中 ({cooldownRemaining:F1}s)，暂存情绪: {emotionData.emotion}");
                    pendingEmotion = emotionData.emotion;
                    return;
                }
                
                // 检查是否正在过渡中
                if (isTransitioning)
                {
                    Debug.Log($"[EEG] ⏸️ 正在过渡中，暂存情绪: {emotionData.emotion}");
                    pendingEmotion = emotionData.emotion;
                    return;
                }
                
                // 满足所有条件，执行切换
                targetEmotion = emotionData.emotion;
                currentEmotion = emotionData.emotion;
                Debug.Log($"[EEG] 🎭 New emotion detected: {emotionData.emotion} (confidence: {emotionData.confidence:F2})");
                StartEmotionTransition(emotionData.emotion);
            }
            else if (!string.IsNullOrEmpty(emotionData.emotion))
            {
                // 即使情绪没变，也要更新 currentEmotion 显示
                currentEmotion = emotionData.emotion;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[EEG] Failed to parse emotion data: {ex.Message}\nRaw data: {jsonData}");
        }
    }

    private void StartEmotionTransition(string newEmotion)
    {
        Debug.Log($"[EEG] Starting emotion transition to: {newEmotion}");
        isTransitioning = true;
        cooldownRemaining = emotionCooldownTime;  // 开始冷却计时
        
        if (emotionSkyboxes.TryGetValue(newEmotion, out Material newSkybox) && newSkybox != null && targetSkybox != newSkybox)
        {
            targetSkybox = newSkybox;
            skyboxBlend = 0f;
            
            // 创建或重置混合材质
            if (blendedSkyboxMaterial == null)
            {
                blendedSkyboxMaterial = new Material(currentSkybox);
            }
        }
        
        if (emotionMusic.TryGetValue(newEmotion, out AudioClip newMusic) && newMusic != null)
        {
            // 检查是否正在播放相同的音乐
            if (audioSource.clip != newMusic && crossfadeSource.clip != newMusic)
            {
                if (musicCrossfadeCoroutine != null)
                {
                    StopCoroutine(musicCrossfadeCoroutine);
                }
                musicCrossfadeCoroutine = StartCoroutine(CrossfadeMusic(newMusic));
            }
        }
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        if (newClip == null) yield break;
        
        // 确保 crossfadeSource 是干净的
        crossfadeSource.Stop();
        crossfadeSource.clip = null;
        
        crossfadeSource.clip = newClip;
        crossfadeSource.time = audioSource.time;
        crossfadeSource.volume = 0f;
        crossfadeSource.Play();
        
        float elapsed = 0f;
        while (elapsed < musicTransitionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / musicTransitionTime;
            float smoothT = SmoothStep(t);  // 使用平滑曲线
            
            audioSource.volume = 1f - smoothT;
            crossfadeSource.volume = smoothT;
            
            yield return null;
        }
        
        // 交换并清理
        var temp = audioSource;
        audioSource = crossfadeSource;
        crossfadeSource = temp;
        
        crossfadeSource.Stop();
        crossfadeSource.clip = null;
        crossfadeSource.volume = 0f;
        audioSource.volume = 1f;
        
        musicCrossfadeCoroutine = null;
    }

    private void Update()
    {
        // ===== 手动测试功能（按1/2/3键切换情绪）=====
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            Debug.Log("[TEST] 手动切换到 HAPPY");
            ForceSetEmotion("happy");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            Debug.Log("[TEST] 手动切换到 SAD");
            ForceSetEmotion("sad");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            Debug.Log("[TEST] 手动切换到 NORMAL");
            ForceSetEmotion("normal");
        }
        
        // 更新冷却时间
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= Time.deltaTime;
            if (cooldownRemaining < 0f) cooldownRemaining = 0f;
        }
        
        // 检查是否有暂存的情绪可以处理
        if (pendingEmotion != null && cooldownRemaining <= 0f && !isTransitioning)
        {
            Debug.Log($"[EEG] 🎭 处理暂存情绪: {pendingEmotion}");
            string tempEmotion = pendingEmotion;
            pendingEmotion = null;
            StartEmotionTransition(tempEmotion);
        }
        
        // 在主线程处理消息队列
        ProcessMessageQueue();
        
        UpdateSkyboxTransition();
    }
    
    // 强制设置情绪（用于测试，忽略所有限制）
    private void ForceSetEmotion(string emotion)
    {
        if (emotionSkyboxes.TryGetValue(emotion, out Material skybox) && skybox != null)
        {
            currentEmotion = emotion;
            targetEmotion = emotion;
            cooldownRemaining = 0f;
            pendingEmotion = null;
            isTransitioning = false;
            StartEmotionTransition(emotion);
        }
        else
        {
            Debug.LogError($"[TEST] 找不到 {emotion} 的天空盒材质！请检查Inspector配置。");
        }
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

    private void UpdateSkyboxTransition()
    {
        if (currentSkybox != targetSkybox && currentSkybox != null && targetSkybox != null)
        {
            skyboxBlend += Time.deltaTime / skyboxTransitionTime;
            skyboxBlend = Mathf.Clamp01(skyboxBlend);
            
            float smoothBlend = SmoothStep(skyboxBlend);  // 使用平滑曲线
            
            // 重用混合材质，避免每帧创建新对象
            if (blendedSkyboxMaterial == null)
            {
                blendedSkyboxMaterial = new Material(currentSkybox);
            }
            
            blendedSkyboxMaterial.CopyPropertiesFromMaterial(currentSkybox);
            blendedSkyboxMaterial.Lerp(currentSkybox, targetSkybox, smoothBlend);
            RenderSettings.skybox = blendedSkyboxMaterial;
            DynamicGI.UpdateEnvironment();
            
            if (skyboxBlend >= 1f)
            {
                currentSkybox = targetSkybox;
                // 确保最终使用原始材质而不是临时混合材质
                RenderSettings.skybox = currentSkybox;
                DynamicGI.UpdateEnvironment();
                isTransitioning = false;
                Debug.Log($"[EEG] Skybox transition complete. Current emotion: {targetEmotion}");
            }
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
        
        GUILayout.BeginArea(new Rect(10, 10, 380, 320));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("🧠 EEG Emotion Receiver", GUI.skin.box);
        GUILayout.Space(10);
        
        bool isConnected = ws != null && ws.IsAlive;
        GUILayout.Label($"🔗 Connection: {(isConnected ? "✅ Connected" : "❌ Disconnected")}");
        GUILayout.Label($"😊 Current Emotion: {currentEmotion.ToUpper()}");
        GUILayout.Label($"📊 Confidence: {currentConfidence:P1}");
        GUILayout.Label($"⏱️ Transition: {transitionProgress:P1}");
        
        GUILayout.Space(5);
        
        // 显示防抖状态
        if (cooldownRemaining > 0f)
        {
            GUI.color = Color.yellow;
            GUILayout.Label($"⏸️ Cooldown: {cooldownRemaining:F1}s");
            GUI.color = Color.white;
        }
        
        if (isTransitioning)
        {
            GUI.color = Color.cyan;
            GUILayout.Label("🔄 Transitioning...");
            GUI.color = Color.white;
        }
        
        if (pendingEmotion != null)
        {
            GUI.color = Color.magenta;
            GUILayout.Label($"📋 Pending: {pendingEmotion.ToUpper()}");
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
