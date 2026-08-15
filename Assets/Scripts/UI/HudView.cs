using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FOG.EscapeTheLava
{
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private RectTransform heartsRoot = null;
        [SerializeField] private RectTransform heartTemplate = null;
        [SerializeField] private Text timerText = null;
        [SerializeField] private Text scoreText = null;
        [SerializeField] private Text levelNameText = null;
        
        [Header("Sprites")]
        [SerializeField] private Sprite heartFullSprite = null;
        [SerializeField] private Sprite heartEmptySprite = null;

        private readonly List<Image> hearts = new();
        private int previousLives = -1;

        public void Initialize(GameConfig config)
        {
            hearts.Clear();
            ClearGeneratedHearts();
            int heartCount = Mathf.Max(1, config.StartingLives);
            
            // Generate heart slots based on starting lives
            for (int i = 0; i < heartCount; i++)
            {
                RectTransform heartRect = Instantiate(heartTemplate, heartsRoot, false);
                heartRect.gameObject.name = $"Heart {i + 1}";
                heartRect.gameObject.SetActive(true);
                
                // Keep the hardcoded spacing or assume user uses a HorizontalLayoutGroup
                // We'll leave anchoredPosition here for fallback
                heartRect.anchoredPosition = new Vector2(22f + i * 43f, 0f);
                
                Image heart = heartRect.GetComponent<Image>();
                hearts.Add(heart);
            }

            previousLives = config.StartingLives;
        }

        private void ClearGeneratedHearts()
        {
            if (heartsRoot == null || heartTemplate == null) return;

            for (int i = heartsRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = heartsRoot.GetChild(i);
                if (child != heartTemplate)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        public void SetTimer(float secondsRemaining)
        {
            if (timerText == null) return;
            
            int seconds = Mathf.CeilToInt(Mathf.Max(0f, secondsRemaining));
            timerText.text = seconds.ToString("00");

            if (seconds <= 10)
            {
                float pulse = (Mathf.Sin(Time.time * 9f) + 1f) * 0.5f;
                timerText.color = Color.Lerp(new Color(1f, 0.23f, 0.15f, 1f), new Color(1f, 0.84f, 0.22f, 1f), pulse);
                timerText.rectTransform.localScale = Vector3.one * (1f + pulse * 0.08f);
            }
            else
            {
                timerText.color = Color.white;
                timerText.rectTransform.localScale = Vector3.one;
            }
        }

        public void SetLives(int livesRemaining)
        {
            int clamped = Mathf.Clamp(livesRemaining, 0, hearts.Count);
            for (int i = 0; i < hearts.Count; i++)
            {
                hearts[i].sprite = i < clamped ? heartFullSprite : heartEmptySprite;
                hearts[i].color = i < clamped ? Color.white : new Color(1f, 1f, 1f, 0.55f);
            }

            if (previousLives >= 0 && clamped < previousLives)
            {
                int lostIndex = Mathf.Clamp(clamped, 0, hearts.Count - 1);
                StartCoroutine(HeartLossRoutine(hearts[lostIndex].rectTransform));
            }

            previousLives = clamped;
        }

        public void SetScore(int score, int diamondsCollected, int totalDiamonds)
        {
            if (scoreText == null) return;
            scoreText.text = $"Score {score}  ·  Diamonds {diamondsCollected}/{totalDiamonds}";
        }

        public void SetLevelName(string name)
        {
            if (levelNameText != null)
            {
                levelNameText.text = name;
            }
        }

        private static IEnumerator HeartLossRoutine(RectTransform heart)
        {
            float elapsed = 0f;
            const float duration = 0.24f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                float pop = Mathf.Sin(normalized * Mathf.PI) * 0.28f;
                heart.localScale = Vector3.one * (1f + pop);
                heart.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(normalized * Mathf.PI * 2f) * 9f);
                yield return null;
            }

            heart.localScale = Vector3.one;
            heart.localRotation = Quaternion.identity;
        }
    }
}
