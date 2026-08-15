using System;
using System.Collections.Generic;
using UnityEngine;

namespace FOG.EscapeTheLava
{
    /// <summary>
    /// Handles the physical instantiation and mathematical positioning of the grid.
    /// It maps raw world coordinates to logical grid coordinates without relying on physics raycasts.
    /// </summary>
    public sealed class BoardController : MonoBehaviour
    {
        #region State
        private readonly List<TileView> _tiles = new();
        [SerializeField] private TileView tilePrefab = null;

        public event Action<TileView, Vector3, Vector2> TileClicked;
        public bool AcceptsInput { get; private set; }
        
        private float _startX;
        private float _startY;
        private float _pitch;
        private int _columns;
        private int _rows;
        #endregion

        #region Setup
        public void SetTilePrefab(TileView prefab)
        {
            tilePrefab = prefab;
        }

        public void Build(LevelDefinition level, GameConfig config)
        {
            level.ValidateOrThrow();
            AcceptsInput = true;

            _pitch = config.CellPitch;
            _columns = level.Columns;
            _rows = level.Rows;
            _startX = -((_columns - 1) * _pitch) * 0.5f;
            _startY = ((_rows - 1) * _pitch) * 0.5f;
            EnsurePool(level.CellCount);

            var tileIndex = 0;
            for (var row = 0; row < _rows; row++)
            {
                for (var column = 0; column < _columns; column++)
                {
                    var type = level.GetTile(column, row);
                    var tile = _tiles[tileIndex];
#if UNITY_EDITOR
                    // Naming is purely a hierarchy-debugging aid, so skip the string allocation in builds.
                    tile.gameObject.name = $"Tile_{column:00}_{row:00}_{type}";
#endif
                    tile.gameObject.SetActive(true);
                    tile.transform.SetParent(transform, false);
                    tile.transform.localPosition = new Vector3(_startX + column * _pitch, _startY - row * _pitch, 0f);
                    tile.Initialize(type, new GridCoordinate(column, row), config.tileSize);
                    tileIndex++;
                }
            }

            for (var i = tileIndex; i < _tiles.Count; i++)
            {
                _tiles[i].gameObject.SetActive(false);
            }
        }

        public void SetInputEnabled(bool enabled)
        {
            AcceptsInput = enabled;
            for (var i = 0; i < _tiles.Count; i++)
            {
                _tiles[i].SetInteractable(enabled);
            }
        }
        #endregion

        #region Input Mapping
        public void NotifyTileClicked(TileView tile, Vector3 worldPosition, Vector2 screenPosition)
        {
            if (!AcceptsInput || tile == null)
            {
                return;
            }

            TileClicked?.Invoke(tile, worldPosition, screenPosition);
        }

        public TileView GetTileAtWorldPosition(Vector3 worldPosition)
        {
            if (!AcceptsInput) return null;

            // Convert world click position to local position relative to the BoardController
            // This fixes offset issues if the BoardController GameObject isn't exactly at (0,0,0)
            var localPosition = transform.InverseTransformPoint(worldPosition);

            var columnFraction = (localPosition.x - _startX) / _pitch;
            var rowFraction = (_startY - localPosition.y) / _pitch;

            var column = Mathf.RoundToInt(columnFraction);
            var row = Mathf.RoundToInt(rowFraction);

            if (column >= 0 && column < _columns && row >= 0 && row < _rows)
            {
                if (Mathf.Abs(columnFraction - column) <= 0.5f && Mathf.Abs(rowFraction - row) <= 0.5f)
                {
                    var tileIndex = row * _columns + column;
                    if (tileIndex >= 0 && tileIndex < _tiles.Count)
                    {
                        return _tiles[tileIndex];
                    }
                }
            }

            return null;
        }
        #endregion

        #region Pooling
        private void EnsurePool(int requiredCount)
        {
            while (_tiles.Count < requiredCount)
            {
                var tile = CreateTile();
                tile.gameObject.SetActive(false);
                _tiles.Add(tile);
            }
        }

        private TileView CreateTile()
        {
            if (tilePrefab != null)
            {
                return Instantiate(tilePrefab, transform);
            }

            var tileObject = new GameObject("Tile");
            tileObject.transform.SetParent(transform, false);
            return tileObject.AddComponent<TileView>();
        }
        #endregion
    }
}
