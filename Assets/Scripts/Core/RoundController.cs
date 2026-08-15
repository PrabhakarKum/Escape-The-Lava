using System;
using System.Collections;
using UnityEngine;

namespace FOG.EscapeTheLava
{
    /// <summary>
    /// Serves as the central state machine for a single round of Escape The Lava.
    /// Tracks time, lives, and score, while broadcasting events for the UI and FX systems to hook into.
    /// </summary>
    public sealed class RoundController : MonoBehaviour
    {
        #region State
        private GameConfig _config;
        private LevelDefinition _level;
        private BoardController _board;

        private RoundState _state = RoundState.Booting;
        private float _remainingTime;
        private int _lives;
        private int _score;
        private int _diamondsCollected;
        private int _totalDiamonds;
        private Coroutine _endRoundRoutine;
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
            _config = gameConfig;
            _board = boardController;

            _board.TileClicked += HandleTileClicked;
        }

        private void OnDestroy()
        {
            if (_board != null)
            {
                _board.TileClicked -= HandleTileClicked;
            }
        }

        private void Update()
        {
            if (_state != RoundState.Playing)
            {
                return;
            }

            _remainingTime = Mathf.Max(0f, _remainingTime - Time.deltaTime);
            OnTimerUpdated?.Invoke(_remainingTime);

            if (_remainingTime <= 0f)
            {
                EndRound(false, RoundEndReason.TimeExpired);
            }
        }
        #endregion

        #region Round Flow
        public void StartRound(LevelDefinition levelDefinition)
        {
            if (_endRoundRoutine != null)
            {
                StopCoroutine(_endRoundRoutine);
                _endRoundRoutine = null;
            }

            _level = levelDefinition;
            _state = RoundState.Restarting;

            _remainingTime = _config.roundDurationSeconds;
            _lives = _config.startingLives;
            _score = 0;
            _diamondsCollected = 0;
            _totalDiamonds = _level.CountDiamonds();

            _board.Build(_level, _config);
            _board.SetInputEnabled(true);

            OnRoundStarted?.Invoke();
            OnTimerUpdated?.Invoke(_remainingTime);
            OnLivesChanged?.Invoke(_lives);
            OnScoreChanged?.Invoke(_score, _diamondsCollected, _totalDiamonds);

            _state = RoundState.Playing;
        }

        private void EndRound(bool won, RoundEndReason reason)
        {
            if (_state != RoundState.Playing)
            {
                return;
            }

            _state = won ? RoundState.Won : RoundState.Lost;
            _board.SetInputEnabled(false);

            var result = new RoundResult(won, reason, _score, _diamondsCollected, _totalDiamonds, _remainingTime);
            _endRoundRoutine = StartCoroutine(ShowEndScreenAfterDelay(result));
        }

        private IEnumerator ShowEndScreenAfterDelay(RoundResult result)
        {
            yield return new WaitForSeconds(_config.endScreenDelay);
            OnRoundEnded?.Invoke(result);
            _endRoundRoutine = null;
        }
        #endregion

        #region Tile Interactions
        private void HandleTileClicked(TileView tile, Vector3 worldPosition, Vector2 screenPosition)
        {
            if (_state != RoundState.Playing || tile == null)
            {
                return;
            }

            switch (tile.Type)
            {
                case TileType.Diamond when !tile.IsCollected:
                    CollectDiamond(tile, worldPosition, screenPosition);
                    return;
                case TileType.Lava:
                    HitLava(tile, worldPosition, screenPosition);
                    return;
                default:
                    tile.PlaySafeFeedback();
                    OnSafeTap?.Invoke(worldPosition);
                    break;
            }
        }

        private void CollectDiamond(TileView tile, Vector3 worldPosition, Vector2 screenPosition)
        {
            tile.Collect();
            tile.SetType(TileType.Island);

            _score += _config.diamondScoreValue;
            _diamondsCollected++;

            OnScoreChanged?.Invoke(_score, _diamondsCollected, _totalDiamonds);
            OnDiamondCollected?.Invoke(worldPosition, screenPosition, _config.diamondScoreValue);

            if (_diamondsCollected >= _totalDiamonds)
            {
                EndRound(true, RoundEndReason.AllDiamondsCollected);
            }
        }

        private void HitLava(TileView tile, Vector3 worldPosition, Vector2 screenPosition)
        {
            _lives = Mathf.Max(0, _lives - 1);
            tile.PlayDamageFeedback();
            
            OnLivesChanged?.Invoke(_lives);
            OnLavaHit?.Invoke(worldPosition, screenPosition);

            if (_lives <= 0)
            {
                EndRound(false, RoundEndReason.LivesDepleted);
            }
        }
        #endregion
    }
}
