using System.Collections;
using UnityEngine;

namespace FOG.EscapeTheLava
{
    public sealed class CameraShake : MonoBehaviour
    {
        private Coroutine shakeRoutine;
        private Vector3 baseLocalPosition;

        private void Awake()
        {
            baseLocalPosition = transform.localPosition;
        }

        public void Shake(float duration, float strength)
        {
            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
            }

            shakeRoutine = StartCoroutine(ShakeRoutine(duration, strength));
        }

        private IEnumerator ShakeRoutine(float duration, float strength)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                float falloff = 1f - normalized;
                float x = Mathf.Sin(Time.time * 85f) * strength * falloff;
                float y = Mathf.Cos(Time.time * 73f) * strength * falloff;
                transform.localPosition = baseLocalPosition + new Vector3(x, y, 0f);
                yield return null;
            }

            transform.localPosition = baseLocalPosition;
            shakeRoutine = null;
        }
    }
}
