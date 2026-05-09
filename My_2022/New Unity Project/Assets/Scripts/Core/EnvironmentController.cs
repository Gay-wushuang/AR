using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class EnvironmentController : MonoBehaviour
{
    [Header("情绪面板链接（可选）")]
    [SerializeField] private EmotionPanel emotionPanel;
    
    [Header("情绪曲线显示链接（可选）")]
    [SerializeField] private MoodWaveGenerator moodWaveGenerator;
    
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
    [SerializeField] private float musicTransitionTime = 2f;
    
    [Header("环境光设置")]
    [SerializeField] private Color[] ambientColors;
    
    [Header("过渡设置")]
    [SerializeField] private float skyboxTransitionDuration = 1.5f;
    [SerializeField] private float transitionSmoothTime = 1.0f;
    
    [Header("控制模式")]
    [SerializeField] private bool useEmotionControl = true;
    
    private AudioSource audioSource;
    private AudioSource crossfadeSource;
    private Coroutine fadeCoroutine;
    private Coroutine musicCrossfadeCoroutine;
    private Coroutine skyboxTransitionCoroutine;
    private Material currentSkyboxMaterial;
    private Material targetSkyboxMaterial;
    private Color currentAmbientColorValue;
    private Color targetAmbientColorValue;
    
    private string currentEmotion = "normal";
    private string targetEmotion = "normal";
    private float skyboxBlend = 0f;
    private Dictionary<string, Material> emotionSkyboxes;
    private Dictionary<string, AudioClip> emotionMusic;
    private string lastEmotion = "";

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = bgmVolume;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        
        crossfadeSource = gameObject.AddComponent<AudioSource>();
        crossfadeSource.loop = true;
        crossfadeSource.volume = 0f;
        crossfadeSource.playOnAwake = false;
        
        if (emotionPanel == null)
        {
            emotionPanel = GetComponent<EmotionPanel>();
        }
        
        if (moodWaveGenerator == null)
        {
            moodWaveGenerator = GetComponent<MoodWaveGenerator>();
        }
    }

    void Start()
    {
        InitializeEmotionMaps();
        
        if (skyboxMaterials != null && currentSkyboxIndex < skyboxMaterials.Length && skyboxMaterials[currentSkyboxIndex] != null)
        {
            currentSkyboxMaterial = skyboxMaterials[currentSkyboxIndex];
            RenderSettings.skybox = currentSkyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }
        else if (normalSkybox != null)
        {
            currentSkyboxMaterial = normalSkybox;
            RenderSettings.skybox = currentSkyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }
        
        if (ambientColors != null && currentSkyboxIndex < ambientColors.Length)
        {
            currentAmbientColorValue = ambientColors[currentSkyboxIndex];
            RenderSettings.ambientSkyColor = currentAmbientColorValue;
        }
        
        if (bgmClips != null && currentBGMIndex < bgmClips.Length && bgmClips[currentBGMIndex] != null)
        {
            audioSource.clip = bgmClips[currentBGMIndex];
            audioSource.Play();
        }
        else if (normalMusic != null)
        {
            audioSource.clip = normalMusic;
            audioSource.Play();
        }
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
        
        UpdateSkyboxTransition();
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
        else if (keyboard.digit1Key.wasPressedThisFrame)
        {
            SetSkybox(0);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {
            SetSkybox(1);
        }
        else if (keyboard.digit3Key.wasPressedThisFrame)
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
        if (string.IsNullOrEmpty(emotion) || emotion == targetEmotion) return;
        
        targetEmotion = emotion;
        currentEmotion = emotion;
        StartEmotionTransition(emotion);
    }

    private void StartEmotionTransition(string newEmotion)
    {
        Debug.Log($"[Env] Starting emotion transition to: {newEmotion}");
        
        // 转换情绪字符串为 MoodWaveGenerator 的整数编码
        int moodCode = EmotionStringToCode(newEmotion);
        
        // 驱动曲线变化
        if (moodWaveGenerator != null)
        {
            moodWaveGenerator.SetEmotionSmooth(moodCode);
        }
        
        // 驱动天空盒变化
        if (emotionSkyboxes.TryGetValue(newEmotion, out Material newSkybox) && newSkybox != null && targetSkyboxMaterial != newSkybox)
        {
            targetSkyboxMaterial = newSkybox;
            skyboxBlend = 0f;
            StartSkyboxTransition();
        }
        
        // 驱动音乐变化
        if (emotionMusic.TryGetValue(newEmotion, out AudioClip newMusic) && newMusic != null)
        {
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
    
    /// <summary>
    /// 将情绪字符串转换为 MoodWaveGenerator 的整数编码
    /// sad=0, normal=1, happy=2
    /// </summary>
    private int EmotionStringToCode(string emotion)
    {
        switch (emotion.ToLower())
        {
            case "sad":
                return 0;
            case "happy":
                return 2;
            case "normal":
            default:
                return 1;
        }
    }

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
        Material startSkybox = currentSkyboxMaterial;
        Color startAmbient = currentAmbientColorValue;
        
        Debug.Log($"开始天空盒过渡: {(startSkybox != null ? startSkybox.name : "null")} -> {targetSkyboxMaterial.name}");
        
        while (elapsed < skyboxTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / skyboxTransitionDuration;
            
            Material blended = new Material(startSkybox);
            blended.Lerp(startSkybox, targetSkyboxMaterial, t);
            RenderSettings.skybox = blended;
            
            RenderSettings.ambientSkyColor = Color.Lerp(startAmbient, targetAmbientColorValue, t);
            
            yield return null;
        }
        
        currentSkyboxMaterial = targetSkyboxMaterial;
        currentAmbientColorValue = targetAmbientColorValue;
        RenderSettings.skybox = currentSkyboxMaterial;
        RenderSettings.ambientSkyColor = currentAmbientColorValue;
        DynamicGI.UpdateEnvironment();
        
        skyboxTransitionCoroutine = null;
        Debug.Log($"天空盒过渡完成: {currentSkyboxMaterial.name}");
    }

    private void UpdateSkyboxTransition()
    {
        if (useEmotionControl && currentSkyboxMaterial != targetSkyboxMaterial && currentSkyboxMaterial != null && targetSkyboxMaterial != null)
        {
            skyboxBlend += Time.deltaTime / transitionSmoothTime;
            skyboxBlend = Mathf.Clamp01(skyboxBlend);
            
            Material blended = new Material(currentSkyboxMaterial);
            blended.Lerp(currentSkyboxMaterial, targetSkyboxMaterial, skyboxBlend);
            RenderSettings.skybox = blended;
            DynamicGI.UpdateEnvironment();
            
            if (skyboxBlend >= 1f)
            {
                currentSkyboxMaterial = targetSkyboxMaterial;
                RenderSettings.skybox = currentSkyboxMaterial;
                DynamicGI.UpdateEnvironment();
                Debug.Log($"[Env] Skybox transition complete. Current emotion: {targetEmotion}");
            }
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
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
                yield return null;
            }
            audioSource.Stop();
        }

        audioSource.clip = newClip;
        audioSource.Play();

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
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
        crossfadeSource.time = audioSource.time;
        crossfadeSource.volume = 0f;
        crossfadeSource.Play();
        
        float elapsed = 0f;
        while (elapsed < musicTransitionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / musicTransitionTime;
            
            audioSource.volume = 1f - t;
            crossfadeSource.volume = t;
            
            yield return null;
        }
        
        var temp = audioSource;
        audioSource = crossfadeSource;
        crossfadeSource = temp;
        
        crossfadeSource.Stop();
        crossfadeSource.clip = null;
        crossfadeSource.volume = 0f;
        audioSource.volume = 1f;
        
        musicCrossfadeCoroutine = null;
    }

    public void SetVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        audioSource.volume = bgmVolume;
    }

    public void ToggleMute()
    {
        audioSource.mute = !audioSource.mute;
    }
}
