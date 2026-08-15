using System.Collections;
using UnityEngine;

namespace FOG.EscapeTheLava
{
    public sealed class TileIdleAnimator : MonoBehaviour
    {
        private TileType tileType;
        private SpriteRenderer baseRenderer;
        private SpriteRenderer iconRenderer;
        private Vector3 iconStartPosition;
        private Vector3 iconStartScale;
        private float baseScale = 1f;
        private Color baseColor;
        private float phase;
        private float impactPulse;
        private Coroutine impactRoutine;

        public void Initialize(TileType type, SpriteRenderer tileBase, SpriteRenderer icon, float tileSize)
        {
            tileType = type;
            baseRenderer = tileBase;
            iconRenderer = icon;
            baseScale = tileSize;
            baseColor = baseRenderer != null ? baseRenderer.color : Color.white;
            phase = Random.Range(0f, 10f);
            impactPulse = 0f;

            if (iconRenderer != null)
            {
                iconStartPosition = iconRenderer.transform.localPosition;
                iconStartScale = iconRenderer.transform.localScale;
            }
        }

        public void TriggerImpactPulse()
        {
            StartPulse(0.2f, 0.16f * baseScale);
        }

        public void TriggerSoftPulse()
        {
            StartPulse(0.14f, 0.07f * baseScale);
        }

        private void Update()
        {
            float t = Time.time + phase;
            float lavaPulse = tileType == TileType.Lava ? Mathf.Sin(t * 5.5f) * (0.025f * baseScale) : 0f;
            transform.localScale = Vector3.one * (baseScale + lavaPulse + impactPulse);

            if (baseRenderer == null)
            {
                return;
            }

            if (tileType == TileType.Lava)
            {
                float heat = (Mathf.Sin(t * 4.7f) + 1f) * 0.5f;
                Color hot = new Color(1f, 0.38f, 0.04f, 1f);
                baseRenderer.color = Color.Lerp(baseColor, hot, heat * 0.32f);
                return;
            }

            if (tileType == TileType.Diamond && iconRenderer != null && iconRenderer.enabled)
            {
                float bob = Mathf.Sin(t * 3.2f) * 0.08f;
                float shine = (Mathf.Sin(t * 5.4f) + 1f) * 0.5f;
                iconRenderer.transform.localPosition = iconStartPosition + new Vector3(0f, bob, 0f);
                iconRenderer.transform.localScale = iconStartScale * (1f + shine * 0.08f);
                iconRenderer.color = Color.Lerp(new Color(0.74f, 0.95f, 1f, 1f), Color.white, shine * 0.45f);
                return;
            }

            float islandGlow = (Mathf.Sin(t * 2.2f) + 1f) * 0.5f;
            baseRenderer.color = Color.Lerp(baseColor, Color.white, islandGlow * 0.035f);
        }

        private void StartPulse(float duration, float strength)
        {
            if (impactRoutine != null)
            {
                StopCoroutine(impactRoutine);
            }

            impactRoutine = StartCoroutine(PulseRoutine(duration, strength));
        }

        private IEnumerator PulseRoutine(float duration, float strength)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                impactPulse = Mathf.Sin(normalized * Mathf.PI) * strength;
                yield return null;
            }

            impactPulse = 0f;
            impactRoutine = null;
        }
    }
}
