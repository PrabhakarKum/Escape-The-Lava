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
        
        private SpriteRenderer _baseRenderer;
        private SpriteRenderer _iconRenderer;
        private BoxCollider2D _boxCollider;
        private TileAnimator _animator;
        private Coroutine _collectRoutine;

        public TileType Type { get; private set; }
        public GridCoordinate Coordinate { get; private set; }
        public bool IsCollected { get; private set; }

        public void Initialize(TileType type, GridCoordinate coordinate, float tileSize)
        {
            Coordinate = coordinate;
            Type = type;
            IsCollected = false;

            if (_collectRoutine != null)
            {
                StopCoroutine(_collectRoutine);
                _collectRoutine = null;
            }

            _baseRenderer = EnsureRenderer("Base", 10, ref baseVisualRoot);
            _iconRenderer = EnsureRenderer("Icon", 20, ref iconVisualRoot);

            _boxCollider = GetComponent<BoxCollider2D>();
            if (_boxCollider == null)
            {
                _boxCollider = gameObject.AddComponent<BoxCollider2D>();
            }

            _boxCollider.size = Vector2.one * tileSize;
            _boxCollider.isTrigger = true;

            _animator = GetComponent<TileAnimator>();
            if (_animator == null)
            {
                _animator = gameObject.AddComponent<TileAnimator>();
            }

            ApplyVisual(type, tileSize);
        }

        public void SetInteractable(bool interactable)
        {
            if (_boxCollider != null)
            {
                _boxCollider.enabled = interactable;
            }
        }

        public void Collect()
        {
            if (IsCollected)
            {
                return;
            }

            IsCollected = true;
            _collectRoutine = StartCoroutine(CollectRoutine());
        }

        public void SetType(TileType newType)
        {
            var tileSize = _boxCollider.size.x;

            if (_collectRoutine != null)
            {
                // A collect animation is still fading the icon out — only swap the
                // base/type here and let CollectRoutine finish owning the icon renderer,
                // otherwise ApplyVisual would disable it mid-animation.
                ApplyBaseVisual(newType, tileSize);
                _animator.Initialize(newType, _baseRenderer, _iconRenderer, tileSize);
                return;
            }

            ApplyVisual(newType, tileSize);
        }

        public void PlayDamageFeedback()
        {
            _animator.TriggerImpactPulse();
        }

        public void PlaySafeFeedback()
        {
            _animator.TriggerSoftPulse();
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

            var renderer = visualRoot.GetComponent<SpriteRenderer>();
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

            var hasDiamond = tileType == TileType.Diamond;
            _iconRenderer.enabled = hasDiamond;
            _iconRenderer.sprite = hasDiamond ? diamondSprite : null;
            _iconRenderer.color = Color.white;
            _iconRenderer.transform.localPosition = new Vector3(0f, 0.08f, -0.02f);
            _iconRenderer.transform.localScale = Vector3.one * 0.5f;

            // Must run after the icon transform above is reset to its canonical scale/position,
            // since Initialize snapshots the icon's current transform as the idle animation's baseline.
            _animator.Initialize(tileType, _baseRenderer, _iconRenderer, tileSize);
        }

        private void ApplyBaseVisual(TileType tileType, float tileSize)
        {
            Type = tileType;
            _baseRenderer.sprite = tileType == TileType.Lava ? lavaSprite : islandSprite;
            _baseRenderer.color = Color.white;
        }

        private IEnumerator CollectRoutine()
        {
            var duration = 0.22f;
            var elapsed = 0f;
            var startScale = _iconRenderer.transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var normalized = Mathf.Clamp01(elapsed / duration);
                var pop = Mathf.Sin(normalized * Mathf.PI) * 0.25f;
                _iconRenderer.transform.localScale = startScale * (1f + pop - normalized * 0.35f);
                _iconRenderer.color = new Color(1f, 1f, 1f, 1f - normalized);
                yield return null;
            }

            _iconRenderer.enabled = false;
            _iconRenderer.sprite = null;
            // Removed MVC violation: We no longer mutate our own type here!
            _collectRoutine = null;
        }
    }
}
