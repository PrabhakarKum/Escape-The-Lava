using System.Collections;
using UnityEngine;

namespace FOG.EscapeTheLava
{
    public sealed class CameraShake : MonoBehaviour
    {
        private const float HorizontalShakeFrequency = 85f;
        private const float VerticalShakeFrequency = 73f;

        private Coroutine _shakeRoutine;
        private Vector3 _baseLocalPosition;

        private void Awake()
        {
            _baseLocalPosition = transform.localPosition;
        }

        public void Shake(float duration, float strength)
        {
            if (_shakeRoutine != null)
            {
                StopCoroutine(_shakeRoutine);
            }

            _shakeRoutine = StartCoroutine(ShakeRoutine(duration, strength));
        }

        private IEnumerator ShakeRoutine(float duration, float strength)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var normalized = Mathf.Clamp01(elapsed / duration);
                var falloff = 1f - normalized;
                var offsetX = Mathf.Sin(Time.time * HorizontalShakeFrequency) * strength * falloff;
                var offsetY = Mathf.Cos(Time.time * VerticalShakeFrequency) * strength * falloff;
                transform.localPosition = _baseLocalPosition + new Vector3(offsetX, offsetY, 0f);
                yield return null;
            }

            transform.localPosition = _baseLocalPosition;
            _shakeRoutine = null;
        }
    }
}
