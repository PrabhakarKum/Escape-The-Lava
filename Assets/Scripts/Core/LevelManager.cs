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
            if (randomMode)
            {
                CurrentLevelIndex++;
                LoadCurrentLevel(config);
            }
            else
            {
                if (CurrentLevelIndex < progressionLevels.Length - 1)
                {
                    CurrentLevelIndex++;
                }
                LoadCurrentLevel(config);
            }
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
            LevelDefinition level = ScriptableObject.CreateInstance<LevelDefinition>();
            level.name = $"Random Level {CurrentLevelIndex + 1}";

            int cols = UnityEngine.Random.Range(config.RandomMinColumns, config.RandomMaxColumns + 1);
            int rows = UnityEngine.Random.Range(config.RandomMinRows, config.RandomMaxRows + 1);
            
            // Optionally increase size based on CurrentLevelIndex
            cols = Mathf.Min(cols + CurrentLevelIndex, 40);
            rows = Mathf.Min(rows + CurrentLevelIndex, 24);

            TileType[] tiles = new TileType[cols * rows];
            for (int i = 0; i < tiles.Length; i++)
            {
                float rand = UnityEngine.Random.value;
                if (rand < config.DiamondDensity)
                {
                    tiles[i] = TileType.Diamond;
                }
                else if (rand < config.DiamondDensity + config.LavaDensity)
                {
                    tiles[i] = TileType.Lava;
                }
                else
                {
                    tiles[i] = TileType.Island;
                }
            }

            // Ensure at least one diamond exists so the level is winnable
            bool hasDiamond = false;
            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] == TileType.Diamond)
                {
                    hasDiamond = true;
                    break;
                }
            }

            if (!hasDiamond)
            {
                int randomDiamondIndex = UnityEngine.Random.Range(0, tiles.Length);
                tiles[randomDiamondIndex] = TileType.Diamond;
            }

            level.Configure(cols, rows, tiles);
            return level;
        }
        #endregion
    }
}
