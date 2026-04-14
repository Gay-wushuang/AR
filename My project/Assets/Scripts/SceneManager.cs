using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SceneManager : MonoBehaviour
{
    [Header("天空盒设置")]
    [SerializeField] private Material[] skyboxMaterials;
    [SerializeField] private int currentSkyboxIndex = 0;

    [Header("BGM 设置")]
    [SerializeField] private AudioClip[] bgmClips;
    [SerializeField] private int currentBGMIndex = 0;
    [SerializeField] private float bgmVolume = 0.7f;
    [SerializeField] private float fadeDuration = 1.0f;

    [Header("环境光设置")]
    [SerializeField] private Color[] ambientColors;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = bgmVolume;
        audioSource.loop = true;
    }

    void Start()
    {
        ApplySkybox(currentSkyboxIndex);
        ApplyAmbientColor(currentSkyboxIndex);
        PlayBGM(currentBGMIndex);
    }

    public void NextSkybox()
    {
        currentSkyboxIndex = (currentSkyboxIndex + 1) % skyboxMaterials.Length;
        ApplySkybox(currentSkyboxIndex);
        ApplyAmbientColor(currentSkyboxIndex);
    }

    public void PreviousSkybox()
    {
        currentSkyboxIndex = (currentSkyboxIndex - 1 + skyboxMaterials.Length) % skyboxMaterials.Length;
        ApplySkybox(currentSkyboxIndex);
        ApplyAmbientColor(currentSkyboxIndex);
    }

    public void SetSkybox(int index)
    {
        if (index >= 0 && index < skyboxMaterials.Length)
        {
            currentSkyboxIndex = index;
            ApplySkybox(currentSkyboxIndex);
            ApplyAmbientColor(currentSkyboxIndex);
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

    private void ApplySkybox(int index)
    {
        if (skyboxMaterials != null && index < skyboxMaterials.Length && skyboxMaterials[index] != null)
        {
            RenderSettings.skybox = skyboxMaterials[index];
            DynamicGI.UpdateEnvironment();
            Debug.Log($"已应用天空盒: {skyboxMaterials[index].name}");
        }
    }

    private void ApplyAmbientColor(int index)
    {
        if (ambientColors != null && index < ambientColors.Length)
        {
            RenderSettings.ambientSkyColor = ambientColors[index];
            DynamicGI.UpdateEnvironment();
        }
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

        Debug.Log($"正在播放 BGM: {newClip.name}");
    }
}
