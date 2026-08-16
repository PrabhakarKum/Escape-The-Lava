using System;
using UnityEngine;

namespace FOG.EscapeTheLava
{
    /// <summary>
    /// Manages the progression of levels, seamlessly transitioning between hand-crafted competitive stages 
    /// and infinitely scaling procedural layouts when random mode is active.
    /// </summary>
    public sealed class LevelManager : MonoBehaviour
    {
        #region Settings
        [Header("Mode")]
        [SerializeField] private bool randomMode = false;
        
        [Header("Progression Levels")]
        [SerializeField] private LevelDefinition[] progressionLevels = Array.Empty<LevelDefinition>();
        #endregion

        #region State
        public int CurrentLevelIndex { get; private set; }
        public LevelDefinition CurrentLevel { get; private set; }

        // Set once the curated progressionLevels run out, so every level after that is freshly
        // generated instead of repeating the last curated level (or the default fallback) forever.
        private bool _proceduralFallbackActive;
        private bool UsingProceduralGeneration => randomMode || _proceduralFallbackActive;
        #endregion

        #region Public API
        public void Initialize(GameConfig config)
        {
            CurrentLevelIndex = 0;
            _proceduralFallbackActive = false;
            LoadCurrentLevel(config);
        }

        public void AdvanceLevel(GameConfig config)
        {
            CurrentLevelIndex++;

            if (!randomMode && !_proceduralFallbackActive && CurrentLevelIndex >= progressionLevels.Length)
            {
                _proceduralFallbackActive = true;
            }

            LoadCurrentLevel(config);
        }

        public void ResetProgression(GameConfig config)
        {
            CurrentLevelIndex = 0;
            _proceduralFallbackActive = false;
            LoadCurrentLevel(config);
        }

        public bool HasNextLevel()
        {
            // There's always a next level: either another curated one, or a freshly generated
            // one once the curated list runs out.
            return true;
        }

        public string GetLevelName()
        {
            if (UsingProceduralGeneration)
            {
                return $"Level {CurrentLevelIndex + 1}";
            }

            if (CurrentLevel != null && !string.IsNullOrEmpty(CurrentLevel.name))
            {
                return CurrentLevel.name;
            }

            return $"Level {CurrentLevelIndex + 1}";
        }
        #endregion

        #region Internal Logic
        private void LoadCurrentLevel(GameConfig config)
        {
            if (UsingProceduralGeneration)
            {
                CurrentLevel = GenerateRandomLevel(config);
            }
            else if (progressionLevels.Length > 0 && CurrentLevelIndex < progressionLevels.Length)
            {
                CurrentLevel = progressionLevels[CurrentLevelIndex];
            }
            else
            {
                // No curated levels assigned at all — generate one instead of always
                // handing back the same static default level.
                _proceduralFallbackActive = true;
                CurrentLevel = GenerateRandomLevel(config);
            }
        }

        private LevelDefinition GenerateRandomLevel(GameConfig config)
        {
            var level = ScriptableObject.CreateInstance<LevelDefinition>();
            level.name = $"Level {CurrentLevelIndex + 1}";

            var columns = UnityEngine.Random.Range(config.randomMinColumns, config.randomMaxColumns + 1);
            var rows = UnityEngine.Random.Range(config.randomMinRows, config.randomMaxRows + 1);

            // Levels grow slightly with progression so later rounds feel harder, but never
            // past the configured max — changing randomMaxColumns/randomMaxRows should always
            // be a hard ceiling, not just a starting point progression can grow past.
            columns = Mathf.Min(columns + CurrentLevelIndex, config.randomMaxColumns);
            rows = Mathf.Min(rows + CurrentLevelIndex, config.randomMaxRows);

            var generatedTiles = new TileType[columns * rows];
            for (var i = 0; i < generatedTiles.Length; i++)
            {
                var randomValue = UnityEngine.Random.value;
                if (randomValue < config.diamondDensity)
                {
                    generatedTiles[i] = TileType.Diamond;
                }
                else if (randomValue < config.diamondDensity + config.lavaDensity)
                {
                    generatedTiles[i] = TileType.Lava;
                }
                else
                {
                    generatedTiles[i] = TileType.Island;
                }
            }

            // Ensure at least one diamond exists so the level is winnable
            var containsDiamond = false;
            foreach (var t in generatedTiles)
            {
                if (t == TileType.Diamond)
                {
                    containsDiamond = true;
                    break;
                }
            }

            if (!containsDiamond)
            {
                var randomDiamondIndex = UnityEngine.Random.Range(0, generatedTiles.Length);
                generatedTiles[randomDiamondIndex] = TileType.Diamond;
            }

            level.Configure(columns, rows, generatedTiles);
            return level;
        }
        #endregion
    }
}
