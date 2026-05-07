using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class EmotionEffectManager : MonoBehaviour
{
    [Header("配置文件")]
    public EmotionEffectConfig effectConfig;

    [Header("引用")]
    public Camera mainCamera;
    public Light directionalLight;
    public Transform particleSpawnPoint;
    public Volume postProcessVolume;

    [Header("调试")]
    public string currentEmotion = "normal";
    public float cooldownRemaining = 0f;
    public bool isTransitioningInternal = false;
    public string pendingEmotion = null;

    private ScreenFlash screenFlash;
    private AudioSource audioSource;
    private AudioSource crossfadeAudioSource;
    private Dictionary<string, EmotionEffectConfig.EmotionSettings> emotionSettingsMap;
    
    private Material targetSkybox;
    private Color targetAmbientColor;
    private Color targetLightColor;
    private float targetLightIntensity;
    private VolumeProfile targetVolumeProfile;
    
    private Material currentSkybox;
    private Material blendedSkyboxMaterial;
    private Color currentAmbientColor;
    private Color currentLightColor;
    private float currentLightIntensity;
    
    private float transitionTimer;
    private float transitionDuration;
    
    private GameObject currentAmbientParticles;

    private void Awake()
    {
        InitializeComponents();
        InitializeEmotionMap();
    }

    private void Start()
    {
        if (effectConfig != null)
        {
            ApplyEmotionEffect("normal", immediate: true);
        }
    }

    private void InitializeComponents()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            screenFlash = mainCamera.GetComponent<ScreenFlash>();
            if (screenFlash == null)
            {
                screenFlash = mainCamera.gameObject.AddComponent<ScreenFlash>();
            }
        }

        if (directionalLight == null)
        {
            directionalLight = FindObjectOfType<Light>();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        crossfadeAudioSource = gameObject.AddComponent<AudioSource>();
        crossfadeAudioSource.loop = true;
        crossfadeAudioSource.volume = 0f;
        crossfadeAudioSource.playOnAwake = false;

        if (particleSpawnPoint == null)
        {
            particleSpawnPoint = transform;
        }
    }

    private void InitializeEmotionMap()
    {
        emotionSettingsMap = new Dictionary<string, EmotionEffectConfig.EmotionSettings>();
        if (effectConfig != null)
        {
            emotionSettingsMap["happy"] = effectConfig.happySettings;
            emotionSettingsMap["sad"] = effectConfig.sadSettings;
            emotionSettingsMap["normal"] = effectConfig.normalSettings;
        }
    }

    // ===== 平滑曲线函数 =====
    private float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    // ===== 公开API - 带防抖 =====
    public void TryTransitionToEmotion(string newEmotion, float confidence = 1f)
    {
        if (effectConfig == null) return;
        
        // 置信度检查
        if (confidence < effectConfig.minConfidenceToSwitch)
        {
            Debug.Log($"[EmotionManager] 置信度不足 ({confidence:F2} < {effectConfig.minConfidenceToSwitch:F2})，忽略切换到 {newEmotion}");
            return;
        }
        
        // 冷却检查
        if (cooldownRemaining > 0f)
        {
            Debug.Log($"[EmotionManager] 冷却中 ({cooldownRemaining:F1}s)，暂存情绪: {newEmotion}");
            pendingEmotion = newEmotion;
            return;
        }
        
        // 过渡中检查
        if (isTransitioningInternal)
        {
            Debug.Log($"[EmotionManager] 正在过渡中，暂存情绪: {newEmotion}");
            pendingEmotion = newEmotion;
            return;
        }
        
        // 执行切换
        ForceTransitionToEmotion(newEmotion);
    }

    // ===== 强制切换（用于测试）=====
    public void ForceTransitionToEmotion(string newEmotion)
    {
        if (newEmotion == currentEmotion) return;
        
        Debug.Log($"[EmotionManager] 🎭 切换情绪: {currentEmotion} → {newEmotion}");

        string oldEmotion = currentEmotion;
        currentEmotion = newEmotion;

        PlayShortWindowEffects(oldEmotion, newEmotion);
        ApplyLongWindowEnvironment(newEmotion, immediate: false);
        
        // 开始冷却（如果没有配置文件，默认3秒）
        cooldownRemaining = effectConfig != null ? effectConfig.emotionCooldown : 3f;
    }

    private void PlayShortWindowEffects(string fromEmotion, string toEmotion)
    {
        if (effectConfig == null || !emotionSettingsMap.TryGetValue(toEmotion, out var settings)) return;

        if (settings.transitionParticlesPrefab != null)
        {
            SpawnTransitionParticles(settings.transitionParticlesPrefab, settings.particleDuration);
        }

        if (settings.transitionSound != null)
        {
            audioSource.PlayOneShot(settings.transitionSound);
        }

        if (screenFlash != null)
        {
            screenFlash.Flash(settings.screenFlashColor, settings.flashDuration);
        }
    }

    private void SpawnTransitionParticles(GameObject prefab, float duration)    
    {
        if (prefab == null || particleSpawnPoint == null) return;

        GameObject instance = Instantiate(prefab, particleSpawnPoint.position, particleSpawnPoint.rotation);
        Destroy(instance, duration);
    }

    private void ApplyLongWindowEnvironment(string emotion, bool immediate)
    {
        if (effectConfig == null || !emotionSettingsMap.TryGetValue(emotion, out var settings)) return;

        targetSkybox = settings.skybox;
        targetAmbientColor = settings.ambientColor;
        targetLightColor = settings.lightColor;
        targetLightIntensity = settings.lightIntensity;
        targetVolumeProfile = settings.volumeProfile;
        transitionDuration = settings.transitionDuration;

        if (immediate)
        {
            currentSkybox = targetSkybox;
            currentAmbientColor = targetAmbientColor;
            currentLightColor = targetLightColor;
            currentLightIntensity = targetLightIntensity;

            RenderSettings.skybox = currentSkybox;
            RenderSettings.ambientLight = currentAmbientColor;
            if (directionalLight != null)
            {
                directionalLight.color = currentLightColor;
                directionalLight.intensity = currentLightIntensity;
            }

            if (postProcessVolume != null && targetVolumeProfile != null)
            {
                postProcessVolume.profile = targetVolumeProfile;
            }

            UpdateAmbientParticles(settings.ambientParticlesPrefab);
            
            // 立即播放音乐
            if (settings.backgroundMusic != null && audioSource != null)
            {
                audioSource.clip = settings.backgroundMusic;
                audioSource.volume = 1f;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            transitionTimer = 0f;
            isTransitioningInternal = true;
            
            // 开始音乐交叉淡入淡出
            if (settings.backgroundMusic != null && settings.backgroundMusic != audioSource.clip)
            {
                StartCoroutine(CrossfadeMusic(settings.backgroundMusic, transitionDuration));
            }
        }
    }

    // ===== 音乐交叉淡入淡出 =====
    private System.Collections.IEnumerator CrossfadeMusic(AudioClip newClip, float duration)
    {
        if (newClip == null) yield break;
        
        crossfadeAudioSource.clip = newClip;
        crossfadeAudioSource.time = audioSource.time;
        crossfadeAudioSource.volume = 0f;
        crossfadeAudioSource.Play();
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = SmoothStep(elapsed / duration);
            
            audioSource.volume = 1f - t;
            crossfadeAudioSource.volume = t;
            
            yield return null;
        }
        
        // 交换
        var temp = audioSource;
        audioSource = crossfadeAudioSource;
        crossfadeAudioSource = temp;
        
        crossfadeAudioSource.Stop();
        crossfadeAudioSource.clip = null;
        crossfadeAudioSource.volume = 0f;
        audioSource.volume = 1f;
    }

    public void ApplyEmotionEffect(string emotion, bool immediate = false)
    {
        if (effectConfig == null || !emotionSettingsMap.TryGetValue(emotion, out var settings)) return;

        currentEmotion = emotion;
        ApplyLongWindowEnvironment(emotion, immediate);
    }

    private void UpdateAmbientParticles(GameObject prefab)
    {
        if (currentAmbientParticles != null)
        {
            Destroy(currentAmbientParticles);
        }

        if (prefab != null && particleSpawnPoint != null)
        {
            currentAmbientParticles = Instantiate(prefab, particleSpawnPoint.position, particleSpawnPoint.rotation, particleSpawnPoint);
        }
    }

    private void Update()
    {
        // ===== 手动测试（按1/2/3键）=====
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
            {
                Debug.Log("[TEST] 手动切换到 HAPPY");
                ForceTransitionToEmotion("happy");
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
            {
                Debug.Log("[TEST] 手动切换到 SAD");
                ForceTransitionToEmotion("sad");
            }
            else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
            {
                Debug.Log("[TEST] 手动切换到 NORMAL");
                ForceTransitionToEmotion("normal");
            }
        }
        
        // ===== 更新冷却时间 =====
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= Time.deltaTime;
            if (cooldownRemaining < 0f) cooldownRemaining = 0f;
        }
        
        // ===== 检查暂存情绪 =====
        if (pendingEmotion != null && cooldownRemaining <= 0f && !isTransitioningInternal)
        {
            Debug.Log($"[EmotionManager] 🎭 处理暂存情绪: {pendingEmotion}");
            string temp = pendingEmotion;
            pendingEmotion = null;
            ForceTransitionToEmotion(temp);
        }
        
        // ===== 过渡更新 =====
        if (isTransitioningInternal)
        {
            transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTimer / transitionDuration);
            float smoothT = SmoothStep(t);

            if (currentSkybox != targetSkybox)
            {
                if (blendedSkyboxMaterial == null)
                {
                    blendedSkyboxMaterial = new Material(currentSkybox);
                }
                blendedSkyboxMaterial.CopyPropertiesFromMaterial(currentSkybox);
                blendedSkyboxMaterial.Lerp(currentSkybox, targetSkybox, smoothT);
                RenderSettings.skybox = blendedSkyboxMaterial;
                DynamicGI.UpdateEnvironment();
            }

            RenderSettings.ambientLight = Color.Lerp(currentAmbientColor, targetAmbientColor, smoothT);
            
            if (directionalLight != null)
            {
                directionalLight.color = Color.Lerp(currentLightColor, targetLightColor, smoothT);
                directionalLight.intensity = Mathf.Lerp(currentLightIntensity, targetLightIntensity, smoothT);
            }

            if (postProcessVolume != null && targetVolumeProfile != null && t >= 0.5f)
            {
                postProcessVolume.profile = targetVolumeProfile;
            }

            if (t >= 1f)
            {
                currentSkybox = targetSkybox;
                currentAmbientColor = targetAmbientColor;
                currentLightColor = targetLightColor;
                currentLightIntensity = targetLightIntensity;
                isTransitioningInternal = false;
                
                RenderSettings.skybox = currentSkybox;
                DynamicGI.UpdateEnvironment();

                if (emotionSettingsMap.TryGetValue(currentEmotion, out var settings))
                {
                    UpdateAmbientParticles(settings.ambientParticlesPrefab);
                }
                
                Debug.Log($"[EmotionManager] ✅ 过渡完成到 {currentEmotion}");
            }
        }
    }
}
