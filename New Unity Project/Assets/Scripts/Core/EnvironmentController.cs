using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class EnvironmentController : MonoBehaviour
{
    [Header("情绪面板链接（可选）")]
    [SerializeField] private EmotionPanel emotionPanel;
    
    [Header("情绪曲线显示链接（可选）")]
    
    [Header("天空盒设置（手动模式）")]
    [SerializeField] private Material[] skyboxMaterials;
    [SerializeField] private int currentSkyboxIndex = 0;
    
    [Header("情绪天空盒设置")]
    [SerializeField] private Material happySkybox;
    [SerializeField] private Material sadSkybox;
    [SerializeField] private Material normalSkybox;
    
    [Header("BGM 设置（手动模式）")]
    [SerializeField] private AudioClip[] bgmClips;
    [SerializeField] private int currentBGMIndex = 0;
    [SerializeField] private float bgmVolume = 0.7f;
    [SerializeField] private float fadeDuration = 1.0f;
    
    [Header("情绪音乐设置")]
    [SerializeField] private AudioClip happyMusic;
    [SerializeField] private AudioClip sadMusic;
    [SerializeField] private AudioClip normalMusic;
    [SerializeField, Min(0.1f)] private float musicTransitionTime = 3f;
    [Tooltip("情绪保持稳定多久后才允许切换 BGM，过滤 EEG 分类抖动。")]
    [SerializeField, Min(0f)] private float musicEmotionHoldSeconds = 6f;
    [Tooltip("每首曲目至少播放多久才允许切换，避免开头反复重播。")]
    [SerializeField, Min(0f)] private float minimumMusicTrackSeconds = 90f;
    
    [Header("环境光设置")]
    [SerializeField] private Color[] ambientColors;
    [SerializeField] private Light directionalLight;
    [Tooltip("0 会沿用场景原本的太阳光强度；大于 0 时作为普通情绪的基准强度。")]
    [SerializeField, Min(0f)] private float normalLightIntensity = 0f;
    [SerializeField] private Color normalLightColor = new Color(1f, 0.98f, 0.95f);
    [SerializeField] private Color happyLightColor = new Color(1f, 0.94f, 0.82f);
    [SerializeField] private Color sadLightColor = new Color(0.82f, 0.9f, 1f);
    [SerializeField] private Color normalAmbientColor = new Color(0.035f, 0.038f, 0.045f);
    [Tooltip("默认保留星空，通过冷暖和亮度表达情绪。开启后在过渡中切换情绪天空。")]
    [SerializeField] private bool switchSkyboxWithEmotion = false;
    
    [Header("过渡设置")]
    [SerializeField] private float skyboxTransitionDuration = 1.5f;
    
    [Header("控制模式")]
    [SerializeField] private bool useEmotionControl = true;
    
    private AudioSource audioSource;
    private AudioSource crossfadeSource;
    private Coroutine fadeCoroutine;
    private Coroutine musicCrossfadeCoroutine;
    private Coroutine skyboxTransitionCoroutine;
    private Material currentSkyboxMaterial;
    private Material targetSkyboxMaterial;
    private Color targetAmbientColorValue;
    
    private string targetEmotion;
    private Color targetLightColor;
    private float targetLightIntensity;
    private Material originalSkybox;
    private AmbientMode originalAmbientMode;
    private Color originalAmbientColor;
    private Light originalSun;
    private Color originalLightColor;
    private float originalLightIntensity;
    private Dictionary<string, Material> emotionSkyboxes;
    private Dictionary<string, AudioClip> emotionMusic;
    private string lastEmotion = "";
    private AudioClip pendingMusicClip;
    private float pendingMusicSince;
    private float lastMusicChangeTime;

    public string CurrentEmotion => targetEmotion;
    public Light DirectionalLight => directionalLight;

    void Awake()
    {
        // Keep BGM isolated from other audio services on this object. In SampleScene the
        // pre-existing AudioSource belongs to TTSService; sharing it lets speech replace
        // the looping music clip (and music can cut speech off in return).
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = bgmVolume;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        
        crossfadeSource = gameObject.AddComponent<AudioSource>();
        crossfadeSource.loop = true;
        crossfadeSource.volume = 0f;
        crossfadeSource.playOnAwake = false;
        crossfadeSource.spatialBlend = 0f;
        
        if (emotionPanel == null)
        {
            emotionPanel = GetComponent<EmotionPanel>();
        }

        InitializeEmotionMaps();
        originalSkybox = RenderSettings.skybox;
        originalAmbientMode = RenderSettings.ambientMode;
        originalAmbientColor = RenderSettings.ambientLight;
        originalSun = RenderSettings.sun;
        if (directionalLight == null && originalSun != null && originalSun.type == LightType.Directional)
            directionalLight = originalSun;
        if (directionalLight == null)
        {
            foreach (Light candidate in FindObjectsOfType<Light>())
            {
                if (candidate.type == LightType.Directional && candidate.isActiveAndEnabled &&
                    (directionalLight == null || candidate.intensity > directionalLight.intensity))
                    directionalLight = candidate;
            }
        }
        if (directionalLight != null)
        {
            originalLightColor = directionalLight.color;
            originalLightIntensity = directionalLight.intensity;
            if (normalLightIntensity <= 0f)
                normalLightIntensity = originalLightIntensity;
            RenderSettings.sun = directionalLight;
        }
        currentSkyboxMaterial = RenderSettings.skybox;
        targetSkyboxMaterial = currentSkyboxMaterial;
    }

    void Start()
    {
        if (skyboxMaterials != null && currentSkyboxIndex >= 0 && currentSkyboxIndex < skyboxMaterials.Length && skyboxMaterials[currentSkyboxIndex] != null)
        {
            currentSkyboxMaterial = skyboxMaterials[currentSkyboxIndex];
            RenderSettings.skybox = currentSkyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }
        else if (currentSkyboxMaterial == null && normalSkybox != null)
        {
            currentSkyboxMaterial = normalSkybox;
            RenderSettings.skybox = currentSkyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }
        
        targetSkyboxMaterial = currentSkyboxMaterial;
        if (string.IsNullOrEmpty(targetEmotion)) SetEmotion("normal", true);
        
        AudioClip startupMusic = null;
        if (bgmClips != null && currentBGMIndex >= 0 && currentBGMIndex < bgmClips.Length)
            startupMusic = bgmClips[currentBGMIndex];
        if (startupMusic == null)
            startupMusic = normalMusic;

        // SetEmotion("normal", true) may already have started this clip. Avoid restarting
        // it a second time during Start, otherwise the intro is played twice at launch.
        if (startupMusic != null && (audioSource.clip != startupMusic || !audioSource.isPlaying))
        {
            audioSource.clip = startupMusic;
            audioSource.time = 0f;
            audioSource.Play();
        }
        if (audioSource.isPlaying)
            lastMusicChangeTime = Time.unscaledTime;
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
    }

    void Update()
    {
        HandleInput();
        
        if (useEmotionControl && emotionPanel != null)
        {
            UpdateFromEmotionPanel();
        }

        UpdatePendingMusic();
        
    }

    private void UpdateFromEmotionPanel()
    {
        if (emotionPanel.currentEmotion != lastEmotion && !string.IsNullOrEmpty(emotionPanel.currentEmotion))
        {
            lastEmotion = emotionPanel.currentEmotion;
            SetEmotion(emotionPanel.currentEmotion);
        }
    }

    private void HandleInput()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard == null) return;
        
        if (keyboard.f1Key.wasPressedThisFrame)
        {
            PreviousSkybox();
        }
        else if (keyboard.f2Key.wasPressedThisFrame)
        {
            NextSkybox();
        }
        else if (!useEmotionControl && keyboard.digit1Key.wasPressedThisFrame)
        {
            SetSkybox(0);
        }
        else if (!useEmotionControl && keyboard.digit2Key.wasPressedThisFrame)
        {
            SetSkybox(1);
        }
        else if (!useEmotionControl && keyboard.digit3Key.wasPressedThisFrame)
        {
            SetSkybox(2);
        }
        
        if (keyboard.f3Key.wasPressedThisFrame)
        {
            PreviousBGM();
        }
        else if (keyboard.f4Key.wasPressedThisFrame)
        {
            NextBGM();
        }
        else if (keyboard.digit4Key.wasPressedThisFrame)
        {
            SetBGM(0);
        }
        else if (keyboard.digit5Key.wasPressedThisFrame)
        {
            SetBGM(1);
        }
        else if (keyboard.digit6Key.wasPressedThisFrame)
        {
            SetBGM(2);
        }
        
        if (keyboard.mKey.wasPressedThisFrame)
        {
            ToggleMute();
        }
    }

    public void SetEmotion(string emotion)
    {
        SetEmotion(emotion, false);
    }

    public void SetEmotion(string emotion, bool immediate)
    {
        if (string.IsNullOrWhiteSpace(emotion)) return;
        emotion = emotion.ToLowerInvariant();
        if (!emotionSkyboxes.ContainsKey(emotion) || (emotion == targetEmotion && !immediate)) return;

        targetEmotion = emotion;
        targetLightColor = emotion == "happy" ? happyLightColor : emotion == "sad" ? sadLightColor : normalLightColor;
        float brightness = emotion == "happy" ? 1.05f : emotion == "sad" ? 0.9f : 1f;
        targetLightIntensity = normalLightIntensity * brightness;
        targetAmbientColorValue = normalAmbientColor * brightness;
        targetAmbientColorValue.a = 1f;
        if (switchSkyboxWithEmotion && emotionSkyboxes.TryGetValue(emotion, out Material newSkybox) && newSkybox != null)
        {
            targetSkyboxMaterial = newSkybox;
        }
        // Flat ambient makes the controlled fill visible and prevents a colored nebula from tinting the moon.
        RenderSettings.ambientMode = AmbientMode.Flat;
        if (skyboxTransitionCoroutine != null) StopCoroutine(skyboxTransitionCoroutine);
        skyboxTransitionCoroutine = null;
        if (immediate)
        {
            ApplyLighting(1f, targetLightColor, targetLightIntensity, targetAmbientColorValue);
            if (targetSkyboxMaterial != null) RenderSettings.skybox = targetSkyboxMaterial;
            currentSkyboxMaterial = targetSkyboxMaterial;
        }
        else StartSkyboxTransition();

        // 情绪只提出候选曲目，实际切换要等分类稳定，避免 EEG 瞬时跳变重启歌曲。
        if (emotionMusic.TryGetValue(emotion, out AudioClip newMusic) && newMusic != null)
        {
            if (immediate)
            {
                pendingMusicClip = null;
                if (musicCrossfadeCoroutine != null) StopCoroutine(musicCrossfadeCoroutine);
                musicCrossfadeCoroutine = null;
                crossfadeSource.Stop();
                crossfadeSource.clip = null;
                if (audioSource.clip != newMusic || !audioSource.isPlaying)
                {
                    audioSource.clip = newMusic;
                    audioSource.time = 0f;
                    audioSource.Play();
                }
                audioSource.volume = bgmVolume;
                lastMusicChangeTime = Time.unscaledTime;
            }
            else if (musicCrossfadeCoroutine != null || audioSource.clip != newMusic)
            {
                if (pendingMusicClip != newMusic)
                {
                    pendingMusicClip = newMusic;
                    pendingMusicSince = Time.unscaledTime;
                }
            }
            else
            {
                pendingMusicClip = null;
            }
        }
    }
    
    /// <summary>
    /// sad=0, normal=1, happy=2
    /// </summary>
    public void NextSkybox()
    {
        if (skyboxMaterials == null || skyboxMaterials.Length == 0) return;
        currentSkyboxIndex = (currentSkyboxIndex + 1) % skyboxMaterials.Length;
        TransitionToSkybox(currentSkyboxIndex);
    }

    public void PreviousSkybox()
    {
        if (skyboxMaterials == null || skyboxMaterials.Length == 0) return;
        currentSkyboxIndex = (currentSkyboxIndex - 1 + skyboxMaterials.Length) % skyboxMaterials.Length;
        TransitionToSkybox(currentSkyboxIndex);
    }

    public void SetSkybox(int index)
    {
        if (skyboxMaterials == null || index < 0 || index >= skyboxMaterials.Length || index == currentSkyboxIndex) return;
        currentSkyboxIndex = index;
        TransitionToSkybox(currentSkyboxIndex);
    }

    private void TransitionToSkybox(int index)
    {
        if (skyboxMaterials == null || index >= skyboxMaterials.Length || skyboxMaterials[index] == null) return;
        
        targetSkyboxMaterial = skyboxMaterials[index];
        
        if (ambientColors != null && index < ambientColors.Length)
        {
            targetAmbientColorValue = ambientColors[index];
        }
        
        StartSkyboxTransition();
    }

    private void StartSkyboxTransition()
    {
        if (skyboxTransitionCoroutine != null)
        {
            StopCoroutine(skyboxTransitionCoroutine);
        }
        skyboxTransitionCoroutine = StartCoroutine(SkyboxTransitionCoroutine());
    }

    private IEnumerator SkyboxTransitionCoroutine()
    {
        float elapsed = 0f;
        Color startAmbient = RenderSettings.ambientLight;
        Color startLight = directionalLight != null ? directionalLight.color : targetLightColor;
        float startIntensity = directionalLight != null ? directionalLight.intensity : targetLightIntensity;
        float duration = Mathf.Max(0.1f, skyboxTransitionDuration);
        bool skyboxChanged = false;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            ApplyLighting(t, startLight, startIntensity, startAmbient);
            // Material.Lerp cannot blend the six skybox textures. Keep one explicit optional switch.
            if (!skyboxChanged && t >= 0.5f && targetSkyboxMaterial != null &&
                RenderSettings.skybox != targetSkyboxMaterial)
            {
                RenderSettings.skybox = targetSkyboxMaterial;
                skyboxChanged = true;
            }
            yield return null;
        }

        currentSkyboxMaterial = targetSkyboxMaterial;
        ApplyLighting(1f, startLight, startIntensity, startAmbient);
        if (skyboxChanged) DynamicGI.UpdateEnvironment();
        skyboxTransitionCoroutine = null;
    }

    private void ApplyLighting(float t, Color startColor, float startIntensity, Color startAmbient)
    {
        RenderSettings.ambientLight = Color.Lerp(startAmbient, targetAmbientColorValue, t);
        if (directionalLight != null)
        {
            directionalLight.color = Color.Lerp(startColor, targetLightColor, t);
            directionalLight.intensity = Mathf.Lerp(startIntensity, targetLightIntensity, t);
        }
    }

    public void NextBGM()
    {
        if (bgmClips == null || bgmClips.Length == 0) return;
        currentBGMIndex = (currentBGMIndex + 1) % bgmClips.Length;
        PlayBGM(currentBGMIndex);
    }

    public void PreviousBGM()
    {
        if (bgmClips == null || bgmClips.Length == 0) return;
        currentBGMIndex = (currentBGMIndex - 1 + bgmClips.Length) % bgmClips.Length;
        PlayBGM(currentBGMIndex);
    }

    public void SetBGM(int index)
    {
        if (bgmClips == null || index < 0 || index >= bgmClips.Length) return;
        currentBGMIndex = index;
        PlayBGM(currentBGMIndex);
    }

    private void PlayBGM(int index)
    {
        if (bgmClips == null || index < 0 || index >= bgmClips.Length || bgmClips[index] == null) return;
        
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeToBGM(bgmClips[index]));
    }

    private IEnumerator FadeToBGM(AudioClip newClip)
    {
        if (audioSource.isPlaying && audioSource.clip != null)
        {
            float startVolume = audioSource.volume;
            for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
                yield return null;
            }
            audioSource.Stop();
        }

        audioSource.clip = newClip;
        audioSource.Play();

        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            audioSource.volume = Mathf.Lerp(0f, bgmVolume, t / fadeDuration);
            yield return null;
        }
        audioSource.volume = bgmVolume;

        Debug.Log($"正在播放 BGM: {newClip.name}");
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        if (newClip == null) yield break;
        crossfadeSource.Stop();
        crossfadeSource.clip = null;
        
        crossfadeSource.clip = newClip;
        crossfadeSource.time = 0f;
        crossfadeSource.volume = 0f;
        crossfadeSource.Play();
        
        float duration = Mathf.Max(0.1f, musicTransitionTime);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Equal-power curves keep perceived loudness steadier during the transition.
            audioSource.volume = bgmVolume * Mathf.Cos(t * Mathf.PI * 0.5f);
            crossfadeSource.volume = bgmVolume * Mathf.Sin(t * Mathf.PI * 0.5f);
            
            yield return null;
        }
        
        var temp = audioSource;
        audioSource = crossfadeSource;
        crossfadeSource = temp;
        
        crossfadeSource.Stop();
        crossfadeSource.clip = null;
        crossfadeSource.volume = 0f;
        audioSource.volume = bgmVolume;
        lastMusicChangeTime = Time.unscaledTime;
        musicCrossfadeCoroutine = null;
    }

    private void UpdatePendingMusic()
    {
        if (pendingMusicClip == null || musicCrossfadeCoroutine != null) return;
        if (Time.unscaledTime - pendingMusicSince < Mathf.Max(0f, musicEmotionHoldSeconds)) return;
        float trackHold = Mathf.Max(0f, minimumMusicTrackSeconds);
        if (Time.unscaledTime - lastMusicChangeTime < trackHold) return;
        if (audioSource.clip == pendingMusicClip)
        {
            pendingMusicClip = null;
            return;
        }

        AudioClip stableClip = pendingMusicClip;
        pendingMusicClip = null;
        musicCrossfadeCoroutine = StartCoroutine(CrossfadeMusic(stableClip));
    }

    public void SetVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        audioSource.volume = bgmVolume;
        if (crossfadeSource.isPlaying)
            crossfadeSource.volume = Mathf.Min(crossfadeSource.volume, bgmVolume);
    }

    public void ToggleMute()
    {
        audioSource.mute = !audioSource.mute;
        crossfadeSource.mute = audioSource.mute;
    }

    private void OnDestroy()
    {
        RenderSettings.skybox = originalSkybox;
        RenderSettings.ambientMode = originalAmbientMode;
        RenderSettings.ambientLight = originalAmbientColor;
        RenderSettings.sun = originalSun;
        if (directionalLight != null)
        {
            directionalLight.color = originalLightColor;
            directionalLight.intensity = originalLightIntensity;
        }
    }
}
