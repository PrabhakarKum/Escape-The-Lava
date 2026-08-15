using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FOG.EscapeTheLava
{
    public sealed class WorldFxSpawner : MonoBehaviour
    {
        [SerializeField] private Sprite ringSprite = null;
        private Material particleMaterial;
        private readonly Queue<ParticleSystem> burstPool = new();
        private readonly Queue<SpriteRenderer> ringPool = new();

        public void Initialize()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                particleMaterial = new Material(shader);
            }
        }

        public void PlayDiamondCollect(Vector3 worldPosition)
        {
            SpawnBurst("Diamond Spark", worldPosition, new Color(0.4f, 0.94f, 1f, 1f), new Color(1f, 1f, 1f, 1f), 16, 2.2f, 0.12f);
            SpawnRing(worldPosition, new Color(0.42f, 0.95f, 1f, 0.82f), 0.48f, 1.35f, 0.34f);
        }

        public void PlayLavaHit(Vector3 worldPosition)
        {
            SpawnBurst("Lava Splash", worldPosition, new Color(1f, 0.22f, 0.04f, 1f), new Color(1f, 0.78f, 0.08f, 1f), 24, 2.8f, 0.16f);
            SpawnRing(worldPosition, new Color(1f, 0.24f, 0.04f, 0.92f), 0.55f, 1.5f, 0.26f);
        }

        public void PlaySafeTap(Vector3 worldPosition)
        {
            SpawnRing(worldPosition, new Color(0.72f, 1f, 0.44f, 0.38f), 0.42f, 0.95f, 0.18f);
        }

        private void SpawnBurst(string name, Vector3 worldPosition, Color startColor, Color endColor, short count, float speed, float size)
        {
            ParticleSystem particles = GetBurst();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            GameObject fxObject = particles.gameObject;
            fxObject.name = name;
            fxObject.SetActive(true);
            fxObject.transform.SetParent(transform, false);
            fxObject.transform.position = worldPosition + new Vector3(0f, 0f, -0.2f);

            ParticleSystem.MainModule main = particles.main;
            main.duration = 0.38f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.22f, 0.46f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.45f, speed);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.55f, size);
            main.startColor = new ParticleSystem.MinMaxGradient(startColor, endColor);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.2f;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.08f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(startColor, 0f), new GradientColorKey(endColor, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;

            ParticleSystemRenderer renderer = fxObject.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 90;
            if (particleMaterial != null)
            {
                renderer.material = particleMaterial;
            }

            particles.Play();
            StartCoroutine(ReleaseBurstAfterDelay(particles, 1.2f));
        }

        private void SpawnRing(Vector3 worldPosition, Color color, float startScale, float endScale, float duration)
        {
            SpriteRenderer renderer = GetRing();
            GameObject ringObject = renderer.gameObject;
            ringObject.name = "Tap Ring";
            ringObject.SetActive(true);
            ringObject.transform.SetParent(transform, false);
            ringObject.transform.position = worldPosition + new Vector3(0f, 0f, -0.15f);

            renderer.sprite = ringSprite; // Replaced sprites.Ring with serialized field
            renderer.color = color;
            renderer.sortingOrder = 80;
            ringObject.transform.localScale = Vector3.one * startScale;

            StartCoroutine(AnimateRing(ringObject, renderer, color, startScale, endScale, duration));
        }

        private ParticleSystem GetBurst()
        {
            while (burstPool.Count > 0)
            {
                ParticleSystem pooled = burstPool.Dequeue();
                if (pooled != null)
                {
                    return pooled;
                }
            }

            GameObject fxObject = new GameObject("Pooled Burst");
            ParticleSystem particles = fxObject.AddComponent<ParticleSystem>();
            
            // Fix: Unity auto-plays new particle systems. We must stop it and disable playOnAwake 
            // before we attempt to modify its properties in SpawnBurst.
            var main = particles.main;
            main.playOnAwake = false;
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            ParticleSystemRenderer renderer = fxObject.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 90;
            if (particleMaterial != null)
            {
                renderer.material = particleMaterial;
            }

            return particles;
        }

        private SpriteRenderer GetRing()
        {
            while (ringPool.Count > 0)
            {
                SpriteRenderer pooled = ringPool.Dequeue();
                if (pooled != null)
                {
                    return pooled;
                }
            }

            GameObject ringObject = new GameObject("Pooled Tap Ring");
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
            burstPool.Enqueue(particles);
        }

        private IEnumerator AnimateRing(GameObject ringObject, SpriteRenderer renderer, Color color, float startScale, float endScale, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - normalized, 2f);
                ringObject.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, eased);
                renderer.color = new Color(color.r, color.g, color.b, color.a * (1f - normalized));
                yield return null;
            }

            ringObject.SetActive(false);
            ringObject.transform.SetParent(transform, false);
            ringPool.Enqueue(renderer);
        }
    }
}
