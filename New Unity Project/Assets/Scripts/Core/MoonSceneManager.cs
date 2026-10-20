using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MoonSceneManager : MonoBehaviour
{
    [Header("Skybox Settings")]
    [SerializeField] private Material[] skyboxMaterials;
    [SerializeField] private int currentSkyboxIndex = 0;

    [Header("BGM Settings")]
    [SerializeField] private AudioClip[] bgmClips;
    [SerializeField] private int currentBGMIndex = 0;
    [SerializeField] private float bgmVolume = 0.7f;
    [SerializeField] private float fadeDuration = 1.0f;

    [Header("Ambient Settings")]
    [SerializeField] private Color[] ambientColors;

    [Header("Transition Settings")]
    [SerializeField] private float skyboxTransitionDuration = 1.5f;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;
    private Coroutine skyboxTransitionCoroutine;
    private Material currentSkyboxMaterial;
    private Material targetSkyboxMaterial;
    private Color currentAmbientColorValue;
    private Color targetAmbientColorValue;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = bgmVolume;
        audioSource.loop = true;
    }

    void Start()
    {
        if (skyboxMaterials != null && currentSkyboxIndex < skyboxMaterials.Length && skyboxMaterials[currentSkyboxIndex] != null)
        {
            currentSkyboxMaterial = skyboxMaterials[currentSkyboxIndex];
            RenderSettings.skybox = currentSkyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }
        
        if (ambientColors != null && currentSkyboxIndex < ambientColors.Length)
        {
            currentAmbientColorValue = ambientColors[currentSkyboxIndex];
            RenderSettings.ambientSkyColor = currentAmbientColorValue;
        }
        
        PlayBGM(currentBGMIndex);
    }

    public void NextSkybox()
    {
        currentSkyboxIndex = (currentSkyboxIndex + 1) % skyboxMaterials.Length;
        TransitionToSkybox(currentSkyboxIndex);
    }

    public void PreviousSkybox()
    {
        currentSkyboxIndex = (currentSkyboxIndex - 1 + skyboxMaterials.Length) % skyboxMaterials.Length;
        TransitionToSkybox(currentSkyboxIndex);
    }

    public void SetSkybox(int index)
    {
        if (index >= 0 && index < skyboxMaterials.Length && index != currentSkyboxIndex)
        {
            currentSkyboxIndex = index;
            TransitionToSkybox(currentSkyboxIndex);
        }
    }

    public void NextBGM()
    {
        currentBGMIndex = (currentBGMIndex + 1) % bgmClips.Length;
        PlayBGM(currentBGMIndex);
    }

    public void PreviousBGM()
    {
        currentBGMIndex = (currentBGMIndex - 1 + bgmClips.Length) % bgmClips.Length;
        PlayBGM(currentBGMIndex);
    }

    public void SetBGM(int index)
    {
        if (index >= 0 && index < bgmClips.Length)
        {
            currentBGMIndex = index;
            PlayBGM(currentBGMIndex);
        }
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

    private void TransitionToSkybox(int index)
    {
        if (skyboxMaterials == null || index >= skyboxMaterials.Length || skyboxMaterials[index] == null) return;
        
        targetSkyboxMaterial = skyboxMaterials[index];
        
        if (ambientColors != null && index < ambientColors.Length)
        {
            targetAmbientColorValue = ambientColors[index];
        }
        
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
        
        Debug.Log($"Skybox transition: {(startSkybox != null ? startSkybox.name : "null")} -> {targetSkyboxMaterial.name}");
        
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
        Debug.Log($"Skybox transition complete: {currentSkyboxMaterial.name}");
    }

    private void PlayBGM(int index)
    {
        if (bgmClips != null && index < bgmClips.Length && bgmClips[index] != null)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeToBGM(bgmClips[index]));
        }
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

        Debug.Log($"Playing BGM: {newClip.name}");
    }
}