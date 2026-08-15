using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FOG.EscapeTheLava
{
    public sealed class EndScreenView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup = null;
        [SerializeField] private RectTransform panel = null;
        [SerializeField] private Text titleText = null;
        [SerializeField] private Text bodyText = null;
        [SerializeField] private Button retryButton = null;
        [SerializeField] private Button nextLevelButton = null;

        private Coroutine _showRoutine;
        public event Action RetryRequested;
        public event Action NextLevelRequested;

        public void Initialize()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(HandleRetryClicked);
                retryButton.onClick.AddListener(HandleRetryClicked);
            }

            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.RemoveListener(HandleNextLevelClicked);
                nextLevelButton.onClick.AddListener(HandleNextLevelClicked);
            }

            gameObject.SetActive(false);
        }

        public void HideImmediate()
        {
            if (_showRoutine != null)
            {
                StopCoroutine(_showRoutine);
                _showRoutine = null;
            }

            gameObject.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        public void Show(RoundResult result, bool hasNextLevel)
        {
            gameObject.SetActive(true);

            if (titleText != null)
            {
                if (result.Won)
                {
                    titleText.text = "All Diamonds Collected!";
                    titleText.color = new Color(0.54f, 0.96f, 1f, 1f);
                }
                else
                {
                    titleText.text = result.Reason == RoundEndReason.TimeExpired ? "Out of Time" : "No Lives Left";
                    titleText.color = new Color(1f, 0.43f, 0.25f, 1f);
                }
            }

            if (bodyText != null)
            {
                bodyText.text = $"Score: {result.Score}\nDiamonds: {result.DiamondsCollected}/{result.TotalDiamonds}\nTime Left: {Mathf.CeilToInt(Mathf.Max(0f, result.TimeRemaining))}s";
            }

            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(result.Won && hasNextLevel);
            }

            if (retryButton != null)
            {
                // Hide Retry once a Next Level button is available, so a win only offers one obvious action.
                retryButton.gameObject.SetActive(!result.Won || !hasNextLevel);
            }

            if (_showRoutine != null)
            {
                StopCoroutine(_showRoutine);
            }

            _showRoutine = StartCoroutine(ShowRoutine());
        }

        private void HandleRetryClicked()
        {
            RetryRequested?.Invoke();
        }

        private void HandleNextLevelClicked()
        {
            NextLevelRequested?.Invoke();
        }

        private IEnumerator ShowRoutine()
        {
            if (canvasGroup == null || panel == null) yield break;

            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            var elapsed = 0f;
            const float duration = 0.42f;
            panel.localScale = Vector3.one * 0.82f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var normalized = Mathf.Clamp01(elapsed / duration);
                var eased = 1f - Mathf.Pow(1f - normalized, 3f);
                canvasGroup.alpha = eased;
                panel.localScale = Vector3.one * Mathf.Lerp(0.82f, 1f, eased);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            panel.localScale = Vector3.one;
            _showRoutine = null;
        }
    }
}
