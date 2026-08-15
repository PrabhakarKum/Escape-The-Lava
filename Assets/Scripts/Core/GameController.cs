using System;
using System.Collections;
using UnityEngine;

namespace FOG.EscapeTheLava
{
    /// <summary>
    /// Serves as the central state machine for a single round of Escape The Lava.
    /// Tracks time, lives, and score, while broadcasting events for the UI and FX systems to hook into.
    /// </summary>
    public sealed class GameController : MonoBehaviour
    {
        #region State
        private GameConfig config;
        private LevelDefinition level;
        private BoardController board;

        private GameState state = GameState.Booting;
        private float remainingTime;
        private int lives;
        private int score;
        private int diamondsCollected;
        private int totalDiamonds;
        private Coroutine endRoundRoutine;
        #endregion

        #region Events
        public event Action<float> OnTimerUpdated;
        public event Action<int> OnLivesChanged;
        public event Action<int, int, int> OnScoreChanged;
        public event Action<RoundResult> OnRoundEnded;
        public event Action OnRoundStarted;
        
        public event Action<Vector3, Vector2, int> OnDiamondCollected;
        public event Action<Vector3, Vector2> OnLavaHit;
        public event Action<Vector3> OnSafeTap;
        #endregion

        #region Lifecycle
        public void Initialize(GameConfig gameConfig, BoardController boardController)
        {
            config = gameConfig;
            board = boardController;

            board.TileClicked += HandleTileClicked;
        }

        private void OnDestroy()
        {
            if (board != null)
            {
                board.TileClicked -= HandleTileClicked;
            }
        }

        private void Update()
        {
            if (state != GameState.Playing)
            {
                return;
            }

            remainingTime = Mathf.Max(0f, remainingTime - Time.deltaTime);
            OnTimerUpdated?.Invoke(remainingTime);

            if (remainingTime <= 0f)
            {
                EndRound(false, RoundEndReason.TimeExpired);
            }
        }
        #endregion

        #region Round Flow
        public void StartRound(LevelDefinition levelDefinition)
        {
            if (endRoundRoutine != null)
            {
                StopCoroutine(endRoundRoutine);
                endRoundRoutine = null;
            }

            level = levelDefinition;
            state = GameState.Restarting;

            remainingTime = config.RoundDurationSeconds;
            lives = config.StartingLives;
            score = 0;
            diamondsCollected = 0;
            totalDiamonds = level.CountDiamonds();

            board.Build(level, config);
            board.SetInputEnabled(true);

            OnRoundStarted?.Invoke();
            OnTimerUpdated?.Invoke(remainingTime);
            OnLivesChanged?.Invoke(lives);
            OnScoreChanged?.Invoke(score, diamondsCollected, totalDiamonds);

            state = GameState.Playing;
        }

        private void EndRound(bool won, RoundEndReason reason)
        {
            if (state != GameState.Playing)
            {
                return;
            }

            state = won ? GameState.Won : GameState.Lost;
            board.SetInputEnabled(false);

            RoundResult result = new RoundResult(won, reason, score, diamondsCollected, totalDiamonds, remainingTime);
            endRoundRoutine = StartCoroutine(ShowEndScreenAfterDelay(result));
        }

        private IEnumerator ShowEndScreenAfterDelay(RoundResult result)
        {
            yield return new WaitForSeconds(config.EndScreenDelay);
            OnRoundEnded?.Invoke(result);
            endRoundRoutine = null;
        }
        #endregion

        #region Tile Interactions
        private void HandleTileClicked(TileView tile, Vector3 worldPosition, Vector2 screenPosition)
        {
            if (state != GameState.Playing || tile == null)
            {
                return;
            }

            if (tile.Type == TileType.Diamond && !tile.IsCollected)
            {
                CollectDiamond(tile, worldPosition, screenPosition);
                return;
            }

            if (tile.Type == TileType.Lava)
            {
                HitLava(tile, worldPosition, screenPosition);
                return;
            }

            tile.PlaySafeFeedback();
            OnSafeTap?.Invoke(worldPosition);
        }

        private void CollectDiamond(TileView tile, Vector3 worldPosition, Vector2 screenPosition)
        {
            tile.Collect();
            tile.SetType(TileType.Island);

            score += config.DiamondScoreValue;
            diamondsCollected++;

            OnScoreChanged?.Invoke(score, diamondsCollected, totalDiamonds);
            OnDiamondCollected?.Invoke(worldPosition, screenPosition, config.DiamondScoreValue);

            if (diamondsCollected >= totalDiamonds)
            {
                EndRound(true, RoundEndReason.AllDiamondsCollected);
            }
        }

        private void HitLava(TileView tile, Vector3 worldPosition, Vector2 screenPosition)
        {
            lives = Mathf.Max(0, lives - 1);
            tile.PlayDamageFeedback();
            
            OnLivesChanged?.Invoke(lives);
            OnLavaHit?.Invoke(worldPosition, screenPosition);

            if (lives <= 0)
            {
                EndRound(false, RoundEndReason.LivesDepleted);
            }
        }
        #endregion
    }
}
