using UnityEngine;

namespace FOG.EscapeTheLava
{
    [CreateAssetMenu(menuName = "Escape The Lava/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Grid")]
        [Min(1)] public int columns = 16;
        [Min(1)] public int rows = 8;
        [Min(0.1f)] public float tileSize = 1f;
        [Min(0f)] public float tileGap = 0.08f;

        [Header("Rules")]
        [Min(1f)] public float roundDurationSeconds = 30f;
        [Min(1)] public int startingLives = 5;
        [Min(1)] public int diamondScoreValue = 1;

        [Header("Feel")]
        [Min(0f)] public float cameraShakeDuration = 0.16f;
        [Min(0f)] public float cameraShakeStrength = 0.12f;
        [Min(0.1f)] public float floatingTextDuration = 0.85f;
        [Min(0.1f)] public float endScreenDelay = 0.35f;
        [Min(0f)] public float lavaFlashDuration = 0.35f;
        [Range(0f, 1f)] public float lavaFlashAlpha = 0.55f;

        [Header("Random Generation")]
        [Min(4)] public int randomMinColumns = 8;
        [Min(4)] public int randomMaxColumns = 24;
        [Min(4)] public int randomMinRows = 8;
        [Min(4)] public int randomMaxRows = 16;
        [Range(0f, 1f)] public float lavaDensity = 0.25f;
        [Range(0f, 1f)] public float diamondDensity = 0.1f;

        public float CellPitch => tileSize + tileGap;
        public int CellCount => columns * rows;

        public static GameConfig CreateRuntimeDefault()
        {
            var config = CreateInstance<GameConfig>();
            config.name = "Runtime Escape The Lava Config";
            return config;
        }
    }
}
