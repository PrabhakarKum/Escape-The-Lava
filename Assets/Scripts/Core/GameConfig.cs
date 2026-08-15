using UnityEngine;

namespace FOG.EscapeTheLava
{
    [CreateAssetMenu(menuName = "Escape The Lava/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Grid")]
        [Min(1)] public int Columns = 16;
        [Min(1)] public int Rows = 8;
        [Min(0.1f)] public float TileSize = 1f;
        [Min(0f)] public float TileGap = 0.08f;

        [Header("Rules")]
        [Min(1f)] public float RoundDurationSeconds = 30f;
        [Min(1)] public int StartingLives = 5;
        [Min(1)] public int DiamondScoreValue = 1;

        [Header("Feel")]
        [Min(0f)] public float CameraShakeDuration = 0.16f;
        [Min(0f)] public float CameraShakeStrength = 0.12f;
        [Min(0.1f)] public float FloatingTextDuration = 0.85f;
        [Min(0.1f)] public float EndScreenDelay = 0.35f;

        [Header("Random Generation")]
        [Min(4)] public int RandomMinColumns = 8;
        [Min(4)] public int RandomMaxColumns = 24;
        [Min(4)] public int RandomMinRows = 8;
        [Min(4)] public int RandomMaxRows = 16;
        [Range(0f, 1f)] public float LavaDensity = 0.25f;
        [Range(0f, 1f)] public float DiamondDensity = 0.1f;

        public float CellPitch => TileSize + TileGap;
        public int CellCount => Columns * Rows;

        public static GameConfig CreateRuntimeDefault()
        {
            GameConfig config = CreateInstance<GameConfig>();
            config.name = "Runtime Escape The Lava Config";
            return config;
        }
    }
}
