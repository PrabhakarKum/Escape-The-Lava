using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FOG.EscapeTheLava
{
    public sealed class FloatingTextSpawner : MonoBehaviour
    {
        [SerializeField] private RectTransform textTemplate = null;
        [SerializeField] private RectTransform root = null;

        private GameConfig _config;
        private readonly Queue<TextMeshProUGUI> _textPool = new();

        public void Initialize(GameConfig gameConfig)
        {
            _config = gameConfig;
            
            if (root == null)
            {
                root = GetComponent<RectTransform>();
            }

            if (textTemplate != null)
            {
                textTemplate.gameObject.SetActive(false);
            }
        }

        public void Spawn(string value, Vector2 screenPosition, Color color)
        {
            if (root == null || textTemplate == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screenPosition, null, out var localPosition);

            var text = GetText();
            var rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = localPosition;
            rect.sizeDelta = new Vector2(120f, 46f);
            text.raycastTarget = false;
            text.text = value;
            text.color = color;
            text.gameObject.SetActive(true);
            rect.localScale = Vector3.one * 0.85f;

            StartCoroutine(Animate(text, rect));
        }

        private TextMeshProUGUI GetText()
        {
            while (_textPool.Count > 0)
            {
                var pooled = _textPool.Dequeue();
                if (pooled != null)
                {
                    return pooled;
                }
            }

            var rect = Instantiate(textTemplate, root, false);
            rect.gameObject.name = "Floating Score";
            rect.gameObject.SetActive(false);
            return rect.GetComponent<TextMeshProUGUI>();
        }

        private IEnumerator Animate(TextMeshProUGUI text, RectTransform rect)
        {
            var elapsed = 0f;
            var duration = _config != null ? _config.floatingTextDuration : 0.85f;
            var start = rect.anchoredPosition;
            var startScale = Vector3.one * 0.85f;
            var startColor = text.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var normalized = Mathf.Clamp01(elapsed / duration);
                var eased = 1f - Mathf.Pow(1f - normalized, 2f);
                rect.anchoredPosition = start + new Vector2(0f, Mathf.Lerp(0f, 76f, eased));
                rect.localScale = Vector3.Lerp(startScale, Vector3.one * 1.15f, Mathf.Sin(normalized * Mathf.PI));
                text.color = new Color(startColor.r, startColor.g, startColor.b, 1f - normalized);
                yield return null;
            }

            rect.gameObject.SetActive(false);
            rect.SetParent(root, false);
            _textPool.Enqueue(text);
        }
    }
}
