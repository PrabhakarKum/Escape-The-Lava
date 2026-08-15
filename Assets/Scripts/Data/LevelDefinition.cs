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

        public static LevelDefinition CreateDefault(GameConfig config)
        {
            var level = CreateInstance<LevelDefinition>();
            level.name = "Level 01";

            string[] layout =
            {
                "GDGLLGDGGLLGDGDG",
                "GGDLGGDGLGGLGDLG",
                "LGGDGLLGGDGLGGDG",
                "GLLGDGGDLGGLDGGG",
                "DGGGLGDGLLGDGGLG",
                "GDLGGLLGGDGGDLGG",
                "GGGDLGGDLGGLGGDD",
                "LDGGLGGDGLGDGGGL"
            };

            if (config.columns != 16 || config.rows != 8)
            {
                level.Configure(config.columns, config.rows, GenerateFallback(config.columns, config.rows));
                return level;
            }

            var generated = new TileType[config.CellCount];
            for (var row = 0; row < config.rows; row++)
            {
                for (var column = 0; column < config.columns; column++)
                {
                    generated[row * config.columns + column] = FromLayoutChar(layout[row][column]);
                }
            }

            level.Configure(config.columns, config.rows, generated);
            return level;
        }

        private static TileType[] GenerateFallback(int columns, int rows)
        {
            var generated = new TileType[columns * rows];
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    var value = Mathf.Abs((row * 17 + column * 7 + row * column * 3) % 13);
                    generated[row * columns + column] = value switch
                    {
                        0 or 1 or 2 => TileType.Lava,
                        3 or 4 => TileType.Diamond,
                        _ => TileType.Island
                    };
                }
            }

            return generated;
        }

        private static TileType FromLayoutChar(char layoutChar)
        {
            return layoutChar switch
            {
                'D' => TileType.Diamond,
                'L' => TileType.Lava,
                _ => TileType.Island
            };
        }
    }
}
