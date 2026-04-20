using UnityEngine;
using EEGAI.AI.LLM;
using EEGAI.AI.Emotion;
using EEGAI.AI.Dialogue;
using EEGAI.Game.Memory;

namespace EEGAI.Game
{
    public class GameManager : MonoBehaviour
    {
        [Header("Core Services")]
        [SerializeField] private QwenService llmService;
        [SerializeField] private EmotionAnalyzer emotionAnalyzer;
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private MemorySystem memorySystem;

        [Header("Game State")]
        [SerializeField] private bool isGameStarted = false;
        [SerializeField] private bool isGamePaused = false;

        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SetupComponents();
        }

        private void Start()
        {
            StartGame();
        }

        private void SetupComponents()
        {
            if (llmService == null)
            {
                llmService = FindAnyObjectByType<QwenService>();
                if (llmService == null)
                {
                    llmService = gameObject.AddComponent<QwenService>();
                }
            }

            if (emotionAnalyzer == null)
            {
                emotionAnalyzer = FindAnyObjectByType<EmotionAnalyzer>();
                if (emotionAnalyzer == null)
                {
                    emotionAnalyzer = gameObject.AddComponent<EmotionAnalyzer>();
                }
            }

            if (dialogueManager == null)
            {
                dialogueManager = FindAnyObjectByType<DialogueManager>();
                if (dialogueManager == null)
                {
                    dialogueManager = gameObject.AddComponent<DialogueManager>();
                }
            }

            if (memorySystem == null)
            {
                memorySystem = FindAnyObjectByType<MemorySystem>();
                if (memorySystem == null)
                {
                    memorySystem = gameObject.AddComponent<MemorySystem>();
                }
            }
        }

        public void StartGame()
        {
            if (isGameStarted) return;

            isGameStarted = true;
            isGamePaused = false;

            Debug.Log("🚀 月球幸存者游戏开始！");
        }

        public void PauseGame()
        {
            if (!isGameStarted) return;

            isGamePaused = true;
            Time.timeScale = 0f;

            Debug.Log("⏸️ 游戏暂停");
        }

        public void ResumeGame()
        {
            if (!isGameStarted) return;

            isGamePaused = false;
            Time.timeScale = 1f;

            Debug.Log("▶️ 游戏继续");
        }

        public void EndGame()
        {
            if (!isGameStarted) return;

            isGameStarted = false;
            Time.timeScale = 1f;

            memorySystem?.SaveProgress();

            Debug.Log("🎮 游戏结束！");
        }

        public bool IsGameActive()
        {
            return isGameStarted && !isGamePaused;
        }

        public bool IsGamePaused()
        {
            return isGamePaused;
        }

        public QwenService GetLLMService()
        {
            return llmService;
        }

        public EmotionAnalyzer GetEmotionAnalyzer()
        {
            return emotionAnalyzer;
        }

        public DialogueManager GetDialogueManager()
        {
            return dialogueManager;
        }

        public MemorySystem GetMemorySystem()
        {
            return memorySystem;
        }
    }
}
