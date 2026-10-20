using UnityEngine;

namespace EEGAI.Demo
{
    public class EmotionGameAdapter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DemoEmotionSimulator emotionSimulator;
        [SerializeField] private EmotionEffectManager emotionEffectManager;
        [SerializeField] private bool forwardToEmotionEffectManager = false;

        [Header("Emotion Colors")]
        [SerializeField] private Color happyLightColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color sadLightColor = new Color(0.3f, 0.4f, 0.8f);
        [SerializeField] private Color normalLightColor = new Color(0.7f, 0.9f, 0.5f);

        [Header("Music")]
        [SerializeField] private bool controlMusic = false;
        [SerializeField] private AudioClip happyMusic;
        [SerializeField] private AudioClip sadMusic;
        [SerializeField] private AudioClip normalMusic;

        [Header("Skyboxes")]
        [SerializeField] private Material happySkybox;
        [SerializeField] private Material sadSkybox;
        [SerializeField] private Material normalSkybox;

        private Light directionalLight;
        private EnvironmentController environmentController;
        private AudioSource audioSource;
        private AudioSource crossfadeSource;
        private Coroutine musicCrossfadeCoroutine;
        private Coroutine lightingCoroutine;

        private void Awake()
        {
            directionalLight = RenderSettings.sun;
            if (directionalLight == null || directionalLight.type != LightType.Directional)
            {
                directionalLight = null;
                foreach (Light candidate in FindObjectsOfType<Light>())
                {
                    if (candidate.type == LightType.Directional && candidate.isActiveAndEnabled &&
                        (directionalLight == null || candidate.intensity > directionalLight.intensity))
                        directionalLight = candidate;
                }
            }
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.loop = true;
            audioSource.volume = 0.5f;
            audioSource.playOnAwake = false;

            crossfadeSource = gameObject.AddComponent<AudioSource>();
            crossfadeSource.loop = true;
            crossfadeSource.volume = 0f;
            crossfadeSource.playOnAwake = false;
        }

        private void Start()
        {
            environmentController = FindObjectOfType<EnvironmentController>();
            if (emotionSimulator == null)
            {
                emotionSimulator = FindAnyObjectByType<DemoEmotionSimulator>();
            }

            if (emotionEffectManager == null)
            {
                emotionEffectManager = FindAnyObjectByType<EmotionEffectManager>();
            }

            if (emotionSimulator != null)
            {
                emotionSimulator.OnEmotionChanged += HandleEmotionChanged;
                ApplyEmotionEffects(emotionSimulator.CurrentEmotion, true);
            }

            if (controlMusic && normalMusic != null)
            {
                audioSource.clip = normalMusic;
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
                crossfadeSource.Stop();
            }
        }

        private void HandleEmotionChanged(DemoEmotionSimulator.EmotionType newEmotion)
        {
            ApplyEmotionEffects(newEmotion, false);
        }

        private void ApplyEmotionEffects(DemoEmotionSimulator.EmotionType emotion, bool immediate)
        {
            string emotionStr = emotion.ToString().ToLower();

            // The same owner handles live EEG and demo emotions, including paused menu transitions.
            if (environmentController != null && environmentController.isActiveAndEnabled)
            {
                environmentController.SetEmotion(emotionStr, immediate);
                return;
            }

            if (forwardToEmotionEffectManager && emotionEffectManager != null)
            {
                emotionEffectManager.ForceTransitionToEmotion(emotionStr);
                return;
            }

            ApplyLighting(emotion, immediate);
            ApplySkybox(emotion, immediate);

            if (controlMusic && !immediate)
            {
                if (musicCrossfadeCoroutine != null)
                {
                    StopCoroutine(musicCrossfadeCoroutine);
                }
                musicCrossfadeCoroutine = StartCoroutine(CrossfadeToMusic(GetMusicForEmotion(emotion), 2f));
            }
        }

        private void ApplyLighting(DemoEmotionSimulator.EmotionType emotion, bool immediate)
        {
            if (directionalLight == null) return;

            Color targetColor = normalLightColor;
            float targetIntensity = 1.5f;

            switch (emotion)
            {
                case DemoEmotionSimulator.EmotionType.Happy:
                    targetColor = happyLightColor;
                    targetIntensity = 1.65f;
                    break;
                case DemoEmotionSimulator.EmotionType.Sad:
                    targetColor = sadLightColor;
                    targetIntensity = 1.275f;
                    break;
                case DemoEmotionSimulator.EmotionType.Normal:
                    targetColor = normalLightColor;
                    targetIntensity = 1.5f;
                    break;
            }

            // Legacy inspector colors are strongly saturated; keep a subtle mood tint for this fallback.
            targetColor = Color.Lerp(Color.white, targetColor, 0.2f);
            if (lightingCoroutine != null)
            {
                StopCoroutine(lightingCoroutine);
                lightingCoroutine = null;
            }

            if (immediate)
            {
                directionalLight.color = targetColor;
                directionalLight.intensity = targetIntensity;
            }
            else
            {
                lightingCoroutine = StartCoroutine(LerpLighting(directionalLight.color, directionalLight.intensity, targetColor, targetIntensity, 1.5f));
            }
        }

        private void ApplySkybox(DemoEmotionSimulator.EmotionType emotion, bool immediate)
        {
            Material targetSkybox = normalSkybox;

            switch (emotion)
            {
                case DemoEmotionSimulator.EmotionType.Happy:
                    targetSkybox = happySkybox;
                    break;
                case DemoEmotionSimulator.EmotionType.Sad:
                    targetSkybox = sadSkybox;
                    break;
                case DemoEmotionSimulator.EmotionType.Normal:
                    targetSkybox = normalSkybox;
                    break;
            }

            if (targetSkybox != null)
            {
                RenderSettings.skybox = targetSkybox;
                DynamicGI.UpdateEnvironment();
            }
        }

        private AudioClip GetMusicForEmotion(DemoEmotionSimulator.EmotionType emotion)
        {
            switch (emotion)
            {
                case DemoEmotionSimulator.EmotionType.Happy:
                    return happyMusic;
                case DemoEmotionSimulator.EmotionType.Sad:
                    return sadMusic;
                default:
                    return normalMusic;
            }
        }

        private System.Collections.IEnumerator LerpLighting(Color startColor, float startIntensity, Color targetColor, float targetIntensity, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);

                directionalLight.color = Color.Lerp(startColor, targetColor, t);
                directionalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);

                yield return null;
            }

            directionalLight.color = targetColor;
            directionalLight.intensity = targetIntensity;
            lightingCoroutine = null;
        }

        private System.Collections.IEnumerator CrossfadeToMusic(AudioClip newClip, float duration)
        {
            if (newClip == null || newClip == audioSource.clip) yield break;

            crossfadeSource.clip = newClip;
            crossfadeSource.time = audioSource.time;
            crossfadeSource.volume = 0f;
            crossfadeSource.Play();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                audioSource.volume = Mathf.Lerp(0.5f, 0f, t);
                crossfadeSource.volume = Mathf.Lerp(0f, 0.5f, t);

                yield return null;
            }

            var temp = audioSource;
            audioSource = crossfadeSource;
            crossfadeSource = temp;

            crossfadeSource.Stop();
            crossfadeSource.volume = 0f;
            audioSource.volume = 0.5f;
            musicCrossfadeCoroutine = null;
        }

        private void OnDestroy()
        {
            if (emotionSimulator != null)
            {
                emotionSimulator.OnEmotionChanged -= HandleEmotionChanged;
            }
        }
    }
}
