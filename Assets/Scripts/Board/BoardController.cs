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
        private readonly List<TileView> tiles = new();
        [SerializeField] private TileView tilePrefab = null;

        public event Action<TileView, Vector3, Vector2> TileClicked;
        public bool AcceptsInput { get; private set; }
        
        private float startX;
        private float startY;
        private float pitch;
        private int columns;
        private int rows;
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

            pitch = config.CellPitch;
            columns = level.Columns;
            rows = level.Rows;
            startX = -((columns - 1) * pitch) * 0.5f;
            startY = ((rows - 1) * pitch) * 0.5f;
            EnsurePool(level.CellCount);

            int tileIndex = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    TileType type = level.GetTile(column, row);
                    TileView tile = tiles[tileIndex];
                    tile.gameObject.name = $"Tile_{column:00}_{row:00}_{type}";
                    tile.gameObject.SetActive(true);
                    tile.transform.SetParent(transform, false);
                    tile.transform.localPosition = new Vector3(startX + column * pitch, startY - row * pitch, 0f);
                    tile.Initialize(type, new GridCoordinate(column, row), config.TileSize);
                    tileIndex++;
                }
            }

            for (int i = tileIndex; i < tiles.Count; i++)
            {
                tiles[i].gameObject.SetActive(false);
            }
        }

        public void SetInputEnabled(bool enabled)
        {
            AcceptsInput = enabled;
            for (int i = 0; i < tiles.Count; i++)
            {
                tiles[i].SetInteractable(enabled);
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
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

            float colFloat = (localPosition.x - startX) / pitch;
            float rowFloat = (startY - localPosition.y) / pitch;

            int col = Mathf.RoundToInt(colFloat);
            int row = Mathf.RoundToInt(rowFloat);

            if (col >= 0 && col < columns && row >= 0 && row < rows)
            {
                if (Mathf.Abs(colFloat - col) <= 0.5f && Mathf.Abs(rowFloat - row) <= 0.5f)
                {
                    int index = row * columns + col;
                    if (index >= 0 && index < tiles.Count)
                    {
                        return tiles[index];
                    }
                }
            }

            return null;
        }
        #endregion

        #region Pooling
        private void EnsurePool(int requiredCount)
        {
            while (tiles.Count < requiredCount)
            {
                TileView tile = CreateTile();
                tile.gameObject.SetActive(false);
                tiles.Add(tile);
            }
        }

        private TileView CreateTile()
        {
            if (tilePrefab != null)
            {
                return Instantiate(tilePrefab, transform);
            }

            GameObject tileObject = new GameObject("Tile");
            tileObject.transform.SetParent(transform, false);
            return tileObject.AddComponent<TileView>();
        }
        #endregion
    }
}
