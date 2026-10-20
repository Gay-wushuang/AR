using UnityEngine;
using UnityEngine.InputSystem;

namespace EEGAI.Demo
{
    /// <summary>Refills the player's existing oxygen supply when used nearby.</summary>
    public class OxygenCanister : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private DemoOxygenSystem oxygenSystem;
        [SerializeField] private DemoGameState gameState;
        [SerializeField, Min(0.1f)] private float interactionDistance = 3f;
        [SerializeField, Min(1f)] private float refillAmount = 30f;
        [SerializeField, Min(0f)] private float reuseDelay = 1f;

        private float nextUseTime;
        private float feedbackUntil;
        private string feedback;

        private void OnEnable()
        {
            if (gameState != null) gameState.OnReset += ResetInteraction;
            ResetInteraction();
        }

        private void OnDisable()
        {
            if (gameState != null) gameState.OnReset -= ResetInteraction;
        }

        private bool CanInteract()
        {
            if (EnergyLinkPuzzle.Active) return false;
            if (!isActiveAndEnabled || player == null || oxygenSystem == null || gameState == null)
                return false;
            if (!oxygenSystem.isActiveAndEnabled || DemoMainMenu.PauseMenuOpen || Time.timeScale <= 0f)
                return false;
            if (gameState.Status != DemoGameState.GameStatus.Playing &&
                gameState.Status != DemoGameState.GameStatus.ReadyToLaunch)
                return false;
            return !oxygenSystem.IsDepleted &&
                (player.position - transform.position).sqrMagnitude <= interactionDistance * interactionDistance;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                TryRefill();
        }

        /// <summary>Attempts one refill; returns false when unavailable or already full.</summary>
        public bool TryRefill()
        {
            if (!CanInteract() || Time.time < nextUseTime) return false;
            if (oxygenSystem.NormalizedOxygen >= 1f)
            {
                feedback = "Oxygen full";
                feedbackUntil = Time.time + 1.5f;
                return false;
            }

            float before = oxygenSystem.CurrentOxygen;
            oxygenSystem.RestoreOxygen(refillAmount);
            float restored = oxygenSystem.CurrentOxygen - before;
            feedback = $"Oxygen +{restored:0}";
            feedbackUntil = Time.time + 1.5f;
            nextUseTime = Time.time + reuseDelay;
            return restored > 0f;
        }

        private void ResetInteraction()
        {
            nextUseTime = 0f;
            feedbackUntil = 0f;
            feedback = null;
        }

        private void OnGUI()
        {
            if (!CanInteract()) return;
            Camera view = Camera.main;
            if (view == null) return;
            Vector3 screen = view.WorldToScreenPoint(transform.position + Vector3.up * 0.7f);
            if (screen.z <= 0f) return;
            string text = Time.time < feedbackUntil ? feedback :
                oxygenSystem.NormalizedOxygen >= 1f ? "Oxygen full" :
                $"[E] Refill oxygen (+{refillAmount:0})";
            GUI.Box(new Rect(Mathf.Clamp(screen.x - 125f, 0f, Mathf.Max(0f, Screen.width - 250f)),
                Mathf.Clamp(Screen.height - screen.y, 0f, Mathf.Max(0f, Screen.height - 36f)), 250f, 36f), text);
        }
    }
}
