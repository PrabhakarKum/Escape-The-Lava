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
        #endregion

        #region Public API
        public void Initialize(GameConfig config)
        {
            CurrentLevelIndex = 0;
            LoadCurrentLevel(config);
        }

        public void AdvanceLevel(GameConfig config)
        {
            if (randomMode || CurrentLevelIndex < progressionLevels.Length - 1)
            {
                CurrentLevelIndex++;
            }

            LoadCurrentLevel(config);
        }

        public void ResetProgression(GameConfig config)
        {
            CurrentLevelIndex = 0;
            LoadCurrentLevel(config);
        }
        
        public bool HasNextLevel()
        {
            return randomMode || CurrentLevelIndex < progressionLevels.Length - 1;
        }
        
        public string GetLevelName()
        {
            if (randomMode)
            {
                return $"Random {CurrentLevelIndex + 1}";
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
            if (randomMode)
            {
                CurrentLevel = GenerateRandomLevel(config);
            }
            else
            {
                if (progressionLevels.Length > 0 && CurrentLevelIndex < progressionLevels.Length)
                {
                    CurrentLevel = progressionLevels[CurrentLevelIndex];
                }
                else
                {
                    // Fallback if no levels are assigned
                    CurrentLevel = LevelDefinition.CreateDefault(config);
                }
            }
        }

        private LevelDefinition GenerateRandomLevel(GameConfig config)
        {
            var level = ScriptableObject.CreateInstance<LevelDefinition>();
            level.name = $"Random Level {CurrentLevelIndex + 1}";

            var columns = UnityEngine.Random.Range(config.randomMinColumns, config.randomMaxColumns + 1);
            var rows = UnityEngine.Random.Range(config.randomMinRows, config.randomMaxRows + 1);

            // Levels grow slightly with progression so later rounds feel harder.
            columns = Mathf.Min(columns + CurrentLevelIndex, 40);
            rows = Mathf.Min(rows + CurrentLevelIndex, 24);

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
            for (var i = 0; i < generatedTiles.Length; i++)
            {
                if (generatedTiles[i] == TileType.Diamond)
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
