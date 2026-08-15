using System.Collections;
using UnityEngine;

namespace FOG.EscapeTheLava
{
    public sealed class TileView : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private Sprite islandSprite = null;
        [SerializeField] private Sprite lavaSprite = null;
        [SerializeField] private Sprite diamondSprite = null;

        [Header("References")]
        [SerializeField] private Transform baseVisualRoot = null;
        [SerializeField] private Transform iconVisualRoot = null;
        
        private SpriteRenderer baseRenderer;
        private SpriteRenderer iconRenderer;
        private BoxCollider2D boxCollider;
        private TileIdleAnimator idleAnimator;
        private Coroutine collectRoutine;

        public TileType Type { get; private set; }
        public GridCoordinate Coordinate { get; private set; }
        public bool IsCollected { get; private set; }

        public void Initialize(TileType type, GridCoordinate coordinate, float tileSize)
        {
            Coordinate = coordinate;
            Type = type;
            IsCollected = false;

            if (collectRoutine != null)
            {
                StopCoroutine(collectRoutine);
                collectRoutine = null;
            }

            baseRenderer = EnsureRenderer("Base", 10, ref baseVisualRoot);
            iconRenderer = EnsureRenderer("Icon", 20, ref iconVisualRoot);

            boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider2D>();
            }

            boxCollider.size = Vector2.one * tileSize;
            boxCollider.isTrigger = true;

            idleAnimator = GetComponent<TileIdleAnimator>();
            if (idleAnimator == null)
            {
                idleAnimator = gameObject.AddComponent<TileIdleAnimator>();
            }

            ApplyVisual(type, tileSize);
        }

        public void SetInteractable(bool interactable)
        {
            if (boxCollider != null)
            {
                boxCollider.enabled = interactable;
            }
        }

        public void Collect()
        {
            if (IsCollected)
            {
                return;
            }

            IsCollected = true;
            collectRoutine = StartCoroutine(CollectRoutine());
        }

        public void SetType(TileType newType)
        {
            float tileSize = boxCollider.size.x;

            if (collectRoutine != null)
            {
                // A collect animation is still fading the icon out — only swap the
                // base/type here and let CollectRoutine finish owning the icon renderer,
                // otherwise ApplyVisual would disable it mid-animation.
                ApplyBaseVisual(newType, tileSize);
                idleAnimator.Initialize(newType, baseRenderer, iconRenderer, tileSize);
                return;
            }

            ApplyVisual(newType, tileSize);
        }

        public void PlayDamageFeedback()
        {
            idleAnimator.TriggerImpactPulse();
        }

        public void PlaySafeFeedback()
        {
            idleAnimator.TriggerSoftPulse();
        }

        private SpriteRenderer EnsureRenderer(string childName, int sortingOrder, ref Transform visualRoot)
        {
            if (visualRoot == null)
            {
                visualRoot = transform.Find(childName);
            }

            if (visualRoot == null)
            {
                visualRoot = new GameObject(childName).transform;
                visualRoot.SetParent(transform, false);
            }

            SpriteRenderer renderer = visualRoot.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = visualRoot.gameObject.AddComponent<SpriteRenderer>();
            }

            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private void ApplyVisual(TileType tileType, float tileSize)
        {
            ApplyBaseVisual(tileType, tileSize);

            bool hasDiamond = tileType == TileType.Diamond;
            iconRenderer.enabled = hasDiamond;
            iconRenderer.sprite = hasDiamond ? diamondSprite : null;
            iconRenderer.color = Color.white;
            iconRenderer.transform.localPosition = new Vector3(0f, 0.08f, -0.02f);
            iconRenderer.transform.localScale = Vector3.one * 0.72f;

            // Must run after the icon transform above is reset to its canonical scale/position,
            // since Initialize snapshots the icon's current transform as the idle animation's baseline.
            idleAnimator.Initialize(tileType, baseRenderer, iconRenderer, tileSize);
        }

        private void ApplyBaseVisual(TileType tileType, float tileSize)
        {
            Type = tileType;
            baseRenderer.sprite = tileType == TileType.Lava ? lavaSprite : islandSprite;
            baseRenderer.color = Color.white;
        }

        private IEnumerator CollectRoutine()
        {
            float duration = 0.22f;
            float elapsed = 0f;
            Vector3 startScale = iconRenderer.transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                float pop = Mathf.Sin(normalized * Mathf.PI) * 0.25f;
                iconRenderer.transform.localScale = startScale * (1f + pop - normalized * 0.35f);
                iconRenderer.color = new Color(1f, 1f, 1f, 1f - normalized);
                yield return null;
            }

            iconRenderer.enabled = false;
            iconRenderer.sprite = null;
            // Removed MVC violation: We no longer mutate our own type here!
            collectRoutine = null;
        }
    }
}
