using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FOG.EscapeTheLava
{
    /// <summary>
    /// Full-screen red vignette that flashes at the screen edges when lava is hit — the UI-space
    /// counterpart to CameraShake. The vignette gradient is baked into a texture once in
    /// Initialize() rather than recomputed on every flash.
    /// </summary>
    public sealed class DamageVignetteView : MonoBehaviour
    {
        [SerializeField] private Image vignetteImage = null;
        [SerializeField] private Color flashColor = new(0.82f, 0.08f, 0.04f, 1f);
        [SerializeField] private int textureResolution = 256;
        [SerializeField] [Range(0f, 1f)] private float innerRadius = 0.35f;

        private Coroutine _flashRoutine;

        public void Initialize()
        {
            if (vignetteImage == null)
            {
                vignetteImage = GetComponent<Image>();
            }

            if (vignetteImage == null)
            {
                return;
            }

            vignetteImage.sprite = BuildVignetteSprite();
            vignetteImage.raycastTarget = false;
            SetAlpha(0f);
        }

        public void Flash(float duration, float peakAlpha)
        {
            if (vignetteImage == null || duration <= 0f)
            {
                return;
            }

            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
            }

            _flashRoutine = StartCoroutine(FlashRoutine(duration, peakAlpha));
        }

        private IEnumerator FlashRoutine(float duration, float peakAlpha)
        {
            SetAlpha(peakAlpha);

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var normalized = Mathf.Clamp01(elapsed / duration);
                SetAlpha(Mathf.Lerp(peakAlpha, 0f, normalized));
                yield return null;
            }

            SetAlpha(0f);
            _flashRoutine = null;
        }

        private void SetAlpha(float alpha)
        {
            var color = vignetteImage.color;
            color.a = alpha;
            vignetteImage.color = color;
        }

        private Sprite BuildVignetteSprite()
        {
            var texture = new Texture2D(textureResolution, textureResolution, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[textureResolution * textureResolution];
            var center = new Vector2(textureResolution - 1, textureResolution - 1) * 0.5f;
            var maxDistance = center.magnitude;
            var baseColor = new Color32(
                (byte)(flashColor.r * 255f),
                (byte)(flashColor.g * 255f),
                (byte)(flashColor.b * 255f),
                0);

            for (var y = 0; y < textureResolution; y++)
            {
                for (var x = 0; x < textureResolution; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), center) / maxDistance;
                    var alpha = Mathf.Clamp01(Mathf.InverseLerp(innerRadius, 1f, distance));
                    var pixelColor = baseColor;
                    pixelColor.a = (byte)(alpha * 255f);
                    pixels[y * textureResolution + x] = pixelColor;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, textureResolution, textureResolution), Vector2.one * 0.5f);
        }
    }
}
