using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EEGAI.Game.Memory;

namespace EEGAI.UI
{
    public class MemoryUI : MonoBehaviour
    {
        [SerializeField] private Transform memoryContainer;
        [SerializeField] private GameObject memoryFragmentPrefab;
        [SerializeField] private Slider memoryProgressBar;
        [SerializeField] private Text progressText;

        [Header("Animation")]
        [SerializeField] private float unlockAnimationDuration = 1.5f;
        [SerializeField] private AnimationCurve unlockCurve;

        [Header("Events")]
        [SerializeField] private GameObject memoryUnlockNotification;

        private MemorySystem memorySystem;
        private List<MemoryFragment> displayedFragments = new List<MemoryFragment>();

        private void Awake()
        {
            memorySystem = FindAnyObjectByType<MemorySystem>();
        }

        private void Start()
        {
            if (memorySystem != null)
            {
                memorySystem.OnMemoryUnlocked += ShowMemoryFragment;
                memorySystem.OnMemoryRecoveryComplete += OnMemoryRecoveryComplete;
                UpdateProgressUI();
            }
        }

        private void OnDestroy()
        {
            if (memorySystem != null)
            {
                memorySystem.OnMemoryUnlocked -= ShowMemoryFragment;
                memorySystem.OnMemoryRecoveryComplete -= OnMemoryRecoveryComplete;
            }
        }

        public void ShowMemoryFragment(MemoryFragment fragment)
        {
            if (displayedFragments.Contains(fragment))
                return;

            displayedFragments.Add(fragment);

            var fragmentUI = Instantiate(memoryFragmentPrefab, memoryContainer);
            SetFragmentUI(fragmentUI, fragment);

            StartCoroutine(UnlockAnimation(fragmentUI.transform));
            ShowUnlockNotification(fragment);
            UpdateProgressUI();
        }

        public void UpdateProgressUI()
        {
            if (memorySystem == null) return;

            var progress = memorySystem.Progress;

            if (memoryProgressBar != null)
            {
                memoryProgressBar.value = progress.currentProgress / 100f;
            }

            if (progressText != null)
            {
                progressText.text = $"记忆恢复: {progress.unlockedFragments}/{progress.totalFragments} ({progress.currentProgress:F0}%)";
            }
        }

        private void SetFragmentUI(GameObject fragmentObj, MemoryFragment fragment)
        {
            var titleText = fragmentObj.transform.Find("Title")?.GetComponent<Text>();
            var contentText = fragmentObj.transform.Find("Content")?.GetComponent<Text>();
            var categoryText = fragmentObj.transform.Find("Category")?.GetComponent<Text>();

            if (titleText != null) titleText.text = fragment.title;
            if (contentText != null) contentText.text = fragment.content;
            if (categoryText != null) categoryText.text = fragment.category;
        }

        private IEnumerator UnlockAnimation(Transform fragmentTransform)
        {
            Vector3 startScale = Vector3.zero;
            Vector3 endScale = Vector3.one;
            float elapsed = 0f;

            fragmentTransform.localScale = startScale;

            while (elapsed < unlockAnimationDuration)
            {
                float t = unlockCurve.Evaluate(elapsed / unlockAnimationDuration);
                fragmentTransform.localScale = Vector3.Lerp(startScale, endScale, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            fragmentTransform.localScale = endScale;
        }

        private void ShowUnlockNotification(MemoryFragment fragment)
        {
            if (memoryUnlockNotification != null)
            {
                var notification = Instantiate(memoryUnlockNotification, transform.parent);
                var notificationText = notification.GetComponentInChildren<Text>();
                if (notificationText != null)
                {
                    notificationText.text = $"记忆解锁: {fragment.title}";
                }
                Destroy(notification, 3f);
            }
        }

        private void OnMemoryRecoveryComplete()
        {
            Debug.Log("🎉 所有记忆已恢复！");
            if (progressText != null)
            {
                progressText.text = "记忆恢复完成！";
            }
        }
    }
}
