using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace EEGAI.Demo
{
    public class DemoHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DemoGameState gameState;
        [SerializeField] private DemoEmotionSimulator emotionSimulator;
        [SerializeField] private DemoAttentionSystem attentionSystem;
        [SerializeField] private DemoMeditationSystem meditationSystem;
        [SerializeField] private DemoOxygenSystem oxygenSystem;
        [SerializeField] private DemoRepairSystem repairSystem;

        [Header("UI Elements")]
        [SerializeField] private Text emotionText;
        [SerializeField] private Text repairProgressText;
        [SerializeField] private Slider repairProgressSlider;
        [SerializeField] private Image emotionIndicator;
        
        [Header("Status Bars")]
        [SerializeField] private Slider attentionSlider;
        [SerializeField] private Text attentionText;
        [SerializeField] private Slider meditationSlider;
        [SerializeField] private Text meditationText;
        [SerializeField] private Slider oxygenSlider;
        [SerializeField] private Text oxygenText;

        [Header("Panels")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private Text victoryText;
        [SerializeField] private Button victoryRestartButton;
        
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text gameOverText;
        [SerializeField] private Button gameOverRestartButton;

        [Header("Emotion Colors")]
        [SerializeField] private Color happyColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color sadColor = new Color(0.3f, 0.4f, 0.8f);
        [SerializeField] private Color normalColor = new Color(0.7f, 0.9f, 0.5f);

        private void Awake()
        {
            if (victoryRestartButton != null)
            {
                victoryRestartButton.onClick.AddListener(ReturnToMenu);
            }

            if (gameOverRestartButton != null)
            {
                gameOverRestartButton.onClick.AddListener(ReturnToMenu);
            }

            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        private void Start()
        {
            FindMissingReferences();

            if (emotionSimulator != null)
            {
                emotionSimulator.OnEmotionChanged += UpdateEmotionUI;
                UpdateEmotionUI(emotionSimulator.CurrentEmotion);
            }

            if (attentionSystem != null)
            {
                attentionSystem.OnAttentionChanged += UpdateAttentionUI;
                UpdateAttentionUI(attentionSystem.CurrentAttention);
            }

            if (meditationSystem != null)
            {
                meditationSystem.OnMeditationChanged += UpdateMeditationUI;
                UpdateMeditationUI(meditationSystem.CurrentMeditation);
            }

            if (oxygenSystem != null)
            {
                oxygenSystem.OnOxygenChanged += UpdateOxygenUI;
                UpdateOxygenUI(oxygenSystem.CurrentOxygen);
            }

            if (repairSystem != null)
            {
                repairSystem.OnProgressChanged += UpdateRepairUI;
                repairSystem.OnNodeRepaired += UpdateRepairNodeUI;
                UpdateRepairUI(repairSystem.CurrentProgress);
            }

            if (gameState != null)
            {
                gameState.OnVictory += ShowVictory;
                gameState.OnGameOver += ShowGameOver;
                gameState.OnReset += HideEndPanels;
            }
        }

        private void FindMissingReferences()
        {
            if (gameState == null) gameState = FindAnyObjectByType<DemoGameState>();
            if (emotionSimulator == null) emotionSimulator = FindAnyObjectByType<DemoEmotionSimulator>();
            if (attentionSystem == null) attentionSystem = FindAnyObjectByType<DemoAttentionSystem>();
            if (meditationSystem == null) meditationSystem = FindAnyObjectByType<DemoMeditationSystem>();
            if (oxygenSystem == null) oxygenSystem = FindAnyObjectByType<DemoOxygenSystem>();
            if (repairSystem == null) repairSystem = FindAnyObjectByType<DemoRepairSystem>();
        }

        private void UpdateEmotionUI(DemoEmotionSimulator.EmotionType emotion)
        {
            string emotionName = emotion.ToString().ToUpper();
            Color emotionColor = normalColor;

            switch (emotion)
            {
                case DemoEmotionSimulator.EmotionType.Happy:
                    emotionColor = happyColor;
                    emotionName = "😊 HAPPY";
                    break;
                case DemoEmotionSimulator.EmotionType.Sad:
                    emotionColor = sadColor;
                    emotionName = "😢 SAD";
                    break;
                case DemoEmotionSimulator.EmotionType.Normal:
                    emotionColor = normalColor;
                    emotionName = "😌 NORMAL";
                    break;
            }

            if (emotionText != null)
            {
                emotionText.text = emotionName;
                emotionText.color = emotionColor;
            }

            if (emotionIndicator != null)
            {
                emotionIndicator.color = emotionColor;
            }
        }

        private void UpdateAttentionUI(float value)
        {
            if (attentionSlider != null)
            {
                attentionSlider.value = value / 100f;
            }

            if (attentionText != null)
            {
                attentionText.text = $"🎯 {value:F0}";
            }
        }

        private void UpdateMeditationUI(float value)
        {
            if (meditationSlider != null)
            {
                meditationSlider.value = value / 100f;
            }

            if (meditationText != null)
            {
                meditationText.text = $"🧘 {value:F0}";
            }
        }

        private void UpdateOxygenUI(float value)
        {
            if (oxygenSlider != null)
            {
                oxygenSlider.value = value / 100f;
                oxygenSlider.fillRect.GetComponent<Image>().color = 
                    value > 50 ? Color.green : (value > 25 ? Color.yellow : Color.red);
            }

            if (oxygenText != null)
            {
                oxygenText.text = $"💨 {value:F0}";
            }
        }

        private void UpdateRepairUI(float progress)
        {
            if (repairSystem == null) return;

            if (repairProgressSlider != null)
            {
                repairProgressSlider.value = repairSystem.OverallProgress / 100f;
            }

            UpdateRepairText();
        }

        private void UpdateRepairNodeUI(int count)
        {
            UpdateRepairText();
        }

        private void UpdateRepairText()
        {
            if (repairSystem == null || repairProgressText == null) return;
            repairProgressText.text = $"🔧 {repairSystem.RepairedNodes}/{repairSystem.TotalNodes}";
        }

        private void ShowVictory()
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }

            if (victoryText != null)
            {
                victoryText.text = "🎉 VICTORY!\n\nYou have repaired all systems\nand saved the ship!";
            }
        }

        private void ShowGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (gameOverText != null)
            {
                gameOverText.text = "💀 GAME OVER\n\nOxygen depleted!\n\nTry again!";
            }
        }

        private void ReturnToMenu()
        {
            HideEndPanels();

            if (gameState != null)
            {
                gameState.ResetGame();
            }
        }

        private void HideEndPanels()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        private void Update()
        {
            if (gameState == null ||
                gameState.Status == DemoGameState.GameStatus.MainMenu ||
                gameState.Status == DemoGameState.GameStatus.Playing ||
                gameState.Status == DemoGameState.GameStatus.ReadyToLaunch ||
                gameState.Status == DemoGameState.GameStatus.Launching)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                ReturnToMenu();
            }
        }

        private void OnGUI()
        {
            if (gameState == null ||
                gameState.Status == DemoGameState.GameStatus.MainMenu ||
                gameState.Status == DemoGameState.GameStatus.Playing ||
                gameState.Status == DemoGameState.GameStatus.ReadyToLaunch ||
                gameState.Status == DemoGameState.GameStatus.Launching)
            {
                return;
            }

            Rect rect = new Rect(Screen.width * 0.5f - 220f, Screen.height * 0.5f - 110f, 440f, 220f);
            GUI.color = new Color(0f, 0f, 0f, 0.78f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;

            GUILayout.BeginArea(rect);
            GUILayout.FlexibleSpace();

            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 34,
                fontStyle = FontStyle.Bold
            };

            var bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18
            };

            if (gameState.Status == DemoGameState.GameStatus.Victory)
            {
                GUI.color = new Color(0.3f, 1f, 0.55f, 1f);
                GUILayout.Label("VICTORY", style);
                GUI.color = Color.white;
                GUILayout.Label("All systems repaired. Ship launch restored.", bodyStyle);
            }
            else
            {
                GUI.color = new Color(1f, 0.25f, 0.15f, 1f);
                GUILayout.Label("MISSION FAILED", style);
                GUI.color = Color.white;
                GUILayout.Label("Oxygen depleted before repairs were completed.", bodyStyle);
            }

            GUILayout.Space(18);
            GUILayout.Label("Press R to return to menu", bodyStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            if (emotionSimulator != null) emotionSimulator.OnEmotionChanged -= UpdateEmotionUI;
            if (attentionSystem != null) attentionSystem.OnAttentionChanged -= UpdateAttentionUI;
            if (meditationSystem != null) meditationSystem.OnMeditationChanged -= UpdateMeditationUI;
            if (oxygenSystem != null) oxygenSystem.OnOxygenChanged -= UpdateOxygenUI;
            if (repairSystem != null)
            {
                repairSystem.OnProgressChanged -= UpdateRepairUI;
                repairSystem.OnNodeRepaired -= UpdateRepairNodeUI;
            }
            if (gameState != null)
            {
                gameState.OnVictory -= ShowVictory;
                gameState.OnGameOver -= ShowGameOver;
                gameState.OnReset -= HideEndPanels;
            }

            if (victoryRestartButton != null) victoryRestartButton.onClick.RemoveListener(ReturnToMenu);
            if (gameOverRestartButton != null) gameOverRestartButton.onClick.RemoveListener(ReturnToMenu);
        }
    }
}
