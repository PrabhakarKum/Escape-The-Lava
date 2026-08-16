using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FOG.EscapeTheLava
{
    public sealed class WorldFxSpawner : MonoBehaviour
    {
        [SerializeField] private Sprite ringSprite = null;
        private Material _particleMaterial;
        private readonly Queue<ParticleSystem> _burstPool = new();
        private readonly Queue<SpriteRenderer> _ringPool = new();

        // Every burst's color-over-lifetime only ever needs one of these two fixed gradients.
        // Baking them once in Initialize avoids allocating a new Gradient + key arrays on
        // every diamond collect / lava hit, without risking two overlapping bursts fighting
        // over a shared mutable Gradient's keys.
        private Gradient _diamondBurstGradient;
        private Gradient _lavaBurstGradient;

        public void Initialize()
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                _particleMaterial = new Material(shader);
            }

            _diamondBurstGradient = BuildGradient(new Color(0.4f, 0.94f, 1f, 1f), new Color(1f, 1f, 1f, 1f));
            _lavaBurstGradient = BuildGradient(new Color(1f, 0.22f, 0.04f, 1f), new Color(1f, 0.78f, 0.08f, 1f));
        }

        private static Gradient BuildGradient(Color startColor, Color endColor)
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(startColor, 0f), new GradientColorKey(endColor, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            return gradient;
        }

        public void PlayDiamondCollect(Vector3 worldPosition)
        {
            SpawnBurst("Diamond Spark", worldPosition, new Color(0.4f, 0.94f, 1f, 1f), new Color(1f, 1f, 1f, 1f), _diamondBurstGradient, 16, 2.2f, 0.12f);
            SpawnRing(worldPosition, new Color(0.42f, 0.95f, 1f, 0.82f), 0.48f, 1.35f, 0.34f);
        }

        public void PlayLavaHit(Vector3 worldPosition)
        {
            SpawnBurst("Lava Splash", worldPosition, new Color(1f, 0.22f, 0.04f, 1f), new Color(1f, 0.78f, 0.08f, 1f), _lavaBurstGradient, 24, 2.8f, 0.16f);
            SpawnRing(worldPosition, new Color(1f, 0.24f, 0.04f, 0.92f), 0.55f, 1.5f, 0.26f);
        }

        public void PlaySafeTap(Vector3 worldPosition)
        {
            SpawnRing(worldPosition, new Color(0.72f, 1f, 0.44f, 0.38f), 0.42f, 0.95f, 0.18f);
        }

        private void SpawnBurst(string name, Vector3 worldPosition, Color startColor, Color endColor, Gradient colorOverLifetimeGradient, short count, float speed, float size)
        {
            var particles = GetBurst();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var fxObject = particles.gameObject;
            fxObject.name = name;
            fxObject.SetActive(true);
            fxObject.transform.SetParent(transform, false);
            fxObject.transform.position = worldPosition + new Vector3(0f, 0f, -0.2f);

            var main = particles.main;
            main.duration = 0.38f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.22f, 0.46f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.45f, speed);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.55f, size);
            main.startColor = new ParticleSystem.MinMaxGradient(startColor, endColor);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.2f;

            var emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.08f;

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = colorOverLifetimeGradient;

            var renderer = fxObject.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 90;
            if (_particleMaterial != null)
            {
                renderer.material = _particleMaterial;
            }

            particles.Play();
            StartCoroutine(ReleaseBurstAfterDelay(particles, 1.2f));
        }

        private void SpawnRing(Vector3 worldPosition, Color color, float startScale, float endScale, float duration)
        {
            var renderer = GetRing();
            var ringObject = renderer.gameObject;
            ringObject.name = "Tap Ring";
            ringObject.SetActive(true);
            ringObject.transform.SetParent(transform, false);
            ringObject.transform.position = worldPosition + new Vector3(0f, 0f, -0.15f);

            renderer.sprite = ringSprite;
            renderer.color = color;
            renderer.sortingOrder = 80;
            ringObject.transform.localScale = Vector3.one * startScale;

            StartCoroutine(AnimateRing(ringObject, renderer, color, startScale, endScale, duration));
        }

        private ParticleSystem GetBurst()
        {
            while (_burstPool.Count > 0)
            {
                var pooled = _burstPool.Dequeue();
                if (pooled != null)
                {
                    return pooled;
                }
            }

            var fxObject = new GameObject("Pooled Burst");
            var particles = fxObject.AddComponent<ParticleSystem>();
            
            // Fix: Unity auto-plays new particle systems. We must stop it and disable playOnAwake 
            // before we attempt to modify its properties in SpawnBurst.
            var main = particles.main;
            main.playOnAwake = false;
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            var renderer = fxObject.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 90;
            if (_particleMaterial != null)
            {
                renderer.material = _particleMaterial;
            }

            return particles;
        }

        private SpriteRenderer GetRing()
        {
            while (_ringPool.Count > 0)
            {
                var pooled = _ringPool.Dequeue();
                if (pooled != null)
                {
                    return pooled;
                }
            }

            var ringObject = new GameObject("Pooled Tap Ring");
            return ringObject.AddComponent<SpriteRenderer>();
        }

        private IEnumerator ReleaseBurstAfterDelay(ParticleSystem particles, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (particles == null)
            {
                yield break;
            }

            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particles.gameObject.SetActive(false);
            particles.transform.SetParent(transform, false);
            _burstPool.Enqueue(particles);
        }

        private IEnumerator AnimateRing(GameObject ringObject, SpriteRenderer renderer, Color color, float startScale, float endScale, float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var normalized = Mathf.Clamp01(elapsed / duration);
                var eased = 1f - Mathf.Pow(1f - normalized, 2f);
                ringObject.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, eased);
                renderer.color = new Color(color.r, color.g, color.b, color.a * (1f - normalized));
                yield return null;
            }

            ringObject.SetActive(false);
            ringObject.transform.SetParent(transform, false);
            _ringPool.Enqueue(renderer);
        }
    }
}
