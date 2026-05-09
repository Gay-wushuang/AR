using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace EEGAI.Audio.TTS
{
    [Serializable]
    public class TTSConfig
    {
        public string apiKey = "sk-c617d9c68fb04b14a19446c1436c1967";
        public string baseUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1/audio/speech";
        public string model = "cosyvoice-v1";
        public string voice = "zhixiaobai";
        public float speed = 1.0f;
        public float pitch = 1.0f;
        public int sampleRate = 24000;
    }

    public class TTSService : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private TTSConfig config = new TTSConfig();

        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private bool autoPlay = true;
        [SerializeField] private float volume = 1.0f;

        public event Action OnSpeechStart;
        public event Action OnSpeechComplete;
        public event Action<string> OnError;

        private Coroutine currentSpeechCoroutine;
        private bool isSpeaking = false;

        public bool IsSpeaking => isSpeaking;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        public void Speak(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                OnError?.Invoke("Text is empty");
                return;
            }

            if (isSpeaking)
            {
                StopSpeaking();
            }

            currentSpeechCoroutine = StartCoroutine(SpeakCoroutine(text));
        }

        public void StopSpeaking()
        {
            if (currentSpeechCoroutine != null)
            {
                StopCoroutine(currentSpeechCoroutine);
                currentSpeechCoroutine = null;
            }

            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            isSpeaking = false;
            OnSpeechComplete?.Invoke();
        }

        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }

        public void SetSpeed(float newSpeed)
        {
            config.speed = Mathf.Clamp(newSpeed, 0.5f, 2.0f);
        }

        public void SetPitch(float newPitch)
        {
            config.pitch = Mathf.Clamp(newPitch, 0.5f, 2.0f);
        }

        public void SetVoice(string voiceName)
        {
            config.voice = voiceName;
        }

        private IEnumerator SpeakCoroutine(string text)
        {
            isSpeaking = true;
            OnSpeechStart?.Invoke();

            var request = CreateSpeechRequest(text);
            string json = JsonUtility.ToJson(request);

            using (UnityWebRequest www = new UnityWebRequest(config.baseUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerAudioClip(config.baseUrl, AudioType.MPEG);
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", "Bearer " + config.apiKey);

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    if (clip != null)
                    {
                        PlayClip(clip);
                    }
                    else
                    {
                        OnError?.Invoke("Failed to decode audio clip");
                        isSpeaking = false;
                        OnSpeechComplete?.Invoke();
                    }
                }
                else
                {
                    string error = $"TTS Error: {www.error}";
                    Debug.LogError(error);
                    OnError?.Invoke(error);
                    isSpeaking = false;
                    OnSpeechComplete?.Invoke();
                }
            }
        }

        private TTSSpeechRequest CreateSpeechRequest(string text)
        {
            return new TTSSpeechRequest
            {
                model = config.model,
                input = new TTSInput { text = text },
                voice = new TTSVoice { voice = config.voice },
                audio_config = new TTSAudioConfig
                {
                    sample_rate = config.sampleRate,
                    speed = config.speed,
                    pitch = config.pitch,
                    format = "mp3"
                }
            };
        }

        private void PlayClip(AudioClip clip)
        {
            if (audioSource != null)
            {
                audioSource.clip = clip;
                audioSource.volume = volume;
                audioSource.Play();

                StartCoroutine(WaitForClipEnd(clip));
            }
        }

        private IEnumerator WaitForClipEnd(AudioClip clip)
        {
            yield return new WaitForSeconds(clip.length);
            isSpeaking = false;
            OnSpeechComplete?.Invoke();
        }
    }

    [Serializable]
    public class TTSSpeechRequest
    {
        public string model;
        public TTSInput input;
        public TTSVoice voice;
        public TTSAudioConfig audio_config;
    }

    [Serializable]
    public class TTSInput
    {
        public string text;
    }

    [Serializable]
    public class TTSVoice
    {
        public string voice;
    }

    [Serializable]
    public class TTSAudioConfig
    {
        public int sample_rate;
        public float speed;
        public float pitch;
        public string format;
    }
}
