using System.Collections;
using UnityEngine;

namespace FOG.EscapeTheLava
{
    public sealed class TileAnimator : MonoBehaviour
    {
        private const float LavaPulseSpeed = 5.5f;
        private const float LavaPulseAmplitudeRatio = 0.025f;
        private const float LavaHeatCycleSpeed = 4.7f;
        private const float LavaHeatIntensity = 0.32f;

        private const float DiamondBobSpeed = 3.2f;
        private const float DiamondBobAmplitude = 0.08f;
        private const float DiamondShineSpeed = 5.4f;
        private const float DiamondShineScaleAmplitude = 0.08f;
        private const float DiamondShineColorBlend = 0.45f;

        private const float IslandGlowSpeed = 2.2f;
        private const float IslandGlowIntensity = 0.035f;

        private TileType _tileType;
        private SpriteRenderer _baseRenderer;
        private SpriteRenderer _iconRenderer;
        private Vector3 _iconStartPosition;
        private Vector3 _iconStartScale;
        private float _baseScale = 1f;
        private Color _baseColor;
        private float _phase;
        private float _impactPulse;
        private Coroutine _impactRoutine;

        public void Initialize(TileType type, SpriteRenderer tileBase, SpriteRenderer icon, float tileSize)
        {
            _tileType = type;
            _baseRenderer = tileBase;
            _iconRenderer = icon;
            _baseScale = tileSize;
            _baseColor = _baseRenderer != null ? _baseRenderer.color : Color.white;
            _phase = Random.Range(0f, 10f);
            _impactPulse = 0f;

            if (_iconRenderer == null) return;
            _iconStartPosition = _iconRenderer.transform.localPosition;
            _iconStartScale = _iconRenderer.transform.localScale;
        }

        public void TriggerImpactPulse()
        {
            StartPulse(0.2f, 0.16f * _baseScale);
        }

        public void TriggerSoftPulse()
        {
            StartPulse(0.14f, 0.07f * _baseScale);
        }

        private void Update()
        {
            var animationTime = Time.time + _phase;
            var lavaPulse = _tileType == TileType.Lava
                ? Mathf.Sin(animationTime * LavaPulseSpeed) * (LavaPulseAmplitudeRatio * _baseScale)
                : 0f;
            transform.localScale = Vector3.one * (_baseScale + lavaPulse + _impactPulse);

            if (_baseRenderer == null)
            {
                return;
            }

            if (_tileType == TileType.Lava)
            {
                var heat = (Mathf.Sin(animationTime * LavaHeatCycleSpeed) + 1f) * 0.5f;
                var hotColor = new Color(1f, 0.38f, 0.04f, 1f);
                _baseRenderer.color = Color.Lerp(_baseColor, hotColor, heat * LavaHeatIntensity);
                return;
            }

            if (_tileType == TileType.Diamond && _iconRenderer != null && _iconRenderer.enabled)
            {
                var bob = Mathf.Sin(animationTime * DiamondBobSpeed) * DiamondBobAmplitude;
                var shine = (Mathf.Sin(animationTime * DiamondShineSpeed) + 1f) * 0.5f;
                _iconRenderer.transform.localPosition = _iconStartPosition + new Vector3(0f, bob, 0f);
                _iconRenderer.transform.localScale = _iconStartScale * (1f + shine * DiamondShineScaleAmplitude);
                _iconRenderer.color = Color.Lerp(new Color(0.74f, 0.95f, 1f, 1f), Color.white, shine * DiamondShineColorBlend);
                return;
            }

            var islandGlow = (Mathf.Sin(animationTime * IslandGlowSpeed) + 1f) * 0.5f;
            _baseRenderer.color = Color.Lerp(_baseColor, Color.white, islandGlow * IslandGlowIntensity);
        }

        private void StartPulse(float duration, float strength)
        {
            if (_impactRoutine != null)
            {
                StopCoroutine(_impactRoutine);
            }

            _impactRoutine = StartCoroutine(PulseRoutine(duration, strength));
        }

        private IEnumerator PulseRoutine(float duration, float strength)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var normalized = Mathf.Clamp01(elapsed / duration);
                _impactPulse = Mathf.Sin(normalized * Mathf.PI) * strength;
                yield return null;
            }

            _impactPulse = 0f;
            _impactRoutine = null;
        }
    }
}
