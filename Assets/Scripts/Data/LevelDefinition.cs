using System;
using UnityEngine;

namespace FOG.EscapeTheLava
{
    [CreateAssetMenu(menuName = "Escape The Lava/Level Definition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [SerializeField] private int columns = 16;
        [SerializeField] private int rows = 8;
        [SerializeField] private TileType[] tiles = Array.Empty<TileType>();

        public int Columns => columns;
        public int Rows => rows;
        public int CellCount => Mathf.Max(0, columns) * Mathf.Max(0, rows);

        public TileType GetTile(int column, int row)
        {
            var index = row * columns + column;
            if (tiles == null || index < 0 || index >= tiles.Length)
            {
                return TileType.Island;
            }

            return tiles[index];
        }

        public int CountDiamonds()
        {
            var count = 0;
            if (tiles == null)
            {
                return count;
            }

            foreach (var t in tiles)
            {
                if (t == TileType.Diamond)
                {
                    count++;
                }
            }

            return count;
        }

        public void ValidateOrThrow()
        {
            if (columns <= 0 || rows <= 0)
            {
                throw new InvalidOperationException($"Level {name} has invalid dimensions {columns} x {rows}.");
            }

            var expected = columns * rows;
            if (tiles == null || tiles.Length != expected)
            {
                throw new InvalidOperationException($"Level {name} has {tiles?.Length ?? 0} tiles but expected {expected}.");
            }
        }

        public void Configure(int levelColumns, int levelRows, TileType[] levelTiles)
        {
            columns = levelColumns;
            rows = levelRows;
            tiles = new TileType[levelTiles.Length];
            Array.Copy(levelTiles, tiles, levelTiles.Length);
        }
    }
}
